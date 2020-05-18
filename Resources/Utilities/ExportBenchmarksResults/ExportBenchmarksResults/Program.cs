using System;
using System.IO;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;

namespace ExportBenchmarksResults
{
    class Program
    {
        private static class FieldsNames
        {
            public const string Min = "min";

            public const string Median = "median";

            public const string Mean = "mean";

            public const string Max = "max";
        }

        private static class TagsNames
        {
            public const string BenchmarkDotNetVersion = "benchmark_dot_net_version";

            public const string OsVersion = "os_version";

            public const string ProcessorName = "processor_name";

            public const string PhysicalProcessorCount = "physical_processor_count";

            public const string PhysicalCoreCount = "physical_core_count";

            public const string LogicalCoreCount = "logical_core_count";

            public const string RuntimeVersion = "runtime_version";

            public const string Architecture = "architecture";

            public const string HasRyuJit = "has_ryu_jit";

            public const string BenchmarkName = "benchmark_name";

            public const string Branch = "branch";
        }

        private sealed class BenchmarksResultsDto
        {
            public sealed class HostEnvironmentInfoDto
            {
                public string BenchmarkDotNetVersion { get; set; }

                public string OsVersion { get; set; }

                public string ProcessorName { get; set; }
                
                public int PhysicalProcessorCount { get; set; }
                
                public int PhysicalCoreCount { get; set; }

                public int LogicalCoreCount { get; set; }

                public string RuntimeVersion { get; set; }
                
                public string Architecture { get; set; }

                public bool HasRyuJit { get; set; }
            }

            public sealed class BenchmarkDto
            {
                public sealed class StatisticsDto
                {
                    public double Min { get; set; }

                    public double Median { get; set; }
                    
                    public double Mean { get; set; }
                    
                    public double Max { get; set; }
                }

                public string Type { get; set; }

                public string MethodTitle { get; set; }

                public StatisticsDto Statistics { get; set; }
            }

            public string Title { get; set; }

            public HostEnvironmentInfoDto HostEnvironmentInfo { get; set; }

            public BenchmarkDto[] Benchmarks { get; set; }
        }

        static void Main(string[] args)
        {
            var url = args[0];
            var token = args[1];
            var organization = args[2];
            var bucket = args[3];
            var measurement = args[4];
            var resultsFolderPath = args[5];
            var branch = args[6];

            var client = InfluxDBClientFactory.Create(url, token.ToCharArray());
            var timestamp = DateTime.UtcNow;

            var filesPathes = Directory.GetFiles(resultsFolderPath, "*.json", SearchOption.AllDirectories);
            var totalFilesCount = filesPathes.Length;

            using (var writeApi = client.GetWriteApi())
            {
                for (var i = 0; i < totalFilesCount; i++)
                {
                    var resultsFilePath = filesPathes[i];
                    Console.WriteLine($"Exporting results from file #{i + 1} from {totalFilesCount}: {resultsFilePath}...");

                    var json = System.IO.File.ReadAllText(resultsFilePath);
                    var benchmarksResults = JsonConvert.DeserializeObject<BenchmarksResultsDto>(json);

                    foreach (var benchmark in benchmarksResults.Benchmarks)
                    {
                        var benchmarkName = $"{benchmark.Type}__{benchmark.MethodTitle}";
                        Console.WriteLine($"    Exporting benchmark '{benchmarkName}'...");

                        var point = PointData
                            .Measurement(measurement)

                            .Tag(TagsNames.BenchmarkDotNetVersion, benchmarksResults.HostEnvironmentInfo.BenchmarkDotNetVersion)
                            .Tag(TagsNames.OsVersion, benchmarksResults.HostEnvironmentInfo.OsVersion)
                            .Tag(TagsNames.ProcessorName, benchmarksResults.HostEnvironmentInfo.ProcessorName)
                            .Tag(TagsNames.PhysicalProcessorCount, benchmarksResults.HostEnvironmentInfo.PhysicalProcessorCount.ToString())
                            .Tag(TagsNames.PhysicalCoreCount, benchmarksResults.HostEnvironmentInfo.PhysicalCoreCount.ToString())
                            .Tag(TagsNames.LogicalCoreCount, benchmarksResults.HostEnvironmentInfo.LogicalCoreCount.ToString())
                            .Tag(TagsNames.RuntimeVersion, benchmarksResults.HostEnvironmentInfo.RuntimeVersion)
                            .Tag(TagsNames.Architecture, benchmarksResults.HostEnvironmentInfo.Architecture)
                            .Tag(TagsNames.HasRyuJit, benchmarksResults.HostEnvironmentInfo.HasRyuJit.ToString())
                            .Tag(TagsNames.BenchmarkName, benchmarkName)
                            .Tag(TagsNames.Branch, branch)

                            .Field(FieldsNames.Min, benchmark.Statistics.Min)
                            .Field(FieldsNames.Max, benchmark.Statistics.Max)
                            .Field(FieldsNames.Median, benchmark.Statistics.Median)
                            .Field(FieldsNames.Mean, benchmark.Statistics.Mean)

                            .Timestamp(timestamp, WritePrecision.Ns);

                        writeApi.WritePoint(bucket, organization, point);
                    }
                }
            }

            client.Dispose();
            Console.WriteLine("All done.");
        }
    }
}

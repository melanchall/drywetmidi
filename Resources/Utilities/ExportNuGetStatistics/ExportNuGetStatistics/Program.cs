using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
using RestSharp;

namespace ExportNuGetStatistics
{
    class Program
    {
        private static class FieldsNames
        {
            public const string Downloads = "downloads";
            public const string TotalDownloads = "total_downloads";
            public const string DownloadsPerDay = "downloads_per_day";
            public const string VersionDownloadsPerDay = "version_downloads_per_day";
            public const string Popularity = "popularity";
        }

        private static class TagsNames
        {
            public const string Version = "version";
        }

        private static class NuGetResources
        {
            public const string SearchQuery = "SearchQueryService";
            public const string PackageMetadata = "RegistrationsBaseUrl";
        }

        private sealed class NuGetIndexDto
        {
            public sealed class ResourceDto
            {
                [JsonProperty("@id")]
                public string Id { get; set; }

                [JsonProperty("@type")]
                public string Type { get; set; }
            }

            public ResourceDto[] Resources { get; set; }
        }

        private sealed class NuGetPackageSummaryDto
        {
            public sealed class DataDto
            {
                public sealed class VersionDto
                {
                    public string Version { get; set; }

                    public int Downloads { get; set; }
                }

                public int TotalDownloads { get; set; }

                public VersionDto[] Versions { get; set; }
            }

            public DataDto[] Data { get; set; }
        }

        private sealed class NuGetPackageMetadataDto
        {
            public sealed class PageDto
            {
                public sealed class VersionDto
                {
                    public sealed class CatalogEntryDto
                    {
                        public string Version { get; set; }

                        [JsonProperty("published")]
                        public string PublishedDate { get; set; }
                    }

                    public CatalogEntryDto CatalogEntry { get; set; }
                }

                [JsonProperty("items")]
                public VersionDto[] Versions { get; set; }
            }

            [JsonProperty("items")]
            public PageDto[] Pages { get; set; }
        }

        private sealed class NuGetPackageVersionStatistics
        {
            public NuGetPackageVersionStatistics(string version, int downloads, DateTime utcReleaseDate)
            {
                Version = version;
                Downloads = downloads;
                UtcReleaseDate = utcReleaseDate;
            }

            public string Version { get; }
            
            public int Downloads { get; }
            
            public DateTime UtcReleaseDate { get; }
        }

        private const string PackageId = "Melanchall.DryWetMidi";

        static void Main(string[] args)
        {
            var url = args[0];
            var token = args[1];
            var organization = args[2];
            var bucket = args[3];
            var measurement = args[4];

            //

            Console.WriteLine("Loading NuGet package info...");

            var nuGetIndex = GetNuGetIndex();
            var packageSummary = GetNuGetPackageSummary(nuGetIndex);
            var packageMetadata = GetNuGetPackageMetadata(nuGetIndex);

            var versions = packageSummary.Data.First().Versions;
            var versionsStatistics = new List<NuGetPackageVersionStatistics>(versions.Length);

            foreach (var versionNumber in versions.Select(v => v.Version))
            {
                var downloads = versions.First(v => v.Version == versionNumber).Downloads;

                var publishedDate = packageMetadata.Pages.First().Versions.First(v => v.CatalogEntry.Version == versionNumber).CatalogEntry.PublishedDate;
                var releaseDate = DateTime.Parse(publishedDate);

                versionsStatistics.Add(new NuGetPackageVersionStatistics(
                    versionNumber,
                    downloads,
                    releaseDate.ToUniversalTime()));
            }

            versionsStatistics.Sort((s1, s2) => s1.Version.CompareTo(s2.Version));

            //

            Console.WriteLine("Exporting NuGet package info to InfluxDB Cloud...");

            var client = InfluxDBClientFactory.Create(url, token.ToCharArray());
            var timestamp = DateTime.UtcNow;
            Console.WriteLine($"Current timestamp is {timestamp}");

            var totalDownloads = packageSummary.Data[0].TotalDownloads;
            var downloadsPerDay = totalDownloads / (timestamp - versionsStatistics[0].UtcReleaseDate).TotalDays;

            using (var writeApi = client.GetWriteApi())
            {
                for (var i = 0; i < versionsStatistics.Count; i++)
                {
                    var statistics = versionsStatistics[i];
                    Console.WriteLine($"Exporting info for version [{statistics.Version}] (released on [{statistics.UtcReleaseDate}])...");

                    var versionDownloads = statistics.Downloads;
                    Console.WriteLine($"    downloads = [{versionDownloads}]");

                    var versionDownloadsPerDay = versionDownloads / (timestamp - statistics.UtcReleaseDate).TotalDays;
                    Console.WriteLine($"    downloads/day = [{versionDownloadsPerDay}]");

                    var versionLifeTime = i < versionsStatistics.Count - 1
                        ? versionsStatistics[i + 1].UtcReleaseDate - statistics.UtcReleaseDate
                        : timestamp - statistics.UtcReleaseDate;
                    Console.WriteLine($"    lifetime = [{versionLifeTime}] [{versionLifeTime.TotalDays} days]");

                    var popularity = versionDownloads / versionLifeTime.TotalDays;
                    Console.WriteLine($"    popularity = [{popularity}]");

                    var point = PointData
                        .Measurement(measurement)
                        
                        .Tag(TagsNames.Version, statistics.Version)
                        
                        .Field(FieldsNames.Downloads, versionDownloads)
                        .Field(FieldsNames.VersionDownloadsPerDay, versionDownloadsPerDay)
                        .Field(FieldsNames.TotalDownloads, totalDownloads)
                        .Field(FieldsNames.DownloadsPerDay, downloadsPerDay)
                        .Field(FieldsNames.Popularity, popularity)

                        .Timestamp(timestamp, WritePrecision.Ns);

                    writeApi.WritePoint(bucket, organization, point);
                }
            }

            client.Dispose();
            Console.WriteLine("All done.");
        }

        private static NuGetIndexDto GetNuGetIndex()
        {
            return LoadDto<NuGetIndexDto>("https://api.nuget.org/v3/index.json");
        }

        private static NuGetPackageSummaryDto GetNuGetPackageSummary(NuGetIndexDto nuGetIndex)
        {
            var searchQueryService = nuGetIndex.Resources.First(r => r.Type == NuGetResources.SearchQuery);
            return LoadDto<NuGetPackageSummaryDto>(searchQueryService.Id, ("q", $"PackageId:{PackageId}"));
        }

        private static NuGetPackageMetadataDto GetNuGetPackageMetadata(NuGetIndexDto nuGetIndex)
        {
            var packageMetadataService = nuGetIndex.Resources.First(r => r.Type == NuGetResources.PackageMetadata);
            return LoadDto<NuGetPackageMetadataDto>($"{packageMetadataService.Id.Trim('/')}/{PackageId.ToLower()}/index.json");
        }

        private static TDto LoadDto<TDto>(string endpoint, params (string Name, string Value)[] queryParameters)
        {
            var client = new RestClient(endpoint);

            var request = new RestRequest(Method.GET);
            queryParameters.ToList().ForEach(p => request.AddQueryParameter(p.Name, p.Value));
            
            var response = client.Execute(request);
            var content = response.Content;
            
            return JsonConvert.DeserializeObject<TDto>(content);
        }
    }
}

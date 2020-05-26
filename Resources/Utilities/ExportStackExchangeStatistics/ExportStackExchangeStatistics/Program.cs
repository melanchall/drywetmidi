using System;
using System.Linq;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
using RestSharp;

namespace ExportStackExchangeStatistics
{
    class Program
    {
        private static class FieldsNames
        {
            public const string QuestionId = "question_id";
            public const string AnswerId = "answer_id";
            public const string Title = "title";
            public const string Dummy = "dummy";
            public const string UnixTime = "unix_time";
        }

        private static class TagsNames
        {
            public const string Site = "site";
            public const string Type = "type";
        }

        private static class Sites
        {
            public const string StackOverflow = "stackoverflow";
            public const string Music = "music";
        }

        private sealed class ItemsDto
        {
            public sealed class ItemDto
            {
                [JsonProperty("item_type")]
                public string ItemType { get; set; }

                public string Title { get; set; }

                [JsonProperty("creation_date")]
                public long CreationDate { get; set; }

                [JsonProperty("answer_id")]
                public long AnswerId { get; set; }

                [JsonProperty("question_id")]
                public long QuestionId { get; set; }

                public string Site { get; set; }
            }

            public ItemDto[] Items { get; set; }
        }

        private static readonly DateTime UnixEpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private const int DaysBack = 30;

        static void Main(string[] args)
        {
            var url = args[0];
            var writeToken = args[1];
            var readToken = args[2];
            var organization = args[3];
            var bucket = args[4];
            var measurement = args[5];

            var items = new[] { Sites.StackOverflow, Sites.Music }
                .SelectMany(site => GetItems(site).Items)
                .ToList();

            //

            Console.WriteLine($"Querying existing data for last {DaysBack} days...");

            var readClient = InfluxDBClientFactory.Create(url, readToken.ToCharArray());

            var queryApi = readClient.GetQueryApi();
            var flux = $@"from(bucket:""DryWetMIDI"")
                          |> range(start: -{DaysBack}d)
                          |> filter(fn: (r) => r[""_measurement""] == ""{measurement}"" and r[""_field""] == ""{FieldsNames.UnixTime}"")";
            var tables = queryApi.QueryAsync(flux, organization).Result;
            
            var records = tables.SelectMany(t => t.Records).ToArray();
            Console.WriteLine($"{records.Length} record(s) found.");

            foreach (var record in records)
            {
                var unixTime = Convert.ToInt64(record.GetValue());
                items.RemoveAll(i => i.CreationDate == unixTime);
            }

            readClient.Dispose();

            //

            if (!items.Any())
            {
                Console.WriteLine("No new items. All done.");
                return;
            }

            //

            Console.WriteLine("Exporting data to InfluxDB Cloud...");

            var writeClient = InfluxDBClientFactory.Create(url, writeToken.ToCharArray());

            using (var writeApi = writeClient.GetWriteApi())
            {
                foreach (var item in items)
                {
                    Console.WriteLine($"    {item.ItemType} '{item.Title}' from {item.Site}...");

                    var timestamp = GetDate(item.CreationDate);
                    var point = PointData
                        .Measurement(measurement)

                        .Tag(TagsNames.Site, item.Site)
                        .Tag(TagsNames.Type, item.ItemType)

                        .Field(FieldsNames.AnswerId, item.AnswerId)
                        .Field(FieldsNames.QuestionId, item.QuestionId)
                        .Field(FieldsNames.Dummy, 1)
                        .Field(FieldsNames.Title, item.Title)
                        .Field(FieldsNames.UnixTime, item.CreationDate)

                        .Timestamp(timestamp, WritePrecision.Ns);

                    writeApi.WritePoint(bucket, organization, point);
                }
            }

            writeClient.Dispose();
            Console.WriteLine("All done.");
        }

        private static ItemsDto GetItems(string site)
        {
            var items = LoadDto<ItemsDto>("https://api.stackexchange.com/2.2/search/excerpts",
                ("site", site),
                ("order", "desc"),
                ("sort", "creation"),
                ("q", "drywetmidi"),
                ("fromdate", GetUnixTimestamp(DateTime.UtcNow.AddDays(-DaysBack)).ToString()));

            foreach (var item in items.Items)
            {
                item.Site = site;
            }

            return items;
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

        private static DateTime GetDate(long unixTime)
        {
            return UnixEpochStart.AddSeconds(unixTime);
        }

        private static long GetUnixTimestamp(DateTime dateTime)
        {
            return (long)Math.Round((dateTime - UnixEpochStart).TotalSeconds);
        }
    }
}

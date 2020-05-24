using System;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Octokit;

namespace ExportGitHubStatistics
{
    class Program
    {
        private static class FieldsNames
        {
            public const string Stars = "stars";
            public const string Forks = "forks";
            public const string Watchers = "watchers";
            public const string StarsPerDay = "stars_per_day";
            public const string DaysPerStar = "days_per_star";
            public const string ForksPerDay = "forks_per_day";
            public const string DaysPerFork = "days_per_fork";
            public const string WatchersPerDay = "watchers_per_day";
            public const string DaysPerWatcher = "days_per_watcher";
            public const string RepoSize = "repo_size";
        }

        private const string RepoOwner = "melanchall";
        private const string RepoName = "drywetmidi";
        private const string AuthAppName = "ExportDwmRepoStat";

        static void Main(string[] args)
        {
            var url = args[0];
            var token = args[1];
            var organization = args[2];
            var bucket = args[3];
            var measurement = args[4];
            var gitHubPat = args[5];

            Console.WriteLine("Loading repo info...");

            var gitHubClient = new GitHubClient(new ProductHeaderValue(AuthAppName));
            gitHubClient.Credentials = new Credentials(gitHubPat);
            var repository = gitHubClient.Repository.Get(RepoOwner, RepoName).Result;

            var influxDbClient = InfluxDBClientFactory.Create(url, token.ToCharArray());
            var timestamp = DateTime.UtcNow;
            var lifeTime = timestamp - repository.CreatedAt.UtcDateTime;

            Console.WriteLine("Exporting repo info...");

            using (var writeApi = influxDbClient.GetWriteApi())
            {
                var point = PointData
                    .Measurement(measurement)

                    .Field(FieldsNames.Stars, repository.StargazersCount)
                    .Field(FieldsNames.Forks, repository.ForksCount)
                    .Field(FieldsNames.Watchers, repository.SubscribersCount)

                    .Field(FieldsNames.StarsPerDay, repository.StargazersCount / lifeTime.TotalDays)
                    .Field(FieldsNames.ForksPerDay, repository.ForksCount / lifeTime.TotalDays)
                    .Field(FieldsNames.WatchersPerDay, repository.SubscribersCount / lifeTime.TotalDays)

                    .Field(FieldsNames.DaysPerStar, lifeTime.TotalDays / repository.StargazersCount)
                    .Field(FieldsNames.DaysPerFork, lifeTime.TotalDays / repository.ForksCount)
                    .Field(FieldsNames.DaysPerWatcher, lifeTime.TotalDays / repository.SubscribersCount)

                    .Field(FieldsNames.RepoSize, repository.Size)

                    .Timestamp(timestamp, WritePrecision.Ns);

                writeApi.WritePoint(bucket, organization, point);
            }

            influxDbClient.Dispose();
            Console.WriteLine("All done.");
        }
    }
}

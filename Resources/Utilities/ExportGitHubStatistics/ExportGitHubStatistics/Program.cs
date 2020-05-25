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
            public const string StarsPerDay = "stars_per_day";
            public const string DaysPerStar = "days_per_star";
            public const string Forks = "forks";
            public const string ForksPerDay = "forks_per_day";
            public const string DaysPerFork = "days_per_fork";
            public const string Watchers = "watchers";
            public const string WatchersPerDay = "watchers_per_day";
            public const string DaysPerWatcher = "days_per_watcher";
            public const string Issues = "issues";
            public const string IssuesPerDay = "issues_per_day";
            public const string DaysPerIssue = "days_per_issue";
            public const string RepoSize = "repo_size";
            public const string RepoLifeDays = "repo_life_days";
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

            var issuesClient = gitHubClient.Issue;
            var allIssues = issuesClient.GetAllForRepository(RepoOwner, RepoName, new RepositoryIssueRequest { State = ItemStateFilter.All }).Result;

            var influxDbClient = InfluxDBClientFactory.Create(url, token.ToCharArray());
            var timestamp = DateTime.UtcNow;
            var lifeTime = timestamp - repository.CreatedAt.UtcDateTime;

            Console.WriteLine("Exporting repo info...");

            using (var writeApi = influxDbClient.GetWriteApi())
            {
                var point = PointData
                    .Measurement(measurement)

                    .Field(FieldsNames.Stars, repository.StargazersCount)
                    .Field(FieldsNames.DaysPerStar, lifeTime.TotalDays / repository.StargazersCount)
                    .Field(FieldsNames.StarsPerDay, repository.StargazersCount / lifeTime.TotalDays)

                    .Field(FieldsNames.Forks, repository.ForksCount)
                    .Field(FieldsNames.ForksPerDay, repository.ForksCount / lifeTime.TotalDays)
                    .Field(FieldsNames.DaysPerFork, lifeTime.TotalDays / repository.ForksCount)

                    .Field(FieldsNames.Watchers, repository.SubscribersCount)
                    .Field(FieldsNames.WatchersPerDay, repository.SubscribersCount / lifeTime.TotalDays)
                    .Field(FieldsNames.DaysPerWatcher, lifeTime.TotalDays / repository.SubscribersCount)

                    .Field(FieldsNames.Issues, allIssues.Count)
                    .Field(FieldsNames.IssuesPerDay, allIssues.Count / lifeTime.TotalDays)
                    .Field(FieldsNames.DaysPerIssue, lifeTime.TotalDays / allIssues.Count)

                    .Field(FieldsNames.RepoSize, repository.Size)
                    .Field(FieldsNames.RepoLifeDays, lifeTime.TotalDays)

                    .Timestamp(timestamp, WritePrecision.Ns);

                writeApi.WritePoint(bucket, organization, point);
            }

            influxDbClient.Dispose();
            Console.WriteLine("All done.");
        }
    }
}

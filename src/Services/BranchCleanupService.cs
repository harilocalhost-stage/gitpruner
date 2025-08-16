
using GitPruner.Models;

namespace GitPruner.Services
{
    public class BranchCleanupService : IBranchCleanupService
    {
        private readonly IGitHubService _gitHub;
        private readonly ConfigOptions _options;

        public BranchCleanupService(IGitHubService gitHub, ConfigOptions options)
        {
            _gitHub = gitHub;
            _options = options;
#pragma warning disable CS8604 // Possible null reference argument.
            _gitHub.SetRepo(options.Owner, options.Repo);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        public async Task ExecuteAsync()
        {
            var branches = await _gitHub.GetBranchesAsync();
            var now = DateTime.UtcNow;

            foreach (var branch in branches)
            {
                if (branch?.Name?.ToLower() == "main" || branch?.Name.ToLower() == "master")
                    continue;

                var prs = await _gitHub.GetOpenPRsAsync(branch?.Name);

                if (prs.Any())
                {
                    var pr = prs.First();
                    var idleDays = (now - pr.UpdatedAt.UtcDateTime).TotalDays;

                    if (idleDays > _options.PrReminderDays)
                    {
                        await _gitHub.PostPRCommentAsync(pr.Number,
                            $"⚠️ Reminder: This PR has been idle for {idleDays:F0} days. Please update or it may be closed.");
                    }
                    continue;
                }

                var commit = await _gitHub.GetLastCommitAsync(branch.Commit.Sha);
                var commitAge = (now - commit.Commit.Author.Date.UtcDateTime).TotalDays;

                if (commitAge > _options.BranchIdleDays)
                {
                    await _gitHub.DeleteBranchAsync(branch.Name);
                    Console.WriteLine($"🗑 Deleted branch {branch.Name}, idle for {commitAge:F0} days");
                }
            }
        }
    }
}

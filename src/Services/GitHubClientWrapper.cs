using Octokit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitPruner.Services
{
    public class GitHubClientWrapper : IGitHubClientWrapper
    {
        private readonly GitHubClient _client;

        public GitHubClientWrapper(string token)
        {
            _client = new GitHubClient(new ProductHeaderValue("GitPruner"))
            {
                Credentials = new Credentials(token)
            };
        }

        public async Task<IReadOnlyList<Branch>> GetBranchesAsync(string owner, string repo)
        {
            return await _client.Repository.Branch.GetAll(owner, repo);
        }

        public async Task<IReadOnlyList<PullRequest>> GetOpenPRsAsync(string owner, string repo, string branchName)
        {
            var prs = await _client.PullRequest.GetAllForRepository(owner, repo, new PullRequestRequest
            {
                State = ItemStateFilter.Open
            });
            return prs.Where(pr => pr.Head.Ref == branchName).ToList();
        }

        public async Task DeleteBranchAsync(string owner, string repo, string branchName)
        {
            await _client.Git.Reference.Delete(owner, repo, $"heads/{branchName}");
        }

        public async Task PostPRCommentAsync(string owner, string repo, int prNumber, string message)
        {
            await _client.Issue.Comment.Create(owner, repo, prNumber, message);
        }

        public async Task<GitHubCommit> GetLastCommitAsync(string owner, string repo, string sha)
        {
            return await _client.Repository.Commit.Get(owner, repo, sha);
        }
    }
}

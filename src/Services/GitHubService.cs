using Octokit;

namespace GitPruner.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client;
        private string? _owner;
        private string? _repo;

        // Allow injecting a mock client for testing
        public GitHubService(string token, GitHubClient? client = null)
        {
            if (client != null)
            {
                _client = client;
            }
            else
            {
                _client = new GitHubClient(new ProductHeaderValue("GitPruner"))
                {
                    Credentials = new Credentials(token)
                };
            }
        }

        public void SetRepo(string owner, string repo)
        {
            _owner = owner;
            _repo = repo;
        }

        public async Task<IReadOnlyList<Branch>> GetBranchesAsync()
        {
            return await _client.Repository.Branch.GetAll(_owner, _repo);
        }

        public async Task<IReadOnlyList<PullRequest>> GetOpenPRsAsync(string branchName)
        {
            var prs = await _client.PullRequest.GetAllForRepository(_owner, _repo, new PullRequestRequest
            {
                State = ItemStateFilter.Open
            });

            return prs.Where(pr => pr.Head.Ref == branchName).ToList();
        }

        public async Task DeleteBranchAsync(string branchName)
        {
            await _client.Git.Reference.Delete(_owner, _repo, $"heads/{branchName}");
        }

        public async Task PostPRCommentAsync(int prNumber, string message)
        {
            await _client.Issue.Comment.Create(_owner, _repo, prNumber, message);
        }

        public async Task<GitHubCommit> GetLastCommitAsync(string sha)
        {
            return await _client.Repository.Commit.Get(_owner, _repo, sha);
        }
    }
}

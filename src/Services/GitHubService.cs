using Octokit;

namespace GitPruner.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly IGitHubClientWrapper _clientWrapper;
        private string? _owner;
        private string? _repo;

        public GitHubService(IGitHubClientWrapper clientWrapper)
        {
            _clientWrapper = clientWrapper;
        }

        public void SetRepo(string owner, string repo)
        {
            _owner = owner;
            _repo = repo;
        }

        public async Task<IReadOnlyList<Branch>> GetBranchesAsync()
        {
            return await _clientWrapper.GetBranchesAsync(_owner, _repo);
        }

        public async Task<IReadOnlyList<PullRequest>> GetOpenPRsAsync(string branchName)
        {
            return await _clientWrapper.GetOpenPRsAsync(_owner, _repo, branchName);
        }

        public async Task DeleteBranchAsync(string branchName)
        {
            await _clientWrapper.DeleteBranchAsync(_owner, _repo, branchName);
        }

        public async Task PostPRCommentAsync(int prNumber, string message)
        {
            await _clientWrapper.PostPRCommentAsync(_owner, _repo, prNumber, message);
        }

        public async Task<GitHubCommit> GetLastCommitAsync(string sha)
        {
            return await _clientWrapper.GetLastCommitAsync(_owner, _repo, sha);
        }
    }
}

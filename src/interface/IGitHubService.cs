using Octokit;

public interface IGitHubService
{
    void SetRepo(string owner, string repo);
    Task<IReadOnlyList<Octokit.Branch>> GetBranchesAsync();
    Task<IReadOnlyList<Octokit.PullRequest>> GetOpenPRsAsync(string branchName);
    Task DeleteBranchAsync(string branchName);
    Task PostPRCommentAsync(int prNumber, string message);
    Task<GitHubCommit> GetLastCommitAsync(string sha);
}

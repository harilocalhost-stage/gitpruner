using Octokit;
// Wrapper interface for GitHubClient to allow mocking in tests
public interface IGitHubClientWrapper
{
    Task<IReadOnlyList<Branch>> GetBranchesAsync(string owner, string repo);
    Task<IReadOnlyList<PullRequest>> GetOpenPRsAsync(string owner, string repo, string branchName);
    Task DeleteBranchAsync(string owner, string repo, string branchName);
    Task PostPRCommentAsync(string owner, string repo, int prNumber, string message);
    Task<GitHubCommit> GetLastCommitAsync(string owner, string repo, string sha);
}

public interface IGitHubService
{
    void SetRepo(string owner, string repo);
    Task<IReadOnlyList<Branch>> GetBranchesAsync();
    Task<IReadOnlyList<PullRequest>> GetOpenPRsAsync(string branchName);
    Task DeleteBranchAsync(string branchName);
    Task PostPRCommentAsync(int prNumber, string message);
    Task<GitHubCommit> GetLastCommitAsync(string sha);
}

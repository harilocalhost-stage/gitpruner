using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Octokit;
using Xunit;
using GitPruner.Services;
using GitPruner.Services;

namespace GitPruner.Tests
{
    public class GitHubServiceTests
    {
        private const string Token = "dummy-token";
        private const string Owner = "owner";
        private const string Repo = "repo";

        [Fact]
        public async Task GetBranchesAsync_ReturnsBranches()
        {
            var mockWrapper = new Mock<IGitHubClientWrapper>();
            var branches = new List<Branch> { new Branch() };
            mockWrapper.Setup(w => w.GetBranchesAsync(Owner, Repo)).ReturnsAsync(branches);
            var service = new GitHubService(mockWrapper.Object);
            service.SetRepo(Owner, Repo);
            var result = await service.GetBranchesAsync();
            Assert.Single(result);
        }

        [Fact]
        public async Task GetOpenPRsAsync_FiltersByBranchName()
        {
            var mockWrapper = new Mock<IGitHubClientWrapper>();
            var prs = new List<PullRequest> {
                new Mock<PullRequest>().Object,
                new Mock<PullRequest>().Object
            };
            mockWrapper.Setup(w => w.GetOpenPRsAsync(Owner, Repo, "feature")).ReturnsAsync(prs.Take(1).ToList());
            var service = new GitHubService(mockWrapper.Object);
            service.SetRepo(Owner, Repo);
            var result = await service.GetOpenPRsAsync("feature");
            Assert.Single(result);
        }

        private PullRequest CreatePullRequestWithHead(string branchName)
        {
            // Use a minimal mock for PullRequest and GitReference
            var mockPr = new Mock<PullRequest>();
            var mockHead = new Mock<GitReference>();
            mockHead.Setup(h => h.Ref).Returns(branchName);
            mockPr.Setup(p => p.Head).Returns(mockHead.Object);
            return mockPr.Object;
        }

        [Fact]
        public async Task DeleteBranchAsync_DeletesBranch()
        {
            var mockWrapper = new Mock<IGitHubClientWrapper>();
            mockWrapper.Setup(w => w.DeleteBranchAsync(Owner, Repo, "feature")).Returns(Task.CompletedTask).Verifiable();
            var service = new GitHubService(mockWrapper.Object);
            service.SetRepo(Owner, Repo);
            await service.DeleteBranchAsync("feature");
            mockWrapper.Verify(w => w.DeleteBranchAsync(Owner, Repo, "feature"), Times.Once);
        }

        [Fact]
        public async Task PostPRCommentAsync_PostsComment()
        {
            var mockWrapper = new Mock<IGitHubClientWrapper>();
            mockWrapper.Setup(w => w.PostPRCommentAsync(Owner, Repo, 1, "Hello")).Returns(Task.CompletedTask).Verifiable();
            var service = new GitHubService(mockWrapper.Object);
            service.SetRepo(Owner, Repo);
            await service.PostPRCommentAsync(1, "Hello");
            mockWrapper.Verify(w => w.PostPRCommentAsync(Owner, Repo, 1, "Hello"), Times.Once);
        }

        [Fact]
        public async Task GetLastCommitAsync_ReturnsCommit()
        {
            var mockWrapper = new Mock<IGitHubClientWrapper>();
            var commit = new GitHubCommit();
            mockWrapper.Setup(w => w.GetLastCommitAsync(Owner, Repo, "sha123")).ReturnsAsync(commit);
            var service = new GitHubService(mockWrapper.Object);
            service.SetRepo(Owner, Repo);
            var result = await service.GetLastCommitAsync("sha123");
            Assert.Equal(commit, result);
        }
    }
}

using Xunit;
using Moq;
using GitPruner.Services;
using GitPruner.Models;
using Octokit;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;
using System.Threading.Tasks;

namespace GitPruner.Tests
{
    public class BranchCleanupServiceTests
    {
        private readonly Mock<IGitHubService> _mockGitHubService;
        private readonly BranchCleanupService _branchCleanupService;
        private readonly ConfigOptions _options;

        public BranchCleanupServiceTests()
        {
            _mockGitHubService = new Mock<IGitHubService>();
            _options = new ConfigOptions
            {
                Owner = "owner",
                Repo = "repo",
                PrReminderDays = 30,
                BranchIdleDays = 90,
                Token = "token"
            };

            _branchCleanupService = new BranchCleanupService(_mockGitHubService.Object, _options);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldSkipMainBranch()
        {
            // Arrange
            var branches = new List<Branch>
            {
                CreateBranch("main", "sha1"),
                CreateBranch("feature/test", "sha2")
            };

            _mockGitHubService.Setup(x => x.GetBranchesAsync()).ReturnsAsync(branches);
            _mockGitHubService.Setup(x => x.GetOpenPRsAsync(It.IsAny<string>())).ReturnsAsync(new List<PullRequest>());
            _mockGitHubService.Setup(x => x.GetLastCommitAsync(It.IsAny<string>())).ReturnsAsync(CreateGitHubCommitWithAuthorDate(DateTimeOffset.UtcNow.AddDays(-10)));

            // Act
            await _branchCleanupService.ExecuteAsync();

            // Assert
            _mockGitHubService.Verify(x => x.DeleteBranchAsync("main"), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldDeleteStaleBranch()
        {
            // Arrange
            var branches = new List<Branch>
            {
                CreateBranch("feature/stale", "sha1")
            };

            var commit = CreateGitHubCommitWithAuthorDate(DateTimeOffset.UtcNow.AddDays(-100));

            _mockGitHubService.Setup(x => x.GetBranchesAsync()).ReturnsAsync(branches);
            _mockGitHubService.Setup(x => x.GetOpenPRsAsync("feature/stale")).ReturnsAsync(new List<PullRequest>());
            _mockGitHubService.Setup(x => x.GetLastCommitAsync("sha1")).ReturnsAsync(commit);

            // Act
            await _branchCleanup_service_or_execute();

            // Assert
            _mockGitHubService.Verify(x => x.DeleteBranchAsync("feature/stale"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotDeleteActiveBranch()
        {
            // Arrange
            var branches = new List<Branch>
            {
                CreateBranch("feature/active", "sha1")
            };

            var commit = CreateGitHubCommitWithAuthorDate(DateTimeOffset.UtcNow.AddDays(-10));

            _mockGitHubService.Setup(x => x.GetBranchesAsync()).ReturnsAsync(branches);
            _mockGitHubService.Setup(x => x.GetOpenPRsAsync("feature/active")).ReturnsAsync(new List<PullRequest>());
            _mockGitHubService.Setup(x => x.GetLastCommitAsync("sha1")).ReturnsAsync(commit);

            // Act
            await _branchCleanupService.ExecuteAsync();

            // Assert
            _mockGitHubService.Verify(x => x.DeleteBranchAsync("feature/active"), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldPostReminderOnStalePR()
        {
            // Arrange
            var branchName = "feature/with-pr";
            var branches = new List<Branch> { CreateBranch(branchName, "sha1") };

            var pr = CreatePullRequestWithUpdatedAt(DateTimeOffset.UtcNow.AddDays(-35), number: 123);
            _mockGitHubService.Setup(x => x.GetBranchesAsync()).ReturnsAsync(branches);
            _mockGitHubService.Setup(x => x.GetOpenPRsAsync(branchName)).ReturnsAsync(new List<PullRequest> { pr });

            // Act
            await _branchCleanup_service_or_execute();

            // Assert
            _mockGitHubService.Verify(x => x.PostPRCommentAsync(123, It.Is<string>(s => s.Contains("35"))), Times.Once);
        }

        // --- helpers to create Octokit objects without invoking complex constructors ---
        private static T CreateUninitialized<T>() where T : class
        {
            return (T)FormatterServices.GetUninitializedObject(typeof(T));
        }

        private static void SetMember(object target, string name, object? value)
        {
            var t = target.GetType();
            var prop = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(target, value);
                return;
            }

            // try backing field
            var field = t.GetField("<" + name + ">k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            // fallback: any field with the name
            field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            throw new InvalidOperationException($"Cannot set member '{name}' on type {t.FullName}");
        }

        private static Branch CreateBranch(string name, string sha)
        {
            var branch = CreateUninitialized<Branch>();
            SetMember(branch, "Name", name);
            var gitRef = CreateUninitialized<GitReference>();
            var shaField = typeof(GitReference).GetField("<Sha>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (shaField != null)
                shaField.SetValue(gitRef, sha);
            SetMember(branch, "Commit", gitRef);
            return branch;
        }

        private static GitHubCommit CreateGitHubCommitWithAuthorDate(DateTimeOffset date)
        {
            var ghCommit = CreateUninitialized<GitHubCommit>();

            var commit = CreateUninitialized<Commit>();
            var author = CreateUninitialized<Committer>();
            SetMember(author, "Date", date);
            SetMember(commit, "Author", author);
            SetMember(ghCommit, "Commit", commit);
            return ghCommit;
        }

        private static PullRequest CreatePullRequestWithUpdatedAt(DateTimeOffset updatedAt, int number)
        {
            var pr = CreateUninitialized<PullRequest>();
            SetMember(pr, "UpdatedAt", updatedAt);
            SetMember(pr, "Number", number);
            return pr;
        }

        // compatibility helpers: some tests above accidentally referenced wrong method names; call ExecuteAsync
        private Task _branchCleanup_service_or_execute()
        {
            return _branchCleanupService.ExecuteAsync();
        }
    }
}
using GitPruner.Models;
using GitPruner.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var config = new ConfigurationBuilder()
.SetBasePath(AppContext.BaseDirectory)
.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
.Build();

var options = new ConfigOptions();
config.Bind(options);
if (string.IsNullOrEmpty(options.Owner) || string.IsNullOrEmpty(options.Repo) || string.IsNullOrEmpty(options.Token))
{
    Console.WriteLine("Missing required configuration in appsettings.json.");
    return;
}

var services = new ServiceCollection()
    .AddSingleton(options)
    .AddSingleton<IGitHubService>(provider => new GitHubService(provider.GetRequiredService<ConfigOptions>().Token))
    .AddSingleton<IBranchCleanupService, BranchCleanupService>()
    .BuildServiceProvider();

var cleanup = services.GetRequiredService<IBranchCleanupService>();
await cleanup.ExecuteAsync();

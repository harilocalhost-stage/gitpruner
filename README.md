# 🧹 Git Prune (C# / .NET)

A simple **.NET CLI tool** + **GitHub Action** that automatically keeps your repository clean by managing branches intelligently.

## ✨ Features
✅ Skip branches with **active PRs**  
✅ Send **reminders** for PRs idle too long (default: 30 days)  
✅ Delete branches with **no commits in last 90 days**  
✅ Works as a **CLI tool** or inside **GitHub Actions**  
✅ Fully configurable  

## ⚙️ Installation (Local Run)

1. Clone the repo:
   ```bash
   git clone https://github.com/harilocalhost-stage/gitprune.git
   cd gitprune
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run with parameters:
   ```bash
   dotnet run --project ./GitPruner/GitPruner.csproj -- \      -o <org-or-user> \      -r <repo-name> \      -t <github-token> \      --prReminderDays 30 \      --branchIdleDays 90
   ```

🔑 **Note:** You need a **GitHub Personal Access Token** with `repo` scope if running locally.  
Inside GitHub Actions, the built-in `${{ secrets.GITHUB_TOKEN }}` is used automatically.  

## 🚀 GitHub Action Setup

Copy `.github/workflows/branch-cleanup.yml` into your repo.  
GitHub Actions will run cleanup weekly or on manual trigger.

## 📊 Example Run Output

```
⚠️ Reminder sent for PR #42 (idle for 45 days)
🗑 Deleted branch feature/old-login, idle for 120 days
✅ Skipped branch feature/new-dashboard (active PR)
```

## 🤝 Contributing
PRs welcome! If you have ideas (like Slack/Teams notifications, custom rules, etc.), open an issue or PR.  

## 📜 License
MIT License – free to use and adapt.

## 📜 Disclaimer
This project is created in my personal capacity and is not related to my employer.
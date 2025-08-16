namespace GitPruner.Models
{

public class ConfigOptions
{
    public string? Owner { get; set; }
    public string? Repo { get; set; }
    public string? Token { get; set; }
    public int PrReminderDays { get; set; }
    public int BranchIdleDays { get; set; }
}

}

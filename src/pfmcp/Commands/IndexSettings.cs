namespace Pfmcp.Commands;

public class IndexSettings : CommandSettings
{
    [CommandOption("-i|--index <INDEX>")]
    [Description("Pagefind bundle path or URL. Repeatable. Use name=source to label an index.")]
    public string[] Indexes { get; init; } = [];

    public override ValidationResult Validate()
    {
        if ((Indexes is null || Indexes.Length == 0)
            && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PFMCP_INDEXES")))
        {
            return ValidationResult.Error("Specify --index or set PFMCP_INDEXES.");
        }

        return ValidationResult.Success();
    }
}

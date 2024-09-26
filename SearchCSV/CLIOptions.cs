using CommandLine;

namespace SearchCSV;

public record CLIOptions
{
    [Option('f', "file", Required = false, HelpText = "Input CSV file paths (comma separated).", Separator = ',')]
    public required IEnumerable<string> InputFiles { get; set; }

    [Option('q', "query", Required = false, HelpText = "Query to search logs.")]
    public required string Query { get; set; }

    [Option('o', "output", Required = false, HelpText = "Output JSON file path.")]
    public string? OutputFile { get; set; }

    [Option('s', "severity", Required = false, HelpText = "Set notifications by severity level")]
    public int? Severity { get; set; }

    [Option('d', "delete-duplicates", Required = false, HelpText = "Delete duplicate entries", Default = false)]
    public bool DeleteDuplicates { get; set; }

    [Option('w', "wizard", Required = false, HelpText = "Enable wizard mode.", Default = false)]
    public bool WizardMode { get; set; }

    [Option('p', "print", Required = false, HelpText = "Print results to the console.", Default = false)]
    public bool PrintResults { get; set; }

    [Option("save-db", Required = false, HelpText = "Saves data to DB", Default = false)]
    public bool SaveDb { get; set; }
}

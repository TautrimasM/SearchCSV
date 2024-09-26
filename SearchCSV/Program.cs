using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using SearchCSV;

// Parse the command-line arguments
var parseResults = Parser.Default.ParseArguments<CLIOptions>(args);

parseResults
    .WithParsed(opts => {
        if(args.Length == 0)
        {
            DisplayHelp(parseResults);
        }
        else
        {
            RunApp(opts);
        }       
    })
    .WithNotParsed(errs => DisplayHelp(parseResults));

static void RunApp(CLIOptions opts)
{
    List<Dictionary<string, string>> logs = [];
    string query = opts.Query;
    string outputFilePath = opts.OutputFile ?? string.Empty;

    if (opts.WizardMode)
    {
        Console.WriteLine("Enter paths to input CSV files (comma separated):");
        string inputFilePaths = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrEmpty(inputFilePaths))
        {
            Console.WriteLine("You must provide at least one file");
            return;
        }
        opts.InputFiles = inputFilePaths.Split(',').Select(f => f.Trim());

        Console.WriteLine("Enter your query:");
        query = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrEmpty(query))
        {
            Console.WriteLine("You must provide search query");
            return;
        }

        Console.WriteLine("Enter severity threshold (optional):");
        string? severityString = Console.ReadLine();
        if (string.IsNullOrEmpty(severityString))
        {
            opts.Severity = null;
        }
        else
        {
            opts.Severity = int.Parse(severityString);
        }

        Console.WriteLine("Enter path to output JSON file (optional):");
        outputFilePath = Console.ReadLine() ?? string.Empty;

        Console.WriteLine("Print all results? (y/N):");
        opts.PrintResults = Console.ReadLine() == "y";

        Console.WriteLine("Save to DB? (y/N)");
        opts.SaveDb = Console.ReadLine() == "y";

    }
    else
    {
        if (!opts.InputFiles.Any())
        {
            Console.WriteLine("You must provide at least one file");
            return;
        }
    }

    foreach (var file in opts.InputFiles)
    {
        if (File.Exists(file))
        {
            logs.AddRange(CSVSearcher.ReadCsvFile(file));
        }
        else
        {
            Console.WriteLine($"File '{file}' not found.");
            return;
        }
    }

    var result = CSVSearcher.ProcessQuery(logs, query);

    int resultCount = result.Count;
    Console.WriteLine($"Found {resultCount} matching logs.");

    if (resultCount > 0)
    {

        result = DuplicateHandler.HandleDuplicates(result, opts.DeleteDuplicates, opts.WizardMode);

        if (opts.Severity is not null)
        {
            Notifyer.NotifyBySeverity(result, (int)opts.Severity);
        }


        if (!string.IsNullOrEmpty(outputFilePath))
        {
            SaveResultsToFile(result, query, outputFilePath);
        }

        if (opts.PrintResults)
        {
            string jsonOutput = JsonConvert.SerializeObject(result, Formatting.Indented);
            Console.WriteLine("Results:");
            Console.WriteLine(jsonOutput);
        }

        if (opts.SaveDb)
        {
            Console.WriteLine("Saving to DB");
            DbHandler.InsertRecordsAsync(opts.InputFiles, query, result);
        }
    }
}
static void SaveResultsToFile(List<Dictionary<string, string>> results, string query, string filePath)
{
    var jsonObject = new
    {
        searchQuery = query,
        result = results
    };

    string jsonContent = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);

    File.WriteAllText(filePath, jsonContent);

    Console.WriteLine($"Results saved to {filePath}");
}

static void DisplayHelp(ParserResult<CLIOptions> result)
{
    var helpText = HelpText.AutoBuild(result, h =>
    {
        return HelpText.DefaultParsingErrorsHandler(result, h);
    }, e => e);

    Console.WriteLine(helpText);
}
using Newtonsoft.Json;

namespace SearchCSV;

public class Notifyer
{
    public static void NotifyBySeverity(List<Dictionary<string, string>> records, int severityThreshold)
    {
        var matches = new List<Dictionary<string, string>>();

        foreach (var record in records)
        {
            if (record.TryGetValue("severity", out string? severityValue) &&
                int.TryParse(severityValue, out int severity) &&
                severity >= severityThreshold)
            {
                matches.Add(record);
            }
        }

        var result = new
        {
            SeverityThreshold = severityThreshold,
            Matches = matches
        };

        if (matches.Count > 0)
        {
            string jsonResult = JsonConvert.SerializeObject(result, Formatting.Indented);

            Console.WriteLine("Severity Matches Found:");
            Console.WriteLine(jsonResult);
        }
        else
        {
            Console.WriteLine($"No Severity Matches Found For Threshold: {severityThreshold}");
        }

    }
}
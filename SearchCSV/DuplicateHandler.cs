using Newtonsoft.Json;

namespace SearchCSV;

public static class DuplicateHandler
{
    public static List<Dictionary<string, string>> HandleDuplicates(List<Dictionary<string, string>> logs, bool deleteDuplicates, bool wizardMode)
    {
        var duplicates = FindDuplicates(logs);
        int duplicateCount = duplicates.Count;

        if (duplicateCount > 0)
        {
            Console.WriteLine($"{duplicateCount} duplicates found.");

            if (wizardMode)
            {
                Console.Write("Do you want to delete the duplicates? (y/n): ");
                var input = Console.ReadLine();
                if (input?.ToLower() == "y")
                {
                    logs = RemoveDuplicates(logs);
                    Console.WriteLine("Duplicates removed.");
                }
            }
            else if (deleteDuplicates)
            {
                logs = RemoveDuplicates(logs);
                Console.WriteLine($"{duplicateCount} duplicates deleted.");
            }
        }
        else
        {
            Console.WriteLine("No duplicates found.");
        }
        return logs;
    }
    private static List<Dictionary<string, string>> FindDuplicates(List<Dictionary<string, string>> logs)
    {
        var duplicates = new List<Dictionary<string, string>>();
        var uniqueLogs = new HashSet<string>();

        foreach (var log in logs)
        {
            string logString = JsonConvert.SerializeObject(log);

            if (!uniqueLogs.Add(logString))
            {
                duplicates.Add(log);
            }
        }

        return duplicates;
    }

    private static List<Dictionary<string, string>> RemoveDuplicates(List<Dictionary<string, string>> logs)
    {
        var uniqueLogs = new HashSet<string>();
        var resultLogs = new List<Dictionary<string, string>>();

        foreach (var log in logs)
        {
            string logString = JsonConvert.SerializeObject(log);

            if (!uniqueLogs.Contains(logString))
            {
                resultLogs.Add(log);
                uniqueLogs.Add(logString);
            }
        }

        return resultLogs;
    }
}
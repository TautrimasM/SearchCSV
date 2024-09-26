using CsvHelper;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SearchCSV;

public static class CSVSearcher
{
    public static List<Dictionary<string, string>> ReadCsvFile(string filePath)
    {
        var records = new List<Dictionary<string, string>>();

        try
        {
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                BadDataFound = context =>
                {
                    Console.WriteLine($"Bad data found: {context.RawRecord}");
                }
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            csv.Read();
            csv.ReadHeader();
            var headers = csv.HeaderRecord;

            if (headers == null)
            {
                Console.WriteLine("Failed to read CSV file");
                return [];
            }

            while (csv.Read())
            {
                var record = new Dictionary<string, string>();
                foreach (var header in headers)
                {
                    var value = csv.GetField(header) ?? string.Empty;
                    record[header] = value;
                }
                records.Add(record);
            }
        }
        catch (BadDataException ex)
        {
            Console.WriteLine($"Error reading CSV file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
        }

        return records;
    }

    public static List<Dictionary<string, string>> ProcessQuery(List<Dictionary<string, string>> logs, string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            Console.WriteLine("Query is empty.");
            return [];
        }

        bool containsAnd = query.Contains("AND");
        bool containsOr = query.Contains("OR");

        string[] conditions = query.Split(separator, StringSplitOptions.RemoveEmptyEntries)
            .Select(cond => cond.Trim()).ToArray();

        bool isSimpleQuery = !containsAnd && !containsOr;

        var results = new List<Dictionary<string, string>>();

        var firstLog = logs.FirstOrDefault();
        if (firstLog != null)
        {
            var missingColumns = conditions
                .Select(cond => GetColumnFromCondition(cond))
                .Where(column => !firstLog.ContainsKey(column))
                .ToList();

            if (missingColumns.Count != 0)
            {
                Console.WriteLine($"Error: Column(s) not found: {string.Join(", ", missingColumns)}");
                return [];
            }
        }

        foreach (var log in logs)
        {
            bool matchesAll = true;
            bool matchesAny = false;

            foreach (string condition in conditions)
            {
                bool negate = false;
                string cond = condition;

                if (cond.StartsWith("NOT"))
                {
                    negate = true;
                    cond = cond[3..].Trim();
                }

                string[] queryParts = cond.Split('=');
                if (queryParts.Length != 2)
                {
                    Console.WriteLine($"Invalid query format: '{condition}'. Use 'column_name=search_string'.");
                    return [];
                }

                string columnName = queryParts[0].Trim();
                string searchValue = queryParts[1].Trim().Trim('\''); 

                bool isPartialSearch = searchValue.Contains('*');
                
                if (log.TryGetValue(columnName, out string? value))
                {
                    bool isMatch;
                    if (isPartialSearch)
                    {
                        searchValue = TranslateSearchValueToRegexPattern(searchValue);
                        isMatch = Regex.IsMatch(value, searchValue);
                    }
                    else
                    {
                        isMatch = value.Equals(searchValue);
                    }

                    isMatch = negate ? !isMatch : isMatch;

                    if (containsAnd)
                    {
                        matchesAll &= isMatch;
                    }
                    if (containsOr)
                    {
                        matchesAny |= isMatch;
                    }
                    if (isSimpleQuery && isMatch)
                    {
                        results.Add(log);
                        break; 
                    }
                }
            }

            if (containsAnd && matchesAll)
            {
                results.Add(log);
            }

            if (containsOr && matchesAny)
            {
                results.Add(log);
            }
        }

        return results;
    }

    private static string GetColumnFromCondition(string condition)
    {
        if (condition.StartsWith("NOT"))
        {
            condition = condition[3..].Trim();
        }

        string[] queryParts = condition.Split('=');
        if (queryParts.Length == 2)
        {
            return queryParts[0].Trim();
        }

        return string.Empty;
    }

    private static string TranslateSearchValueToRegexPattern(string pattern)
    {
        if (pattern.StartsWith("*") && pattern.EndsWith("*"))
        {
            return ".*" + Regex.Escape(pattern.Trim('*')) + ".*";
        }
        else if (pattern.StartsWith("*"))
        {
            return ".*" + Regex.Escape(pattern.Trim('*')) + "$";
        }
        else if (pattern.EndsWith("*"))
        {
            return "^" + Regex.Escape(pattern.Trim('*')) + ".*";
        }
        else
        {
            return Regex.Escape(pattern);
        }
    }

    private static readonly string[] separator = ["AND", "OR"];
}
using MongoDB.Bson;
using MongoDB.Driver;

namespace SearchCSV;

public class DbHandler
{
    public static void InsertRecordsAsync(IEnumerable<string> fileNames, string query, List<Dictionary<string, string>> result)
    {
        try
        {   
            //Hardcoded for testing purposes. in production app it should be taken from config
            string connectionString = "mongodb://admin:password@localhost:27017/DeviceLogsDB?authSource=admin";

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("DeviceLogsDB");

            try
            {
                using var ctsPing = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var pingCommand = new BsonDocument("ping", 1);
                database.RunCommandAsync<BsonDocument>(pingCommand, cancellationToken: ctsPing.Token).Wait();
                Console.WriteLine("Successfully connected to MongoDB!");
            }
            catch (AggregateException aggEx)
            {
                HandleAggregateException(aggEx);
            }

            var collection = database.GetCollection<SearchResultModel>("Logs");

            var searchResult = new SearchResultModel
            {
                FilesSearched = fileNames,
                SearchQuery = query,
                Results = result
            };

            try
            {
                using var ctsInsert = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                collection.InsertOneAsync(searchResult, cancellationToken: ctsInsert.Token).Wait();
                Console.WriteLine("Search result inserted into MongoDB!");
            }
            catch (AggregateException aggEx)
            {
                HandleAggregateException(aggEx);
            }

        }
        catch (MongoException mongoEx)
        {
            Console.WriteLine($"MongoDB Error: {mongoEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    public static void HandleAggregateException(AggregateException aggEx)
    {
        aggEx.Handle(ex =>
        {
            if (ex is TaskCanceledException)
            {
                throw new TimeoutException("Connection to MongoDB timed out.", ex);
            }
            return false;
        });
    }
}
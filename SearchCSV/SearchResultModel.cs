using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SearchCSV;

public record SearchResultModel
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("filesSearched")]
    public required IEnumerable<string> FilesSearched { get; set; }

    [BsonElement("searchQuery")]
    public required string SearchQuery { get; set; }

    [BsonElement("results")]
    public required List<Dictionary<string, string>> Results { get; set; }
}
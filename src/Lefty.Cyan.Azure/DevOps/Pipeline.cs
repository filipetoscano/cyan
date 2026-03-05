using System.Text.Json.Serialization;

namespace Lefty.Cyan.Azure.DevOps;

/// <summary />
public class Pipeline
{
    /// <summary />
    [JsonPropertyName( "id" )]
    public required int Id { get; set; }

    /// <summary />
    [JsonPropertyName( "name" )]
    public required string Name { get; set; }


    /// <summary />
    [JsonPropertyName( "process" )]
    public PipelineProcess? Process { get; set; }
}


/// <summary />
public class PipelineProcess
{
    /// <summary />
    [JsonPropertyName( "type" )]
    public required int Type { get; set; }

    /// <summary />
    [JsonPropertyName( "yamlFilename" )]
    public string? YamlFilename { get; set; }
}
using System.Text.Json.Serialization;

namespace BPMS.Modules.WorkflowRuntime.Models;

public class WorkflowDefinition
{
    public List<WorkflowNode> Nodes { get; set; } = [];
    public List<WorkflowConnection> Connections { get; set; } = [];
}

public class WorkflowNode
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public WorkflowNodeConfig? Config { get; set; }
}

public class WorkflowNodeConfig
{
    public string? TaskType { get; set; }
    public string? AssignTo { get; set; }
    public string? AssignToValue { get; set; }
    public string? FormId { get; set; }
    public string? Condition { get; set; }
    public string? SystemTaskType { get; set; }
    public string? To { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? Message { get; set; }
}

public class WorkflowConnection
{
    public string Id { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string? Label { get; set; }
}

[JsonSerializable(typeof(WorkflowDefinition))]
[JsonSerializable(typeof(List<WorkflowNode>))]
[JsonSerializable(typeof(List<WorkflowConnection>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class WorkflowDefinitionContext : JsonSerializerContext
{
}
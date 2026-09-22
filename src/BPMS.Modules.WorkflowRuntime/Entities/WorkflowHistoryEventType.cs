namespace BPMS.Modules.WorkflowRuntime.Entities;

public static class WorkflowHistoryEventType
{
    public const string Started = "Started";
    public const string NodeExecuted = "NodeExecuted";
    public const string TaskCreated = "TaskCreated";
    public const string TaskCompleted = "TaskCompleted";
    public const string WorkflowCompleted = "WorkflowCompleted";
    public const string WorkflowCancelled = "WorkflowCancelled";
}
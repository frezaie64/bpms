using System.Text.RegularExpressions;
using System.Text.Json;
using BPMS.Modules.Notifications;
using BPMS.Modules.Notifications.Models;
using BPMS.Modules.WorkflowDefinitions.Entities;
using BPMS.Modules.WorkflowRuntime.Entities;
using BPMS.Modules.WorkflowRuntime.Models;
using BPMS.Shared.Abstractions;
using BPMS.Shared.Persistence;
using BPMS.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BPMS.Modules.WorkflowRuntime.Services;

public partial class WorkflowEngine
{
    private readonly BpmsDbContext _db;
    private readonly ITenantService _tenant;
    private readonly ISystemTaskService _systemTask;
    private readonly INotificationsModule _notifications;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(
        BpmsDbContext db,
        ITenantService tenant,
        ISystemTaskService systemTask,
        INotificationsModule notifications,
        ILogger<WorkflowEngine> logger)
    {
        _db = db;
        _tenant = tenant;
        _systemTask = systemTask;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<WorkflowInstance> StartWorkflowAsync(Guid workflowId, string userId, CancellationToken ct = default)
    {
        var workflow = await _db.Set<Modules.WorkflowDefinitions.Entities.Workflow>()
            .FirstOrDefaultAsync(w => w.Id == workflowId && w.Status == Modules.WorkflowDefinitions.Entities.WorkflowStatus.Published)
            ?? throw new InvalidOperationException("Workflow not found or not published.");

        var isSubscribed = await _db.Set<WorkflowSubscription>()
            .AnyAsync(s => s.WorkflowId == workflowId && s.UserId == userId);

        if (!isSubscribed)
            throw new InvalidOperationException("You are not subscribed to this workflow.");

        var version = await _db.Set<Modules.WorkflowDefinitions.Entities.WorkflowVersion>()
            .Where(v => v.WorkflowId == workflowId && v.Status == Modules.WorkflowDefinitions.Entities.WorkflowStatus.Published)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No published version found for this workflow.");

        var definition = JsonSerializer.Deserialize<WorkflowDefinition>(version.JsonDefinition, WorkflowDefinitionContext.Default.WorkflowDefinition)
            ?? throw new InvalidOperationException("Invalid workflow definition.");

        var startNode = definition.Nodes.FirstOrDefault(n => n.Type == "start")
            ?? throw new InvalidOperationException("Workflow definition has no start node.");

        var variables = new Dictionary<string, string>
        {
            ["startedBy"] = userId
        };

        var instance = new WorkflowInstance
        {
            TenantId = _tenant.TenantId,
            WorkflowId = workflow.Id,
            WorkflowVersionId = version.Id,
            WorkflowName = workflow.Name,
            WorkflowVersion = version.VersionNumber,
            Status = WorkflowInstanceStatus.Running,
            StartedBy = userId,
            StartedDate = DateTime.UtcNow,
            ActiveNodeIds = JsonSerializer.Serialize(new[] { startNode.Id }),
            CompletedNodeIds = "[]",
            Variables = JsonSerializer.Serialize(variables),
            JsonDefinition = version.JsonDefinition
        };

        _db.Set<WorkflowInstance>().Add(instance);
        await AddHistoryAsync(instance, WorkflowHistoryEventType.Started, null, null, userId);
        await _db.SaveChangesAsync();

        await _notifications.CreateNotificationAsync(new CreateNotificationRequest(
            userId,
            "Workflow Started",
            $"Workflow '{workflow.Name}' has been started successfully.",
            "WorkflowStarted"
        ), ct);

        await _notifications.CreateAuditLogAsync(
            "WorkflowStarted",
            userId,
            null,
            "WorkflowInstance",
            instance.Id.ToString(),
            $"Workflow '{workflow.Name}' started"
        , ct);

        await AdvanceWorkflowAsync(instance.Id, ct);
        return instance;
    }

    public async Task AdvanceWorkflowAsync(Guid instanceId, CancellationToken ct = default)
    {
        var instance = await _db.Set<WorkflowInstance>()
            .Include(i => i.Tasks)
            .FirstOrDefaultAsync(i => i.Id == instanceId)
            ?? throw new KeyNotFoundException($"Workflow instance {instanceId} not found.");

        if (instance.Status != WorkflowInstanceStatus.Running)
            return;

        var definition = JsonSerializer.Deserialize<WorkflowDefinition>(instance.JsonDefinition, WorkflowDefinitionContext.Default.WorkflowDefinition);
        if (definition == null) return;

        var activeNodes = JsonSerializer.Deserialize<List<string>>(instance.ActiveNodeIds) ?? [];
        var completedNodes = JsonSerializer.Deserialize<List<string>>(instance.CompletedNodeIds) ?? [];
        var variables = JsonSerializer.Deserialize<Dictionary<string, string>>(instance.Variables) ?? [];

        bool progressed;
        do
        {
            progressed = false;
            var nodesToProcess = activeNodes.ToList();

            foreach (var nodeId in nodesToProcess)
            {
                var node = definition.Nodes.FirstOrDefault(n => n.Id == nodeId);
                if (node == null) continue;

                switch (node.Type)
                {
                    case "start":
                        activeNodes.Remove(nodeId);
                        completedNodes.Add(nodeId);
                        AddNextNodes(definition, nodeId, null, activeNodes);
                        await AddHistoryAsync(instance, WorkflowHistoryEventType.NodeExecuted, nodeId, node.Label, null);
                        progressed = true;
                        break;

                    case "userTask":
                        var existingTask = instance.Tasks.FirstOrDefault(t => t.NodeId == nodeId);
                        if (existingTask == null)
                        {
                            var task = new WorkflowTask
                            {
                                TenantId = instance.TenantId,
                                WorkflowInstanceId = instance.Id,
                                NodeId = nodeId,
                                NodeName = node.Label,
                                TaskType = node.Config?.TaskType == "fillForm" ? WorkflowTaskType.FillForm : WorkflowTaskType.Approval,
                                Title = node.Label,
                                AssignedToUserId = node.Config?.AssignTo == "user" ? ResolveTemplate(node.Config?.AssignToValue, variables) : null,
                                AssignedToRole = node.Config?.AssignTo == "role" ? node.Config?.AssignToValue : null,
                                Status = WorkflowTaskStatus.Pending
                            };
                            _db.Set<WorkflowTask>().Add(task);
                            await AddHistoryAsync(instance, WorkflowHistoryEventType.TaskCreated, nodeId, node.Label, null);

                            if (!string.IsNullOrEmpty(task.AssignedToUserId))
                            {
                                await _notifications.CreateNotificationAsync(new CreateNotificationRequest(
                                    task.AssignedToUserId,
                                    "New Task Assigned",
                                    $"A new task '{task.Title}' has been assigned to you in workflow '{instance.WorkflowName}'.",
                                    "TaskAssigned"
                                ), ct);
                            }

                            progressed = true;
                        }
                        break;

                    case "systemTask":
                        await ExecuteSystemTaskAsync(node, variables);
                        activeNodes.Remove(nodeId);
                        completedNodes.Add(nodeId);
                        AddNextNodes(definition, nodeId, null, activeNodes);
                        await AddHistoryAsync(instance, WorkflowHistoryEventType.NodeExecuted, nodeId, node.Label, null);
                        progressed = true;
                        break;

                    case "decisionGateway":
                        var conditionResult = EvaluateCondition(node.Config?.Condition, variables);
                        activeNodes.Remove(nodeId);
                        completedNodes.Add(nodeId);
                        var label = conditionResult ? "true" : "false";
                        AddNextNodes(definition, nodeId, label, activeNodes);
                        await AddHistoryAsync(instance, WorkflowHistoryEventType.NodeExecuted, nodeId, node.Label, $"{{\"result\":{conditionResult.ToString().ToLower()}}}");
                        progressed = true;
                        break;

                    case "parallelSplit":
                        activeNodes.Remove(nodeId);
                        completedNodes.Add(nodeId);
                        AddNextNodes(definition, nodeId, null, activeNodes);
                        await AddHistoryAsync(instance, WorkflowHistoryEventType.NodeExecuted, nodeId, node.Label, null);
                        progressed = true;
                        break;

                    case "parallelJoin":
                        var predecessors = GetPredecessorNodes(definition, nodeId);
                        if (predecessors.All(p => completedNodes.Contains(p)))
                        {
                            activeNodes.Remove(nodeId);
                            completedNodes.Add(nodeId);
                            AddNextNodes(definition, nodeId, null, activeNodes);
                            await AddHistoryAsync(instance, WorkflowHistoryEventType.NodeExecuted, nodeId, node.Label, null);
                            progressed = true;
                        }
                        break;

                    case "end":
                        activeNodes.Remove(nodeId);
                        completedNodes.Add(nodeId);
                        instance.Status = WorkflowInstanceStatus.Completed;
                        instance.CompletedDate = DateTime.UtcNow;
                        await AddHistoryAsync(instance, WorkflowHistoryEventType.WorkflowCompleted, nodeId, node.Label, null);

                        await _notifications.CreateNotificationAsync(new CreateNotificationRequest(
                            instance.StartedBy,
                            "Workflow Completed",
                            $"Workflow '{instance.WorkflowName}' has been completed successfully.",
                            "WorkflowCompleted"
                        ), ct);

                        await _notifications.CreateAuditLogAsync(
                            "WorkflowCompleted",
                            instance.StartedBy,
                            null,
                            "WorkflowInstance",
                            instance.Id.ToString(),
                            $"Workflow '{instance.WorkflowName}' completed"
                        , ct);

                        progressed = true;
                        break;
                }
            }
        } while (progressed);

        instance.ActiveNodeIds = JsonSerializer.Serialize(activeNodes);
        instance.CompletedNodeIds = JsonSerializer.Serialize(completedNodes);
        instance.Variables = JsonSerializer.Serialize(variables);

        await _db.SaveChangesAsync();
    }

    public async Task CompleteTaskAsync(Guid taskId, WorkflowTaskStatus newStatus, string? userId, string? comments, string? formData, CancellationToken ct = default)
    {
        var task = await _db.Set<WorkflowTask>()
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException($"Task {taskId} not found.");

        if (task.Status != WorkflowTaskStatus.Pending)
            throw new InvalidOperationException("Task is already completed.");

        task.Status = newStatus;
        task.CompletedBy = userId;
        task.CompletedDate = DateTime.UtcNow;
        task.Comments = comments;

        if (formData != null)
            task.FormData = formData;

        var instance = await _db.Set<WorkflowInstance>()
            .FirstOrDefaultAsync(i => i.Id == task.WorkflowInstanceId)
            ?? throw new KeyNotFoundException($"Workflow instance {task.WorkflowInstanceId} not found.");

        var variables = JsonSerializer.Deserialize<Dictionary<string, string>>(instance.Variables) ?? [];

        if (newStatus == WorkflowTaskStatus.Approved)
            variables["approved"] = "true";
        else if (newStatus == WorkflowTaskStatus.Rejected)
            variables["approved"] = "false";

        if (formData != null)
            variables["formData"] = formData;

        instance.Variables = JsonSerializer.Serialize(variables);

        await AddHistoryAsync(instance, WorkflowHistoryEventType.TaskCompleted, task.NodeId, task.NodeName, userId);

        await _db.SaveChangesAsync();

        var eventType = newStatus == WorkflowTaskStatus.Approved ? "TaskApproved" : newStatus == WorkflowTaskStatus.Rejected ? "TaskRejected" : "TaskSubmitted";

        await _notifications.CreateNotificationAsync(new CreateNotificationRequest(
            instance.StartedBy,
            eventType == "TaskApproved" ? "Task Approved" : eventType == "TaskRejected" ? "Task Rejected" : "Task Submitted",
            $"Task '{task.NodeName}' in workflow '{instance.WorkflowName}' has been {eventType.ToLower()}.",
            eventType
        ), ct);

        await _notifications.CreateAuditLogAsync(
            eventType,
            userId ?? "system",
            null,
            "WorkflowTask",
            task.Id.ToString(),
            $"Task '{task.NodeName}' in workflow '{instance.WorkflowName}' {eventType.ToLower()}"
        , ct);

        await AdvanceWorkflowAsync(instance.Id, ct);
    }

    public async Task CancelWorkflowAsync(Guid instanceId, string? reason)
    {
        var instance = await _db.Set<WorkflowInstance>()
            .Include(i => i.Tasks)
            .FirstOrDefaultAsync(i => i.Id == instanceId)
            ?? throw new KeyNotFoundException($"Workflow instance {instanceId} not found.");

        if (instance.Status != WorkflowInstanceStatus.Running)
            throw new InvalidOperationException("Only running workflows can be cancelled.");

        instance.Status = WorkflowInstanceStatus.Cancelled;
        instance.CompletedDate = DateTime.UtcNow;

        var pendingTasks = instance.Tasks.Where(t => t.Status == WorkflowTaskStatus.Pending).ToList();
        foreach (var task in pendingTasks)
        {
            task.Status = WorkflowTaskStatus.Rejected;
            task.CompletedDate = DateTime.UtcNow;
        }

        var data = reason != null ? $"{{\"reason\":\"{reason}\"}}" : null;
        await AddHistoryAsync(instance, WorkflowHistoryEventType.WorkflowCancelled, null, null, data);

        await _db.SaveChangesAsync();

        await _notifications.CreateNotificationAsync(new CreateNotificationRequest(
            instance.StartedBy,
            "Workflow Cancelled",
            reason != null
                ? $"Workflow '{instance.WorkflowName}' has been cancelled. Reason: {reason}"
                : $"Workflow '{instance.WorkflowName}' has been cancelled.",
            "WorkflowCancelled"
        ), CancellationToken.None);

        await _notifications.CreateAuditLogAsync(
            "WorkflowCancelled",
            instance.StartedBy,
            null,
            "WorkflowInstance",
            instance.Id.ToString(),
            reason != null ? $"Workflow '{instance.WorkflowName}' cancelled. Reason: {reason}" : $"Workflow '{instance.WorkflowName}' cancelled"
        , CancellationToken.None);
    }

    private async Task ExecuteSystemTaskAsync(WorkflowNode node, Dictionary<string, string> variables)
    {
        var config = node.Config;
        if (config == null) return;

        var to = ResolveTemplate(config.To, variables);
        var subject = ResolveTemplate(config.Subject, variables);
        var body = ResolveTemplate(config.Body, variables);
        var message = ResolveTemplate(config.Message, variables);

        switch (config.SystemTaskType)
        {
            case "sendEmail":
                await _systemTask.SendEmailAsync(to, subject, body);
                break;
            case "sendSms":
                await _systemTask.SendSmsAsync(to, message);
                break;
            default:
                _logger.LogWarning("Unknown system task type: {Type}", config.SystemTaskType);
                break;
        }
    }

    private static bool EvaluateCondition(string? condition, Dictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(condition)) return true;

        var match = ConditionRegex().Match(condition);
        if (!match.Success) return true;

        var varName = match.Groups[1].Value;
        var op = match.Groups[2].Value;
        var value = match.Groups[3].Value.Trim().Trim('"');

        if (!variables.TryGetValue(varName, out var varValue)) return false;

        return op switch
        {
            "==" => string.Equals(varValue, value, StringComparison.OrdinalIgnoreCase),
            "!=" => !string.Equals(varValue, value, StringComparison.OrdinalIgnoreCase),
            ">" => double.TryParse(varValue, out var v) && double.TryParse(value, out var c) && v > c,
            "<" => double.TryParse(varValue, out var v) && double.TryParse(value, out var c) && v < c,
            _ => false
        };
    }

    private static string ResolveTemplate(string? template, Dictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;
        return TemplateRegex().Replace(template, match =>
        {
            var name = match.Groups[1].Value;
            return variables.TryGetValue(name, out var value) ? value : match.Value;
        });
    }

    private static void AddNextNodes(WorkflowDefinition definition, string nodeId, string? conditionLabel, List<string> activeNodes)
    {
        var connections = definition.Connections.Where(c => c.Source == nodeId).ToList();
        if (conditionLabel != null)
            connections = connections.Where(c => c.Label == conditionLabel).ToList();

        foreach (var conn in connections)
        {
            if (!activeNodes.Contains(conn.Target))
                activeNodes.Add(conn.Target);
        }
    }

    private static List<string> GetPredecessorNodes(WorkflowDefinition definition, string nodeId)
    {
        return definition.Connections
            .Where(c => c.Target == nodeId)
            .Select(c => c.Source)
            .ToList();
    }

    private async Task AddHistoryAsync(WorkflowInstance instance, string eventType, string? nodeId, string? nodeName, string? performedBy)
    {
        var history = new WorkflowHistory
        {
            WorkflowInstanceId = instance.Id,
            EventType = eventType,
            NodeId = nodeId,
            NodeName = nodeName,
            Timestamp = DateTime.UtcNow,
            PerformedBy = performedBy
        };
        _db.Set<WorkflowHistory>().Add(history);
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}\s*(==|!=|>|<)\s*(.+)")]
    private static partial Regex ConditionRegex();

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex TemplateRegex();
}
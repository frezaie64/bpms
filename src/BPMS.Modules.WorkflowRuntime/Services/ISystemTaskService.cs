namespace BPMS.Modules.WorkflowRuntime.Services;

public interface ISystemTaskService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendSmsAsync(string to, string message);
}
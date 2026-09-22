using Microsoft.Extensions.Logging;

namespace BPMS.Modules.WorkflowRuntime.Services;

public class SystemTaskService : ISystemTaskService
{
    private readonly ILogger<SystemTaskService> _logger;

    public SystemTaskService(ILogger<SystemTaskService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("[SEND EMAIL] To: {To}, Subject: {Subject}, Body: {Body}", to, subject, body);
        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string to, string message)
    {
        _logger.LogInformation("[SEND SMS] To: {To}, Message: {Message}", to, message);
        return Task.CompletedTask;
    }
}
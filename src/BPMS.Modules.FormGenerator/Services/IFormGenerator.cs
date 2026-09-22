using BPMS.Modules.FormGenerator.Models;

namespace BPMS.Modules.FormGenerator.Services;

public interface IFormGenerator
{
    Task<GenerateFormResponse> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
}
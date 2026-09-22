using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace BPMS.Modules.FormGenerator.Services;

public sealed class AiGenerationState
{
    public string RawResponse { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public bool UsedFallback { get; set; }

    public void Reset()
    {
        RawResponse = string.Empty;
        Error = string.Empty;
        UsedFallback = false;
    }

    public void SetRawResponse(string rawResponse)
    {
        RawResponse = rawResponse;
        Error = string.Empty;
        UsedFallback = false;
    }

    public void SetFallback(string rawResponse, string error)
    {
        RawResponse = rawResponse;
        Error = error;
        UsedFallback = true;
    }
}
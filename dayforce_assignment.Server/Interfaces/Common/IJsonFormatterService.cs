using dayforce_assignment.Server.DTOs.Confluence;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Common
{
    public interface IJsonFormatterService
    {
        JsonElement FormatJson(string responseString);
    }
}

using System.ComponentModel;
using System.Net;
using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.WorkItems;

/// <summary>提供在 Azure DevOps 專案中建立新工作項目的 MCP 工具。</summary>
[McpServerToolType]
public static class CreateWorkItemTool
{
    /// <summary>在指定專案中建立新的工作項目。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_items_create"), 
        Description("Create a new work item. Supported types include: Bug, Task, User Story, Feature, Epic, Issue, and Test Case.")
    ]
    public static async Task<string> Execute(
        IAdoWorkItemsService workItemsService,
        AdoOptions adoOptions,
        [Description("Work item type (e.g., Bug, Task, User Story)")] string type,
        [Description("Work item title")] string title,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Work item description")] string? description = null,
        [Description("Assigned to user")] string? assignedTo = null,
        [Description("Area path")] string? areaPath = null,
        [Description("Iteration path")] string? iterationPath = null,
        [Description("Additional fields as a JSON object using field reference names as keys (e.g., {\"Microsoft.VSTS.Scheduling.Effort\": 8, \"Microsoft.VSTS.Common.Priority\": 2}). Use this to supply any fields required by your process template.")] string? additionalFields = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");

        Dictionary<string, object>? parsedFields = null;
        if (additionalFields != null)
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(additionalFields);
            parsedFields = raw?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        }

        try
        {
            var result = await workItemsService.CreateWorkItemAsync(resolvedProject, type, title, description, assignedTo, areaPath, iterationPath, parsedFields);
            return result.ToString();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            var validationMessage = TryExtractValidationErrors(ex.Message);
            if (validationMessage != null)
                return validationMessage;
            throw;
        }
    }

    private static string? TryExtractValidationErrors(string exceptionMessage)
    {
        const string marker = "Response: ";
        var idx = exceptionMessage.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0) return null;

        var json = exceptionMessage[(idx + marker.Length)..];
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("customProperties", out var customProps)) return null;
            if (!customProps.TryGetProperty("RuleValidationErrors", out var errors)) return null;
            if (errors.ValueKind != JsonValueKind.Array || errors.GetArrayLength() == 0) return null;

            var fields = new List<(string RefName, string Hint)>();
            foreach (var error in errors.EnumerateArray())
            {
                var refName = error.TryGetProperty("fieldReferenceName", out var r) ? r.GetString() : null;
                if (refName == null) continue;
                var hint = InferFieldHint(refName);
                fields.Add((refName, hint));
            }

            if (fields.Count == 0) return null;

            var examplePairs = string.Join(", ", fields.Select(f => $"\"{f.RefName}\": {f.Hint}"));
            var fieldList = string.Join("\n", fields.Select(f => $"  - \"{f.RefName}\" (expected: {f.Hint})"));

            return $$"""
                Work item creation failed because the following fields are required by this project's process template, but were not provided:

                {{fieldList}}

                ACTION REQUIRED: Ask the user to provide values for each field above, then call mcp_ado_work_items_create again with the 'additionalFields' parameter:

                  additionalFields: {{{examplePairs}}}

                Replace each placeholder with the actual value provided by the user.
                """;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string InferFieldHint(string fieldReferenceName)
    {
        // Use the last segment of the reference name to infer a human-readable hint
        var segment = fieldReferenceName.Split('.').LastOrDefault() ?? fieldReferenceName;
        return segment.ToLowerInvariant() switch
        {
            var s when s.Contains("effort") || s.Contains("storypoints") || s.Contains("points")
                || s.Contains("size") || s.Contains("estimate") || s.Contains("remainingwork")
                || s.Contains("completedwork") || s.Contains("originalestimate") => "<number (e.g. 8)>",
            var s when s.Contains("date") || s.Contains("finish") || s.Contains("start") => "<date (e.g. 2026-04-30)>",
            var s when s.Contains("priority") || s.Contains("severity") || s.Contains("triage")
                || s.Contains("stackrank") || s.Contains("rating") => "<number (e.g. 2)>",
            var s when s.Contains("state") || s.Contains("reason") || s.Contains("type") => "<string>",
            var s when s.Contains("bool") || s.Contains("flag") => "<true or false>",
            _ => "<value>"
        };
    }
}

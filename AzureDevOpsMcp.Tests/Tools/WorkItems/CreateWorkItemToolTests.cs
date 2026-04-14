using System.Net;
using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.WorkItems;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AzureDevOpsMcp.Tests.Tools.WorkItems;

public class CreateWorkItemToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat"
    };

    [Fact(DisplayName = "執行工具（指定專案）- 使用指定的專案名稱建立工作項目")]
    public async Task Execute_UsesProvidedProject()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":1,"fields":{"System.Title":"New Task"}}""").RootElement;
        service.CreateWorkItemAsync("MyProject", "Task", "New Task", null, null, null, null, null).Returns(json);

        // Act
        var result = await CreateWorkItemTool.Execute(service, _options, "Task", "New Task", "MyProject");

        // Assert
        Assert.Contains("New Task", result);
        await service.Received(1).CreateWorkItemAsync("MyProject", "Task", "New Task", null, null, null, null, null);
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat", Project = "DefaultProject" };
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":1,"fields":{}}""").RootElement;
        service.CreateWorkItemAsync("DefaultProject", "Bug", "Bug Title", null, null, null, null, null).Returns(json);

        // Act
        await CreateWorkItemTool.Execute(service, options, "Bug", "Bug Title");

        // Assert
        await service.Received(1).CreateWorkItemAsync("DefaultProject", "Bug", "Bug Title", null, null, null, null, null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateWorkItemTool.Execute(service, options, "Task", "Title"));
    }

    [Fact(DisplayName = "執行工具並傳入選填欄位 - 正確傳遞所有選填欄位至服務")]
    public async Task Execute_PassesOptionalFields()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":1,"fields":{}}""").RootElement;
        service.CreateWorkItemAsync("Proj", "Task", "Title", "Desc", "user@test.com", "Proj\\Area", "Proj\\Sprint1", null).Returns(json);

        // Act
        await CreateWorkItemTool.Execute(service, _options, "Task", "Title", "Proj", "Desc", "user@test.com", "Proj\\Area", "Proj\\Sprint1");

        // Assert
        await service.Received(1).CreateWorkItemAsync("Proj", "Task", "Title", "Desc", "user@test.com", "Proj\\Area", "Proj\\Sprint1", null);
    }

    [Fact(DisplayName = "執行工具並傳入 additionalFields JSON - 正確解析並傳遞額外欄位至服務")]
    public async Task Execute_ParsesAndPassesAdditionalFields()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":1,"fields":{}}""").RootElement;
        service.CreateWorkItemAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<Dictionary<string, object>?>()).Returns(json);

        const string additionalFields = """{"Microsoft.VSTS.Scheduling.Effort": 8, "Microsoft.VSTS.Common.Priority": 2}""";

        // Act
        await CreateWorkItemTool.Execute(service, _options, "Task", "Title", "Proj", additionalFields: additionalFields);

        // Assert
        await service.Received(1).CreateWorkItemAsync(
            "Proj", "Task", "Title", null, null, null, null,
            Arg.Is<Dictionary<string, object>?>(d =>
                d != null &&
                d.ContainsKey("Microsoft.VSTS.Scheduling.Effort") &&
                d.ContainsKey("Microsoft.VSTS.Common.Priority")));
    }

    [Fact(DisplayName = "執行工具並傳入無效 JSON additionalFields - 應拋出 JsonException")]
    public async Task Execute_ThrowsOnInvalidAdditionalFieldsJson()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat", Project = "Proj" };

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() =>
            CreateWorkItemTool.Execute(service, options, "Task", "Title", additionalFields: "not-valid-json"));
    }

    [Fact(DisplayName = "API 回傳 400 含 RuleValidationErrors - 應回傳必填欄位說明而非拋出例外")]
    public async Task Execute_Returns_RequiredFieldsMessage_On400WithValidationErrors()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var responseBody = """
            {
              "$id":"1",
              "customProperties":{
                "RuleValidationErrors":[
                  {
                    "fieldReferenceName":"Microsoft.VSTS.Scheduling.Effort",
                    "fieldStatusFlags":"required, invalidEmpty",
                    "errorMessage":"TF401320: Rule Error for field Effort. Error code: Required, InvalidEmpty.",
                    "fieldStatusCode":524289,
                    "ruleValidationErrors":null
                  }
                ]
              },
              "message":"TF401320: Rule Error for field Effort. Error code: Required, InvalidEmpty."
            }
            """;
        service.CreateWorkItemAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<Dictionary<string, object>?>())
            .ThrowsAsync(new HttpRequestException(
                $"Azure DevOps API request failed with status 400 (BadRequest). Response: {responseBody}",
                null,
                HttpStatusCode.BadRequest));

        // Act
        var result = await CreateWorkItemTool.Execute(service, _options, "Task", "Title", "Proj");

        // Assert
        Assert.Contains("Microsoft.VSTS.Scheduling.Effort", result);
        Assert.Contains("ACTION REQUIRED", result);
        Assert.Contains("additionalFields", result);
        Assert.Contains("<number", result); // inferred hint for Effort field
    }

    [Fact(DisplayName = "API 回傳 400 但無 RuleValidationErrors - 應重新拋出例外")]
    public async Task Execute_Rethrows_On400WithoutValidationErrors()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        service.CreateWorkItemAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<Dictionary<string, object>?>())
            .ThrowsAsync(new HttpRequestException(
                "Azure DevOps API request failed with status 400 (BadRequest). Response: {\"message\":\"bad input\"}",
                null,
                HttpStatusCode.BadRequest));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            CreateWorkItemTool.Execute(service, _options, "Task", "Title", "Proj"));
    }
}

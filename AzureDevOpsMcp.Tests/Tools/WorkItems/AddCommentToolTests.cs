using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.WorkItems;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.WorkItems;

public class AddCommentToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（新增評論）- 成功呼叫服務並回傳評論內容")]
    public async Task Execute_AddsComment()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":5,"text":"Work in progress","createdBy":{"displayName":"Alice"}}""").RootElement;
        service.AddCommentAsync("DefaultProject", 100, "Work in progress").Returns(json);

        // Act
        var result = await AddCommentTool.Execute(service, _options, 100, "Work in progress");

        // Assert
        Assert.Contains("Work in progress", result);
        await service.Received(1).AddCommentAsync("DefaultProject", 100, "Work in progress");
    }

    [Fact(DisplayName = "執行工具（驗證輸出 JSON）- 輸出應包含 id 欄位")]
    public async Task Execute_OutputContainsIdField()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":7,"text":"Done","workItemId":100}""").RootElement;
        service.AddCommentAsync("DefaultProject", 100, "Done").Returns(json);

        // Act
        var result = await AddCommentTool.Execute(service, _options, 100, "Done");

        // Assert
        var doc = JsonDocument.Parse(result);
        Assert.True(doc.RootElement.TryGetProperty("id", out _));
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":1}""").RootElement;
        service.AddCommentAsync("DefaultProject", 200, "Comment").Returns(json);

        // Act
        await AddCommentTool.Execute(service, _options, 200, "Comment");

        // Assert
        await service.Received(1).AddCommentAsync("DefaultProject", 200, "Comment");
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            AddCommentTool.Execute(service, options, 1, "text"));
    }
}

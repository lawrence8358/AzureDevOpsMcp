using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.WorkItems;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.WorkItems;

public class GetCommentsToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（取得評論列表）- 回傳包含評論內容的 JSON")]
    public async Task Execute_ReturnsComments()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"count":2,"value":[{"id":1,"text":"First comment"},{"id":2,"text":"Second comment"}]}""").RootElement;
        service.GetCommentsAsync("DefaultProject", 50, null).Returns(json);

        // Act
        var result = await GetCommentsTool.Execute(service, _options, 50);

        // Assert
        Assert.Contains("First comment", result);
        Assert.Contains("Second comment", result);
        await service.Received(1).GetCommentsAsync("DefaultProject", 50, null);
    }

    [Fact(DisplayName = "執行工具並傳入 top 參數 - 正確傳遞 top 至服務")]
    public async Task Execute_PassesTopParam()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"count":1,"value":[{"id":1,"text":"Comment"}]}""").RootElement;
        service.GetCommentsAsync("DefaultProject", 50, 5).Returns(json);

        // Act
        await GetCommentsTool.Execute(service, _options, 50, top: 5);

        // Assert
        await service.Received(1).GetCommentsAsync("DefaultProject", 50, 5);
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"count":0,"value":[]}""").RootElement;
        service.GetCommentsAsync("DefaultProject", 100, null).Returns(json);

        // Act
        await GetCommentsTool.Execute(service, _options, 100);

        // Assert
        await service.Received(1).GetCommentsAsync("DefaultProject", 100, null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            GetCommentsTool.Execute(service, options, 1));
    }
}

using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.WorkItems;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.WorkItems;

public class QueryByWiqlToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（指定專案）- 使用指定的專案名稱執行 WIQL 查詢")]
    public async Task Execute_UsesProvidedProject()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"workItems":[{"id":1}]}""").RootElement;
        service.QueryByWiqlAsync("ExplicitProject", "SELECT [System.Id] FROM workitems", null).Returns(json);

        // Act
        var result = await QueryByWiqlTool.Execute(service, _options, "SELECT [System.Id] FROM workitems", "ExplicitProject");

        // Assert
        await service.Received(1).QueryByWiqlAsync("ExplicitProject", "SELECT [System.Id] FROM workitems", null);
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案執行 WIQL 查詢")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"workItems":[]}""").RootElement;
        service.QueryByWiqlAsync("DefaultProject", "SELECT [System.Id] FROM workitems", 10).Returns(json);

        // Act
        await QueryByWiqlTool.Execute(service, _options, "SELECT [System.Id] FROM workitems", top: 10);

        // Assert
        await service.Received(1).QueryByWiqlAsync("DefaultProject", "SELECT [System.Id] FROM workitems", 10);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            QueryByWiqlTool.Execute(service, options, "SELECT [System.Id] FROM workitems"));
    }
}

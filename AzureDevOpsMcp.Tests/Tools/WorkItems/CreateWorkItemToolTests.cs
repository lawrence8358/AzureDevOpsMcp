using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.WorkItems;
using NSubstitute;

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
        service.CreateWorkItemAsync("MyProject", "Task", "New Task", null, null, null, null).Returns(json);

        // Act
        var result = await CreateWorkItemTool.Execute(service, _options, "Task", "New Task", "MyProject");

        // Assert
        Assert.Contains("New Task", result);
        await service.Received(1).CreateWorkItemAsync("MyProject", "Task", "New Task", null, null, null, null);
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat", Project = "DefaultProject" };
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":1,"fields":{}}""").RootElement;
        service.CreateWorkItemAsync("DefaultProject", "Bug", "Bug Title", null, null, null, null).Returns(json);

        // Act
        await CreateWorkItemTool.Execute(service, options, "Bug", "Bug Title");

        // Assert
        await service.Received(1).CreateWorkItemAsync("DefaultProject", "Bug", "Bug Title", null, null, null, null);
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
        service.CreateWorkItemAsync("Proj", "Task", "Title", "Desc", "user@test.com", "Proj\\Area", "Proj\\Sprint1").Returns(json);

        // Act
        await CreateWorkItemTool.Execute(service, _options, "Task", "Title", "Proj", "Desc", "user@test.com", "Proj\\Area", "Proj\\Sprint1");

        // Assert
        await service.Received(1).CreateWorkItemAsync("Proj", "Task", "Title", "Desc", "user@test.com", "Proj\\Area", "Proj\\Sprint1");
    }
}

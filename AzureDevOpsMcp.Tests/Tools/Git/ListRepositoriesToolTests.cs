using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Git;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.Git;

public class ListRepositoriesToolTests
{
    [Fact(DisplayName = "執行工具（指定專案）- 使用指定的專案名稱列出儲存庫")]
    public async Task Execute_UsesProvidedProject()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"value":[{"name":"Repo1"}]}""").RootElement;
        service.ListRepositoriesAsync("MyProject").Returns(json);
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act
        var result = await ListRepositoriesTool.Execute(service, options, "MyProject");

        // Assert
        Assert.Contains("Repo1", result);
        await service.Received(1).ListRepositoriesAsync("MyProject");
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"value":[]}""").RootElement;
        service.ListRepositoriesAsync("DefaultProject").Returns(json);
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat", Project = "DefaultProject" };

        // Act
        var result = await ListRepositoriesTool.Execute(service, options);

        // Assert
        await service.Received(1).ListRepositoriesAsync("DefaultProject");
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            ListRepositoriesTool.Execute(service, options));
    }
}

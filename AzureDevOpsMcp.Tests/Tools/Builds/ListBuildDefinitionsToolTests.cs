using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Builds;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.Builds;

public class ListBuildDefinitionsToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（指定專案）- 使用指定的專案名稱列出建置定義")]
    public async Task Execute_UsesProvidedProject()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"value":[{"id":1,"name":"CI"}]}""").RootElement;
        service.ListDefinitionsAsync("ExplicitProject", null, null).Returns(json);

        // Act
        var result = await ListBuildDefinitionsTool.Execute(service, _options, "ExplicitProject");

        // Assert
        Assert.Contains("CI", result);
        await service.Received(1).ListDefinitionsAsync("ExplicitProject", null, null);
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"value":[]}""").RootElement;
        service.ListDefinitionsAsync("DefaultProject", null, null).Returns(json);

        // Act
        await ListBuildDefinitionsTool.Execute(service, _options);

        // Assert
        await service.Received(1).ListDefinitionsAsync("DefaultProject", null, null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            ListBuildDefinitionsTool.Execute(service, options));
    }

    [Fact(DisplayName = "執行工具並傳入名稱與數量篩選 - 正確傳遞篩選參數至服務")]
    public async Task Execute_PassesNameAndTopFilters()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"value":[]}""").RootElement;
        service.ListDefinitionsAsync("DefaultProject", "CI-Pipeline", 5).Returns(json);

        // Act
        await ListBuildDefinitionsTool.Execute(service, _options, name: "CI-Pipeline", top: 5);

        // Assert
        await service.Received(1).ListDefinitionsAsync("DefaultProject", "CI-Pipeline", 5);
    }
}

# AzureDevOpsMcp NuGet 發布指南

## 概述

AzureDevOpsMcp 以 .NET 全域工具（Global Tool）形式發布至 NuGet.org，使用者可透過 .NET 10 SDK 的 `dnx` 命令直接執行，無需手動安裝或 clone 原始碼。

## 前置準備

### 1. 申請 NuGet.org API Key

1. 前往 [https://www.nuget.org/](https://www.nuget.org/) 並登入（或建立帳號）
2. 點擊右上角帳號名稱 → **API Keys**
3. 點選 **Create** 建立新 API Key：
   - **Key Name**：如 `AzureDevOpsMcp-CI`
   - **Package Owner**：選擇你的帳號
   - **Glob Pattern**：`AzureDevOpsMcp`（限制僅此套件可用）
   - **Expiration**：建議 365 天（定期輪換）
4. 複製產生的 Key，儲存於安全位置（此 Key 只顯示一次）

### 2. 確認 .csproj 設定正確

確認 `AzureDevOpsMcp/AzureDevOpsMcp.csproj` 中包含以下必要的 NuGet 中繼資料：

```xml
<PropertyGroup>
  <PackageId>AzureDevOpsMcp</PackageId>
  <Version>1.0.0</Version>                           <!-- 每次發布前更新 -->
  <Authors>Lawrence Shen</Authors>
  <Description>A .NET 10 MCP Server for Azure DevOps, supporting 29 tools across 5 domains with STDIO and HTTP transport modes.</Description>
  <PackageTags>mcp;azure-devops;ado;modelcontextprotocol;ai</PackageTags>
  <PackageReadmeFile>README.md</PackageReadmeFile>
  <IsPackable>true</IsPackable>
  <PackAsTool>true</PackAsTool>                      <!-- 必須：標記為全域工具 -->
  <ToolCommandName>azure-devops-mcp</ToolCommandName> <!-- dnx 執行的命令名稱 -->
</PropertyGroup>
```

> 目前 `.csproj` 已包含上述設定，`<ToolCommandName>` 為 `azure-devops-mcp`。

## 發布流程

### 步驟一：更新版本號

在 `AzureDevOpsMcp/AzureDevOpsMcp.csproj` 中更新 `<Version>` 欄位，遵循 [語意化版本](https://semver.org/) 規則：

| 變更類型 | 版本遞增規則 | 範例 |
|---|---|---|
| 修正 BUG、文件更新 | Patch（修補版本） | `1.0.0` → `1.0.1` |
| 新增功能、向下相容 | Minor（次要版本） | `1.0.0` → `1.1.0` |
| 破壞性變更 | Major（主要版本） | `1.0.0` → `2.0.0` |

### 步驟二：建置與封裝

```bash
# 切換至主要專案目錄
cd AzureDevOpsMcp

# Release 模式建置
dotnet build -c Release

# 執行測試確認無誤
cd ../AzureDevOpsMcp.Tests
dotnet test
cd ../AzureDevOpsMcp

# 封裝為 NuGet 套件（輸出至 bin/Release/）
dotnet pack -c Release
```

封裝後的 `.nupkg` 檔案將位於 `AzureDevOpsMcp/bin/Release/AzureDevOpsMcp.{版本}.nupkg`。

### 步驟三：本地測試封裝結果（建議）

在推送至 NuGet.org 前，建議先在本地測試：

```bash
# 安裝至本地全域工具路徑測試
dotnet tool install --global AzureDevOpsMcp \
  --add-source ./bin/Release/ \
  --version 1.0.0

# 測試執行（ToolCommandName 為 azure-devops-mcp）
azure-devops-mcp --help

# 測試完成後解除安裝
dotnet tool uninstall --global AzureDevOpsMcp
```

### 步驟四：推送至 NuGet.org

```bash
dotnet nuget push bin/Release/AzureDevOpsMcp.{版本}.nupkg \
  --api-key <your-nuget-api-key> \
  --source https://api.nuget.org/v3/index.json
```

> **安全提示**：請勿將 API Key 直接寫入腳本或提交至版本控制。建議使用環境變數：
> ```bash
> dotnet nuget push bin/Release/AzureDevOpsMcp.*.nupkg \
>   --api-key $NUGET_API_KEY \
>   --source https://api.nuget.org/v3/index.json
> ```

### 步驟五：確認發布結果

1. 前往 [https://www.nuget.org/packages/AzureDevOpsMcp](https://www.nuget.org/packages/AzureDevOpsMcp) 確認套件已上架
2. 套件通常在 15-30 分鐘後完成索引，才能透過 `dotnet tool install` 安裝

## 使用 GitHub Actions 自動發布（CI/CD）

建立 `.github/workflows/nuget-publish.yml`：

```yaml
name: Publish NuGet

on:
  release:
    types: [published]

env:
  DOTNET_VERSION: '10.0.x'

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Build
        run: dotnet build AzureDevOpsMcp.sln -c Release

      - name: Test
        run: dotnet test AzureDevOpsMcp.sln -c Release --no-build

      - name: Pack
        run: dotnet pack AzureDevOpsMcp -c Release --no-build -o ./artifacts

      - name: Push to NuGet.org
        run: |
          dotnet nuget push ./artifacts/*.nupkg \
            --api-key ${{ secrets.NUGET_API_KEY }} \
            --source https://api.nuget.org/v3/index.json \
            --skip-duplicate
```

> **注意**：需在 GitHub Repository → Settings → Secrets → Actions 中新增 `NUGET_API_KEY` 秘密。

## 版本管理建議

- 使用 Git Tag 標記每次發布版本（如 `v1.0.0`）
- 利用 GitHub Releases 觸發自動發布 CI/CD
- 維護 `CHANGELOG.md` 記錄每版異動

## 常見問題

### Q：推送後出現 `403 Forbidden`
**A**：API Key 可能過期或沒有對應套件的權限，請至 NuGet.org 重新建立 API Key。

### Q：推送後找不到套件
**A**：NuGet.org 需要 15-30 分鐘完成套件索引，請稍後再試。若超過 1 小時仍未出現，可至 NuGet.org 的 `Manage Packages` 頁面確認上傳狀態。

### Q：`azure-devops-mcp` 命令找不到
**A**：請確認已透過 `dotnet tool install --global AzureDevOpsMcp` 完成安裝，並確認 .NET 全域工具路徑已加入系統 `PATH`（通常為 `~/.dotnet/tools`）。安裝後重新開啟終端機即可使用 `azure-devops-mcp` 命令。

### Q：`dnx` 命令找不到
**A**：`dnx` 隨附於 .NET 10 SDK（Preview 6 起，正式版 10.0.x 均已內含）。請確認已安裝 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)，並重新開啟終端機。若仍無法使用，可改用等效命令 `dotnet tool exec AzureDevOpsMcp -y`。

### Q：Version 衝突（`409 Conflict`）
**A**：NuGet.org 不允許覆蓋已上傳的版本。請遞增 `<Version>` 後重新封裝並推送。

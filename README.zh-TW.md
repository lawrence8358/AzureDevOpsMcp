[English](https://github.com/lawrence8358/AzureDevOpsMcp/blob/main/README.md) | [繁體中文](https://github.com/lawrence8358/AzureDevOpsMcp/blob/main/README.zh-TW.md)

# Azure DevOps MCP Server

![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-green.svg)
[![NuGet](https://img.shields.io/nuget/v/AzureDevOpsMcp?logo=nuget&label=NuGet)](https://www.nuget.org/packages/AzureDevOpsMcp)
[![Build status](https://primeeagle.visualstudio.com/PrimeEagleX/_apis/build/status/GitHub/Nuget%20-%20AzureDevOpsMcp)](https://primeeagle.visualstudio.com/PrimeEagleX/_build/latest?definitionId=90)

基於 .NET 10 建置的 Azure DevOps MCP Server，提供 **29 個工具**，涵蓋專案管理、工作項目（讀取/寫入分離）、Git 操作、Pull Request 審查、CI/CD Pipeline 及 Sprint 規劃，可透過 STDIO 或 HTTP (SSE) 模式與 AI 助手（如 GitHub Copilot、Claude）整合使用。

## 前置需求

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Azure DevOps Services 或 Azure DevOps Server 2022
- 有效的個人存取權杖 (Personal Access Token, PAT)

## 安裝方式

### 方式一：NuGet 全域工具（推薦）

透過 .NET 全域工具安裝，無需 clone 原始碼，開箱即用。

```bash
dotnet tool install --global AzureDevOpsMcp
```

安裝後即可直接使用 `azure-devops-mcp` 命令。升級至新版本：

```bash
dotnet tool update --global AzureDevOpsMcp
```

解除安裝：

```bash
dotnet tool uninstall --global AzureDevOpsMcp
```

> **需求**：[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

---

### 方式二：不安裝直接執行（dnx）

.NET 10 SDK 內建的 `dnx` 命令（類似 Node.js 的 `npx`），無需全域安裝，從 NuGet 下載後即時執行，適合臨時使用或 CI 環境。

```bash
dnx AzureDevOpsMcp --yes
```

指定版本：

```bash
dnx AzureDevOpsMcp@1.0.1 --yes
```

在 MCP 設定中使用：

```json
{
  "mcpServers": {
    "azure-devops": {
      "command": "dnx",
      "args": ["AzureDevOpsMcp", "--yes", "--"],
      "env": {
        "ADO_ORG": "https://dev.azure.com/your-org",
        "ADO_PAT": "your-personal-access-token",
        "ADO_PROJECT": "your-default-project"
      }
    }
  }
}
```

> `dnx` 隨附於 .NET 10 SDK（Preview 6 起），`--yes` 參數用於自動接受確認提示，無需手動互動。若未安裝 .NET 10 SDK，可改用 `dotnet tool exec AzureDevOpsMcp -y` 替代。

---

### 方式三：從原始碼執行

```bash
git clone <repo-url>
cd AzureDevOpsMcp
dotnet run
```

---

## 設定方式

### 環境變數

| 變數 | 必填 | 說明 |
|---|---|---|
| `ADO_ORG` | 是 | Azure DevOps 組織 URL，例如 `https://dev.azure.com/your-org` 或地端 `https://tfs.company.com/DefaultCollection` |
| `ADO_PAT` | 是 | 個人存取權杖 (Personal Access Token) |
| `ADO_PROJECT` | 否 | 預設專案名稱。設定後，所有工具在未指定 `project` 參數時會自動套用此專案 |

#### 產生個人存取權杖 (PAT)

1. 前往 `https://dev.azure.com/{your-org}` → 右上角個人資料圖示 → **Personal Access Tokens**
2. 點選 **New Token**，設定以下必要權限：
   - **Code**：Read & Write（Git 操作與 PR 審查）
   - **Work Items**：Read & Write（工作項目增刪查改）
   - **Project and Team**：Read（列出專案、Sprint）
   - **Build**：Read & Execute（觸發與查詢 Pipeline）
3. 複製產生的 Token，設定至環境變數 `ADO_PAT`

> **⚠️ 安全提示**：請勿將 PAT 明文寫入設定檔並提交至版本控制。建議使用作業系統層級的環境變數或密鑰管理工具管理 Token，並定期輪換。

---

### mcp.json 設定

以下為完整設定範例（載入全部 29 個工具）。

#### Claude Desktop（`claude_desktop_config.json`）

**NuGet 全域工具安裝後（推薦）：**

```json
{
  "mcpServers": {
    "azure-devops": {
      "command": "azure-devops-mcp",
      "args": [],
      "env": {
        "ADO_ORG": "https://dev.azure.com/your-org",
        "ADO_PAT": "your-personal-access-token",
        "ADO_PROJECT": "your-default-project"
      }
    }
  }
}
```

**從原始碼執行：**

```json
{
  "mcpServers": {
    "azure-devops": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "path/to/AzureDevOpsMcp/AzureDevOpsMcp.csproj",
        "--"
      ],
      "env": {
        "ADO_ORG": "https://dev.azure.com/your-org",
        "ADO_PAT": "your-personal-access-token",
        "ADO_PROJECT": "your-default-project"
      }
    }
  }
}
```

#### VS Code / GitHub Copilot（`.vscode/mcp.json`）

**NuGet 全域工具安裝後（推薦）：**

```json
{
  "servers": {
    "azure-devops": {
      "type": "stdio",
      "command": "azure-devops-mcp",
      "args": [],
      "env": {
        "ADO_ORG": "https://dev.azure.com/your-org",
        "ADO_PAT": "your-personal-access-token",
        "ADO_PROJECT": "your-default-project"
      }
    }
  }
}
```

**從原始碼執行：**

```json
{
  "servers": {
    "azure-devops": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "path/to/AzureDevOpsMcp/AzureDevOpsMcp.csproj",
        "--"
      ],
      "env": {
        "ADO_ORG": "https://dev.azure.com/your-org",
        "ADO_PAT": "your-personal-access-token",
        "ADO_PROJECT": "your-default-project"
      }
    }
  }
}
```

#### 選擇性載入工具 Domain（`-d` 參數）

透過 `-d`（`--domains`）參數可只載入需要的工具組，減少工具數量、加速啟動、降低雜訊。

| Domain 名稱 | 說明 | 工具數 |
|---|---|---|
| `core` | 組織與專案管理 | 1 |
| `work-items` | 工作項目**唯讀**操作（查詢、取得、搜尋） | 5 |
| `work-items-write` | 工作項目**寫入**操作（建立、更新、刪除、留言、連結） | 5 |
| `git` | Git 儲存庫操作（瀏覽 repo、branch、file、commit） | 4 |
| `pull-requests` | Pull Request 審查流程（建立、審查、留言） | 6 |
| `builds` | CI/CD Pipeline 執行與記錄 | 5 |
| `work` | Sprint / Backlog 管理 | 3 |

不指定 `-d` 時，預設載入全部 7 個 Domain 的 29 個工具。

> **為什麼 work-items 要分兩個 Domain？**  
> 唯讀工具（`work-items`）適合給**只需查詢**的情境；寫入工具（`work-items-write`）僅在**需要修改**時才載入，有助於縮小 LLM 可呼叫的工具範圍，提升精確度並降低誤操作風險。

**選擇性載入範例**（僅載入 `git`、`pull-requests`、`work-items`）：

```json
{
  "mcpServers": {
    "azure-devops": {
      "command": "azure-devops-mcp",
      "args": [
        "-d", "git",
        "-d", "pull-requests",
        "-d", "work-items",
        "-d", "work-items-write"
      ],
      "env": {
        "ADO_ORG": "https://dev.azure.com/your-org",
        "ADO_PAT": "your-personal-access-token",
        "ADO_PROJECT": "your-default-project"
      }
    }
  }
}
```

---

## 可用工具（29 個）

### Core — 專案管理（1 個）

| 工具名稱 | 說明 | 必填參數 | 選填參數 |
|---|---|---|---|
| `mcp_ado_core_list_projects` | 列出 Azure DevOps 組織中的所有專案 | 無 | `stateFilter`（wellFormed / all 等）、`top`、`skip` |

#### 使用範例

> 「請列出我的 Azure DevOps 組織中所有的專案」

```json
{
  "count": 28,
  "value": [
    {
      "id": "f2d77642-25ea-4ce3-867f-e7435e88382d",
      "name": "PrimeEagleX",
      "state": "wellFormed",
      "visibility": "private"
    }
  ]
}
```

---

### WorkItems — 工作項目唯讀 Domain：`work-items`（5 個）

| 工具名稱 | 說明 | 必填參數 | 選填參數 |
|---|---|---|---|
| `mcp_ado_work_items_get` | 依 ID 查詢單一工作項目 | `id` | `expand`（all / relations / fields）|
| `mcp_ado_work_items_batch_get` | 批次查詢多個工作項目（比逐一查詢更有效率） | `ids`（ID 陣列）| `expand` |
| `mcp_ado_work_items_get_comments` | 取得工作項目的所有留言 | `workItemId` | `project`、`top` |
| `mcp_ado_work_items_get_updates` | 取得工作項目的變更歷程（誰、何時、改了什麼） | `id` | `top` |
| `mcp_ado_work_items_query_by_wiql` | 使用 WIQL（類 SQL 語法）搜尋工作項目 | `query` | `project`、`top` |

#### 使用範例

**取得單一工作項目**

> 「請幫我查詢工作項目 471 的詳細資訊」

```json
{
  "id": 471,
  "rev": 7,
  "fields": {
    "System.WorkItemType": "Bug",
    "System.Title": "[School] robots 防機器人設定，導致 SEO 異常",
    "System.State": "Done",
    "System.AssignedTo": { "displayName": "Lawrence" },
    "Microsoft.VSTS.Common.Severity": "3 - Medium"
  }
}
```

**批次查詢多個工作項目**

> 「請一次取得工作項目 6 和 33 的資訊」

```json
{
  "count": 2,
  "value": [
    { "id": 6, "fields": { "System.Title": "PayGoGo 行動考勤", "System.State": "Active" } },
    { "id": 33, "fields": { "System.Title": "APP 新增最新消息推撥功能", "System.State": "Active" } }
  ]
}
```

**取得工作項目留言**

> 「顯示工作項目 471 的所有留言」

```json
{
  "totalCount": 1,
  "comments": [
    {
      "id": 5879892,
      "text": "Completing Pull Request 58 and the associated work items.",
      "createdBy": { "displayName": "Lawrence" },
      "createdDate": "2026-04-13T15:25:21Z"
    }
  ]
}
```

**取得工作項目變更歷程**

> 「工作項目 471 最近做了哪些修改？」

```json
{
  "count": 7,
  "value": [
    {
      "id": 7,
      "revisedBy": { "displayName": "Lawrence" },
      "revisedDate": "2026-04-13T15:25:21Z",
      "fields": {
        "System.State": { "oldValue": "Active", "newValue": "Done" }
      }
    }
  ]
}
```

**WIQL 查詢**

> 「用 WIQL 查詢最新的一筆工作項目」

```json
{
  "queryType": "flat",
  "workItems": [
    { "id": 471, "url": "https://primeeagle.visualstudio.com/.../_apis/wit/workItems/471" }
  ]
}
```

### WorkItems — 工作項目寫入 Domain：`work-items-write`（5 個）

| 工具名稱 | 說明 | 必填參數 | 選填參數 |
|---|---|---|---|
| `mcp_ado_work_items_create` | 建立新工作項目（類型如 Bug、Task、User Story、Feature、Epic） | `type`、`title` | `project`、`description`、`assignedTo`、`areaPath`、`iterationPath` |
| `mcp_ado_work_items_update` | 更新工作項目欄位（以 key-value 字典傳入，支援 System 欄位名稱） | `id`、`fields` | 無 |
| `mcp_ado_work_items_delete` | 刪除工作項目（預設移至回收桶；`destroy=true` 為永久刪除，不可恢復） | `id` | `destroy`（預設 false）|
| `mcp_ado_work_items_add_comment` | 新增留言至工作項目 | `workItemId`、`text` | `project` |
| `mcp_ado_work_items_add_link` | 建立兩個工作項目之間的關聯（常用類型：`Related`、`System.LinkTypes.Hierarchy-Forward`（child）、`System.LinkTypes.Hierarchy-Reverse`（parent）） | `id`、`targetId`、`linkType` | `comment` |

#### 使用範例

**建立工作項目（mcp_ado_work_items_create）**

> 「幫我建立一個 Bug，標題為「修復登入頁面按鈕點擊無反應」，指派給 Lawrence」

> ⚠️ 此操作會在 Azure DevOps 中建立真實資料。

```json
{
  "id": 500,
  "rev": 1,
  "fields": {
    "System.Title": "修復登入頁面按鈕點擊無反應",
    "System.WorkItemType": "Bug",
    "System.State": "Active",
    "System.AssignedTo": { "displayName": "Lawrence" }
  }
}
```

**更新工作項目（mcp_ado_work_items_update）**

> 「把工作項目 471 的狀態改為 Done」

> ⚠️ 此操作會修改 Azure DevOps 中的資料。

**新增留言（mcp_ado_work_items_add_comment）**

> 「對工作項目 471 加一條留言：這個 Bug 已由 PR #58 修復」

> ⚠️ 此操作會新增留言至 Azure DevOps，無法撤回。

**刪除工作項目（mcp_ado_work_items_delete）**

> ⚠️ 請謹慎使用！軟刪除可恢復，永久刪除不可還原。

呼叫範例：「將工作項目 #500 移至回收桶」（`destroy = false`，可從回收桶還原）

```json
{
  "id": 500,
  "project": { "name": "PrimeEagleX" },
  "destroyedDate": null,
  "url": "https://..."
}
```

呼叫範例：「永久刪除工作項目 #500」（`destroy = true`，無法恢復）

```json
{
  "deleted": true,
  "id": 500,
  "permanent": true
}
```

**建立工作項目關聯（mcp_ado_work_items_add_link）**

> 「將工作項目 #501 設定為工作項目 #500 的子項目」  
> （`linkType = "System.LinkTypes.Hierarchy-Forward"`）

回應為更新後的工作項目，`relations` 欄位中會包含新增的連結資訊，例如：

```json
{
  "id": 500,
  "rev": 2,
  "relations": [
    {
      "rel": "System.LinkTypes.Hierarchy-Forward",
      "url": "https://.../workItems/501",
      "attributes": { "isLocked": false, "name": "Child" }
    }
  ]
}
```

---

### Git — 儲存庫操作（4 個）

| 工具名稱 | 說明 | 必填參數 | 選填參數 |
|---|---|---|---|
| `mcp_ado_git_list_repositories` | 列出專案中所有 Git 儲存庫 | 無 | `project` |
| `mcp_ado_git_list_branches` | 列出儲存庫的所有分支 | `repositoryId` | `project`、`filter`（名稱前綴過濾）|
| `mcp_ado_git_get_item` | 讀取 Git 儲存庫中的檔案內容或目錄列表（可用於讀取原始碼、設定檔） | `repositoryId`、`path` | `project`、`branch` |
| `mcp_ado_git_get_commits` | 取得 Git commit 歷程，可依分支、檔案路徑、作者篩選 | `repositoryId` | `project`、`branch`、`itemPath`、`author`、`top` |

#### 使用範例

**列出儲存庫**

> 「列出所有的 Git 儲存庫」

```json
{
  "count": 12,
  "value": [
    {
      "id": "94408af5-6c38-45d2-a5d3-cbcfd38b8ae7",
      "name": "AiCodeReivew",
      "defaultBranch": "refs/heads/main",
      "remoteUrl": "https://primeeagle.visualstudio.com/DefaultCollection/PrimeEagleX/_git/AiCodeReivew"
    }
  ]
}
```

**列出分支**

> 「AiCodeReivew 儲存庫有哪些分支？」

```json
{
  "count": 3,
  "value": [
    { "name": "refs/heads/main" },
    { "name": "refs/heads/newfeature" },
    { "name": "refs/heads/test/20260416" }
  ]
}
```

**瀏覽目錄內容**

> 「列出 AiCodeReivew 的根目錄有哪些檔案」

```json
{
  "count": 5,
  "value": [
    { "path": "/Program.cs", "isFolder": false },
    { "path": "/README.md", "isFolder": false },
    { "path": "/index.html", "isFolder": false }
  ]
}
```

**查看 Commit 歷程**

> 「顯示 AiCodeReivew 最近 3 筆 commit 紀錄」

```json
{
  "count": 3,
  "value": [
    {
      "commitId": "8f73ed83...",
      "author": { "name": "Lawrence", "date": "2026-04-13T04:43:06Z" },
      "comment": "test: add PR marker",
      "changeCounts": { "Add": 0, "Edit": 1, "Delete": 0 }
    }
  ]
}
```

---

### Pull Requests — 程式碼審查（6 個）

| 工具名稱 | 說明 | 必填參數 | 選填參數 |
|---|---|---|---|
| `mcp_ado_pr_list_pull_requests` | 列出儲存庫中的 PR，可依狀態、建立者、審查者篩選 | `repositoryId` | `project`、`status`（active / completed / abandoned / all）、`creatorId`、`reviewerId`、`top` |
| `mcp_ado_pr_get_pull_request` | 取得指定 PR 的詳細資訊（狀態、來源/目標分支、審查者、合併狀態） | `repositoryId`、`pullRequestId` | `project` |
| `mcp_ado_pr_create_pull_request` | 建立新的 PR（從來源分支合併至目標分支） | `repositoryId`、`sourceBranch`、`targetBranch`、`title` | `project`、`description`、`reviewers` |
| `mcp_ado_pr_update_pull_request` | 更新 PR 的標題、說明或狀態（`completed` = 完成合併；`abandoned` = 放棄） | `repositoryId`、`pullRequestId` | `project`、`status`、`title`、`description` |
| `mcp_ado_pr_get_threads` | 取得 PR 上所有的審查留言討論串（含 inline 程式碼審查留言） | `repositoryId`、`pullRequestId` | `project` |
| `mcp_ado_pr_create_thread` | 在 PR 上新增審查留言或討論串（可設定 thread 狀態：active、fixed、wontFix 等） | `repositoryId`、`pullRequestId`、`content` | `project`、`status` |

#### 使用範例

**列出 PR**

> 「AiCodeReivew 儲存庫有哪些 Pull Request？」

```json
{
  "count": 1,
  "value": [
    {
      "pullRequestId": 16,
      "status": "active",
      "title": "AI Code Review 測試",
      "sourceRefName": "refs/heads/newfeature",
      "targetRefName": "refs/heads/main",
      "createdBy": { "displayName": "Lawrence" },
      "mergeStatus": "succeeded"
    }
  ]
}
```

**查看 PR 審查討論**

> 「顯示 PR #16 的所有審查意見」

```json
{
  "count": 5,
  "value": [
    {
      "id": 1,
      "status": "active",
      "comments": [
        {
          "content": "審查意見...",
          "author": { "displayName": "Lawrence" },
          "publishedDate": "2026-01-28T14:30:00Z"
        }
      ]
    }
  ]
}
```

**建立 PR / 新增審查留言 / 更新 PR 狀態**

> 「建立一個從 newfeature 到 main 的 PR，標題為「新功能：AI 程式碼審查」」

> ⚠️ 建立、留言、更新 PR 狀態等操作會實際異動 Azure DevOps 中的資料。

---

### Builds — CI/CD Pipeline（5 個）

| 工具名稱 | 說明 | 必填參數 | 選填參數 |
|---|---|---|---|
| `mcp_ado_builds_list_definitions` | 列出 Pipeline 定義（範本）—觸發建置前需先取得 `definitionId` | 無 | `project`、`name`（名稱過濾）、`top` |
| `mcp_ado_builds_list` | 列出建置執行紀錄，可依 Pipeline ID、分支、狀態或結果篩選 | 無 | `project`、`definitionId`、`statusFilter`、`resultFilter`、`branchName`、`top` |
| `mcp_ado_builds_get` | 取得指定建置的詳細資訊（狀態、結果、來源分支、執行時間等） | `buildId` | `project` |
| `mcp_ado_builds_get_logs` | 取得指定建置的執行日誌；可指定 `logId` 取得特定段落，或省略以取得全部 | `buildId` | `project`、`logId` |
| `mcp_ado_builds_queue` | 觸發並佇列新的建置（即「跑 Pipeline」）。需提供 Pipeline 定義 ID | `definitionId` | `project`、`sourceBranch`、`parameters`（JSON 字串）|

> **注意**：Builds 工具需要在 Azure DevOps 中預先設置 CI/CD Pipeline 才能使用。若專案尚未建立 Pipeline，工具將回傳空列表。

#### 使用範例

**列出所有 Pipeline 定義（mcp_ado_builds_list_definitions）**

> 「請列出 MyProject 的所有 Pipeline」

```json
{
  "count": 2,
  "value": [
    { "id": 1, "name": "CI Build", "type": "build", "queueStatus": "enabled" },
    { "id": 2, "name": "Release Pipeline", "type": "yaml", "queueStatus": "enabled" }
  ]
}
```

**查詢建置執行紀錄（mcp_ado_builds_list）**

> 「查詢 main 分支最近 3 次建置結果」

```json
{
  "count": 3,
  "value": [
    {
      "id": 456,
      "buildNumber": "20260414.1",
      "status": "completed",
      "result": "succeeded",
      "sourceBranch": "refs/heads/main",
      "startTime": "2026-04-14T08:00:00Z",
      "finishTime": "2026-04-14T08:15:00Z",
      "definition": { "id": 1, "name": "CI Build" }
    }
  ]
}
```

**取得建置詳細資訊（mcp_ado_builds_get）**

> 「取得建置 ID 456 的詳細資訊」

```json
{
  "id": 456,
  "buildNumber": "20260414.1",
  "status": "completed",
  "result": "succeeded",
  "sourceBranch": "refs/heads/main",
  "startTime": "2026-04-14T08:00:00Z",
  "finishTime": "2026-04-14T08:15:00Z",
  "requestedFor": { "displayName": "Lawrence" },
  "definition": { "id": 1, "name": "CI Build" }
}
```

**取得建置日誌（mcp_ado_builds_get_logs）**

> 「取得建置 456 的所有日誌清單」

```json
{
  "count": 5,
  "value": [
    { "id": 1, "type": "build", "url": "https://..." },
    { "id": 2, "type": "agent", "url": "https://..." }
  ]
}
```

> 若指定 `logId`，回應內容將直接返回該段日誌的純文字內容，而非清單結構。

**觸發建置（mcp_ado_builds_queue）**

> 「觸發 Pipeline ID 1 的建置，分支為 main」

> ⚠️ 此操作會實際啟動 CI/CD 流程，可能消耗建置資源或影響部署環境。

```json
{
  "id": 457,
  "buildNumber": "20260414.2",
  "status": "notStarted",
  "sourceBranch": "refs/heads/main",
  "definition": { "id": 1, "name": "CI Build" },
  "startTime": null
}
```

---

### Work — Sprint & Backlog（3 個）

| 工具名稱 | 說明 | 必填參數 | 選填參數 |
|---|---|---|---|
| `mcp_ado_work_list_iterations` | 列出 Sprint（Iteration），可依時間範圍篩選 | 無 | `project`、`team`、`timeframe`（past / current / future）|
| `mcp_ado_work_get_iteration_work_items` | 取得指定 Sprint 的所有工作項目 | `iterationId` | `project`、`team` |
| `mcp_ado_work_list_backlogs` | 列出 Backlog 層級（Epic / Feature / User Story / Task）與各層級的工作項目數量 | 無 | `project`、`team` |

#### 使用範例

**列出 Sprint**

> 「目前進行中的 Sprint 是哪一個？」

```json
{
  "count": 1,
  "value": [
    {
      "id": "a7152f2e-5d26-4b6f-8be8-e27d840171bf",
      "name": "Sprint_2026",
      "path": "PrimeEagleX\\Sprint_2026",
      "attributes": {
        "startDate": "2026-01-01T00:00:00Z",
        "finishDate": "2026-12-31T00:00:00Z",
        "timeFrame": "current"
      }
    }
  ]
}
```

**取得 Sprint 工作項目**

> 「列出目前 Sprint 中的所有工作項目」

```json
{
  "workItemRelations": [
    { "target": { "id": 471 } },
    { "target": { "id": 455 } }
  ]
}
```

**列出 Backlog 層級**

> 「顯示 PrimeEagleX 的 Backlog 結構有哪些層級」

```json
{
  "count": 4,
  "value": [
    { "name": "Epics", "rank": 4, "workItemTypes": [{ "name": "Epic" }] },
    { "name": "Features", "rank": 3, "workItemTypes": [{ "name": "Feature" }] },
    { "name": "Backlog items", "rank": 2, "workItemTypes": [{ "name": "Bug" }, { "name": "Product Backlog Item" }] },
    { "name": "Tasks", "rank": 1, "workItemTypes": [{ "name": "Task" }] }
  ]
}
```

---

## 常見使用情境

### 情境 1：查詢目前 Sprint 狀態

組合工具：`mcp_ado_work_list_iterations` → `mcp_ado_work_get_iteration_work_items` → `mcp_ado_work_items_batch_get`

1. 列出 Sprint，找出目前進行中的 `iterationId`
2. 取得該 Sprint 的所有工作項目 ID
3. 批次查詢工作項目詳情，確認哪些尚未完成

### 情境 2：追蹤並修復 Bug

組合工具：`mcp_ado_work_items_create` → `mcp_ado_work_items_add_comment` → `mcp_ado_pr_create_pull_request` → `mcp_ado_pr_create_thread`

1. 建立 Bug 工作項目，填入標題與說明
2. 對 Bug 新增留言，記錄重現步驟
3. 修復完成後，建立從 `bugfix/xxx` 到 `main` 的 PR
4. 在 PR 上新增審查說明留言

### 情境 3：程式碼審查流程

組合工具：`mcp_ado_pr_list_pull_requests` → `mcp_ado_pr_get_pull_request` → `mcp_ado_pr_get_threads` → `mcp_ado_pr_create_thread` → `mcp_ado_pr_update_pull_request`

1. 列出所有 active PR
2. 取得指定 PR 的詳細資訊
3. 查看已有的審查討論串
4. 新增審查意見
5. 審查通過後，更新 PR 狀態為 `completed`

### 情境 4：監控 CI/CD Pipeline

組合工具：`mcp_ado_builds_list_definitions` → `mcp_ado_builds_queue` → `mcp_ado_builds_get` → `mcp_ado_builds_get_logs`

1. 列出 Pipeline 定義，取得目標 `definitionId`
2. 觸發建置（可指定 branch 與 parameters）
3. 查詢建置執行狀態（succeded / failed / inProgress）
4. 若失敗，取得執行日誌進行診斷

### 情境 5：功能開發全流程追蹤

組合工具：`mcp_ado_work_items_create` → `mcp_ado_work_items_add_link` → `mcp_ado_git_get_commits` → `mcp_ado_work_items_update`

1. 建立 Feature → 建立子 User Story → 建立子 Task
2. 使用 `add_link` 建立 parent-child 關聯
3. 開發期間查看 commit 歷程確認進度
4. 完成後批次更新工作項目狀態為 `Done`

---

## WIQL 查詢範例

WIQL（Work Item Query Language）為類 SQL 語法，透過 `mcp_ado_work_items_query_by_wiql` 執行。

**查詢指派給我的進行中工作項目**

```sql
SELECT [System.Id], [System.Title], [System.State]
FROM WorkItems
WHERE [System.AssignedTo] = @Me
  AND [System.State] = 'Active'
ORDER BY [System.ChangedDate] DESC
```

**查詢最近 7 天修改的 Bug**

```sql
SELECT [System.Id], [System.Title], [System.State], [System.ChangedDate]
FROM WorkItems
WHERE [System.WorkItemType] = 'Bug'
  AND [System.ChangedDate] >= @Today - 7
ORDER BY [System.ChangedDate] DESC
```

**查詢特定 Sprint 中所有未完成的 Task**

```sql
SELECT [System.Id], [System.Title], [System.AssignedTo], [System.State]
FROM WorkItems
WHERE [System.WorkItemType] = 'Task'
  AND [System.IterationPath] = 'MyProject\Sprint 5'
  AND [System.State] <> 'Done'
  AND [System.State] <> 'Removed'
```

**查詢某個 Feature 下的所有子項目**

```sql
SELECT [System.Id], [System.Title], [System.WorkItemType], [System.State]
FROM WorkItemLinks
WHERE [Source].[System.Id] = 42
  AND [System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward'
  AND [Target].[System.WorkItemType] IN ('User Story', 'Task')
MODE (Recursive)
```

## 授權

MIT License

[English](https://github.com/lawrence8358/AzureDevOpsMcp/blob/main/README.md) | [繁體中文](https://github.com/lawrence8358/AzureDevOpsMcp/blob/main/README.zh-TW.md)

# Azure DevOps MCP Server

![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-green.svg)
[![NuGet](https://img.shields.io/nuget/v/AzureDevOpsMcp?logo=nuget&label=NuGet)](https://www.nuget.org/packages/AzureDevOpsMcp)
[![Build status](https://primeeagle.visualstudio.com/PrimeEagleX/_apis/build/status/GitHub/Nuget%20-%20AzureDevOpsMcp)](https://primeeagle.visualstudio.com/PrimeEagleX/_build/latest?definitionId=90)

An Azure DevOps MCP Server built on .NET 10, providing **29 tools** covering project management, work items (read/write separation), Git operations, Pull Request reviews, CI/CD pipelines, and Sprint planning. Integrates with AI assistants (such as GitHub Copilot and Claude) via STDIO or HTTP (SSE) mode.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Azure DevOps Services or Azure DevOps Server 2022
- A valid Personal Access Token (PAT)

## Installation

### Option 1: NuGet Global Tool (Recommended)

Install as a .NET global tool — no need to clone the source code.

```bash
dotnet tool install --global AzureDevOpsMcp
```

Once installed, use the `azure-devops-mcp` command directly. To upgrade:

```bash
dotnet tool update --global AzureDevOpsMcp
```

To uninstall:

```bash
dotnet tool uninstall --global AzureDevOpsMcp
```

> **Requirement**: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

---

### Option 2: Run Without Installing (dnx)

The `dnx` command built into .NET 10 SDK (similar to Node.js `npx`) downloads and runs the tool on the fly — no global installation required. Ideal for one-off use or CI environments.

```bash
dnx AzureDevOpsMcp --yes
```

Specify a version:

```bash
dnx AzureDevOpsMcp@1.0.1 --yes
```

Use in MCP configuration:

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

> `dnx` is included with .NET 10 SDK (Preview 6+). The `--yes` flag auto-accepts confirmation prompts. If you don't have .NET 10 SDK, use `dotnet tool exec AzureDevOpsMcp -y` instead.

---

### Option 3: Run from Source

```bash
git clone <repo-url>
cd AzureDevOpsMcp
dotnet run
```

---

## Configuration

### Environment Variables

| Variable | Required | Description |
|---|---|---|
| `ADO_ORG` | Yes | Azure DevOps organization URL, e.g. `https://dev.azure.com/your-org` or on-premises `https://tfs.company.com/DefaultCollection` |
| `ADO_PAT` | Yes | Personal Access Token |
| `ADO_PROJECT` | No | Default project name. When set, all tools will use this project if no `project` parameter is provided |

#### Generating a Personal Access Token (PAT)

1. Go to `https://dev.azure.com/{your-org}` → profile icon (top right) → **Personal Access Tokens**
2. Click **New Token** and grant the following permissions:
   - **Code**: Read & Write (Git operations and PR reviews)
   - **Work Items**: Read & Write (create, read, update, delete work items)
   - **Project and Team**: Read (list projects and sprints)
   - **Build**: Read & Execute (trigger and query pipelines)
3. Copy the generated token and set it as the `ADO_PAT` environment variable

> **⚠️ Security Note**: Never store your PAT in plain text in configuration files that are committed to version control. Use OS-level environment variables or a secrets manager, and rotate your token regularly.

---

### MCP Configuration

The following examples show how to configure the server to load all 29 tools.

#### Claude Desktop (`claude_desktop_config.json`)

**After installing the NuGet global tool (recommended):**

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

**Running from source:**

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

#### VS Code / GitHub Copilot (`.vscode/mcp.json`)

**After installing the NuGet global tool (recommended):**

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

**Running from source:**

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

#### Selective Tool Loading (`-d` flag)

Use the `-d` (`--domains`) flag to load only the tool groups you need — reducing noise, speeding up startup, and narrowing the tools available to the AI.

| Domain | Description | Tools |
|---|---|---|
| `core` | Organization and project management | 1 |
| `work-items` | Work item **read-only** operations (query, get, search) | 5 |
| `work-items-write` | Work item **write** operations (create, update, delete, comment, link) | 5 |
| `git` | Git repository operations (browse repos, branches, files, commits) | 4 |
| `pull-requests` | Pull request review workflow (create, review, comment) | 6 |
| `builds` | CI/CD pipeline execution and logs | 5 |
| `work` | Sprint / Backlog management | 3 |

If `-d` is not specified, all 29 tools across all 7 domains are loaded by default.

> **Why are work-items split into two domains?**  
> The read-only domain (`work-items`) is suitable for query-only scenarios. The write domain (`work-items-write`) should only be loaded when modifications are needed — this limits what the AI can do, improving accuracy and reducing the risk of unintended changes.

**Example** (load only `git`, `pull-requests`, and `work-items`):

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

## Available Tools (29)

### Core — Project Management (1)

| Tool | Description | Required | Optional |
|---|---|---|---|
| `mcp_ado_core_list_projects` | List all projects in the Azure DevOps organization | — | `stateFilter` (wellFormed / all / etc.), `top`, `skip` |

#### Example

> "List all projects in my Azure DevOps organization"

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

### WorkItems — Read-Only Domain: `work-items` (5)

| Tool | Description | Required | Optional |
|---|---|---|---|
| `mcp_ado_work_items_get` | Get a single work item by ID | `id` | `expand` (all / relations / fields) |
| `mcp_ado_work_items_batch_get` | Bulk-fetch multiple work items (more efficient than individual calls) | `ids` (array of IDs) | `expand` |
| `mcp_ado_work_items_get_comments` | Get all comments on a work item | `workItemId` | `project`, `top` |
| `mcp_ado_work_items_get_updates` | Get the change history of a work item (who changed what and when) | `id` | `top` |
| `mcp_ado_work_items_query_by_wiql` | Search work items using WIQL (SQL-like syntax) | `query` | `project`, `top` |

#### Examples

**Get a single work item**

> "Show me the details of work item 471"

```json
{
  "id": 471,
  "rev": 7,
  "fields": {
    "System.WorkItemType": "Bug",
    "System.Title": "[School] robots.txt misconfiguration causing SEO issues",
    "System.State": "Done",
    "System.AssignedTo": { "displayName": "Lawrence" },
    "Microsoft.VSTS.Common.Severity": "3 - Medium"
  }
}
```

**Bulk-fetch multiple work items**

> "Get work items 6 and 33 at the same time"

```json
{
  "count": 2,
  "value": [
    { "id": 6, "fields": { "System.Title": "PayGoGo Mobile Attendance", "System.State": "Active" } },
    { "id": 33, "fields": { "System.Title": "Add push notifications to app", "System.State": "Active" } }
  ]
}
```

**Get work item comments**

> "Show all comments on work item 471"

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

**Get work item change history**

> "What changes were recently made to work item 471?"

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

**WIQL query**

> "Query the most recently updated work item using WIQL"

```json
{
  "queryType": "flat",
  "workItems": [
    { "id": 471, "url": "https://dev.azure.com/org/project/_apis/wit/workItems/471" }
  ]
}
```

---

### WorkItems — Write Domain: `work-items-write` (5)

| Tool | Description | Required | Optional |
|---|---|---|---|
| `mcp_ado_work_items_create` | Create a new work item (types: Bug, Task, User Story, Feature, Epic, etc.) | `type`, `title` | `project`, `description`, `assignedTo`, `areaPath`, `iterationPath` |
| `mcp_ado_work_items_update` | Update work item fields (pass as key-value pairs using System field names) | `id`, `fields` | — |
| `mcp_ado_work_items_delete` | Delete a work item (moves to recycle bin by default; `destroy=true` permanently deletes) | `id` | `destroy` (default: false) |
| `mcp_ado_work_items_add_comment` | Add a comment to a work item | `workItemId`, `text` | `project` |
| `mcp_ado_work_items_add_link` | Link two work items together (common types: `Related`, `System.LinkTypes.Hierarchy-Forward` (child), `System.LinkTypes.Hierarchy-Reverse` (parent)) | `id`, `targetId`, `linkType` | `comment` |

#### Examples

**Create a work item (`mcp_ado_work_items_create`)**

> "Create a Bug titled 'Login button does not respond to clicks', assigned to Lawrence"

> ⚠️ This operation creates real data in Azure DevOps.

```json
{
  "id": 500,
  "rev": 1,
  "fields": {
    "System.Title": "Login button does not respond to clicks",
    "System.WorkItemType": "Bug",
    "System.State": "Active",
    "System.AssignedTo": { "displayName": "Lawrence" }
  }
}
```

**Update a work item (`mcp_ado_work_items_update`)**

> "Set work item 471 state to Done"

> ⚠️ This operation modifies data in Azure DevOps.

**Add a comment (`mcp_ado_work_items_add_comment`)**

> "Add a comment to work item 471: This bug was fixed by PR #58"

> ⚠️ Comments cannot be undone once added.

**Delete a work item (`mcp_ado_work_items_delete`)**

> ⚠️ Use with caution. Soft delete is recoverable; permanent delete is not.

Soft delete — "Move work item #500 to the recycle bin" (`destroy = false`, recoverable):

```json
{
  "id": 500,
  "project": { "name": "PrimeEagleX" },
  "destroyedDate": null,
  "url": "https://..."
}
```

Permanent delete — "Permanently delete work item #500" (`destroy = true`, cannot be undone):

```json
{
  "deleted": true,
  "id": 500,
  "permanent": true
}
```

**Link work items (`mcp_ado_work_items_add_link`)**

> "Set work item #501 as a child of work item #500"  
> (`linkType = "System.LinkTypes.Hierarchy-Forward"`)

The response is the updated work item. The `relations` field will contain the new link:

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

### Git — Repository Operations (4)

| Tool | Description | Required | Optional |
|---|---|---|---|
| `mcp_ado_git_list_repositories` | List all Git repositories in a project | — | `project` |
| `mcp_ado_git_list_branches` | List all branches in a repository | `repositoryId` | `project`, `filter` (name prefix) |
| `mcp_ado_git_get_item` | Read file content or list directory contents from a Git repository | `repositoryId`, `path` | `project`, `branch` |
| `mcp_ado_git_get_commits` | Get commit history, filterable by branch, file path, or author | `repositoryId` | `project`, `branch`, `itemPath`, `author`, `top` |

#### Examples

**List repositories**

> "List all Git repositories"

```json
{
  "count": 12,
  "value": [
    {
      "id": "94408af5-6c38-45d2-a5d3-cbcfd38b8ae7",
      "name": "AiCodeReivew",
      "defaultBranch": "refs/heads/main",
      "remoteUrl": "https://dev.azure.com/org/project/_git/AiCodeReivew"
    }
  ]
}
```

**List branches**

> "What branches exist in the AiCodeReivew repository?"

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

**Browse directory contents**

> "List the files in the root of the AiCodeReivew repository"

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

**View commit history**

> "Show the last 3 commits in the AiCodeReivew repository"

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

### Pull Requests — Code Review (6)

| Tool | Description | Required | Optional |
|---|---|---|---|
| `mcp_ado_pr_list_pull_requests` | List PRs in a repository, filterable by status, creator, or reviewer | `repositoryId` | `project`, `status` (active / completed / abandoned / all), `creatorId`, `reviewerId`, `top` |
| `mcp_ado_pr_get_pull_request` | Get details of a specific PR (status, branches, reviewers, merge status) | `repositoryId`, `pullRequestId` | `project` |
| `mcp_ado_pr_create_pull_request` | Create a new PR from a source branch into a target branch | `repositoryId`, `sourceBranch`, `targetBranch`, `title` | `project`, `description`, `reviewers` |
| `mcp_ado_pr_update_pull_request` | Update a PR's title, description, or status (`completed` = merge; `abandoned` = close) | `repositoryId`, `pullRequestId` | `project`, `status`, `title`, `description` |
| `mcp_ado_pr_get_threads` | Get all review comment threads on a PR (including inline code review comments) | `repositoryId`, `pullRequestId` | `project` |
| `mcp_ado_pr_create_thread` | Add a review comment or thread to a PR (supports thread status: active, fixed, wontFix, etc.) | `repositoryId`, `pullRequestId`, `content` | `project`, `status` |

#### Examples

**List PRs**

> "What pull requests are open in the AiCodeReivew repository?"

```json
{
  "count": 1,
  "value": [
    {
      "pullRequestId": 16,
      "status": "active",
      "title": "AI Code Review Test",
      "sourceRefName": "refs/heads/newfeature",
      "targetRefName": "refs/heads/main",
      "createdBy": { "displayName": "Lawrence" },
      "mergeStatus": "succeeded"
    }
  ]
}
```

**View PR review threads**

> "Show all review comments on PR #16"

```json
{
  "count": 5,
  "value": [
    {
      "id": 1,
      "status": "active",
      "comments": [
        {
          "content": "Review comment...",
          "author": { "displayName": "Lawrence" },
          "publishedDate": "2026-01-28T14:30:00Z"
        }
      ]
    }
  ]
}
```

**Create PR / Add review comment / Update PR status**

> "Create a PR from newfeature to main titled 'Feature: AI Code Review'"

> ⚠️ Creating, commenting on, and updating the status of a PR will make real changes in Azure DevOps.

---

### Builds — CI/CD Pipeline (5)

| Tool | Description | Required | Optional |
|---|---|---|---|
| `mcp_ado_builds_list_definitions` | List pipeline definitions (templates) — get the `definitionId` before queuing a build | — | `project`, `name` (name filter), `top` |
| `mcp_ado_builds_list` | List build run history, filterable by pipeline ID, branch, status, or result | — | `project`, `definitionId`, `statusFilter`, `resultFilter`, `branchName`, `top` |
| `mcp_ado_builds_get` | Get details of a specific build run (status, result, branch, timing, etc.) | `buildId` | `project` |
| `mcp_ado_builds_get_logs` | Get build logs; omit `logId` for a list of all logs, or specify one to get its content | `buildId` | `project`, `logId` |
| `mcp_ado_builds_queue` | Trigger and queue a new build (i.e. run a pipeline). Requires a pipeline definition ID | `definitionId` | `project`, `sourceBranch`, `parameters` (JSON string) |

> **Note**: Builds tools require CI/CD pipelines to be configured in Azure DevOps. If no pipelines exist in the project, the tools will return an empty list.

#### Examples

**List pipeline definitions (`mcp_ado_builds_list_definitions`)**

> "List all pipelines in MyProject"

```json
{
  "count": 2,
  "value": [
    { "id": 1, "name": "CI Build", "type": "build", "queueStatus": "enabled" },
    { "id": 2, "name": "Release Pipeline", "type": "yaml", "queueStatus": "enabled" }
  ]
}
```

**Query build run history (`mcp_ado_builds_list`)**

> "Show the last 3 build results on the main branch"

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

**Get build details (`mcp_ado_builds_get`)**

> "Get the details of build ID 456"

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

**Get build logs (`mcp_ado_builds_get_logs`)**

> "Get all logs for build 456"

```json
{
  "count": 5,
  "value": [
    { "id": 1, "type": "build", "url": "https://..." },
    { "id": 2, "type": "agent", "url": "https://..." }
  ]
}
```

> When `logId` is specified, the response contains the plain-text content of that specific log segment rather than a list.

**Trigger a build (`mcp_ado_builds_queue`)**

> "Trigger pipeline ID 1 on the main branch"

> ⚠️ This will start a real CI/CD process and may consume build resources or affect deployment environments.

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

### Work — Sprint & Backlog (3)

| Tool | Description | Required | Optional |
|---|---|---|---|
| `mcp_ado_work_list_iterations` | List sprints (iterations), filterable by timeframe | — | `project`, `team`, `timeframe` (past / current / future) |
| `mcp_ado_work_get_iteration_work_items` | Get all work items assigned to a specific sprint | `iterationId` | `project`, `team` |
| `mcp_ado_work_list_backlogs` | List backlog levels (Epic / Feature / User Story / Task) and their work item counts | — | `project`, `team` |

#### Examples

**List sprints**

> "Which sprint is currently active?"

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

**Get sprint work items**

> "List all work items in the current sprint"

```json
{
  "workItemRelations": [
    { "target": { "id": 471 } },
    { "target": { "id": 455 } }
  ]
}
```

**List backlog levels**

> "What are the backlog levels in PrimeEagleX?"

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

## Common Scenarios

### Scenario 1: Check Current Sprint Status

Tools: `mcp_ado_work_list_iterations` → `mcp_ado_work_get_iteration_work_items` → `mcp_ado_work_items_batch_get`

1. List sprints to find the current `iterationId`
2. Fetch all work item IDs in that sprint
3. Bulk-fetch work item details to identify incomplete items

### Scenario 2: Track and Fix a Bug

Tools: `mcp_ado_work_items_create` → `mcp_ado_work_items_add_comment` → `mcp_ado_pr_create_pull_request` → `mcp_ado_pr_create_thread`

1. Create a Bug work item with a title and description
2. Add a comment with reproduction steps
3. After fixing, create a PR from `bugfix/xxx` to `main`
4. Add a review comment to the PR explaining the fix

### Scenario 3: Code Review Workflow

Tools: `mcp_ado_pr_list_pull_requests` → `mcp_ado_pr_get_pull_request` → `mcp_ado_pr_get_threads` → `mcp_ado_pr_create_thread` → `mcp_ado_pr_update_pull_request`

1. List all active PRs
2. Get details of a specific PR
3. Review existing comment threads
4. Add review feedback
5. Once approved, set the PR status to `completed` to merge

### Scenario 4: Monitor CI/CD Pipeline

Tools: `mcp_ado_builds_list_definitions` → `mcp_ado_builds_queue` → `mcp_ado_builds_get` → `mcp_ado_builds_get_logs`

1. List pipeline definitions to find the target `definitionId`
2. Queue a build (optionally specify branch and parameters)
3. Poll for build status (succeeded / failed / inProgress)
4. If failed, retrieve the logs to diagnose the issue

### Scenario 5: Full Feature Development Lifecycle

Tools: `mcp_ado_work_items_create` → `mcp_ado_work_items_add_link` → `mcp_ado_git_get_commits` → `mcp_ado_work_items_update`

1. Create a Feature → create child User Stories → create child Tasks
2. Use `add_link` to establish parent-child relationships
3. Monitor commit history during development
4. Bulk-update work item states to `Done` when complete

---

## WIQL Query Examples

WIQL (Work Item Query Language) is a SQL-like syntax used with `mcp_ado_work_items_query_by_wiql`.

**Find active work items assigned to me**

```sql
SELECT [System.Id], [System.Title], [System.State]
FROM WorkItems
WHERE [System.AssignedTo] = @Me
  AND [System.State] = 'Active'
ORDER BY [System.ChangedDate] DESC
```

**Find bugs modified in the last 7 days**

```sql
SELECT [System.Id], [System.Title], [System.State], [System.ChangedDate]
FROM WorkItems
WHERE [System.WorkItemType] = 'Bug'
  AND [System.ChangedDate] >= @Today - 7
ORDER BY [System.ChangedDate] DESC
```

**Find all incomplete tasks in a specific sprint**

```sql
SELECT [System.Id], [System.Title], [System.AssignedTo], [System.State]
FROM WorkItems
WHERE [System.WorkItemType] = 'Task'
  AND [System.IterationPath] = 'MyProject\Sprint 5'
  AND [System.State] <> 'Done'
  AND [System.State] <> 'Removed'
```

**Find all child items of a specific Feature**

```sql
SELECT [System.Id], [System.Title], [System.WorkItemType], [System.State]
FROM WorkItemLinks
WHERE [Source].[System.Id] = 42
  AND [System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward'
  AND [Target].[System.WorkItemType] IN ('User Story', 'Task')
MODE (Recursive)
```

## License

MIT License

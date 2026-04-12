---
name: coding-standards
description: PrimeEagleX backend coding standards. Covers naming conventions, async rules, region structure, comment rules, data access rules, exception handling, and SOLID principles. Reference this when writing or reviewing any backend code.
user-invocable: false
---

# 編碼規範

## 命名慣例

| 類型 | 格式 | 範例 |
|------|------|------|
| 類別 / 方法 / 屬性 | PascalCase | `UserService`, `GetUserAsync` |
| 私有欄位 | _camelCase | `_userId` |
| 參數 / 區域變數 | camelCase | `userId` |
| 常數 | UPPER_SNAKE_CASE | `MAX_RETRY_COUNT` |

---

## 非同步方法

- 非同步方法名稱必須以 `Async` 結尾
- 返回型別為 `Task` 或 `Task<T>`

> 例外：MediatR `IRequestHandler<TRequest, TResponse>` 的 `Handle` 方法為框架協議方法，
> 不加 `Async` 後綴。若方法內有 `await`，使用 `public async Task<T> Handle(...)`；
> 若無 `await`，使用 `public Task<T> Handle(...)` 搭配 `return Task.FromResult(value)`。

---

## Region 結構

### 分類順序（不存在則省略，存在則依序排列）

1. `#region Members` — 常數、私有欄位（變數之間**不需**空行）
2. `#region Properties` — 屬性（屬性之間**需一空行**）
3. `#region Constructors` — 建構子（建構子之間**需一空行**）
4. `#region Public Methods` — 公開方法
5. `#region Public {業務群組} Methods` — Controller / Service 內有多組業務方法時，**務必**用此命名區分（例：`客戶資料管理 Methods`、`日誌類別 Methods`）
6. `#region Protected Methods` — 覆寫（override）或 protected 方法（如繼承 DelegatingHandler、基底類別的 abstract 方法等），排列於 Public Methods 之後
7. `#region Private Methods` — 私有方法（方法之間**需一空行**）

### 空行規則

- 各 `#region` 區塊之間須有一行空白
- 最上方與最下方的 `#region` 不須多餘空白

### 完整範例

```csharp
namespace ExampleNamespace;

public class ExampleClass
{
    #region Members

    private int _exampleVariable;
    private const string ExampleConstant = "Example";

    #endregion

    #region Properties

    /// <summary>範例屬性</summary>
    public int ExampleProperty { get; set; }

    #endregion

    #region Constructors

    /// <summary>初始化 ExampleClass 的新執行個體</summary>
    public ExampleClass()
    {
    }

    #endregion

    #region Public Methods

    /// <summary>範例方法</summary>
    public void ExampleMethod()
    {
    }

    #endregion

    #region Public 使用者 Methods

    /// <summary>建立新使用者</summary>
    public async Task CreateUserAsync()
    {
    }

    /// <summary>刪除指定的使用者</summary>
    public async Task<bool> DeleteUserAsync()
    {
    }

    #endregion

    #region Public 使用者裝置 Methods

    /// <summary>
    /// 建立新裝置
    /// <param name="name">裝置名稱。</param>
    /// </summary>
    public async Task CreateDeviceAsync(string name)
    { 
    }

    /// <summary>刪除指定的裝置</summary>
    public async Task DeleteDeviceAsync()
    {
    }

    #endregion

    #region Private Methods

    /// <summary>私有方法實作範例</summary>
    private void ExamplePrivateMethod()
    {
    }

    #endregion
}
```

---

## 程式碼註解規範

- **公開屬性和方法**：必須撰寫 XML Summary 註解（`///`）
- **私有方法**：可用 `///` 撰寫 Summary，若不撰寫則至少須有單行 `//` 說明用途
- 註解應清楚描述「**為什麼**這麼寫」，而非「**做什麼**」
- 避免無意義的註解（如：`// 設定 i 為 1`）
- 註解請使用繁體中文撰寫，且保持簡潔明確

---

## 程式風格

> 本專案遵循 **Clean Code** 風格，強調程式碼的可讀性、可維護性與單一職責。

### 函式設計

- 每個函式只做一件事，命名清楚表達用途（例：`FetchUserData()`、`FormatCurrency()`）

### Early Return（提早返回）

```csharp
// ✅ 推薦：避免多層巢狀
if (user == null) throw new KeyNotFoundException("使用者不存在");
if (!user.IsActive) return false;
return true;

// ❌ 避免：多層巢狀
if (user != null)
{
    if (user.IsActive) { return true; }
    else { return false; }
}
else { throw new KeyNotFoundException(); }
```

### 命名參數

多個參數時，語意不明確的參數必須使用命名參數呼叫：

```csharp
// ✅ 推薦
CreateUserAsync(userName: "user1", isActive: false);

// ✅ 若 userName 已明確表意，也可僅對不明確的參數使用命名參數
CreateUserAsync("user2", isActive: true);

// ❌ 避免
CreateUserAsync("user1", false);
```

---

# 測試案例

請使用 Arrange & Act & Assert 結構撰寫測試案例，並使用 `Fact` 或 `Theory` 屬性標註測試方法：

```csharp
[Fact(DisplayName = "建立{說明} - 成功")]
public async Task TestCase()
{
    // Arrange
    var model = new {Entity}AddModel { ... };
    var command = new Add{Entity}Command(model);
    var handler = new Add{Entity}CommandHandler(_mockRepo.Object, _mockUnitOfWork.Object);

    // Act
    await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Single(_data);
    Assert.Equal(expected, _data[0].Property);
}
```

## SOLID 原則

### S — 單一職責（SRP）
一個類別應只負責一項職責，避免將過多功能塞進同一個類別（God Class）。

### O — 開放封閉（OCP）
對擴充開放，對修改封閉。需求變更時應透過以下方式擴充：
- 繼承基底類別
- 實作介面定義新行為
- 採用策略模式或裝飾者模式
- 透過依賴注入替換實作

### L — 里氏替換（LSP）
子類別必須能完整替換父類別，且不破壞程式正確性：
- 子類別不應拋出 `NotImplementedException`
- 子類別不應改變父類別方法的預期行為（回傳值範圍、副作用等）
- 範例：若 `BaseRepository.Get()` 找不到資料時回傳 `null`，則所有繼承的 Repository 都應遵循相同行為

### I — 介面隔離（ISP）
不應強迫實作端依賴不需要的方法，將大型介面拆分為多個專用小介面（通常 3–5 個方法）：
- ❌ `IUserService`（包含所有使用者操作）
- ✅ `IUserReader`、`IUserWriter`、`IUserValidator`（各司其職）

### D — 依賴反轉（DIP）
高層模組不依賴低層模組，兩者都依賴抽象：
- ✅ 業務邏輯依賴 `IRepository<T>` 介面
- ❌ 業務邏輯直接依賴 `DbContext` 實作


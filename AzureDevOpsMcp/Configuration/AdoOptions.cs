namespace AzureDevOpsMcp.Configuration;

/// <summary>Azure DevOps 連線設定選項，對應環境變數 ADO_ORG、ADO_PAT 與 ADO_PROJECT。</summary>
public class AdoOptions
{
    #region Properties

    /// <summary>Azure DevOps 服務的根 URL（如 https://dev.azure.com/myorg 或 https://tfs.company.com/DefaultCollection）。</summary>
    public string ServerUrl { get; set; } = string.Empty;

    /// <summary>用於認證的個人存取權杖（Personal Access Token）。序列化時自動忽略以避免洩漏敏感資訊。</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string PatToken { get; set; } = string.Empty;

    /// <summary>預設 Azure DevOps 專案名稱。設定後，工具在未指定 project 參數時自動使用此專案。</summary>
    public string? Project { get; set; }

    #endregion
}

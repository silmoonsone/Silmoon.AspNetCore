# WebAuthn 认证使用指南

## 概述

WebAuthn（Web Authentication）是一种基于标准的Web API，允许网站使用公钥密码学来注册和认证用户，无需密码。本库提供了完整的WebAuthn认证解决方案，支持三种使用模式：

1. **简单认证模式**：一步完成用户身份验证
2. **分阶段认证模式**：先获取认证数据，再提交到后端API验证
3. **自定义签名模式**：对特定业务数据进行数字签名（类似密钥系统）

## 快速开始

### 后端配置

#### 1. 注册服务
```csharp
// Program.cs
builder.Services.AddWebAuthn<WebAuthnServiceImpl>();
builder.Services.Configure<WebAuthnServiceOptions>(options =>
{
    options.AppName = "您的应用名称";
    options.Host = "您的域名";
});
app.UseWebAuthn();
```

#### 2. 实现服务类
继承`WebAuthnService`抽象类，实现以下方法：

```csharp
public class WebAuthnServiceImpl : WebAuthnService
{
    // 依赖注入您的数据访问层和用户服务
    public WebAuthnServiceImpl(YourDataService dataService, YourUserService userService, IOptions<WebAuthnServiceOptions> options) 
        : base(options) { }

    // 获取用户凭证列表（认证时调用）
    public override async Task<AllowUserCredential> GetAllowCredentials(HttpContext httpContext, string userId)
    {
        // 注意：以下方法调用需要您自己实现
        var user = await GetUser(userId);  // 根据userId获取用户信息
        AllowUserCredential result = new()
        {
            UserId = user.Id,                    // 必填：用户ID字符串
            Credentials = user.WebAuthnCredentials.Select(c => new Credential
            {
                Id = Convert.ToBase64String(c.CredentialId),  // 必填：凭证ID的Base64编码
                Type = "public-key"                           // 必填：固定值"public-key"
            }).ToArray()
        };
        return result;
    }

    // 获取用户信息（创建Passkey时调用）
    public override async Task<StateSet<bool, ClientWebAuthnOptions.ClientWebAuthnUser>> GetClientCreateWebAuthnOptions(HttpContext httpContext)
    {
        // 注意：以下方法调用需要您自己实现
        var user = await GetCurrentUser();  // 获取当前登录用户
        var userInfo = new ClientWebAuthnOptions.ClientWebAuthnUser
        {
            Id = user.Id.ToByteArray(),          // 必填：用户ID转换为字节数组
            Name = user.Username,                // 必填：用户名
            DisplayName = user.DisplayName       // 必填：显示名称
        };
        return true.ToStateSet(userInfo);
    }

    // 保存新创建的Passkey（创建流程的最后一步）
    public override async Task<StateSet<bool>> OnCreate(HttpContext httpContext, WebAuthnCreateResponse webAuthnCreateResponse)
    {
        // 注意：以下方法调用需要您自己实现
        var user = await GetCurrentUser();  // 获取当前登录用户
        var webAuthnInfo = webAuthnCreateResponse.WebAuthnInfo;  // 包含完整的WebAuthn凭证信息
        
        // 保存到数据库
        await SaveWebAuthnCredential(user.Id, webAuthnInfo);  // 保存WebAuthn凭证到数据库
        return true.ToStateSet();
    }

    // 删除Passkey
    public override async Task<StateSet<bool>> OnDelete(HttpContext httpContext, byte[] credentialId)
    {
        // 注意：以下方法调用需要您自己实现
        var user = await GetCurrentUser();  // 获取当前登录用户
        var deleted = await DeleteWebAuthnCredential(user.Id, credentialId);  // 从数据库删除凭证
        return deleted ? true.ToStateSet() : false.ToStateSet("凭证不存在");
    }

    // 获取公钥信息（验证签名时调用）
    public override async Task<PublicKeyInfo> OnGetPublicKeyInfo(HttpContext httpContext, byte[] rawId, string userId = null)
    {
        // 注意：以下方法调用需要您自己实现
        var credential = await GetWebAuthnCredential(rawId, userId);  // 根据凭证ID获取凭证信息
        if (credential == null) return null;
        
        return new PublicKeyInfo
        {
            PublicKey = credential.PublicKey,                    // 必填：公钥字节数组
            PublicKeyAlgorithm = credential.PublicKeyAlgorithm   // 必填：公钥算法标识
        };
    }

    // 认证完成处理（可选）
    public override Task<StateSet<bool>> OnAuthenticateCompleted(HttpContext httpContext, WebAuthnAuthenticateResponse webAuthnAuthenticateResponse, StateSet<bool> result, object flagData)
    {
        // flagData包含前端传递的业务数据，如 {do: 'verifyUser'}
        // 可以在这里处理认证完成后的业务逻辑
        return Task.FromResult(result);
    }
}
```

#### 3. 创建验证API（分阶段模式需要）
```csharp
[HttpPost]
public async Task<IActionResult> ValidateWebAuthnData(
    [FromForm] string webAuthnData, 
    [FromForm] string challenge = null)
{
    var result = await webAuthnService.ValidateData(HttpContext, webAuthnData, challenge);
    return this.JsonStateFlag(result.State, result.Message);
}
```

### 前端配置
```html
<script src="~/_content/Silmoon.AspNetCore.Encryption/js/webauthnSupport.js"></script>
```

## 使用方法

### 创建Passkey

```javascript
// 创建Passkey
const result = await createWebAuthn();
if (result.Success) {
    console.log("Passkey创建成功");
    location.reload();
} else {
    console.error("Passkey创建失败:", result.Message);
}
```

**创建流程**：
1. 前端调用`createWebAuthn()`函数
2. 系统预生成挑战数据和用户信息
3. 浏览器调用`navigator.credentials.create()`
4. 用户通过生物识别或PIN码确认
5. 浏览器生成凭证并自动提交到后端的`OnCreate`方法
6. 后端验证并保存Passkey信息

### 1. 简单认证模式

适用于基本的用户身份验证：

```javascript
// 验证用户
const authResult = await authenticateWebAuthn('userId', {do: 'verifyUser'});
if (authResult.Success) {
    console.log("认证成功");
}
```

### 2. 分阶段认证模式（推荐）

适用于需要自定义验证逻辑的场景：

```javascript
// 第一步：获取认证数据
const authData = await initWebAuthnRequest('userId', null, {do: 'verifyUser'});

// 第二步：提交到后端验证
const formData = new FormData();
formData.append('webAuthnData', JSON.stringify(authData.Data));
const response = await fetch("/Api/ValidateWebAuthnData", {
    method: 'POST',
    body: formData
});
```

### 3. 自定义签名模式（高级功能）

**重要功能**：对特定业务数据进行数字签名，类似密钥系统。

**⚠️ 安全警告**：使用此功能时必须防范重放攻击！

```javascript
// 对转账数据进行签名
const transferData = `${targetAccount}|${amount}|${memo}|${Date.now()}`;
const hashedData = await sha256Hash(transferData); // 需要自己实现哈希函数

const authData = await initWebAuthnRequest('userId', hashedData);

// 提交签名验证
const formData = new FormData();
formData.append('webAuthnData', JSON.stringify(authData.Data));
formData.append('challenge', await sha256Hash(transferData));
const response = await fetch("/Api/ValidateWebAuthnData", {
    method: 'POST',
    body: formData
});
```

## 应用场景

### 简单认证
- 用户登录验证
- 操作权限确认

### 分阶段认证
- 需要自定义业务逻辑的验证
- 多步骤认证流程

### 自定义签名
- **转账请求签名**：确保转账数据的完整性和用户授权
- **重要操作确认**：删除账户、修改关键设置等
- **文档签名**：对重要文档进行数字签名

## 防重放攻击措施

使用自定义签名模式时，必须包含以下安全元素：

1. **时间戳**：确保请求时效性
2. **随机数**：防止重放攻击
3. **唯一标识**：确保每次请求唯一性

```javascript
// 安全的数据格式示例
const secureData = `${operation}|${target}|${Date.now()}|${generateNonce()}|${userId}`;
```

## 管理Passkey

### 删除Passkey

```javascript
// 删除指定的Passkey
const result = await deleteWebAuthn('credentialId');
if (result.Success) {
    console.log("Passkey删除成功");
    location.reload();
} else {
    console.error("Passkey删除失败:", result.Message);
}
```

**删除流程**：
1. 前端调用`deleteWebAuthn()`函数
2. 系统调用后端的`OnDelete`方法
3. 后端从数据库中删除对应的WebAuthn凭证
4. 返回删除结果

### 查看Passkey列表

Passkey列表通常由后端提供数据，前端展示：

```html
<!-- 示例：显示用户的所有Passkey -->
<div class="passkey-list">
    @foreach (var passkey in userPasskeys)
    {
        <div class="passkey-item">
            <span>算法: @passkey.PublicKeyAlgorithm</span>
            <span>类型: @passkey.AuthenticatorAttachment</span>
            <span>ID: @passkey.CredentialId</span>
            <button onclick="deletePasskey('@passkey.CredentialId')">删除</button>
        </div>
    }
</div>
```

## 注意事项

1. **HTTPS要求**：WebAuthn只能在HTTPS环境下工作（localhost除外）
2. **设备要求**：用户需要支持生物识别或PIN码的设备
3. **浏览器兼容**：支持Chrome 67+、Firefox 60+、Safari 13+、Edge 18+
4. **安全提醒**：自定义签名模式需要特别注意重放攻击防护

## 完整示例

参考 `Silmoon.AspNetCore.UserAuthTest` 项目中的 `Passkey.cshtml` 页面，该页面展示了所有三种使用模式的完整实现示例。

**项目地址**：`Silmoon.AspNetCore.UserAuthTest.csproj`

## API端点

| 端点 | 说明 | 参数 |
|------|------|------|
| `/_webAuthn/getWebAuthnOptions` | 获取创建Passkey选项 | - |
| `/_webAuthn/createWebAuthn` | 创建Passkey | - |
| `/_webAuthn/deleteWebAuthn` | 删除Passkey | credentialId |
| `/_webAuthn/getWebAuthnAuthenticateOptions` | 获取认证选项 | UserId, Challenge (POST FormData) |
| `/_webAuthn/authenticateWebAuthn` | 执行认证 | UserId (POST FormData) |

## 错误处理

所有操作返回统一格式：
```javascript
{
    Success: boolean,    // 操作是否成功
    Message: string,     // 错误信息
    Data: object         // 返回数据
}
```

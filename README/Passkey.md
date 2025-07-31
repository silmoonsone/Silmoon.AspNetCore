# WebAuthn 认证使用指南

## 概述

WebAuthn（Web Authentication）是一种基于标准的Web API，允许网站使用公钥密码学来注册和认证用户，无需密码。本库提供了完整的WebAuthn认证解决方案，支持两种认证模式：

1. **直接认证模式**：客户端直接与WebAuthn服务进行认证
2. **分阶段认证模式**：客户端先获取认证数据，然后提交到后端API进行验证

## 功能特性

- ✅ 创建和管理Passkey
- ✅ 用户身份验证
- ✅ 支持多种认证器类型
- ✅ 安全的挑战-响应机制
- ✅ 灵活的认证流程

## 快速开始

### 1. 配置服务

在您的ASP.NET Core应用程序中配置WebAuthn服务：

```csharp
// Program.cs
builder.Services.Configure<WebAuthnServiceOptions>(options =>
{
    options.AppName = "您的应用名称";
    options.Host = "您的域名";
});

// 注册WebAuthn中间件
app.UseMiddleware<WebAuthnMiddleware>();
```

### 2. 引入JavaScript库

在您的页面中引入WebAuthn支持脚本：

```html
<script src="/js/webauthnSupport.js"></script>
```

## 使用方法

### 创建Passkey

用户可以通过以下方式创建新的Passkey：

```javascript
async function createPasskey() {
    const result = await createWebAuthn();
    if (result.Success) {
        console.log("Passkey创建成功");
        // 刷新页面或更新UI
        location.reload();
    } else {
        console.error("Passkey创建失败:", result.Message);
    }
}
```

### 直接认证模式

直接与WebAuthn服务进行认证，适用于大多数场景：

```javascript
async function authenticateUser(userId, flagData) {
    const result = await authenticateWebAuthn(userId, flagData);
    if (result.Success) {
        console.log("认证成功，返回数据:", result.Data);
        // 处理认证成功逻辑
    } else {
        console.error("认证失败:", result.Message);
    }
}

// 示例调用
// 验证特定用户
authenticateUser('user123', {do: 'verifyUser'});

// 验证所有用户
authenticateUser(null, {do: 'verifyAllUser'});
```

### 分阶段认证模式

适用于需要自定义验证逻辑的场景：

```javascript
// 第一步：初始化认证请求
async function initAuthentication(userId, flagData) {
    const result = await initWebAuthnRequest(userId, flagData);
    if (result.Success) {
        // 保存认证数据供后续使用
        window.authData = result.Data;
        console.log("认证数据已准备就绪");
    } else {
        console.error("初始化认证失败:", result.Message);
    }
}

// 第二步：提交到后端API验证
async function validateWithBackend() {
    if (!window.authData) {
        console.error("请先初始化认证");
        return;
    }

    const formData = new FormData();
    formData.append('webAuthnData', JSON.stringify(window.authData));
    
    const response = await fetch("/Api/ValidateWebAuthnData", {
        method: 'POST',
        body: formData
    });
    
    if (response.ok) {
        const result = await response.json();
        if (result.Success) {
            console.log("后端验证成功");
        } else {
            console.error("后端验证失败:", result.Message);
        }
    }
}
```

### 管理Passkey

#### 删除Passkey

```javascript
async function deletePasskey(credentialId) {
    const result = await deleteWebAuthn(credentialId);
    if (result.Success) {
        console.log("Passkey删除成功");
        location.reload();
    } else {
        console.error("Passkey删除失败:", result.Message);
    }
}
```

#### 查看Passkey列表

Passkey列表通常由后端提供，前端展示：

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

## 认证流程说明

### 创建Passkey流程

1. 用户点击"创建Passkey"按钮
2. 系统生成挑战（Challenge）和用户信息
3. 浏览器调用`navigator.credentials.create()`
4. 用户通过生物识别或PIN码确认
5. 浏览器生成凭证并发送到服务器
6. 服务器验证并保存Passkey信息

### 认证流程

1. 用户点击"验证"按钮
2. 系统生成新的挑战和允许的凭证列表
3. 浏览器调用`navigator.credentials.get()`
4. 用户通过生物识别或PIN码确认
5. 浏览器生成认证断言
6. 服务器验证签名和挑战

## 参数说明

### authenticateWebAuthn(userId, flagData)

- `userId`: 用户ID，如果为null则验证所有用户
- `flagData`: 自定义数据，会在认证成功后返回

### initWebAuthnRequest(userId, flagData)

- `userId`: 用户ID
- `flagData`: 自定义数据，会在认证数据中保留

## 错误处理

所有WebAuthn操作都会返回统一的结果格式：

```javascript
{
    Success: boolean,    // 操作是否成功
    Message: string,     // 错误信息（如果失败）
    Data: object         // 返回数据（如果成功）
}
```

常见错误情况：
- 用户取消操作
- 浏览器不支持WebAuthn
- 网络连接问题
- 服务器验证失败

## 最佳实践

1. **用户体验**：在认证过程中显示加载状态
2. **错误处理**：为用户提供清晰的错误提示
3. **降级方案**：为不支持WebAuthn的浏览器提供替代方案
4. **安全性**：确保所有通信都通过HTTPS进行
5. **测试**：在不同设备和浏览器上测试认证流程

## 浏览器兼容性

WebAuthn支持所有现代浏览器：
- Chrome 67+
- Firefox 60+
- Safari 13+
- Edge 18+

## 注意事项

1. WebAuthn只能在HTTPS环境下工作（localhost除外）
2. 用户需要支持生物识别或PIN码的设备
3. 每个域名的Passkey是独立的
4. 建议为用户提供多种认证方式作为备选

## 示例页面

参考 `Passkey.cshtml` 页面了解完整的使用示例，该页面展示了：
- Passkey列表展示
- 创建新Passkey
- 直接认证
- 分阶段认证
- 删除Passkey

这个示例页面可以作为您实现WebAuthn功能的参考模板。

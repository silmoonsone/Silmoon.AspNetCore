// WebAuthn support script
let webAuthnClientOptions = {
    getWebAuthnOptionsUrl: '/_webAuthn/getWebAuthnOptions',
    createWebAuthnUrl: '/_webAuthn/createWebAuthn',
    deleteWebAuthnUrl: '/_webAuthn/deleteWebAuthn',
    getWebAuthnAuthenticateOptions: '/_webAuthn/getWebAuthnAuthenticateOptions',
    authenticateWebAuthnUrl: '/_webAuthn/authenticateWebAuthn'
}

async function createWebAuthn() {
    try {
        // 1. 向服务器请求创建挑战 (challenge) 和 RP 信息
        const response = await fetch(webAuthnClientOptions.getWebAuthnOptionsUrl);
        const result = await response.json();
        const options = result.Data;

        if (!result.Success) return { Success: false, Message: result.Message }

        // 2. 将 challenge 和 user.id 转换为 Uint8Array
        options.challenge = base64ToUint8Array(options.challenge);
        options.user.id = base64ToUint8Array(options.user.id);

        // 3. 创建密钥对
        const credential = await navigator.credentials.create({
            publicKey: options
        });

        // 4. 获取相关信息
        const attestationObject = new Uint8Array(credential.response.attestationObject);
        const clientDataJSON = new Uint8Array(credential.response.clientDataJSON);
        const rawId = arrayBufferToBase64(new Uint8Array(credential.rawId));

        // 5. 将数据发送到服务器进行验证
        const data = {
            rawId: rawId,
            type: credential.type,
            authenticatorAttachment: credential.authenticatorAttachment,
            response: {
                attestationObject: arrayBufferToBase64(attestationObject),
                clientDataJSON: arrayBufferToBase64(clientDataJSON),
            },
        };

        // 6. 将数据发送到服务器进行注册
        const createResponse = await fetch(webAuthnClientOptions.createWebAuthnUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (createResponse.ok) {
            const responseData = await createResponse.json();
            return responseData;
        } else {
            return { Success: false, Message: createResponse.statusText }
        }
    } catch (err) {
        return { Success: false, Message: err.message, Data: err }
    }
}
async function deleteWebAuthn(credentialId) {
    if (confirm('确定删除？')) {
        try {
            const data = new FormData();
            data.append("CredentialId", credentialId);
            const response = await fetch(webAuthnClientOptions.deleteWebAuthnUrl, {
                method: "POST",
                body: data
            });

            if (response.ok) {
                const responseData = await response.json();
                return responseData;
            } else {
                return { Success: false, Message: response.statusText }
            }
        } catch (err) {
            return { Success: false, Message: err.message, Data: err }
        }
    } else return { Success: false, Message: "Delete canceled.", Data: null }
}
async function authenticateWebAuthn(userId, flagData) {
    try {
        if (typeof flagData == "undefined") flagData = null;
        // 1. 向服务器请求挑战 (challenge) 和其他验证选项
        const response = await fetch(webAuthnClientOptions.getWebAuthnAuthenticateOptions + '?UserId=' + userId);
        const responseData = await response.json();

        const options = responseData.Data;
        // 2. 将 challenge 和允许的凭证ID (allowedCredentials.id) 转换为 Uint8Array
        options.challenge = base64ToUint8Array(options.challenge);
        options.allowCredentials = options.allowCredentials.map(cred => {
            return {
                ...cred,
                id: base64ToUint8Array(cred.id)
            };
        });

        // 3. 调用 navigator.credentials.get() 获取签名
        const assertion = await navigator.credentials.get({
            publicKey: options
        });

        // 4. 获取相关信息
        const authenticatorData = new Uint8Array(assertion.response.authenticatorData);
        const clientDataJSON = new Uint8Array(assertion.response.clientDataJSON);
        const signature = new Uint8Array(assertion.response.signature);
        const rawId = new Uint8Array(assertion.rawId);

        // 5. 将数据发送到服务器进行验证
        const data = {
            rawId: arrayBufferToBase64(rawId),
            type: assertion.type,
            response: {
                authenticatorData: arrayBufferToBase64(authenticatorData),
                clientDataJSON: arrayBufferToBase64(clientDataJSON),
                signature: arrayBufferToBase64(signature),
            },
            flagData: flagData
        };

        const verificationResponse = await fetch(webAuthnClientOptions.authenticateWebAuthnUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (verificationResponse.ok) {
            const result = await verificationResponse.json();
            return result;
        } else {
            return { Success: false, Message: verificationResponse.statusText }
        }
    } catch (err) {
        return { Success: false, Message: err.message, Data: err }
    }
}

async function initWebAuthnRequest(userId, flagData) {
    try {
        if (typeof flagData == "undefined") flagData = null;
        // 1. 向服务器请求挑战 (challenge) 和其他验证选项
        const response = await fetch(webAuthnClientOptions.getWebAuthnAuthenticateOptions + '?UserId=' + userId);
        const responseData = await response.json();

        const options = responseData.Data;
        // 2. 将 challenge 和允许的凭证ID (allowedCredentials.id) 转换为 Uint8Array
        options.challenge = base64ToUint8Array(options.challenge);
        options.allowCredentials = options.allowCredentials.map(cred => {
            return {
                ...cred,
                id: base64ToUint8Array(cred.id)
            };
        });

        // 3. 调用 navigator.credentials.get() 获取签名
        const assertion = await navigator.credentials.get({
            publicKey: options
        });

        // 4. 获取相关信息
        const authenticatorData = new Uint8Array(assertion.response.authenticatorData);
        const clientDataJSON = new Uint8Array(assertion.response.clientDataJSON);
        const signature = new Uint8Array(assertion.response.signature);
        const rawId = new Uint8Array(assertion.rawId);

        // 5. 将数据发送到服务器进行验证
        const data = {
            rawId: arrayBufferToBase64(rawId),
            type: assertion.type,
            response: {
                authenticatorData: arrayBufferToBase64(authenticatorData),
                clientDataJSON: arrayBufferToBase64(clientDataJSON),
                signature: arrayBufferToBase64(signature),
            },
            flagData: flagData
        };
        return { Success: true, Data: data }
    } catch (err) {
        return { Success: false, Message: err.message, Data: err }
    }
}

function arrayBufferToBase64(buffer) {
    // 创建一个 Uint8Array 视图，表示原始 ArrayBuffer 的字节内容
    const uint8Array = new Uint8Array(buffer);

    // 将每个字节转换为字符，并将它们连接成一个字符串
    let binaryString = '';
    for (let i = 0; i < uint8Array.length; i++) {
        binaryString += String.fromCharCode(uint8Array[i]);
    }

    // 使用 btoa() 将二进制字符串转换为 Base64 编码
    return base64UrlToBase64(btoa(binaryString));
}
function base64ToArrayBuffer(base64) {
    // 使用 atob() 将 Base64 字符串解码为二进制字符串
    const binaryString = atob(base64);

    // 创建一个与二进制字符串长度相同的 ArrayBuffer
    const buffer = new ArrayBuffer(binaryString.length);

    // 创建一个 Uint8Array 视图，表示这个 ArrayBuffer
    const uint8Array = new Uint8Array(buffer);

    // 将二进制字符串的每个字符转换为对应的字节值
    for (let i = 0; i < binaryString.length; i++) {
        uint8Array[i] = binaryString.charCodeAt(i);
    }

    // 返回最终的 ArrayBuffer
    return buffer;
}
function uint8ArrayToBase64(uint8Array) {
    let binaryString = '';
    for (let i = 0; i < uint8Array.length; i++) {
        binaryString += String.fromCharCode(uint8Array[i]);
    }
    return base64UrlToBase64(btoa(binaryString));
}
function base64ToUint8Array(base64) {
    const binaryString = atob(base64);
    const uint8Array = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
        uint8Array[i] = binaryString.charCodeAt(i);
    }
    return uint8Array;
}
function base64UrlToBase64(base64Url) {
    // 1. 替换Base64Url中的特殊字符
    let base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');

    // 2. 计算需要填充的等号数量
    const padding = 4 - (base64.length % 4);
    if (padding !== 4) {
        base64 += '='.repeat(padding);
    }

    return base64;
}

// WebAuthn support script
export async function createWebAuthn(options, createDotNetObjRef) {
    try {
        options.challenge = base64ToUint8Array(options.challenge);
        options.user.id = base64ToUint8Array(options.user.id);

        const credential = await navigator.credentials.create({
            publicKey: options
        });

        const attestationObject = new Uint8Array(credential.response.attestationObject);
        const clientDataJSON = new Uint8Array(credential.response.clientDataJSON);
        const rawId = arrayBufferToBase64(new Uint8Array(credential.rawId));

        const data = {
            rawId: rawId,
            type: credential.type,
            authenticatorAttachment: credential.authenticatorAttachment,
            response: {
                attestationObject: arrayBufferToBase64(attestationObject),
                clientDataJSON: arrayBufferToBase64(clientDataJSON),
            },
        };
        const result = {
            state: true,
            data: data,
            message: 'Success'
        };
        return result;
    }
    catch (err) {
        const result = {
            state: false,
            data: null,
            message: err.message
        };
        return result;
    }
}
export async function authenticateWebAuthn(options, authenticateDotNetObjRef) {
    try {
        options.challenge = base64ToUint8Array(options.challenge);
        options.allowCredentials = options.allowCredentials.map(cred => {
            return {
                ...cred,
                id: base64ToUint8Array(cred.id)
            };
        });

        const assertion = await navigator.credentials.get({
            publicKey: options
        });

        const authenticatorData = new Uint8Array(assertion.response.authenticatorData);
        const clientDataJSON = new Uint8Array(assertion.response.clientDataJSON);
        const signature = new Uint8Array(assertion.response.signature);
        const rawId = new Uint8Array(assertion.rawId);

        const data = {
            rawId: arrayBufferToBase64(rawId),
            type: assertion.type,
            response: {
                authenticatorData: arrayBufferToBase64(authenticatorData),
                clientDataJSON: arrayBufferToBase64(clientDataJSON),
                signature: arrayBufferToBase64(signature),
            },
        };

        const result = {
            state: true,
            data: data,
            message: 'Success'
        };
        return result;
    } catch (err) {
        const result = {
            state: false,
            data: null,
            message: err.message
        };
        return result;
    }
}


export function arrayBufferToBase64(buffer) {
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
export function base64ToArrayBuffer(base64) {
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
export function uint8ArrayToBase64(uint8Array) {
    let binaryString = '';
    for (let i = 0; i < uint8Array.length; i++) {
        binaryString += String.fromCharCode(uint8Array[i]);
    }
    return base64UrlToBase64(btoa(binaryString));
}
export function base64ToUint8Array(base64) {
    const binaryString = atob(base64);
    const uint8Array = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
        uint8Array[i] = binaryString.charCodeAt(i);
    }
    return uint8Array;
}
export function base64UrlToBase64(base64Url) {
    // 1. 替换Base64Url中的特殊字符
    let base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');

    // 2. 计算需要填充的等号数量
    const padding = 4 - (base64.length % 4);
    if (padding !== 4) {
        base64 += '='.repeat(padding);
    }

    return base64;
}
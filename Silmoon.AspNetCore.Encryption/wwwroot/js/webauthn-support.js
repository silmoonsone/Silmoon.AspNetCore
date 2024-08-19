// WebAuthn support script
let webAuthnClientOptions = {
    getWebAuthnOptionsUrl: '/getWebAuthnOptions',
    createWebAuthnUrl: '/createWebAuthn',
    deleteWebAuthnUrl: '/deleteWebAuthn'
}

async function createWebAuthn() {
    try {
        // 1. 向服务器请求创建挑战 (challenge) 和 RP 信息
        const response = await fetch(webAuthnClientOptions.getWebAuthnOptionsUrl);
        const options = (await response.json()).Data;

        console.log("s: " + options.challenge);

        // 2. 将 challenge 和 user.id 转换为 Uint8Array
        options.challenge = Uint8Array.from(atob(options.challenge), c => c.charCodeAt(0));
        options.user.id = Uint8Array.from(atob(options.user.id), c => c.charCodeAt(0));
        options.timeout = 120000;
        console.log("e: " + options.challenge);
        // 3. 创建密钥对
        const credential = await navigator.credentials.create({
            publicKey: options
        });

        // 4. 获取相关信息
        const attestationObject = new Uint8Array(credential.response.attestationObject);
        const clientDataJSON = new Uint8Array(credential.response.clientDataJSON);
        const rawId = new Uint8Array(credential.rawId);

        const data = {
            rawId: Array.from(rawId),
            type: credential.type,
            authenticatorAttachment: credential.authenticatorAttachment,
            response: {
                attestationObject: Array.from(attestationObject),
                clientDataJSON: Array.from(clientDataJSON),
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
            if (responseData.Success) {
                alert('注册成功');
                location.reload();
            } else {
                alert('注册失败 ' + responseData.Message);
            }
        } else {
            alert('注册失败 ERR');
        }
    } catch (err) {
        console.error(err);
        alert('注册过程中出错');
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
                if (responseData.Success) {
                    alert("SUCCESS!");
                    window.location.href = "/User";
                } else {
                    alert("Failed:\r\n" + responseData.Message);
                }
            } else {
                alert("Delete error");
            }
        } catch (err) {
            console.error(err);
            alert("删除过程中出错");
        }
    }
}

async function authenticateWebAuthn(userId) {
    try {
        // 1. 向服务器请求挑战 (challenge) 和其他验证选项
        const response = await fetch('/getWebAuthnAssertionOptions?UserId=' + userId);
        const options = (await response.json()).Data;

        // 2. 将 challenge 和允许的凭证ID (allowedCredentials.id) 转换为 Uint8Array
        options.challenge = Uint8Array.from(atob(options.challenge), c => c.charCodeAt(0));
        options.allowCredentials = options.allowCredentials.map(cred => {
            return {
                ...cred,
                id: Uint8Array.from(atob(cred.id), c => c.charCodeAt(0))
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
            rawId: Array.from(rawId),
            type: assertion.type,
            response: {
                authenticatorData: Array.from(authenticatorData),
                clientDataJSON: Array.from(clientDataJSON),
                signature: Array.from(signature),
            },
        };

        const verificationResponse = await fetch('/verifyWebAuthn', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (verificationResponse.ok) {
            const result = await verificationResponse.json();
            if (result.Success) {
                alert('认证成功');
            } else {
                alert('认证失败: ' + result.Message);
            }
        } else {
            alert('登录失败');
        }
    } catch (err) {
        console.error(err);
        alert('登录过程中出错');
    }
}

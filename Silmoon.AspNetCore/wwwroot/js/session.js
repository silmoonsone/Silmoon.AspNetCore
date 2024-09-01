let silmoonAuthOptions = {
    createSessionUrl: "/_session/createSession",
    clearSessionUrl: "/_session/clearSession"
}

/**
 * 发起创建会话请求
 * @param {string} username
 * @param {string} password
 * @returns {object} StateFlag类型的JSON。
 */
async function doCreateSession(username, password) {
    var formData = new FormData();
    formData.append('Username', username ?? '');
    formData.append('Password', password ?? '');

    try {
        const response = await fetch(silmoonAuthOptions.createSessionUrl, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            return { Success: false, Data: null, Message: await response.text() }
        }
        else {
            const data = await response.json();
            return data;
        }

    } catch (error) {
        console.error(error);
        return { Success: false, Data: null, Message: error.message }
    }
}

/**
 * 清除会话状态
 * @returns
 */
async function doClearSession() {
    try {
        const response = await fetch(silmoonAuthOptions.clearSessionUrl);

        if (!response.ok) {
            return { Success: false, Data: null, Message: await response.text() }
        }

        const data = await response.json();
        return data;

    } catch (error) {
        console.error('发送数据出错:', error);
        return { Success: false, Data: null, Message: error.message }
    }
}
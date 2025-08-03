let silmoonAuthOptions = {
    signInUrl: "/_session/signIn",
    signOutUrl: "/_session/signOut"
}

/**
 * 发起创建会话请求
 * @param {string} username
 * @param {string} password
 * @returns {object} StateResult类型的JSON。
 */
async function doSignIn(username, password) {
    var formData = new FormData();
    formData.append('Username', username ?? '');
    formData.append('Password', password ?? '');

    try {
        const response = await fetch(silmoonAuthOptions.signInUrl, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            return { success: false, data: null, message: await response.text() }
        }
        else {
            const data = await response.json();
            return data;
        }

    } catch (error) {
        console.error(error);
        return { success: false, data: null, message: error.message }
    }
}

/**
 * 清除会话状态
 * @returns
 */
async function doSignOut() {
    try {
        const response = await fetch(silmoonAuthOptions.signOutUrl);

        if (!response.ok) {
            return { success: false, data: null, message: await response.text() }
        }

        const data = await response.json();
        return data;

    } catch (error) {
        console.error('发送数据出错:', error);
        return { success: false, data: null, message: error.message }
    }
}
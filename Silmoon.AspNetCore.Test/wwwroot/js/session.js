/**
 * 发起创建会话请求
 * @param {string} username
 * @param {string} password
 * @returns {object} StateFlag类型的JSON。
 */
async function doCreateSession(username, password) {
    var fetchUrl = "/WebApi/CreateSession";

    // 创建表单数据
    var formData = new FormData();
    formData.append('Username', username);
    formData.append('Password', password);

    // 发送POST请求
    return await fetch(fetchUrl, {
        method: 'POST', // 设置方法为POST
        body: formData // 设置正文为表单数据
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('网络错误，无法联系到服务器');
            }
            return response.json();
        })
        .then(data => {
            return data;
            // 处理返回的数据
        })
        .catch(error => {
            console.error('发送数据出错:', error);
        });
}

/**
 * 清除会话状态
 * @returns
 */
async function doClearSession() {
    var fetchUrl = "/WebApi/ClearSession";
    // 发送POST请求
    return await fetch(fetchUrl)
        .then(response => {
            if (!response.ok) {
                throw new Error('网络错误，无法联系到服务器');
            }
            return response.json();
        })
        .then(data => {
            return data;
            // 处理返回的数据
        })
        .catch(error => {
            console.error('发送数据出错:', error);
        });
}
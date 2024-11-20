const myFile = document.getElementById('myfile')
myFile.addEventListener('change', async function () {
    if (myFile.value != null) {
        const file = myFile.files[0];
        const result = await uploadFile(file, `web/${file.name}`)
        console.log('上传成功：', result)
    }
})
// file：传入的文件 path：本地路径
async function uploadFile(file, path, url, key, authorization, token, cos_file_id) {
    const data = new FormData();
    data.append("key", key);
    data.append("Signature", authorization);
    data.append("x-cos-security-token", token);
    data.append("x-cos-meta-fileid", cos_file_id);
    data.append("file", file, path);
    const fileraw = await request(url, 'POST', data)
    console.log('fileraw:', fileraw)
    return fileraw
}

function request(url, method = 'GET', data = null) {
    return new Promise(function (resolve, reject) {
        const xhr = new XMLHttpRequest();
        xhr.withCredentials = true;
        xhr.addEventListener("readystatechange", function () {
            if (this.readyState === 4) {
                resolve(this.responseText)
            }
        });
        xhr.open(method, url);
        xhr.send(data);
    })
}
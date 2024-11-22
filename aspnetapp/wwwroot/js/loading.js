// 显示加载中
async function loadHTML() {
	document.getElementById('loadding').style.display = 'block';
	setTimeout(() => {
		cancelloadHTML();
	}, 10000);

}
// 取消显示加载中
function cancelloadHTML() {
	document.getElementById('loadding').style.display = 'none';
}

setTimeout(() => {
	cancelloadHTML();
}, 10000);

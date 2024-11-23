// 显示加载中
async function loadHTML() {
	document.getElementById('loadding').style.display = 'block';
	lockPage();
	setTimeout(() => {
		cancelloadHTML();
	}, 10000);

}
// 取消显示加载中
function cancelloadHTML() {
	document.getElementById('loadding').style.display = 'none';
	unlockPage();
}

function lockPage() {
	document.getElementById('overlay').style.opacity = '0.3'; // 设置遮罩层为半透明
	document.getElementById('overlay').style.display = 'block';

}

function unlockPage() {
	document.getElementById('overlay').style.opacity = '0'; // 隐藏遮罩层
	document.getElementById('overlay').style.display = 'none';

}



// 显示消息的方法
function showMessage(elementId, duration = 2000) {
	// 隐藏所有消息
	const messages = document.querySelectorAll('.message');
	messages.forEach((msg) => {
		msg.classList.remove('visible');
		msg.classList.add('hidden');
	});

	// 显示指定消息
	const message = document.getElementById(elementId);
	message.classList.remove('hidden');
	message.classList.add('visible');

	// 自动隐藏消息
	setTimeout(() => {
		message.classList.remove('visible');
		message.classList.add('hidden');
	}, duration);
}
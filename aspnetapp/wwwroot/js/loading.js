// 显示分页提示（支持自定义提示时长）
function showPaginationTip(message, duration = 3000) {
	const tipElement = document.getElementById("paginationTips");
	if (tipElement) {
		tipElement.textContent = message;
		tipElement.style.display = "block";

		// 自动隐藏提示
		setTimeout(() => {
			tipElement.style.display = "none";
		}, duration);
	}
}

// 显示加载中
async function loadHTML(timeout = 10000) {
	const loaddingElement = document.getElementById('loadding');
	if (loaddingElement) {
		loaddingElement.style.display = 'block';
		lockPage();

		// 设置超时自动取消加载
		setTimeout(() => {
			cancelloadHTML();
		}, timeout);
	}
}
// 取消显示加载中
function cancelloadHTML() {
	const loaddingElement = document.getElementById('loadding');
	if (loaddingElement) {
		loaddingElement.style.display = 'none';
	}
	unlockPage();
}

// 锁定页面（显示遮罩层）
function lockPage() {
	const overlay = document.getElementById('overlay');
	if (overlay) {
		overlay.style.opacity = '0.3'; // 设置半透明
		overlay.style.display = 'block'; // 显示遮罩层
	}
}

// 解锁页面（隐藏遮罩层）
function unlockPage() {
	const overlay = document.getElementById('overlay');
	if (overlay) {
		overlay.style.opacity = '0'; // 透明度为 0
		overlay.style.display = 'none'; // 隐藏遮罩层
	}
}



// 显示消息的方法
function showMessage(elementId, duration = 2000) {
	// 隐藏所有消息
	const messages = document.querySelectorAll('.message,.searchMessage');
	messages.forEach((msg) => {
		msg.classList.remove('visible');
		msg.classList.add('hidden');
	});

	// 显示指定消息
	const message = document.getElementById(elementId);
	if (message) {
		message.classList.remove('hidden');
		message.classList.add('visible');

		// 自动隐藏消息
		setTimeout(() => {
			message.classList.remove('visible');
			message.classList.add('hidden');
		}, duration);
	}
}



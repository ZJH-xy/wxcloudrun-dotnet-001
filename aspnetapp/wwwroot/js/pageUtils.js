

// 打开模态窗口并加载点击的图片
function openModal(imgElement) {
	var modal = document.getElementById("imageModal");
	var modalImg = document.getElementById("modalImage");

	// 设置模态窗口中的图片为当前点击的图片
	modal.style.display = "flex"; // 使用 flexbox 居中
	modalImg.src = imgElement.src; // 设置模态窗口图片源
}


// 关闭模态窗口
function closeModal() {
	var modal = document.getElementById("imageModal");
	modal.style.display = "none";
}

// 如果点击模态窗口外部区域，关闭模态窗口
window.onclick = function (event) {
	var modal = document.getElementById("imageModal");
	if (event.target === modal) {
		modal.style.display = "none";
	}
}
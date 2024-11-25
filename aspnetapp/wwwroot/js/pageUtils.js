
const RESPONSEURL = `/Admin/Subpages/UserManagement/UserOverview`;
const TOKEN = document.querySelector('input[name="__RequestVerificationToken"]').value;
const LENGTH = document.getElementById('PageCount').value;
var file;
var fileId;
// 监听每行的文件是否有变化
for (var i = 0; i < 10; i++) {
	document.getElementById('fileInput' + i).addEventListener('change', function (event) {
		if (event.target.files[0]) {
			file = event.target.files[0]; // 获取用户上传的文件
			console.log('myFile', file)
		}
	});
}
// 保存数据
async function saveClick(event) {
	loadHTML(); // 显示加载中

	console.log("开始调用接口...");

	if (!file) {
		console.log("未选择文件"); // 用户没有选择文件不请求
		return;
	} else {
		const response = await fetch(`${RESPONSEURL}?handler=SayHello`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
				'RequestVerificationToken': TOKEN // 添加 CSRF Token
			},
		});

		if (!response.ok) {
			throw new Error(`HTTP 错误！状态码: ${response.status}`);
		}

		const data = await response.json();  // 等待接口返回数据

		if (data.success) {
			console.log("接口返回数据:", data);
		} else {
			alert("文件上传失败: " + (data.message || "未知错误"));
		}

		try {
			const formData = new FormData();
			formData.append("key", data["filePath"]);
			formData.append("Signature", data["authorization"]);
			formData.append("x-cos-security-token", data["token"]);
			formData.append("x-cos-meta-fileid", data["cos_file_id"]);
			formData.append("file", file, file.name);

			const uploadResponse = await uploadFile(data["imageUrl"], formData);  // 异步上传文件
			console.log("Upload success:", uploadResponse);

			setTimeout(function () {
				cancelloadHTML();// 取消显示加载中
			}, 2500)

		} catch (error) {
			console.error("Error during upload:", error);
		}
	}

}

// 加载文件
async function uploadFile(url, data) {
	try {
		const response = await fetch(url, {
			method: 'POST',
			body: data,
		});

		if (!response.ok) {
			throw new Error(`HTTP 错误！状态码: ${response.status}`);
		}

		return;  // 返回服务器的响应
	} catch (error) {
		throw error;
	}
}

// 获取图片url
async function loadImageUrl(event) {
	loadHTML();
	// 使用 event.target 获取被点击的按钮元素
	const button = event.target;

	// 获取 data-id 属性的值
	const fileId = button.getAttribute('data-id');
	const requestData = {
		fileid: fileId
	};
	console.log("requestData...", requestData);

	try {
		const response = await fetch(`${RESPONSEURL}?handler=ImageDownload`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
				'RequestVerificationToken': TOKEN // 添加 CSRF Token
			},
			body: JSON.stringify(requestData), // 将数据序列化为 JSON 格式
		});

		if (!response.ok) {
			throw new Error(`HTTP 错误！状态码: ${response.status}`);
		}

		const data = await response.json();  // 等待接口返回数据

		if (data.success) {
			console.log("接口返回数据:", data);
			// 设置图片路径
			const image = button.parentElement.querySelector('.thumbnail'); // 获取当前行的图片元素
			image.src = data.file_list[0].download_url; // 设置图片路径
			image.style.display = 'block'; // 显示图片
			showMessage("message");
			// 隐藏加载图片按钮
			button.style.display = 'none';
		} else {
			alert("文件上传失败: " + (data.message || "未知错误"));
		}
		cancelloadHTML()

	} catch (error) {
		console.error("API 调用失败:", error);
		cancelloadHTML();
	}
}

// 更改页面
async function changePage(index) {
	loadHTML();
	try {
		const response = await fetch(`${RESPONSEURL}?handler=PageIndex`, {
			method: 'Get',
			headers: {
				'Content-Type': 'application/json',
				'RequestVerificationToken': TOKEN
			},
		});

		if (!response.ok) {
			throw new Error(`HTTP 错误！状态码: ${response.status}`);
		}

		const data = await response.json();  // 等待接口返回数据

		if (data.success) {
			var pageIndex = data.pageIndex;
			const PageIndexTotal = document.getElementById('PageCount').value;
			const PageLimit = document.getElementById('PageLimit').value;
			if (index === 0) {
				pageIndex--;
				if (pageIndex < 1) {
					return;
				}
			} else if (index === 1) {
				pageIndex++;
				if (PageIndexTotal < PageLimit) {
					return;
				}
			} else if (index === 2) {
				pageIndex = 1;

			} else if (index > 1) {
				const pageIndexRedirect = document.getElementById('redirect').value;
				pageIndex = pageIndexRedirect;
			}

			confirmChangePage(pageIndex);
			cancelloadHTML();

		} else {
			alert("失败： " + (data.message || "未知错误"));
		}

	} catch (error) {
		console.error("API 调用失败:", error);
	}
}

// 跳转到尾页
async function changeEndPage(index) {
	try {
		const pageSum = await getPageSum();
		if (pageSum) {
			console.log('pageSum', pageSum);
			confirmChangePage(pageSum);
		}
	} catch (error) {
		console.error('获取总页数失败:', error);
	}
}

// 请求修改页面
async function confirmChangePage(pageIndex) {
	loadHTML();
	const requestData = {
		"PageIndex": pageIndex
	};
	console.log("requestData", requestData);
	try {
		const response = await fetch(`${RESPONSEURL}?handler=ChangePage`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
				'RequestVerificationToken': TOKEN
			},
			body: JSON.stringify(requestData),
		});

		if (!response.ok) {
			throw new Error(`HTTP 错误！状态码: ${response.status}`);
		}
		cancelloadHTML()

	} catch (error) {
		cancelloadHTML()
		console.error("API 调用失败:", error);
	}
}

// 获取当前页面页码
async function getIndexPage() {
	try {
		const response = await fetch(`${RESPONSEURL}?handler=PageIndex`, {
			method: 'Get',
			headers: {
				'Content-Type': 'application/json',
				'RequestVerificationToken': TOKEN
			},
		});

		if (!response.ok) {
			throw new Error(`HTTP 错误！状态码: ${response.status}`);
		}

		const data = await response.json();

		if (data.success) {
			var pageIndex = data.pageIndex;
			document.getElementById('page').textContent = `第 ${pageIndex} 页`;

			return pageIndex;

		} 

	} catch (error) {
		console.error("API 调用失败:", error);
	}
}

// 加载当前页面页码
async function loadIndexPage() {
	const IndexPage = await getIndexPage();
	document.getElementById('page').textContent = `第 ${IndexPage} 页`;
}

// 导出文件
async function exportFile() {
	window.location.href = `${RESPONSEURL}?handler=ExportToExcel`;

}

// 获取总页数
async function getPageSum() {
	loadHTML();
	try {
		const response = await fetch(`${RESPONSEURL}?handler=PageSum`, {
			method: 'GET',
			headers: {
				'Content-Type': 'application/json',
				'RequestVerificationToken': TOKEN
			},
		});
		if (!response.ok) {
			throw new Error(`HTTP 错误！状态码: ${response.status}`);
		}
		const data = await response.json();
		cancelloadHTML();

		if (data.totalPages) {
			return data.totalPages;
		} else {
			throw new Error("失败： " + (data.message || "未知错误"));
		}
	} catch (error) {
		cancelloadHTML(); // 确保在出错时也取消加载动画
		console.error('出错:', error);
		alert("操作过于频繁 " + (error || "未知错误"));
		return null; // 返回 null 表示没有有效结果
	}
}

// 加载总页数
async function loadPageSum() {
	const pageSum = await getPageSum();
	// 更新总页数
	document.getElementById('pageSum').textContent = `共 ${pageSum} 页`;
	document.getElementById('pageSumButton').style.display = 'none';
}



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


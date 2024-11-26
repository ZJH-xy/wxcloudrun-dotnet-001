window.addEventListener('beforeunload', function (e) {
	e.preventDefault(); // 阻止默认行为
	e.returnValue = ''; // Chrome 要求的操作
});
// 加载默认界面
window.onload = function () {
	loadPage("统计数据", document.querySelector('.menu > li > ul > li > a'));
};
function loadPage(pageName, clickedLink) {
	const frame = document.getElementById('contentFrame');


	// 更新 selectedPage 变量
	//这里修改switch的selectedPage的页面代码
	switch (pageName) {
		case "统计数据":
			selectedPage = "Subpages/Overview/StatisticsData";
			break;
		case "车辆总览":
			selectedPage = "Subpages/VehicleManagement/VehicleOverview";
			break;
		case "订单总览":
			selectedPage = "Subpages/OrderManagement/OrderOverview";
			break;
		case "门店总览":
			selectedPage = "Subpages/StoreManagement/StoreOverview";
			break;
		case "申请审核":
			selectedPage = "Subpages/StoreManagement/ApplyForReview";
			break;
		case "用户总览":
			selectedPage = "Subpages/UserManagement/UserOverview";
			break;
		case "广告总览":
			selectedPage = "Subpages/MiniProgramManagement/AdvertisingOverview";
			break;
		case "公告总览":
			selectedPage = "Subpages/MiniProgramManagement/AnnouncementOverview";
			break;
		default:
			selectedPage = "Subpages/UserManagement/UserOverview"; // 默认页面
			break;
	}

	frame.src = selectedPage; // 设置 iframe 的 src
	// 移除所有子菜单项的选中状态
	const allMenuItems = document.querySelectorAll('.menu > li > ul > li > a');
	allMenuItems.forEach(function (menuItem) {
		menuItem.classList.remove('submenuSelected'); // 清除选中状态
	});

	// 为当前点击的子菜单项添加选中状态
	clickedLink.classList.add('submenuSelected');
}


function toggleSubMenu(event) {
	event.preventDefault(); // 阻止默认点击行为
	// 获取当前被点击的主菜单项
	const clickedLink = event.target;
	const submenu = clickedLink.nextElementSibling; // 获取当前链接的子菜单

	// 获取当前子菜单的展开状态
	const isOpen = submenu.classList.contains('show'); // 检查当前子菜单是否已展开

	// 隐藏所有子菜单
	const allSubMenus = document.querySelectorAll('.menu > li > ul');
	allSubMenus.forEach(function (menu) {
		menu.classList.remove('show'); // 移除展开状态
	});

	// 移除所有主菜单的选中状态
	const allLinks = document.querySelectorAll('.menu > li > a');
	allLinks.forEach(function (link) {
		link.classList.remove('selected'); // 移除选中状态
	});

	// 如果当前子菜单未展开，则展开它并添加选中状态
	if (!isOpen) {
		submenu.classList.add('show'); // 展开当前子菜单
		clickedLink.classList.add('selected'); // 添加选中状态
	}

}
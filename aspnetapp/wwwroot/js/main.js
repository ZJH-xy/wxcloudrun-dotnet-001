// 页面卸载前保存路径
window.addEventListener('beforeunload', function (e) {
    e.preventDefault(); // 阻止默认行为
    e.returnValue = ''; // Chrome 要求的操作
});

// 页面路径映射配置
const pageMap = {
    "统计数据": "Subpages/Overview/StatisticsData",
    "车辆总览": "Subpages/VehicleManagement/VehicleOverview",
    "订单总览": "Subpages/OrderManagement/OrderOverview",
    "换车记录": "Subpages/OrderManagement/VehicleReplacementRecord",
    "门店总览": "Subpages/StoreManagement/StoreOverview",
    "套餐总览": "Subpages/StoreManagement/StoreMenu",
    "账号总览": "Subpages/StoreManagement/StoreAccount",
    "申请审核": "Subpages/StoreManagement/ApplyForReview",
    "用户总览": "Subpages/UserManagement/UserOverview",
    "广告总览": "Subpages/MiniProgramManagement/AdvertisingOverview",
    "公告总览": "Subpages/MiniProgramManagement/AnnouncementOverview"
};

// 根据页面名称获取路径
function getPagePath(pageName) {
    return pageMap[pageName] || pageMap["统计数据"]; // 默认页面为 "统计数据"
}

// 加载页面
function loadPage(pageName, clickedLink) {
    const frame = document.getElementById('contentFrame');
    const selectedPage = getPagePath(pageName);

    // 存储当前页面名称到 localStorage
    localStorage.setItem('lastVisitedPage', pageName);

    // 设置 iframe 的 src
    frame.src = selectedPage;

    // 更新菜单项的选中状态
    updateMenuSelection(clickedLink);
}

// 更新菜单项的选中状态
function updateMenuSelection(clickedLink) {
    const allMenuItems = document.querySelectorAll('.menu > li > ul > li > a');
    allMenuItems.forEach(menuItem => menuItem.classList.remove('submenuSelected'));

    if (clickedLink) {
        clickedLink.classList.add('submenuSelected');
    }
}

// 展开对应的最高级菜单
function expandTopLevelMenu(pageName) {
    const allTopLevelMenus = document.querySelectorAll('.menu > li > ul');
    const topLevelLinks = document.querySelectorAll('.menu > li > a');

    allTopLevelMenus.forEach(menu => menu.classList.remove('show'));
    topLevelLinks.forEach(link => {
        link.classList.remove('selected');
        const arrow = link.querySelector('.arrow');
        if (arrow) arrow.classList.remove('active');
    });

    // 查找对应的最高级菜单并展开
    const targetLink = Array.from(document.querySelectorAll('.menu > li > ul > li > a'))
        .find(link => link.innerText.trim() === pageName);

    if (targetLink) {
        const topMenu = targetLink.closest('.menu > li > ul');
        const topLevelLink = targetLink.closest('.menu > li').querySelector('a');

        if (topMenu && topLevelLink) {
            topMenu.classList.add('show');
            topLevelLink.classList.add('selected');
            const arrow = topLevelLink.querySelector('.arrow');
            if (arrow) arrow.classList.add('active');
        }
    }
}

// 页面加载时读取上次访问的路径
window.onload = function () {
    const lastVisitedPage = localStorage.getItem('lastVisitedPage');
    const frame = document.getElementById('contentFrame');
    const defaultPageName = "统计数据";

    if (lastVisitedPage && pageMap[lastVisitedPage]) {
        frame.src = getPagePath(lastVisitedPage);
        expandTopLevelMenu(lastVisitedPage);

        // 设置默认选中项
        const defaultLink = Array.from(document.querySelectorAll('.menu > li > ul > li > a'))
            .find(link => link.innerText.trim() === lastVisitedPage);
        if (defaultLink) {
            updateMenuSelection(defaultLink);
        }
    } else {
        // 加载默认页面并展开对应菜单
        loadPage(defaultPageName, document.querySelector('.menu > li > ul > li > a'));
    }
};

// 切换子菜单
function toggleSubMenu(event) {
    event.preventDefault(); // 阻止默认点击行为

    const clickedLink = event.currentTarget; // 当前被点击的链接
    const submenu = clickedLink.nextElementSibling; // 子菜单
    const arrow = clickedLink.querySelector('.arrow'); // 箭头

    const isOpen = submenu.classList.contains('show'); // 检查是否已展开

    // 隐藏所有子菜单
    document.querySelectorAll('.menu > li > ul').forEach(menu => menu.classList.remove('show'));

    // 重置所有主菜单状态
    document.querySelectorAll('.menu > li > a').forEach(link => {
        link.classList.remove('selected');
        const linkArrow = link.querySelector('.arrow');
        if (linkArrow) {
            linkArrow.classList.remove('active');
        }
    });

    // 展开当前子菜单并更新状态
    if (!isOpen) {
        submenu.classList.add('show');
        clickedLink.classList.add('selected');
        if (arrow) {
            arrow.classList.add('active');
        }
    }
}

// 切换侧边栏
function toggleSidebar() {
    const sidebar = document.querySelector('.sidebar');
    sidebar.classList.toggle('open');
}

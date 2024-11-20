document.addEventListener("DOMContentLoaded", function () {
    // 获取所有文件上传控件
    const fileInputs = document.querySelectorAll(".fileUpload");

    fileInputs.forEach(input => {
        input.addEventListener("change", function () {
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();

                // 读取文件完成后设置预览图片的 src 属性
                reader.onload = function (e) {
                    const preview = input.closest(".imageContainer").querySelector(".preview");
                    preview.src = e.target.result; // 设置预览图像的路径
                    preview.style.display = "block"; // 显示预览图片
                };

                // 读取文件为 Data URL
                reader.readAsDataURL(file);
            }
        });
    });

    // 编辑按钮逻辑
    const editButtons = document.querySelectorAll(".editBtn");
    editButtons.forEach(button => {
        button.addEventListener("click", function () {
            const id = this.dataset.id;
            const container = document.querySelector(`.imageContainer[data-id="${id}"]`);
            const img = container.querySelector(".thumbnail");
            const fileInput = container.querySelector(".fileUpload");

            // 隐藏旧图片
            if (img) img.style.display = "none";

            // 显示文件上传控件
            fileInput.style.display = "block";

            // 切换其他字段为可编辑状态
            const inputs = container.closest("tr").querySelectorAll("input:not(.fileUpload)");
            inputs.forEach(input => input.disabled = false);

            // 切换按钮状态
            this.style.display = "none"; // 隐藏编辑按钮
            container.closest("tr").querySelector(".saveBtn").style.display = "inline-block";
        });
    });
});

document.addEventListener("DOMContentLoaded", function () {
    // 获取所有编辑按钮
    const editButtons = document.querySelectorAll(".editBtn");

    editButtons.forEach(button => {
        button.addEventListener("click", function () {
            const id = this.dataset.id; // 当前行的 ID
            const container = document.querySelector(`.imageContainer[data-id="${id}"]`);
            const img = container.querySelector(".thumbnail");
            const fileInput = container.querySelector(".fileUpload");
            // 隐藏图片
            if (img) {
                img.style.display = "none"; // 隐藏图片
            }
            // 显示文件上传控件
            fileInput.style.display = "block";

            // 切换其他字段为可编辑状态
            const inputs = container.closest("tr").querySelectorAll("input:not(.fileUpload)");
            inputs.forEach(input => input.disabled = false);

            // 切换按钮状态
            this.style.display = "none"; // 隐藏编辑按钮
            container.closest("tr").querySelector(".saveBtn").style.display = "inline-block";
        });
    });
});



// 获取元素
const fileInput = document.getElementById('fileInput');
const customButton = document.getElementById('customButton');
const imagePreview = document.getElementById('imagePreview');
const fileName = document.getElementById('fileName');

// 点击自定义按钮触发文件选择
customButton.addEventListener('click', () => {
    fileInput.click();
});

// 文件选择后展示预览并隐藏按钮
fileInput.addEventListener('change', () => {
    const file = fileInput.files[0];

    if (file) {
        // 显示文件名
        fileName.textContent = file.name;

        // 预览图片
        const reader = new FileReader();
        reader.onload = (e) => {
            imagePreview.src = e.target.result; // 设置图片预览的src
            imagePreview.style.display = 'block'; // 显示图片
        };
        reader.readAsDataURL(file); // 读取文件

        // 隐藏按钮
        customButton.style.display = 'none';
    } else {
        // fileName.textContent = '未选择文件';
        // imagePreview.style.display = 'none'; // 隐藏图片
        // customButton.style.display = 'inline-block'; // 显示按钮
    }
});

// 点击图片重新选择文件
imagePreview.addEventListener('click', () => {
    fileInput.click();
});

// 添加按钮
document.getElementById("addRowBtn").addEventListener("click", function () {
    var newRow = document.getElementById("newRow");
    newRow.style.display = "table-row"; // 显示新增行
});

// 为所有“编辑”按钮添加事件监听
document.addEventListener("click", function (event) {
    if (event.target.classList.contains("editBtn")) {
        // 获取按钮所在行
        var row = event.target.closest("tr");

        // 隐藏当前的“编辑”按钮
        var editBtn = row.querySelector(".editBtn");
        editBtn.style.display = "none";

        // 显示“保存”按钮
        var saveBtn = row.querySelector(".saveBtn");
        saveBtn.style.display = "inline-block";

        // 解锁输入框，使其可编辑
        var inputs = row.querySelectorAll("input[type='text']");
        inputs.forEach(function (input) {
            input.removeAttribute("disabled");
        });
    }
});
// 获取模态窗口和相关元素
var modal = document.getElementById("imageModal");
var modalImage = document.getElementById("modalImage");
var closeBtn = document.getElementsByClassName("close")[0];

// 为所有缩略图绑定点击事件
document.querySelectorAll(".thumbnail").forEach(function (img) {
    img.onclick = function () {
        modal.style.display = "flex"; // 显示模态窗口
        modalImage.src = this.src; // 设置大图的地址为点击的图片地址
    };
});

// 点击关闭按钮时关闭模态窗口
closeBtn.onclick = function () {
    modal.style.display = "none";
};

// 点击模态窗口背景关闭窗口
window.onclick = function (event) {
    if (event.target == modal) {
        modal.style.display = "none";
    }
};

// 确保页面加载时不触发显示
window.onload = function () {
    modal.style.display = "none"; // 确保模态窗口在加载时隐藏
};
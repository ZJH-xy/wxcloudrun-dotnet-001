var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// 激活本地缓存
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<MyDbContext>();
builder.Services.AddScoped<IUserRepository, UserController>();// 依赖注入
builder.Services.AddScoped<IStoreRepository, StoreController>();

// 用于完成 Senparc.Weixin 的注册。
//builder.Services.AddSenparcWeixinServices(builder.Configuration);

//Senparc.Weixin 注册（必须）
builder.Services.AddSenparcWeixin(builder.Configuration);

var app = builder.Build();



//启用微信配置（必须）
var registerService = app.UseSenparcWeixin(app.Environment,
    null /* 不为 null 则覆盖 appsettings  中的 SenpacSetting 配置*/,
    null /* 不为 null 则覆盖 appsettings  中的 SenpacWeixinSetting 配置*/,
    register => { },
    (register, weixinSetting) => {
        //注册公众号信息（可以执行多次，注册多个小程序）
        register.RegisterWxOpenAccount(weixinSetting, "【盛派网络小助手】小程序");
    });


//var registerService = app.UseSenparcWeixin(app.Environment, null, null,
//    register => { },
//    (register, weixinSetting) => { });

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}
//app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error");
//}


//#region 此部分代码为 Sample 共享文件需要而添加，实际项目无需添加
//#if DEBUG
////app.UseStaticFiles(new StaticFileOptions
////{
////    FileProvider = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly(), "wwwroot"),
////    RequestPath = new PathString("")
////});

//app.UseStaticFiles(new StaticFileOptions {
//    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"..", "..", "Shared", "Senparc.Weixin.Sample.Shared", "wwwroot")),
//    RequestPath = new PathString("")
//});
//#endif
//#endregion

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();


//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

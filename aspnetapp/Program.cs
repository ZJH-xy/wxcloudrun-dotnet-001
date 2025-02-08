using aspnetapp.Controllers.API.Miniprogram;
using aspnetapp.Controllers.API.StoreAccount;
using aspnetapp.Controllers.Miniprogram;
using Senparc.Weixin.TenPayV3;

var builder = WebApplication.CreateBuilder(args);

// 注入 DatabaseConfig 实例
builder.Services.AddSingleton(new DatabaseConfig(builder.Configuration));

// 使用 AddDbContext 注册 MyDbContext
builder.Services.AddDbContext<MyDbContext>((serviceProvider, options) => {
    // 获取 DatabaseConfig 实例
    var databaseConfig = serviceProvider.GetRequiredService<DatabaseConfig>();
    // 配置 MySQL 数据库
    databaseConfig.ConfigureMySql(options);
});


// Add services to the container.
builder.Services.AddRazorPages();

// 激活本地缓存
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<MyDbContext>();
builder.Services.AddScoped<ConfigurationService>();
builder.Services.AddScoped<IUserRepository, UserController>();// 依赖注入
builder.Services.AddScoped<IStoreRepository, StoreController>();
builder.Services.AddScoped<IVehicleRepository, VehicleController>();
builder.Services.AddScoped<IOrderRepository, OrderController>();
builder.Services.AddScoped<IStoreAccountRepository, StoreAccountController>();
builder.Services.AddScoped<IFavoritesStoreRepository, FavoritesStoreController>();

builder.Services.AddScoped<OrderAPI>(); // 注册 OrderAPI，定时取消订单
builder.Services.AddHostedService<TimedHostedService>();


builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWT"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
    opt => {
        var jwtSettings = builder.Configuration.GetSection("JWT").Get<JWTSettings>();
        byte[] keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecKey);
        var secKey = new SymmetricSecurityKey(keyBytes);
        opt.TokenValidationParameters = new() {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = secKey
        };
    });


//Senparc.Weixin 注册（必须）
builder.Services.AddSenparcWeixin(builder.Configuration);
// 用于完成 Senparc.Weixin 的注册。
//builder.Services.AddSenparcWeixinServices(builder.Configuration);
// 读取微信配置
builder.Services.Configure<WeixinSetting>(builder.Configuration.GetSection("SenparcWeixinSetting"));// WeixinSetting


// 配置日志
builder.Logging.ClearProviders();                    // 清除默认日志提供程序
builder.Logging.AddConsole();                        // 添加控制台日志
//builder.Logging.AddDebug();                          // 添加 Debug 输出日志（适合调试环境）
//builder.Logging.AddEventLog();                       // Windows 事件日志
builder.Logging.SetMinimumLevel(LogLevel.Information); // 设置最小日志级别

// 还可以从 appsettings.json 中读取配置
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));


var app = builder.Build();



//启用微信配置（必须）
var registerService = app.UseSenparcWeixin(app.Environment,
    null /* 不为 null 则覆盖 appsettings  中的 SenpacSetting 配置*/,
    null /* 不为 null 则覆盖 appsettings  中的 SenpacWeixinSetting 配置*/,
    register => { },
    (register, weixinSetting) => {
        AccessTokenContainer.RegisterAsync(weixinSetting.WxOpenAppId, weixinSetting.WxOpenAppSecret, "小程序");
		//注册公众号信息（可以执行多次，注册多个小程序）
		register.RegisterWxOpenAccount(weixinSetting, "文旅小程序");
		//注册微信支付（可以执行多次，注册多个微信支付）
		register.RegisterTenpayApiV3(weixinSetting, "【盛派网络小助手】微信支付（ApiV3）");
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


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();


//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

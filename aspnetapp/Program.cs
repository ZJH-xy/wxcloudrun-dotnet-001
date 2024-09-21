using aspnetapp.Dao;
using Senparc.Weixin.AspNet;
using Senparc.Weixin.RegisterServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<UserContext>();

// 激活本地缓存
builder.Services.AddMemoryCache();

// 用于完成 Senparc.Weixin 的注册。
builder.Services.AddSenparcWeixinServices(builder.Configuration);

var app = builder.Build();

var registerService = app.UseSenparcWeixin(app.Environment, null, null,
    register => { },
    (register, weixinSetting) => { });


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

app.Run();

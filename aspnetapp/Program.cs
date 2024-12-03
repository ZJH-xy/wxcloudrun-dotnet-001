using aspnetapp.Controllers.API.Miniprogram;
using aspnetapp.Controllers.API.StoreAccount;
using aspnetapp.Controllers.Miniprogram;
using Senparc.Weixin.TenPayV3;
using aspnetapp.Controllers.Web;
using aspnetapp.Dao.RepositoryInterface.Web;
using Senparc.Weixin.AspNet;

var builder = WebApplication.CreateBuilder(args);

// ע�� DatabaseConfig ʵ��
builder.Services.AddSingleton(new DatabaseConfig(builder.Configuration));

// ʹ�� AddDbContext ע�� MyDbContext
builder.Services.AddDbContext<MyDbContext>((serviceProvider, options) => {
	// ��ȡ DatabaseConfig ʵ��
	var databaseConfig = serviceProvider.GetRequiredService<DatabaseConfig>();
	// ���� MySQL ���ݿ�
	databaseConfig.ConfigureMySql(options);
});

// Add services to the container.
builder.Services.AddRazorPages();

// Web��̨���
builder.Services.AddScoped<UserControllerWeb>();
builder.Services.AddScoped<VehicleControllerWeb>();
builder.Services.AddScoped<OrderControllerWeb>();

// ����ػ���
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<MyDbContext>();
builder.Services.AddScoped<IUserRepository, UserController>();// ����ע��
builder.Services.AddScoped<IStoreRepository, StoreController>();
builder.Services.AddScoped<IVehicleRepository, VehicleController>();
builder.Services.AddScoped<IOrderRepository, OrderController>();
builder.Services.AddScoped<IStoreAccountRepository, StoreAccountController>();
builder.Services.AddScoped<IFavoritesStoreRepository, FavoritesStoreController>();

// Web��̨���
builder.Services.AddScoped<UserControllerWeb>();
builder.Services.AddScoped<VehicleControllerWeb>();
builder.Services.AddScoped<StatisticsControllerWeb>();

builder.Services.AddScoped<OrderAPI>(); // ע�� OrderAPI����ʱȡ������
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


//Senparc.Weixin ע�ᣨ���룩
builder.Services.AddSenparcWeixin(builder.Configuration);
// ������� Senparc.Weixin ��ע�ᡣ
builder.Services.Configure<WeixinSetting>(builder.Configuration.GetSection("SenparcWeixinSetting"));// ����WeixinSetting


// ������� Senparc.Weixin ��ע�ᡣ
//builder.Services.AddSenparcWeixinServices(builder.Configuration);


// ������־
builder.Logging.ClearProviders();                    // ���Ĭ����־�ṩ����
builder.Logging.AddConsole();                        // ���ӿ���̨��־
													 //builder.Logging.AddDebug();                          // ���� Debug �����־���ʺϵ��Ի�����
													 //builder.Logging.AddEventLog();                       // Windows �¼���־
builder.Logging.SetMinimumLevel(LogLevel.Information); // ������С��־����

// �����Դ� appsettings.json �ж�ȡ����
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));


var app = builder.Build();



//����΢�����ã����룩
var registerService = app.UseSenparcWeixin(app.Environment,
	null /* ��Ϊ null �򸲸� appsettings  �е� SenpacSetting ����*/,
	null /* ��Ϊ null �򸲸� appsettings  �е� SenpacWeixinSetting ����*/,
	register => { },
	(register, weixinSetting) => {
		//ע�ṫ�ں���Ϣ������ִ�ж�Σ�ע����С����
		register.RegisterWxOpenAccount(weixinSetting, "����С����");
		//ע��΢��֧��������ִ�ж�Σ�ע����΢��֧����
		register.RegisterTenpayApiV3(weixinSetting, "��ʢ������С���֡�΢��֧����ApiV3��");
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


//#region �˲��ִ���Ϊ Sample �����ļ���Ҫ�����ӣ�ʵ����Ŀ��������
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

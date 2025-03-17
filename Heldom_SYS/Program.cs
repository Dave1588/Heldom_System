using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Heldom_SYS.Interface;
using Heldom_SYS.Service;
using Heldom_SYS.Models;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// 配置 JSON 序列化選項（支援中文）
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Encoder = JavaScriptEncoder.Create(
        UnicodeRanges.BasicLatin,
        UnicodeRanges.Cyrillic,
        UnicodeRanges.CjkUnifiedIdeographs);
    options.WriteIndented = true;
});

// 添加 MVC 服務
builder.Services.AddControllersWithViews();

// 添加 role 服務
builder.Services.AddSingleton<IUserStoreService, UserStoreService>();

//PrintCategory
builder.Services.AddScoped<IPrintCategoryService, PrintCategoryService>();
builder.Services.AddSession(); // 啟用 Session

// 資料庫連線
builder.Services.AddDbContext<ConstructionDbContext>(
            options => options.UseSqlServer(builder.Configuration.GetConnectionString("ConstructionDB")));

// 添加身份驗證
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Profile/Login"; // 明確指向 Login 頁面
//        options.AccessDeniedPath = "/Profile/AccessDenied";
//    });

// 從 appsettings.json 中讀取 DefaultConnection 連接字串
string? connectionString = builder.Configuration.GetConnectionString("ConstructionDB");

// 可以將連接字串註冊到依賴注入容器
builder.Services.AddScoped<SqlConnection>(provider => new SqlConnection(connectionString));

var app = builder.Build();

app.UseSession(); // 使用 Session

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}




app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

// 自訂路由
//app.MapControllerRoute(
//    name: "AttendanceById",
//    pattern: "Attendance/{id}",
//    defaults: new { controller = "Attendance", action = "Index" });

//app.MapControllerRoute(
//    name: "profileById",
//    pattern: "profile/{id}",
//    defaults: new { controller = "Profile", action = "Index" });

//app.MapControllerRoute(
//    name: "login",
//    pattern: "Login",
//    defaults: new { controller = "Login", action = "Index" });

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Login}/{action=Index}");
    pattern: "{controller=Login}/{action=Index}");

// 預設路由
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Dashboard}/{action=Dashboard}/{id?}");
//Project   Profile  Index  IssuesDetail
app.Run();

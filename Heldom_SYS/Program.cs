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

// �t�m JSON �ǦC�ƿﶵ�]�䴩����^
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Encoder = JavaScriptEncoder.Create(
        UnicodeRanges.BasicLatin,
        UnicodeRanges.Cyrillic,
        UnicodeRanges.CjkUnifiedIdeographs);
    options.WriteIndented = true;
});

// �K�[ MVC �A��
builder.Services.AddControllersWithViews();

// �K�[ role �A��
builder.Services.AddSingleton<IUserStoreService, UserStoreService>();

//PrintCategory
builder.Services.AddScoped<IPrintCategoryService, PrintCategoryService>();
builder.Services.AddSession(); // �ҥ� Session

// ��Ʈw�s�u
builder.Services.AddDbContext<ConstructionDbContext>(
            options => options.UseSqlServer(builder.Configuration.GetConnectionString("ConstructionDB")));

// �K�[��������
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Profile/Login"; // ���T���V Login ����
//        options.AccessDeniedPath = "/Profile/AccessDenied";
//    });

// �q appsettings.json ��Ū�� DefaultConnection �s���r��
string? connectionString = builder.Configuration.GetConnectionString("ConstructionDB");

// �i�H�N�s���r����U��̿�`�J�e��
builder.Services.AddScoped<SqlConnection>(provider => new SqlConnection(connectionString));

var app = builder.Build();

app.UseSession(); // �ϥ� Session

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

// �ۭq����
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

// �w�]����
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Dashboard}/{action=Dashboard}/{id?}");
//Project   Profile  Index  IssuesDetail
app.Run();

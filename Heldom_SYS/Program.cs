using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

//JSON 字符串 中文資料
var options1 = new JsonSerializerOptions
{
    Encoder = JavaScriptEncoder.Create(
        UnicodeRanges.BasicLatin,
        UnicodeRanges.Cyrillic,
        UnicodeRanges.CjkUnifiedIdeographs),
    WriteIndented = true
};
// Add services to the container.
builder.Services.AddControllersWithViews();

//建立連線
//builder.Services.AddDbContext<XXXXXXXXXXContext>(
//            options => options.UseSqlServer(builder.Configuration.GetConnectionString("XXXXXXXXXXXXXXConnstring")));

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Monitor}/{id?}");

app.Run();

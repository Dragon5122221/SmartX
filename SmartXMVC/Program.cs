using SmartXMVC.Services;
// BOOT-MVC-01: render custom workspaces and route all data operations through the asynchronous API client.
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(o => o.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddHttpClient<SmartXApiClient>(http =>
{
    http.BaseAddress = new Uri(builder.Configuration["SmartXApi:BaseUrl"] ?? "https://localhost:7034/");
    http.Timeout = TimeSpan.FromSeconds(30);
});
var app = builder.Build();
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Index");
if (!app.Configuration.GetValue<bool>("SmartX:AllowHttp")) app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();

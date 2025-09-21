var builder = WebApplication.CreateBuilder(args);

//DotNetEnv.Env.Load();

var apiBaseUrl = Environment.GetEnvironmentVariable("API__BASEURL") ?? "http://localhost:5001";
Console.WriteLine($"API BaseUrl: {apiBaseUrl}"); // ← Обязательно

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization(); // ✅ Вот эта строка нужна

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization(); // Это работает только если ты добавил AddAuthorization()

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
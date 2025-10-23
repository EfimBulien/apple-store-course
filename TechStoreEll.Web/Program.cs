using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using StackExchange.Redis;
using TechStoreEll.Core.Infrastructure.Data;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;
using TechStoreEll.Web.Helpers;
using Prometheus;
using TechStoreEll.Core.Infrastructure.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AppDbContext>(sp =>
{
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var config = sp.GetRequiredService<IConfiguration>();

    string? dbRole;
    string? password;

    if (httpContext?.User?.Identity?.IsAuthenticated == true)
    {
        var jwtRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(jwtRole) || !config.GetSection($"DbRoles:{jwtRole}").Exists())
        {
            Console.WriteLine(jwtRole);
            throw new Exception("Invalid or unsupported user role");
        }

        dbRole = config[$"DbRoles:{jwtRole}:DbRole"];
        password = config[$"DbRoles:{jwtRole}:Password"];
    }
    else
    {
        dbRole = config["DbRoles:Guest:DbRole"] ?? "app_guest";
        password = config["DbRoles:Guest:Password"] ?? throw new Exception("Guest DB password not configured");
    }

    if (string.IsNullOrEmpty(dbRole) || string.IsNullOrEmpty(password))
        throw new Exception("Database role or password missing");

    var connectionString = new NpgsqlConnectionStringBuilder
    {
        Host = config["Db:Host"] ?? "localhost",
        Port = int.Parse(config["Db:Port"] ?? "5432"),
        Database = config["Db:Database"] ?? "TechStoreEll",
        Username = dbRole,
        Password = password,
        Pooling = false
    }.ToString();

    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
    optionsBuilder.UseNpgsql(connectionString);
    return new AppDbContext(optionsBuilder.Options);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

//builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("AuthToken"))
                {
                    context.Token = context.Request.Cookies["AuthToken"];
                }
                return Task.CompletedTask;
            }
        };
        
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<IMinioService, MinioService>();
builder.Services.AddScoped<AddressService>();
builder.Services.AddScoped<ICartService, RedisCartService>();
builder.Services.AddScoped<IRestockService, RestockService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHostedService<MetricsService>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpMetrics();
app.MapMetrics();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseSession();
app.Run();
using Microsoft.EntityFrameworkCore;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(1800);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddDbContext<SafepotDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlDataConnection"))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking), ServiceLifetime.Scoped);

//var connectionString = builder.Configuration.GetConnectionString("SqlDataConnection");
//var serverVersion = ServerVersion.AutoDetect(connectionString);
//builder.Services.AddDbContext<SafepotDbContext>(options =>
//            options.UseMySql(connectionString, serverVersion: serverVersion)
//            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking), ServiceLifetime.Scoped);

builder.Services.AddTransient(typeof(ISfpDataRepository<>), typeof(SfpDataRepository<>));
builder.Services.AddTransient<ISfpUserService, SfpUserService>();
builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<ISfpStateMasterService, SfpStateMasterService>();
builder.Services.AddTransient<ISfpCityMasterService, SfpCityMasterService>();
builder.Services.AddTransient<ISfpActivityLogService, SfpActivityLogService>();
builder.Services.AddTransient<ISfpSubscriptionHistoryService, SfpSubscriptionHistoryService>();
builder.Services.AddTransient<ISfpRoleMasterService, SfpRoleMasterService>();
builder.Services.AddTransient<ISfpMakeModelMasterService, SfpMakeModelMasterService>();
builder.Services.AddTransient<ISfpCompanyService, SfpCompanyService>();
builder.Services.AddTransient<IUserRoleMapService, UserRoleMapService>();
builder.Services.AddTransient<ISfpInvoiceService, SfpInvoiceService>();


builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

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
    pattern: "{controller=Login}/{action=LoginPage}/{id?}");

app.UseSession();

app.Run();

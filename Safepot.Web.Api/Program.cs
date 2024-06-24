using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;
using Quartz;
using Safepot.Business;
using Safepot.Contracts;
using Safepot.DataAccess;
using Safepot.Web.Api.Helpers.Schedulers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
builder.Host.UseNLog();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient();

//builder.Services.AddDbContext<SafepotDbContext>(options =>
//            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlDataConnection"))
//            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking), ServiceLifetime.Scoped);


var connectionString = builder.Configuration.GetConnectionString("SqlDataConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<SafepotDbContext>(options =>
            options.UseMySql(connectionString, serverVersion: serverVersion)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking), ServiceLifetime.Scoped);


builder.Services.AddTransient(typeof(ISfpDataRepository<>), typeof(SfpDataRepository<>));
builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<ISfpActivityLogService, SfpActivityLogService>();
builder.Services.AddTransient<ISfpAgentCustDeliveryMapService, SfpAgentCustDeliveryMapService>();
builder.Services.AddTransient<ISfpAgentCustDlivryChargeService, SfpAgentCustDlivryChargeService>();
builder.Services.AddTransient<ISfpCityMasterService, SfpCityMasterService>();
builder.Services.AddTransient<ISfpCustomerAbsentService, SfpCustomerAbsentService>();
//builder.Services.AddTransient<ISfpCustomizeQuantityService, SfpCustomizeQuantityService>();
builder.Services.AddTransient<ISfpCustomizedQuantityService, SfpCustomizedQuantityService>();
builder.Services.AddTransient<ISfpMakeModelMasterService, SfpMakeModelMasterService>();
builder.Services.AddTransient<ISfpPaymentConfirmationService, SfpPaymentConfirmationService>();
builder.Services.AddTransient<ISfpReturnQuantityService, SfpReturnQuantityService>();
builder.Services.AddTransient<ISfpRoleMasterService, SfpRoleMasterService>();
builder.Services.AddTransient<ISfpStateMasterService, SfpStateMasterService>();
builder.Services.AddTransient<ISfpStockInwardEntryService, SfpStockInwardEntryService>();
builder.Services.AddTransient<ISfpSubscriptionHistoryService, SfpSubscriptionHistoryService>();
builder.Services.AddTransient<ISfpUserService, SfpUserService>();
builder.Services.AddTransient<ISfpCutoffTimeMasterService, SfpCutoffTimeMasterService>();
builder.Services.AddTransient<IUserRoleMapService, UserRoleMapService>();
builder.Services.AddTransient<ISfpPriceMasterService, SfpPriceMasterService>();
builder.Services.AddTransient<ISfpMembershipNotificationService, SfpMembershipNotificationService>();
builder.Services.AddTransient<ISfpMakeMasterService, SfpMakeMasterService>();
builder.Services.AddTransient<ISfpModelMasterService, SfpModelMasterService>();
builder.Services.AddTransient<ISfpUomMasterService, SfpUomMasterService>();
builder.Services.AddTransient<ISfpMappingApprovalService, SfpMappingApprovalService>();
builder.Services.AddTransient<ISfpCustomerQuantityService, SfpCustomerQuantityService>();
builder.Services.AddTransient<ISfpPaymentReminderService, SfpPaymentReminderService>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddTransient<ISfpOrderService, SfpOrderService>();
builder.Services.AddTransient<ISfpSettingService, SfpSettingService>();
builder.Services.AddTransient<ISfpCompanyService, SfpCompanyService>();
builder.Services.AddTransient<ISfpInvoiceService, SfpInvoiceService>();
builder.Services.AddTransient<ISfpCustomerInvoiceService, SfpCustomerInvoiceService>();
builder.Services.AddTransient<ISfpOrderSwitchService, SfpOrderSwitchService>();
builder.Services.AddTransient<ISfpOtpService, SfpOtpService>();



// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})// Adding Jwt Bearer
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
                };
            });

builder.Services.AddQuartz(q =>
{
    // Just use the name of your job that you created in the Jobs folder.
    var jobKey = new JobKey("OrderCreationJob");
    q.AddJob<OrderCreationJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("OrderCreationJob-trigger")
        //This Cron interval can be described as "run every minute" (when second is zero)
        .WithCronSchedule("0 0 1 ? * *")
    );
});

builder.Services.AddQuartz(q =>
{
    // Just use the name of your job that you created in the Jobs folder.
    var jobKey = new JobKey("PendingOrderRejectionJob");
    q.AddJob<OrderCreationJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("PendingOrderRejectionJob-trigger")
        //This Cron interval can be described as "run every minute" (when second is zero)
        .WithCronSchedule("0 0 2 ? * *")
    );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

//app.UseSwagger();
//app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

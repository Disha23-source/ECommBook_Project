using EComm_Project.Data;
using EComm_Project_DataAccess.Repository;
using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Stripe;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("conStr") ?? throw new InvalidOperationException("Connection string 'conStr' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

// to configure the Identity User as well Roles  
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();

//builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
//builder.Services.AddScoped<ICoverTypeRepository, CoverTypeRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
    options.LogoutPath = $"/Identity/Account/Logout";
});

builder.Services.AddAuthentication()
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddLinkedIn(options =>
    {
        options.ClientId = builder.Configuration["Authentication:LinkedIn:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:LinkedIn:ClientSecret"];
    })
    .AddTwitter(options =>
    {
        options.ConsumerKey = builder.Configuration["Authentication:Twitter:ConsumerKey"];
        options.ConsumerSecret = builder.Configuration["Authentication:Twitter:ConsumerSecret"];
    })
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
    });

//Configure the session variables
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
//Configure Stripe Method 
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
//Configure the PayPal Payment Method 
builder.Services.Configure<PayPalSettings>(builder.Configuration.GetSection("PayPalSettings"));
//Configure the Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
//Configure Twillo 
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("TwilioSettings"));
builder.Services.AddScoped<SmsSender>();
builder.Services.AddScoped<ITwilioServices, TwilioServices>();
//Configure RazorPay
builder.Services.Configure<RazorpaySettings>(builder.Configuration.GetSection("RazorpaySettings"));

builder.Services.AddTransient<ISendOtpRepository, SendOtpRepository>();

//Configure the calender code to not enable to select the date before the fromdate (in todate)
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
//Added the APi id in secret.json
var stripeKey = builder.Configuration["StrpieSettings:Secretkey"];
var twilioToken = builder.Configuration["TwilioSettings:AuthToken"];
//MiddleWare - TopDown Approach
var app = builder.Build();
// Configure the HTTP request pipeline.
 if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSession();     // part of session variable configuration
app.UseRouting();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSettings")["Secretkey"]; // part of Stripe Payment Method Configuration

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

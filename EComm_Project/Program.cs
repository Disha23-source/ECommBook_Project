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

// For OAuth of Facebook and Google
builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "948252274667626";
    options.AppSecret = "ddb0019bd605cb5455f6a68ef75d3329";
});

builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = "255880690319-1rort58j57f8kkg3l3lauebniscgaa5j.apps.googleusercontent.com";
    options.ClientSecret = "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";
});

builder.Services.AddAuthentication().AddLinkedIn(options =>
{
    options.ClientId = "863zgbji4sd7tk";
    options.ClientSecret = "WPL_AP1.clZiwDsoyTk5IlGf.aGMkUw==";
});

builder.Services.AddAuthentication().AddTwitter(options =>
{
    options.ConsumerKey = "Gcby86w1bNFYaTAnD25dQwa9C";
    options.ConsumerSecret = "fsP0i2kzRHD6Yh4jaUDzxTQfTDfJL0ky4Pl0LVSPDPhxHf98xZ";
});

builder.Services.AddAuthentication().AddGitHub(options =>
{
    options.ClientId = "Ov23lifTtulz9tfK883T";
    options.ClientSecret = "781d583c24f39b5555119e3eef28c2c69f935378";
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

// This MUST be there — without it GetAll() returns empty due to circular reference
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

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

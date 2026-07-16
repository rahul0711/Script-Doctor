
using Microsoft.AspNetCore.Authentication.Cookies;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.Repositories.Implementations;
using Pharma_Script.Routing;
using Pharma_Script.Services.Interfaces;
using Pharma_Script.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Public tenant website: slug route constraint backed by an in-memory cache
// of active Organization slugs (see Services/Implementations/OrganizationSlugCache.cs).
builder.Services.AddSingleton<IOrganizationSlugCache, OrganizationSlugCache>();
builder.Services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(options =>
{
    options.ConstraintMap.Add("activeOrgSlug", typeof(OrganizationSlugRouteConstraint));
});

// Register ADO.NET Unit of Work with MySQL Connection string
builder.Services.AddScoped<IUnitOfWork>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("WebPortalMYSQLConnection")
        ?? throw new InvalidOperationException("Connection string 'WebPortalMYSQLConnection' not found.");
    return new UnitOfWork(connectionString);
});

// Register BLL Services
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IPatientProvisioningService, PatientProvisioningService>();
builder.Services.AddScoped<IConsultationSessionService, ConsultationSessionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddHttpClient<IPaymentService, PaymentService>();

// Configure Cookie Authentication
// OnRedirectToLogin: when a patient hits a [Authorize(Roles="Patient")] public route,
// we detect the org slug from the returnUrl and send them to /{slug}/login instead of
// the internal /Account/Login (which is for Admin/Doctor/Receptionist only).
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.Events.OnRedirectToLogin = ctx =>
        {
            var returnUrl = ctx.RedirectUri;
            // Extract the returnUrl query param from the redirect URI
            var uri = new Uri(returnUrl);
            var qs = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            if (qs.TryGetValue("ReturnUrl", out var ret))
            {
                var retPath = ret.ToString(); // e.g. /abc-hospital/doctors/3/book
                var parts = retPath.TrimStart('/').Split('/');
                if (parts.Length >= 1)
                {
                    var slugCandidate = parts[0];
                    var slugCache = ctx.HttpContext.RequestServices
                        .GetRequiredService<Pharma_Script.Services.Interfaces.IOrganizationSlugCache>();
                    if (slugCache.IsActiveSlug(slugCandidate))
                    {
                        // Redirect to tenant-branded login, preserving the returnUrl
                        var tenantLogin = $"/{slugCandidate}/login?ReturnUrl={Uri.EscapeDataString(retPath)}";
                        ctx.Response.Redirect(tenantLogin);
                        return Task.CompletedTask;
                    }
                }
            }
            // Fall through to admin login for non-public routes
            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
    });

var app = builder.Build();

// Database Seeding
try
{
    using (var scope = app.Services.CreateScope())
    {
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await Pharma_Script.Helpers.DbInitializer.SeedAsync(uow);

        var slugCache = scope.ServiceProvider.GetRequiredService<IOrganizationSlugCache>();
        await slugCache.RefreshAsync(uow);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database seeding failed: {ex.Message}");
}

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();


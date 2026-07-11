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

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
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


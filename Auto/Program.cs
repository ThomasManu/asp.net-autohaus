using Auto.Data;
using Auto.Models;
using Auto.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Auto
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Bildspeicher-Konfiguration
            var imageFolder = Path.Combine(builder.Environment.ContentRootPath, "UploadedImages");
            builder.Services.Configure<ImageStorageSettings>(options =>
            {
                options.ImageFolder = imageFolder;
            });

            // Verzeichnis erstellen, falls nicht vorhanden
            if (!Directory.Exists(imageFolder))
            {
                Directory.CreateDirectory(imageFolder);
            }

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                 .AddRoles<IdentityRole>()
                 .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();

            // Identity Services konfigurieren
            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@!+";
                options.User.RequireUniqueEmail = false;
            });

            builder.Logging.AddConsole();
            // Eigene Services
            builder.Services.AddScoped<UserService>();

            // Cookies konfigurieren
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            // Session hinzufügen
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Email Services
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

            var app = builder.Build();

            // Datenbank-Migrationen anwenden
            using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

                // Prüfen, ob die Datenbank existiert, bevor versucht wird, sie zu erstellen
                if (!dbContext.Database.CanConnect())
                {
                    Console.WriteLine("Datenbank existiert nicht oder kann keine Verbindung herstellen. Versuche zu erstellen und zu migrieren...");
                    dbContext.Database.Migrate();
                    Console.WriteLine("Datenbank erstellt und Migrationen angewendet.");
                }
                else
                {
                    Console.WriteLine("Datenbank existiert bereits. Wende ausstehende Migrationen an...");
                    dbContext.Database.Migrate(); // Wende Migrationen auch an, wenn sie existiert
                    Console.WriteLine("Ausstehende Migrationen angewendet.");
                }
            }

            // HTTP-Request-Pipeline konfigurieren
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            var serviceProvider = app.Services;
            await CreateRole(serviceProvider, "Admin");
            await CreateRole(serviceProvider, "User");
            await CreateDefaultUser(serviceProvider, "Admin", "admina@inserat.com", "Test1.");

            app.Run();
        }

        private static async Task CreateRole(IServiceProvider serviceProvider, string roleName)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private static async Task CreateDefaultUser(IServiceProvider serviceProvider, string roleName, string username, string passwort)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var user = await userManager.FindByNameAsync(username);

            if (user == null)
            {
                var newUser = new IdentityUser
                {
                    UserName = username,
                    Email = username,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, passwort);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create user {username}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            user = await userManager.FindByNameAsync(username);
            await userManager.AddToRoleAsync(user, roleName);
        }
    }
}
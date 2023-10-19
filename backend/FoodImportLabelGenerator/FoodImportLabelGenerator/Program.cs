using System.Text;
using FoodImportLabelGenerator;
using FoodImportLabelGenerator.Data;
using FoodImportLabelGenerator.Repository;
using FoodImportLabelGenerator.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json")
    .Build();

var appSettings = configuration.GetSection("AppSettings");
var validIssuer = appSettings["ValidIssuer"];
var validAudience = appSettings["ValidAudience"];
var issuerSigningKey = builder.Configuration["UserSecrets:IssuerSigningKey"];

AddServices();
ConfigureSwagger();
AddDbContext();
AddAuthentication();
AddIdentity();

var app = builder.Build(); // Create an instance of a WebApplication

// Configure the HTTP request pipeline.
// Swagger and Swagger UI should be used in a development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.UseDeveloperExceptionPage(); // Handy feature during development. In production: set up a custom exception page
}

// More config steps
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Adds endpoints for controller actions without specifying any routes

//string connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
/*
void InitializeDb()
{
    using var db = new FoodImportLabelGeneratorContext(new ConfigurationManager());
    db.Database.EnsureCreated();
    PrintLabels();
    
    void PrintLabels()
    {
        foreach (var label in db.Labels)
        {
            Console.WriteLine($"{label.Id}, {label.LegalName}");
        }
    }
}

InitializeDb();
*/

app.Run(); // Starts the web server


void AddServices()
{
    // Add services to the container.
    builder.Services.AddControllers(); // Registers Controller classes in the builder

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    
    // Register the repository interfaces and implementations:
    builder.Services.AddSingleton<ILabelRepository, LabelRepository>();
    
    // Add AuthService and TokenService as scoped services:
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITokenService>(provider => new TokenService(issuerSigningKey!, validIssuer!, validAudience!));
    
    /*
    // A way to avoid "xy field is required." and 400 Bad Request (empty strings are allowed)
    builder.Services.AddControllers(
        options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
    */
}

void ConfigureSwagger()
{
    builder.Services.AddSwaggerGen(
        option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
            });
        });
}

void AddDbContext()
{
    builder.Services.AddDbContext<FoodImportLabelGeneratorContext>();
    builder.Services.AddDbContext<UsersContext>();
}

void AddAuthentication()
{
    // JWT token authentication scheme:
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = validIssuer,
                ValidAudience = validAudience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(issuerSigningKey!)
                ),
            };
        });
}

void AddIdentity()
{
    builder.Services
        .AddIdentityCore<IdentityUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddEntityFrameworkStores<UsersContext>();
}
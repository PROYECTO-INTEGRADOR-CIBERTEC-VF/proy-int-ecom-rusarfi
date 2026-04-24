using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e =>
                    string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Campo inválido"
                        : e.ErrorMessage).ToArray());

        return new BadRequestObjectResult(
            RusarfiServer.Dtos.Common.ApiResponse<object>.Fail(
                "Validación fallida",
                errors));
    };
});

// DbContext
builder.Services.AddDbContext<RusarfiServer.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cargar configuración
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// JWT Authentication
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];
        var key = builder.Configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new Exception("JWT Key no está configurada");
        }

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// Services
builder.Services.AddScoped<RusarfiServer.Service.IAuthService, RusarfiServer.Service.AuthService>();
builder.Services.AddScoped<RusarfiServer.Service.ICategoryService, RusarfiServer.Service.CategoryService>();
builder.Services.AddScoped<RusarfiServer.Service.IProductService, RusarfiServer.Service.ProductService>();
builder.Services.AddScoped<RusarfiServer.Service.ICartService, RusarfiServer.Service.CartService>();
builder.Services.AddScoped<RusarfiServer.Service.IProductImageService, RusarfiServer.Service.ProductImageService>();
builder.Services.AddSingleton<RusarfiServer.Service.IJwtTokenService, RusarfiServer.Service.JwtTokenService>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Crear carpeta de imágenes si no existe
var productImagesPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "images", "productos");
Directory.CreateDirectory(productImagesPath);

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
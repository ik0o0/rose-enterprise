using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using backend.database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Environment.IsDevelopment()
        ? builder.Configuration.GetConnectionString("DevConnection")
        : builder.Configuration.GetConnectionString("ProdConnection");

    if (builder.Environment.IsDevelopment())
        options.UseSqlite(connectionString);
    else
        options.UseNpgsql(connectionString);
});

var jwtConfig = builder.Configuration.GetSection("Jwt");
var secretKey = jwtConfig["SecretKey"];
var issuer = jwtConfig["Issuer"];
var audience = jwtConfig["Audience"];

if (secretKey == null || issuer == null || audience == null)
{
    throw new Exception("Missing required configuration: Jwt");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = 404;
        return;
    }
    
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "index.html")
    );
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")
    ),
    RequestPath = "/public"
});

app.Run();

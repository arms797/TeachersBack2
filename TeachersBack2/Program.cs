using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TeachersBack2.Data;
using TeachersBack2.Services;
using Microsoft.Extensions.DependencyInjection; // برای AddNewtonsoftJson

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Authentication (JWT in cookie)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
}).AddJwtBearer("JwtBearer", options =>
{
    var cfg = builder.Configuration;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = cfg["Jwt:Issuer"],
        ValidAudience = cfg["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!))
    };
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var cookieName = builder.Configuration["Jwt:CookieName"];
            if (!string.IsNullOrEmpty(cookieName) && context.Request.Cookies.ContainsKey(cookieName))
            {
                context.Token = context.Request.Cookies[cookieName!];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        // تنظیمات اختیاری
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "TeachersBack2 API", Version = "v1" });
    var securitySchema = new OpenApiSecurityScheme
    {
        Description = "JWT Bearer",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };
    opt.AddSecurityDefinition("Bearer", securitySchema);
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securitySchema, new List<string>() }
    });
});

builder.Services.AddSingleton<JwtService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.MapControllers();

app.Run();

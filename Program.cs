using backend.Models;
using backend.Service;
using backend.Service.Interfaces;
using backend.Services;
using LineLoginBackend.Configurations;
using LineLoginBackend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==============================
// Load config and services
// ==============================
builder.Services.Configure<LineLoginOptions>(builder.Configuration.GetSection("LineLogin"));
builder.Services.Configure<FileSettings>(builder.Configuration.GetSection("FileSettings"));
builder.Services.Configure<PosApiSettings>(builder.Configuration.GetSection("PosAPI"));
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILineLoginService, LineLoginService>();
builder.Services.AddScoped<IPointService, PointService>();
builder.Services.AddScoped<IRedeemService, RedeemService>();
builder.Services.AddScoped<IRewardService, RewardService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IPosService, PosService>();
builder.Services.AddScoped<LineLoginService>();
builder.Services.AddScoped<IPointSyncToPosService, PointSyncToPosService>();
builder.Services.AddScoped<PointService>();
builder.Services.AddScoped<IOtpService, OtpService>();

builder.Services.AddLogging();




builder.Services.AddSignalR();

builder.Services.AddHttpClient("PosApiClient");

// ==============================
// JWT Authentication
// ==============================
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
        // ValidateLifetime = true,
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/coupon"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// ==============================
// DbContext
// ==============================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PosDatabase")));

// ==============================
// CORS Policy
// ==============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(allowedOrigins) // ใส่ URL frontend ของคุณ

.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});


// ==============================
// Swagger + Controllers
// ==============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LineLogin API", Version = "v1" });

    // กำหนด Security Definition สำหรับ JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // กำหนด Security Requirement ให้กับทุก endpoint
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
}); builder.Services.AddControllers();

var app = builder.Build();

// ==============================
// Middlewares
// ==============================

if (app.Environment.IsDevelopment())
{
    // ใช้ Developer Exception Page เฉพาะตอน Dev
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // ✅ ซ่อน error ที่ละเอียดใน Production
    app.UseExceptionHandler("/error");
}


app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication(); // ✅ ก่อน UseAuthorization
app.UseAuthorization();

app.UseStaticFiles(); // ← ต้องมีบรรทัดนี้ เพื่อเปิดใช้งาน wwwroot

app.MapControllers();
app.MapHub<backend.Hubs.CouponHub>("/hubs/coupon");

app.Run();

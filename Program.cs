using BankLite.Api.Data;
using BankLite.Api.Middleware;
using BankLite.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================== SERVICES ==================

// ✅ SECURITY: Enable automatic model validation
builder.Services.AddControllers(options =>
{
    // Suppress automatic 400 response to allow custom error handling
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = false;
});

// ✅ SECURITY: Add global exception handler to prevent information disclosure
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


// DB
builder.Services.AddDbContext<BankLiteDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

// Services
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<AlertService>();
builder.Services.AddScoped<BeneficiaryService>();
builder.Services.AddScoped<IbanService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<ReceiptService>();
builder.Services.AddScoped<TermDepositService>();
builder.Services.AddScoped<TransferService>();
builder.Services.AddScoped<TwoFactorService>();
// ✅ SECURITY: ID Obfuscation Service - prevents ID enumeration attacks
builder.Services.AddScoped<IdObfuscationService>();

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey!)
            )
        };
    });

builder.Services.AddAuthorization();

// Swagger + JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BankLite API",
        Version = "v1"
    });


    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token gir: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ✅ SECURITY: Strict CORS policy - only allow specific origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("Secure", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://localhost:50076",
                "https://localhost:50076")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // If using cookies/authentication
    });
});

// ✅ SECURITY: Add HSTS for production
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

var app = builder.Build();

// ================== PIPELINE ==================

// ✅ SECURITY: Add rate limiting middleware
app.UseMiddleware<RateLimitingMiddleware>();

// ✅ SECURITY: Add global exception handler
app.UseExceptionHandler();

// ✅ SECURITY: Force HTTPS in all environments
app.UseHttpsRedirection();

// ✅ SECURITY: Add HSTS header
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// ✅ SECURITY: Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    // ✅ SECURITY: Content Security Policy - prevents XSS and injection attacks
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self' https:; frame-ancestors 'none'; base-uri 'self'; form-action 'self'");
    // ✅ SECURITY: Permissions Policy - restricts browser features
    context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=(), payment=()");
    await next();
});

// ✅ SECURITY: Use strict CORS policy
app.UseCors("Secure");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

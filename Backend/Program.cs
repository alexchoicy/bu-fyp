using Backend.Data;
using Backend.Data.Seed;
using Backend.Models;
using Backend.Services.AI;
using Backend.Services.Auth;
using Backend.Services.Courses;
using Backend.Services.Facts;
using Backend.Services.Programmes;
using Backend.Services.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


builder.Services.AddDbContextPool<AppDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration["DataBase:ConnectionString"], o => o.UseVector());
});

// User Identity
builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    //is FINE
    opt.Password.RequireDigit = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequiredLength = 4;
}).AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}
).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]!))
    };

    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("fypToken"))
            {
                context.Token = context.Request.Cookies["fypToken"];
            }
            return Task.CompletedTask;
        }
    };
});


builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddSingleton<OpenAIProvider>();
builder.Services.AddSingleton<GeminiProvider>();
builder.Services.AddSingleton<IAIProviderFactory, AIProviderFactory>();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IFactService, FactService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProgrammeService, ProgrammeService>();
builder.Services.AddScoped<IEvaluateRule, EvaluateRule>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

// DB Migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    Console.WriteLine("Database Migrated");

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await UserSeed.SeedAsync(userManager, roleManager);

    var shouldSeedData = builder.Configuration.GetValue<bool?>("SeedData")
                         ?? app.Environment.IsDevelopment();

    if (shouldSeedData)
    {
        Console.WriteLine("Seeding data...");
        var aiProviderFactory = scope.ServiceProvider.GetRequiredService<IAIProviderFactory>();
        await DataSeed.SeedAsync(dbContext, aiProviderFactory);
        Console.WriteLine("Data seeded");
    }
    else
    {
        Console.WriteLine("Data seeding skipped");
    }
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

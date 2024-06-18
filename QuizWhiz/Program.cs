using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using QuizWhiz.Application.Interfaces;
using QuizWhiz.Application.Services;
using QuizWhiz.Domain.Interfaces;
using QuizWhiz.Infrastructure.Data;
using QuizWhiz.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting.Server;
using QuizWhiz.Domain.Helpers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("defaultString")));



builder.Services.AddScoped<QuizWhiz.Domain.Helpers.HashingHelper>();
builder.Services.AddScoped<QuizWhiz.Domain.Helpers.JwtHelper>();
builder.Services.AddScoped<QuizWhiz.Domain.Helpers.EmailSenderHelper>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("*")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
       .AddNewtonsoftJson(options =>
       {
           options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
           options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; // Optional: Ignore null values
       });


builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseExceptionHandler();
app.UseAuthorization();
app.MapControllers();
app.Run();

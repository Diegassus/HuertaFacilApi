using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using HuertaFacilApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.WebHost.UseUrls("http://localhost:5200","http://*:5200");

// Add services to the container.
builder.Services.AddAuthentication().AddJwtBearer(options =>{ // configuracion del token
   options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters{
       ValidateIssuer = true,
       ValidateAudience = true,
       ValidateLifetime = true,
       ValidateIssuerSigningKey = true,
       ValidIssuer = configuration["TokenAuthentication:Issuer"],
       ValidAudience = configuration["TokenAuthentication:Audience"],
       IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(configuration["tokenAuthentication:SecretKey"]))
   };

   options.Events = new JwtBearerEvents{
       OnMessageReceived = context =>{
           var accessToken = context.Request.Query["access_token"];
           var path = context.HttpContext.Request.Path;
           if(!String.IsNullOrEmpty(accessToken)) context.Token = accessToken;
           return Task.CompletedTask;
       }
   };
});
builder.Services.AddAuthorization(options =>{
    options.AddPolicy("Usuario", policy => {policy.RequireClaim(ClaimTypes.Role, "Usuario");});
});
builder.Services.AddControllers();
builder.Services.AddDbContext<DataContext>(opt => opt.UseMySql(
    configuration["ConnectionStrings:MySql"],
    ServerVersion.AutoDetect(configuration["ConnectionStrings:MySql"])
));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

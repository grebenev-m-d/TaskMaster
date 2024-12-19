using AutoMapper;
using CommonLib.Services.EmailServices;
using Microsoft.EntityFrameworkCore;
using TaskMaster.AuthWebApi.Helpers;
using TaskMaster.AuthWebApi.Service.JwtTokenService;
using TaskMaster.DataAccessModule;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.RoleRepository;
using TaskMaster.DataAccessModule.Repository.UserRepository;

// �������� ����� ����������
var builder = WebApplication.CreateBuilder(args);

// ������������ ���������� �� ����� appsettings.json
builder.WebHost.ConfigureAppConfiguration((hostingContext, config) =>
{
	config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
});

// ��������� ������������
var configuration = builder.Configuration;

// ���������� ������������ � ����������
builder.Services.AddControllers();

// ���������� ��������� ���� ������ TaskMasterContext
builder.Services.AddDbContext<TaskMasterContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ���������� ������ ��� API Explorer
builder.Services.AddEndpointsApiExplorer();

// ���������� Swagger
builder.Services.AddSwaggerGen();

// ���������� CORS �������
builder.Services.AddCors();

// ����������� ����������� � �����
builder.Services.AddSingleton<IDbBoardViewHistoryMapRepository, RoleRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IEmailServices, EmailServices>();
builder.Services.AddSingleton<IConfiguration>(configuration);

// �������� � ��������� �����
var app = builder.Build();

// ��������� CORS ��� ���������� �������� � ����������� ����������
app.UseCors(policy =>
{
	policy.WithOrigins("http://localhost:4200") // ����������� �������� ��������
		  .AllowAnyMethod() // ���������� ����� HTTP �������
		  .AllowAnyHeader() // ���������� ����� HTTP ����������
		  .AllowCredentials(); // ���������� �������� ������� ������
});

// ������������� Swagger � ������ ����������
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// ��������������� HTTP �������� �� HTTPS
app.UseHttpsRedirection();

// ������������� �����������
app.UseAuthorization();

// ��������� ��������� ��� ������������
app.MapControllers();

// ������ ����������
app.Run();
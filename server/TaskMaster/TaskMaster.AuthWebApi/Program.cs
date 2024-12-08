using AutoMapper;
using CommonLib.Services.EmailServices;
using Microsoft.EntityFrameworkCore;
using TaskMaster.AuthWebApi.Helpers;
using TaskMaster.AuthWebApi.Service.JwtTokenService;
using TaskMaster.DataAccessModule;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.RoleRepository;
using TaskMaster.DataAccessModule.Repository.UserRepository;

// Создание хоста приложения
var builder = WebApplication.CreateBuilder(args);

// Конфигурация приложения из файла appsettings.json
builder.WebHost.ConfigureAppConfiguration((hostingContext, config) =>
{
	config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
});

// Получение конфигурации
var configuration = builder.Configuration;

// Добавление контроллеров в приложение
builder.Services.AddControllers();

// Добавление контекста базы данных TaskMasterContext
builder.Services.AddDbContext<TaskMasterContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Добавление службы для API Explorer
builder.Services.AddEndpointsApiExplorer();

// Добавление Swagger
builder.Services.AddSwaggerGen();

// Добавление CORS сервиса
builder.Services.AddCors();

// Регистрация репозитория и служб
builder.Services.AddSingleton<IDbBoardViewHistoryMapRepository, RoleRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IEmailServices, EmailServices>();
builder.Services.AddSingleton<IConfiguration>(configuration);

// Создание и настройка хоста
var app = builder.Build();

// Настройка CORS для разрешения запросов с клиентского приложения
app.UseCors(policy =>
{
	policy.WithOrigins("http://localhost:4200") // Разрешенный источник запросов
		  .AllowAnyMethod() // Разрешение любых HTTP методов
		  .AllowAnyHeader() // Разрешение любых HTTP заголовков
		  .AllowCredentials(); // Разрешение отправки учетных данных
});

// Использование Swagger в режиме разработки
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Перенаправление HTTP запросов на HTTPS
app.UseHttpsRedirection();

// Использование авторизации
app.UseAuthorization();

// Установка маршрутов для контроллеров
app.MapControllers();

// Запуск приложения
app.Run();
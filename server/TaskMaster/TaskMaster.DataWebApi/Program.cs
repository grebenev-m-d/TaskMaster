using Microsoft.AspNetCore.ResponseCompression;
using TaskMaster.DataWebApi.AppSetup;
using TaskMaster.DataWebApi.Hubs.BoardHub;
using TaskMaster.DataWebApi.Hubs.BoardAccessLevelHub;
using TaskMaster.DataWebApi.Hubs.CardCommentHub;
using TaskMaster.DataWebApi.Hubs.CardHub;
using TaskMaster.DataWebApi.Hubs.CardListHub;
using TaskMaster.DataWebApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация приложения
builder.WebHost.ConfigureAppConfiguration((hostingContext, config) =>
{
	// Добавление файла конфигурации
	config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
});

// Добавление контроллеров
builder.Services.AddControllers();


// Добавление сжатия ответов
builder.Services.AddResponseCompression(options =>
	options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));

// Добавление политики CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("MyPolicy", policy =>
		policy.WithOrigins("http://localhost:4200")
			  .AllowAnyMethod()
			  .AllowAnyHeader()
			  .AllowCredentials());
});



builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{



	o.TokenValidationParameters = new TokenValidationParameters
	{
		ValidIssuer = builder.Configuration["JwtIssuer"],
		ValidAudience = builder.Configuration["JwtAudience"],
		IssuerSigningKey = new SymmetricSecurityKey
			(Encoding.UTF8.GetBytes(builder.Configuration["JwtSecretKey"])),
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true
	};

	o.Events = new JwtBearerEvents
	{
		OnMessageReceived = context =>
		{
			var accessToken = context.Request.Query["access_token"];
			if (string.IsNullOrEmpty(accessToken) == false)
			{
				context.Token = accessToken;
			}

			return Task.CompletedTask;
		}
	};


});



builder.Services.AddSignalR(options =>
{
	// Установка максимального размера сообщения для SignalR
	options.MaximumReceiveMessageSize = 102400000; // Пример: 100 MB
});
builder.Services.AddControllers();

// Настройка сервисов
ServicesConfig.Add(builder);

// Добавление Swagger
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	// Использование Swagger в режиме разработки
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Использование HTTPS
app.UseHttpsRedirection();

// Использование политики CORS
app.UseCors("MyPolicy");

// Использование сжатия ответов
app.UseResponseCompression();

// Использование аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

// Подключение хабов SignalR к маршруту
app.MapHub<BoardHub>("hub/board");
app.MapHub<CardListHub>("hub/card-list");
app.MapHub<CardHub>("hub/card");
app.MapHub<CardCommentHub>("hub/card-comment");
app.MapHub<BoardAccessLevelHub>("hub/board-permission");

// Маршрутизация к контроллерам
app.MapControllers();

app.Run();

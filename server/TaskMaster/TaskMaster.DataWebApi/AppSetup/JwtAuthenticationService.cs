using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TaskMaster.DataWebApi.AppSetup
{
	/// <summary>
	/// Сервис аутентификации с использованием JWT.
	/// </summary>
	public class JwtAuthenticationService
	{
		/// <summary>
		/// Добавляет настройки аутентификации с использованием JWT в сервисы приложения.
		/// </summary>
		/// <param name="services">Коллекция сервисов приложения.</param>
		/// <param name="configuration">Конфигурационные данные для JWT.</param>
		public static void Add(IServiceCollection services, ConfigurationManager configuration)
		{
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidIssuer = configuration["JwtIssuer"],
						ValidateAudience = true,
						ValidAudience = configuration["JwtAudience"],
						ValidateLifetime = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSecretKey"])),
						ValidateIssuerSigningKey = true
					};
				});
		}
	}
}

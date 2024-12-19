using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TaskMaster.AuthWebApi.Service.JwtTokenService
{
	/// <summary>
	/// Сервис для работы с JWT-токенами.
	/// </summary>
	public class JwtTokenService : IJwtTokenService
	{
		private readonly IConfiguration _configuration;
		private readonly string _jwtSecretKey;
		private readonly string _jwtIssuer;
		private readonly string _jwtAudience;

		/// <summary>
		/// Конструктор класса JwtTokenService.
		/// </summary>
		/// <param name="configuration">Конфигурация приложения.</param>
		public JwtTokenService(IConfiguration configuration)
		{
			_configuration = configuration;
			_jwtSecretKey = configuration["JwtSecretKey"];
			_jwtIssuer = configuration["JwtIssuer"];
			_jwtAudience = configuration["JwtAudience"];
		}

		/// <summary>
		/// Генерирует JWT-токен на основе указанного пейлоада и типа токена.
		/// </summary>
		/// <param name="payload">Пейлоад для токена.</param>
		/// <param name="tokenType">Тип токена (доступа или обновления).</param>
		/// <returns>JWT-токен.</returns>
		public async Task<string> GenerateToken(Models.JwtPayload payload)
		{
			// Получаем ключ подписи
			var signingKey = Encoding.UTF8.GetBytes(_jwtSecretKey);
			var issuer = _jwtIssuer;
			var audience = _jwtAudience;

			// Получаем продолжительность жизни токена
			var tokenLifetime = int.Parse(_configuration["Jwt:AccessLifetime"]);

			// Формируем список утверждений для токена
			var claims = new List<Claim>
		{
			new Claim("keyType", "access"),
			new Claim(JwtRegisteredClaimNames.Sub, payload.Subject),
			new Claim(JwtRegisteredClaimNames.Email, payload.Email),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new Claim("name", payload.Name.ToString()),
			new Claim("role", payload.Role.ToString())
		};

			// Создаем учетные данные для подписи токена
			var credentials = new SigningCredentials(new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha256);

			// Создание токена
			var token = new JwtSecurityToken(
				issuer: issuer,
				audience: audience,
				claims: claims,
				expires: DateTime.Now.AddSeconds(tokenLifetime),
				signingCredentials: credentials
			);

			// Генерация JWT-строки
			var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

			return jwtToken;
		}
	}
}

using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;

namespace TaskMaster.DataWebApi.Models
{
	/// <summary>
	/// Представляет полезную нагрузку (payload) JWT токена.
	/// </summary>
	public class JwtPayload
	{
		/// <summary>
		/// Получает или задает идентификатор субъекта.
		/// </summary>
		public Guid Sub { get; set; }

		/// <summary>
		/// Получает или задает имя пользователя.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Получает или задает адрес электронной почты пользователя.
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Получает или задает роль пользователя.
		/// </summary>
		public string Role { get; set; }

		/// <summary>
		/// Получает или задает время жизни токена.
		/// </summary>
		public DateTime Lifetime { get; set; }

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="JwtPayload"/> из JWT токена.
		/// </summary>
		/// <param name="jwtToken">JWT токен.</param>
		public JwtPayload(string jwtToken)
		{
			var handler = new JwtSecurityTokenHandler();
			var token = handler.ReadJwtToken(jwtToken);

			Sub = Guid.Parse(token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
			Name = token.Claims.First(c => c.Type == "name").Value;
			Email = token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;
			Role = token.Claims.First(c => c.Type == "role").Value;
			Lifetime = token.ValidTo;
		}
	}

}

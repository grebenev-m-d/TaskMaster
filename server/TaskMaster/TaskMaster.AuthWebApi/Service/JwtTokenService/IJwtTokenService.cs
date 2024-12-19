using System.Security.Claims;

namespace TaskMaster.AuthWebApi.Service.JwtTokenService
{
	/// <summary>
	/// Интерфейс для генерации и валидации JWT-токенов.
	/// </summary>
	public interface IJwtTokenService
	{
		/// <summary>
		/// Генерирует JWT-токен на основе указанного пейлоада и типа токена.
		/// </summary>
		/// <param name="payload">Пейлоад для токена.</param>
		/// <returns>JWT-токен.</returns>
		Task<string> GenerateToken(Models.JwtPayload payload);
	}
}

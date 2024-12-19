using TaskMaster.DataAccessModule.Constants;


namespace TaskMaster.AuthWebApi.Models
{
	/// <summary>
	/// Полезная нагрузка JWT-токена.
	/// </summary>
	public class JwtPayload
	{
		/// <summary>
		/// Уникальный идентификатор пользователя.
		/// </summary>
		public string Subject { get; set; }

		/// <summary>
		/// Имя пользователя.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Email пользователя.
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Роль пользователя.
		/// </summary>
		public RoleType Role { get; set; }
	}
}

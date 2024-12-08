using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataWebApi.Models
{
	/// <summary>
	/// Представляет пользователя.
	/// </summary>
	public class User
	{
		/// <summary>
		/// Идентификатор пользователя.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Имя пользователя.
		/// </summary>
		public string Name { get; set; } = null!;

		/// <summary>
		/// Адрес электронной почты пользователя.
		/// </summary>
		public string Email { get; set; } = null!;

		/// <summary>
		/// Дата создания пользователя.
		/// </summary>
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		/// Роль пользователя.
		/// </summary>
		public RoleType Role { get; set; }

		/// <summary>
		/// Список досок, принадлежащих пользователю.
		/// </summary>
		public List<Board> OwnBoards { get; set; } = new List<Board>();
	}
}

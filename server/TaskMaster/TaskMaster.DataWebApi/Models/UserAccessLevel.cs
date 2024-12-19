using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataWebApi.Models
{
	/// <summary>
	/// Уровень доступа пользователя к доске.
	/// </summary>
	public class UserAccessLevel
	{
		/// <summary>
		/// Пользователь.
		/// </summary>
		public User User { get; set; }

		/// <summary>
		/// Уровень доступа.
		/// </summary>
		public AccessLevelType AccessLevel { get; set; }
	}
}

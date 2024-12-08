using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataWebApi.Models
{
	/// <summary>
	/// Уровень доступа к доске.
	/// </summary>
	public class BoardAccessLevel
	{
		/// <summary>
		/// Указывает, является ли доска общедоступной.
		/// </summary>
		public bool IsPublic { get; set; }

		/// <summary>
		/// Список уровней доступа для отдельных пользователей.
		/// </summary>
		public List<UserAccessLevel> PersonalAccessLevels { get; set; }

		/// <summary>
		/// Тип уровня доступа по умолчанию.
		/// </summary>
		public AccessLevelType DefaultAccessLevelType { get; set; }
	}
}

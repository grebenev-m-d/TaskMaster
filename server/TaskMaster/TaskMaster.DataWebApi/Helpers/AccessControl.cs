using TaskMaster.DataAccessModule.Constants;

namespace TaskMaster.DataWebApi.Helpers
{
	/// <summary>
	/// Класс, отвечающий за контроль доступа.
	/// </summary>
	public class AccessControl
	{
		/// <summary>
		/// Проверка уровня доступа пользователя.
		/// </summary>
		/// <param name="userAccessLevel">Уровень доступа пользователя.</param>
		/// <param name="requiredAccessLevel">Требуемый уровень доступа.</param>
		/// <exception cref="UnauthorizedAccessException">Исключение, выбрасываемое при отсутствии доступа.</exception>
		public static void CheckAccessLevel(AccessLevelType? userAccessLevel, AccessLevelType requiredAccessLevel)
		{
			if (userAccessLevel == null || userAccessLevel < requiredAccessLevel)
			{
				throw new UnauthorizedAccessException("Отказано в доступе.");
			}
		}
	}

}

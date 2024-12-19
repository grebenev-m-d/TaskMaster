using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Repository.UserAccessLevelRepository
{
	/// <summary>
	/// Репозиторий для проверки уровня доступа пользователей к данным доски.
	/// </summary>
	public interface IUserAccessLevelRepository
	{
		/// <summary>
		/// Проверяет уровень доступа пользователя к данным доски по их идентификаторам.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Задача, представляющая операцию проверки уровня доступа.</returns>
		public Task<AccessLevelType?> HasBoardDataAccess(Guid userId, Guid boardId);

		/// <summary>
		/// Проверяет уровень доступа списка пользователей к данным доски.
		/// </summary>
		/// <param name="userIds">Список идентификаторов пользователей.</param>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Задача, представляющая операцию проверки уровня доступа для каждого пользователя.</returns>
		public Task<Dictionary<Guid, AccessLevelType?>> HasBoardDataAccess(List<Guid> userIds, Guid boardId);
	}
}

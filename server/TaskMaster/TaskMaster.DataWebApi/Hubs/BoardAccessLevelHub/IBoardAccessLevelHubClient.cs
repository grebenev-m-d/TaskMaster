using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataWebApi.Models;

namespace TaskMaster.DataWebApi.Hubs.BoardAccessLevelHub
{
	/// <summary>
	/// Интерфейс для клиентов Hub'а уровня доступа к доске.
	/// </summary>
	public interface IBoardAccessLevelHubClient
	{
		/// <summary>
		/// Получает уведомление об обновлении публичного статуса доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="isPublic">Новый публичный статус.</param>
		/// <returns>Задача</returns>
		Task ReceiveBoardPublicStatusUpdated(Guid boardId, bool isPublic);

		/// <summary>
		/// Получает уведомление об обновлении стандартного уровня доступа к доске.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="permission">Обновленный уровень доступа.</param>
		/// <returns>Задача</returns>
		Task ReceiveBoardDefaultAccessLevelUpdated(Guid boardId, AccessLevelType permission);

		/// <summary>
		/// Получает уведомление о добавлении персонального уровня доступа.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="user">Пользователь, которому был предоставлен доступ.</param>
		/// <param name="permission">Уровень доступа.</param>
		/// <returns>Задача</returns>
		Task ReceivePersonalAccessLevelAdded(Guid boardId, User user, AccessLevelType permission);

		/// <summary>
		/// Получает уведомление об обновлении персонального уровня доступа.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <param name="permission">Обновленный уровень доступа.</param>
		/// <returns>Задача</returns>
		Task ReceivePersonalAccessLevelUpdated(Guid boardId, Guid userId, AccessLevelType permission);

		/// <summary>
		/// Получает уведомление об удалении персонального уровня доступа.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <returns>Задача</returns>
		Task ReceivePersonalAccessLevelDeleted(Guid boardId, Guid userId);

		/// <summary>
		/// Получает уведомление об обновлении уровня доступа пользователя для этой доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="user">Пользователь, которому был предоставлен доступ.</param>
		/// <param name="permission">Обновленный уровень доступа.</param>
		/// <returns>Задача</returns>
		Task ReceiveUserAccessLevelAdded(Guid boardId, User user, AccessLevelType permission);

		/// <summary>
		/// Получает уведомление об обновлении уровня доступа пользователя для этой доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="permission">Обновленный уровень доступа.</param>
		/// <returns>Задача</returns>
		Task ReceiveUserAccessLevelUpdated(Guid boardId, AccessLevelType? permission);

		/// <summary>
		/// Получает уведомление об удалении уровня доступа пользователя для этой доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="user">Пользователь, уровень доступа которого был удален.</param>
		/// <returns>Задача</returns>
		Task ReceiveUserAccessLevelDeleted(Guid boardId, User user);
	}
}

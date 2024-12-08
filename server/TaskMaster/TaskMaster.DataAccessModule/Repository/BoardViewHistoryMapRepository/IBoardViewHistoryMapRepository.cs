using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Repository.RoleRepository
{
	/// <summary>
	/// Интерфейс репозитория для работы с записями истории просмотра досок.
	/// </summary>
	public interface IBoardViewHistoryMapRepository
	{
		/// <summary>
		/// Получить список записей истории просмотра досок пользователя по его идентификатору.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <returns>Список записей истории просмотра досок пользователя.</returns>
		Task<List<DbBoardViewHistoryMap>> GetAllByUserIdAsync(Guid userId);

		/// <summary>
		/// Добавить новую запись истории просмотра доски.
		/// </summary>
		/// <param name="entity">Сущность записи истории просмотра доски для добавления.</param>
		/// <returns>Добавленная сущность записи истории просмотра доски.</returns>
		Task<DbBoardViewHistoryMap> AddAsync(DbBoardViewHistoryMap entity);

		/// <summary>
		/// Обновить запись истории просмотра доски.
		/// </summary>
		/// <param name="entity">Сущность записи истории просмотра доски для обновления.</param>
		/// <returns>Обновленная сущность записи истории просмотра доски.</returns>
		Task<DbBoardViewHistoryMap> UpdateAsync(DbBoardViewHistoryMap entity);

		/// <summary>
		/// Удалить последнюю запись истории просмотра доски пользователя по его идентификатору.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <returns>Удаленная сущность записи истории просмотра доски.</returns>
		Task<DbBoardViewHistoryMap> DeleteLastByUserId(Guid userId);

		/// <summary>
		/// Удалить запись истории просмотра доски по идентификаторам пользователя и доски.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Удаленная сущность записи истории просмотра доски.</returns>
		Task<DbBoardViewHistoryMap> DeleteByIdAsync(Guid userId, Guid boardId);
	}
}

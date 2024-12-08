using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Repository.BaseRepository
{
	/// <summary>
	/// Интерфейс репозитория для работы с отображениями уровней доступа к доскам.
	/// </summary>
	public interface IBoardAccessLevelMapRepository
	{
		/// <summary>
		/// Получить запись отображения уровней доступа к доскам по идентификаторам доски и пользователя.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <returns>Сущность, если найдена, иначе null.</returns>
		Task<DbBoardAccessLevelMap> GetByIdAsync(Guid boardId, Guid userId);

		/// <summary>
		/// Добавить новую запись отображения уровней доступа к доскам.
		/// </summary>
		/// <param name="entity">Добавляемая сущность.</param>
		/// <returns>Добавленная сущность.</returns>
		Task<DbBoardAccessLevelMap> AddAsync(DbBoardAccessLevelMap entity);

		/// <summary>
		/// Обновить запись отображения уровней доступа к доскам.
		/// </summary>
		/// <param name="entity">Обновляемая сущность.</param>
		/// <returns>Обновленная сущность.</returns>
		Task<DbBoardAccessLevelMap> UpdateAsync(DbBoardAccessLevelMap entity);

		/// <summary>
		/// Удалить запись отображения уровней доступа к доскам по идентификаторам доски и пользователя.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <returns>Удаленная сущность.</returns>
		Task<DbBoardAccessLevelMap> DeleteByIdAsync(Guid boardId, Guid userId);
	}

}

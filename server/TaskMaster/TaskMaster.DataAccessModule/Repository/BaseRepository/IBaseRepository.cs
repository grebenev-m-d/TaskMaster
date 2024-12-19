using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Repository.BaseRepository
{
	/// <summary>
	/// Интерфейс базового репозитория для работы с сущностями базы данных.
	/// </summary>
	/// <typeparam name="T">Тип сущности, наследующей от DbBaseEntity.</typeparam>
	public interface IBaseRepository<T> where T 
		: DbBaseEntity
	{
		/// <summary>
		/// Получить сущность по идентификатору с загрузкой связанных сущностей.
		/// </summary>
		/// <param name="id">Идентификатор сущности.</param>
		/// <returns>Сущность с загруженными связанными сущностями, если найдена, иначе null.</returns>
		public Task<T> GetByIdAsyncIncludes(Guid id);

		/// <summary>
		/// Получить сущность по идентификатору.
		/// </summary>
		/// <param name="id">Идентификатор сущности.</param>
		/// <returns>Сущность, если найдена, иначе null.</returns>
		Task<T> GetByIdAsync(Guid id);

		/// <summary>
		/// Получить все сущности.
		/// </summary>
		/// <returns>Список всех сущностей.</returns>
		public Task<List<T>> GetAllAsync();

		/// <summary>
		/// Добавить новую сущность.
		/// </summary>
		/// <param name="entity">Добавляемая сущность.</param>
		/// <returns>Добавленная сущность.</returns>
		public Task<T> AddAsync(T entity);

		/// <summary>
		/// Обновить существующую сущность.
		/// </summary>
		/// <param name="entity">Обновляемая сущность.</param>
		/// <returns>Обновленная сущность.</returns>
		public Task<T> UpdateAsync(T entity);

		/// <summary>
		/// Удалить сущность по идентификатору.
		/// </summary>
		/// <param name="id">Идентификатор удаляемой сущности.</param>
		/// <returns>Удаленная сущность.</returns>
		public Task<T> DeleteByIdAsync(Guid id);
	}
}

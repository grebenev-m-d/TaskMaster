using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Repository.BaseRepository
{
	/// <summary>
	/// Базовый репозиторий для работы с сущностями базы данных.
	/// </summary>
	/// <typeparam name="T">Тип сущности, наследующей от DbBaseEntity.</typeparam>
	public class BaseRepository<T> : IBaseRepository<T> where T : DbBaseEntity
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Конструктор базового репозитория.
		/// </summary>
		/// <param name="serviceProvider">Провайдер служб для доступа к контексту базы данных.</param>
		public BaseRepository(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Получить сущность по идентификатору с загрузкой связанных сущностей.
		/// </summary>
		/// <param name="id">Идентификатор сущности.</param>
		/// <returns>Сущность с загруженными связанными сущностями, если найдена, иначе null.</returns>
		public virtual async Task<T> GetByIdAsyncIncludes(Guid id)
		{
			// Создаем область видимости для создания новой области использования служб
			using (var scope = _serviceProvider.CreateScope())
			{
				// Получаем экземпляр контекста базы данных из служб
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				// Возвращаем первую сущность из базы данных,
				// удовлетворяющую указанному условию и загружаем все связанные сущности
				return await dbContext.Set<T>().FirstOrDefaultAsync(i => i.Id == id);
			}
		}

		/// <summary>
		/// Получить сущность по идентификатору.
		/// </summary>
		/// <param name="id">Идентификатор сущности.</param>
		/// <returns>Сущность, если найдена, иначе null.</returns>
		public virtual async Task<T> GetByIdAsync(Guid id)
		{
			// Создаем область видимости для создания новой области использования служб
			using (var scope = _serviceProvider.CreateScope())
			{
				// Получаем экземпляр контекста базы данных из служб
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				// Возвращаем сущность из базы данных, если найдена с указанным идентификатором
				return await dbContext.Set<T>().FirstOrDefaultAsync(i => i.Id == id);
			}
		}

		/// <summary>
		/// Получить все сущности.
		/// </summary>
		/// <returns>Список всех сущностей.</returns>
		public virtual async Task<List<T>> GetAllAsync()
		{
			// Создаем область видимости для создания новой области использования служб
			using (var scope = _serviceProvider.CreateScope())
			{
				// Получаем экземпляр контекста базы данных из служб
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				// Возвращаем список всех сущностей из базы данных
				return await dbContext.Set<T>().ToListAsync();
			}
		}

		/// <summary>
		/// Добавить новую сущность.
		/// </summary>
		/// <param name="entity">Добавляемая сущность.</param>
		/// <returns>Добавленная сущность.</returns>
		public virtual async Task<T> AddAsync(T entity)
		{
			// Создаем область видимости для создания новой области использования служб
			using (var scope = _serviceProvider.CreateScope())
			{
				// Получаем экземпляр контекста базы данных из служб
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				// Добавляем новую сущность в базу данных
				await dbContext.Set<T>().AddAsync(entity);
				// Сохраняем изменения в базе данных
				await dbContext.SaveChangesAsync();

				// Возвращаем добавленную сущность
				return entity;
			}
		}

		/// <summary>
		/// Обновить существующую сущность.
		/// </summary>
		/// <param name="entity">Обновляемая сущность.</param>
		/// <returns>Обновленная сущность.</returns>
		public virtual async Task<T> UpdateAsync(T entity)
		{
			// Создаем область видимости для создания новой области использования служб
			using (var scope = _serviceProvider.CreateScope())
			{
				// Получаем экземпляр контекста базы данных из служб
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				// Обновляем сущность в базе данных
				dbContext.Set<T>().Update(entity);
				// Сохраняем изменения в базе данных
				await dbContext.SaveChangesAsync();

				// Возвращаем обновленную сущность
				return entity;
			}
		}

		/// <summary>
		/// Удалить сущность по идентификатору.
		/// </summary>
		/// <param name="id">Идентификатор удаляемой сущности.</param>
		/// <returns>Удаленная сущность.</returns>
		public virtual async Task<T> DeleteByIdAsync(Guid id)
		{
			// Создаем область видимости для создания новой области использования служб
			using (var scope = _serviceProvider.CreateScope())
			{
				// Получаем экземпляр контекста базы данных из служб
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				// Находим сущность в базе данных по указанному идентификатору
				var entity = await dbContext.Set<T>().FindAsync(id);

				// Если сущность найдена
				if (entity != null)
				{
					// Удаляем сущность из контекста базы данных
					dbContext.Set<T>().Remove(entity);
					// Сохраняем изменения в базе данных
					await dbContext.SaveChangesAsync();
				}

				// Возвращаем удаленную сущность или null, если сущность не найдена
				return entity;
			}
		}
	}

}

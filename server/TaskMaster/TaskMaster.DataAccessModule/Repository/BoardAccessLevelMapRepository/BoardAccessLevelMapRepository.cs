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
	/// Репозиторий для работы с отображениями уровней доступа к доскам.
	/// </summary>
	public class BoardAccessLevelMapRepository : IBoardAccessLevelMapRepository
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Конструктор репозитория для отображений уровней доступа к доскам.
		/// </summary>
		/// <param name="serviceProvider">Провайдер служб для доступа к контексту базы данных.</param>
		public BoardAccessLevelMapRepository(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Добавить новую запись отображения уровней доступа к доскам.
		/// </summary>
		/// <param name="entity">Добавляемая сущность.</param>
		/// <returns>Добавленная сущность.</returns>
		public async Task<DbBoardAccessLevelMap> AddAsync(DbBoardAccessLevelMap entity)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				await dbContext.BoardAccessLevelMaps.AddAsync(entity);
				await dbContext.SaveChangesAsync();

				return entity;
			}
		}

		/// <summary>
		/// Удалить запись отображения уровней доступа к доскам по идентификаторам доски и пользователя.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <returns>Удаленная сущность.</returns>
		public async Task<DbBoardAccessLevelMap> DeleteByIdAsync(Guid boardId, Guid userId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				var boardAccessLevelMaps = await dbContext.BoardAccessLevelMaps
					.Include(b => b.User)
					.Include(b => b.AccessLevel)
					.Include(b => b.Board)
					.FirstOrDefaultAsync(b => b.BoardId == boardId && b.UserId == userId);

				if (boardAccessLevelMaps != null)
				{
					dbContext.BoardAccessLevelMaps.Remove(boardAccessLevelMaps);
				}

				await dbContext.SaveChangesAsync();

				return boardAccessLevelMaps;
			}
		}

		/// <summary>
		/// Получить запись отображения уровней доступа к доскам по идентификаторам доски и пользователя.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <returns>Сущность, если найдена, иначе null.</returns>
		public async Task<DbBoardAccessLevelMap> GetByIdAsync(Guid boardId, Guid userId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.BoardAccessLevelMaps
					.Include(b => b.User)
					.Include(b => b.AccessLevel)
					.Include(b => b.Board)
					.FirstOrDefaultAsync(b => b.BoardId == boardId && b.UserId == userId);
			}
		}

		/// <summary>
		/// Обновить запись отображения уровней доступа к доскам.
		/// </summary>
		/// <param name="entity">Обновляемая сущность.</param>
		/// <returns>Обновленная сущность.</returns>
		public async Task<DbBoardAccessLevelMap> UpdateAsync(DbBoardAccessLevelMap entity)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				entity.User = dbContext.Users.FirstOrDefault(i => i.Id == entity.UserId);
				entity.Board = dbContext.Boards.FirstOrDefault(i => i.Id == entity.BoardId);
				entity.AccessLevel = dbContext.AccessLevels.FirstOrDefault(i => i.Id == entity.AccessLevelId);

				dbContext.BoardAccessLevelMaps.Update(entity);
				await dbContext.SaveChangesAsync();

				return entity;
			}
		}
	}

}

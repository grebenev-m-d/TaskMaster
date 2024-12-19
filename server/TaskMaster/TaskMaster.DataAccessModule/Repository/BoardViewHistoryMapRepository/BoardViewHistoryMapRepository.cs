using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskMaster.DataAccessModule.Repository.BaseRepository;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using System.Collections.Generic;

namespace TaskMaster.DataAccessModule.Repository.RoleRepository
{
	/// <summary>
	/// Репозиторий для работы с записями истории просмотра досок.
	/// </summary>
	public class BoardViewHistoryMapRepository : IBoardViewHistoryMapRepository
	{
		/// <summary>
		/// Поставщик сервисов для доступа к контексту данных.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Инициализирует новый экземпляр класса BoardViewHistoryMapRepository с указанным поставщиком сервисов.
		/// </summary>
		/// <param name="serviceProvider">Поставщик сервисов.</param>
		public BoardViewHistoryMapRepository(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Получить список записей истории просмотра досок пользователя по его идентификатору.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <returns>Список записей истории просмотра досок пользователя.</returns>
		public async Task<List<DbBoardViewHistoryMap>> GetAllByUserIdAsync(Guid userId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.BoardViewHistoryMaps
					.Include(i => i.User)
					.Include(i => i.Board)
						.ThenInclude(b => b.User)
					.Include(i => i.Board)
						.ThenInclude(b => b.ImageFile)
					.Include(i => i.Board)
						.ThenInclude(b => b.DesignType)
					.Include(i => i.Board)
						.ThenInclude(b => b.GeneralAccessLevel)
					.Include(i => i.Board)
						.ThenInclude(b => b.CardLists)
					.Include(i => i.Board)
						.ThenInclude(b => b.BoardAccessLevelMaps)
							.ThenInclude(b => b.User)
						.ThenInclude(b => b.BoardAccessLevelMaps)
							.ThenInclude(b => b.AccessLevel)
					.Where(i => i.UserId == userId).ToListAsync();
			}
		}

		/// <summary>
		/// Обновить запись истории просмотра доски.
		/// </summary>
		/// <param name="entity">Сущность записи истории просмотра доски для обновления.</param>
		/// <returns>Обновленная сущность записи истории просмотра доски.</returns>
		public virtual async Task<DbBoardViewHistoryMap> UpdateAsync(DbBoardViewHistoryMap entity)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();
				dbContext.BoardViewHistoryMaps.Update(entity);
				await dbContext.SaveChangesAsync();

				return entity;
			}
		}

		/// <summary>
		/// Добавить новую запись истории просмотра доски.
		/// </summary>
		/// <param name="entity">Сущность записи истории просмотра доски для добавления.</param>
		/// <returns>Добавленная сущность записи истории просмотра доски.</returns>
		public async Task<DbBoardViewHistoryMap> AddAsync(DbBoardViewHistoryMap entity)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();
				await dbContext.BoardViewHistoryMaps.AddAsync(entity);
				await dbContext.SaveChangesAsync();

				return entity;
			}
		}

		/// <summary>
		/// Удалить последнюю запись истории просмотра доски пользователя по его идентификатору.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <returns>Удаленная сущность записи истории просмотра доски.</returns>
		public async Task<DbBoardViewHistoryMap> DeleteLastByUserId(Guid userId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();
				var entity = await dbContext.BoardViewHistoryMaps
					.Where(i => i.UserId == userId)
					.OrderBy(i => i.LastViewedAt)
					.LastOrDefaultAsync();

				if (entity != null)
				{
					dbContext.BoardViewHistoryMaps.Remove(entity);
					await dbContext.SaveChangesAsync();
				}

				return entity;
			}
		}

		/// <summary>
		/// Удалить запись истории просмотра доски по идентификаторам пользователя и доски.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Удаленная сущность записи истории просмотра доски.</returns>
		public async Task<DbBoardViewHistoryMap> DeleteByIdAsync(Guid userId, Guid boardId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();
				var boardViewHistoryMaps = await dbContext.BoardViewHistoryMaps
					.FirstOrDefaultAsync(b => b.BoardId == boardId && b.UserId == userId);

				if (boardViewHistoryMaps != null)
				{
					dbContext.BoardViewHistoryMaps.Remove(boardViewHistoryMaps);
				}

				await dbContext.SaveChangesAsync();

				return boardViewHistoryMaps;
			}
		}
	}
}

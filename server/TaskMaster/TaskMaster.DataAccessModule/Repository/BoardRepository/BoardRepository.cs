using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;

namespace TaskMaster.DataAccessModule.Repository.BoardRepository
{
	/// <summary>
	/// Репозиторий для работы с досками.
	/// </summary>
	public class BoardRepository 
		: BaseRepository<DbBoard>, IBoardRepository
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Конструктор репозитория для работы с досками.
		/// </summary>
		/// <param name="serviceProvider">Провайдер служб для доступа к контексту базы данных.</param>
		public BoardRepository(IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Получить доску по идентификатору с загрузкой связанных сущностей.
		/// </summary>
		/// <param name="id">Идентификатор доски.</param>
		/// <returns>Доска с загруженными связанными сущностями, если найдена, иначе null.</returns>
		public override async Task<DbBoard> GetByIdAsyncIncludes(Guid id)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Boards
					.Include(b => b.User)
					.Include(b => b.ImageFile)
					.Include(b => b.DesignType)
					.Include(b => b.GeneralAccessLevel)
					.Include(b => b.CardLists)
					.Include(b=>b.BoardAccessLevelMaps)
						.ThenInclude(b=>b.User)
					.Include(b => b.BoardAccessLevelMaps)
						.ThenInclude(b => b.AccessLevel)
					.FirstOrDefaultAsync(b => b.Id == id);
			}
		}

		/// <summary>
		/// Получить доску по идентификатору списка карточек.
		/// </summary>
		/// <param name="cardListId">Идентификатор списка карточек.</param>
		/// <returns>Доска, если найдена, иначе null.</returns>
		public async Task<DbBoard> GetBoardByCardListIdAsync(Guid cardListId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Boards
					.Include(b => b.User)
					.Include(b => b.ImageFile)
					.Include(b => b.DesignType)
					.Include(b => b.GeneralAccessLevel)
					.Include(b => b.CardLists)
					.FirstOrDefaultAsync(i => i.CardLists
						.Any(i => i.Id == cardListId));
			}
		}

		/// <summary>
		/// Получить доску по идентификатору карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <returns>Доска, если найдена, иначе null.</returns>
		public async Task<DbBoard> GetBoardByCardIdAsync(Guid cardId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Boards
					.Include(b => b.User)
					.Include(b => b.ImageFile)
					.Include(b => b.DesignType)
					.Include(b => b.GeneralAccessLevel)
					.Include(b => b.CardLists)
					.FirstOrDefaultAsync(i => i.CardLists
						.Any(i => i.Cards
							.Any(j => j.Id == cardId)));
			}
		}

		/// <summary>
		/// Получить доску по идентификатору комментария карточки.
		/// </summary>
		/// <param name="commentId">Идентификатор комментария карточки.</param>
		/// <returns>Доска, если найдена, иначе null.</returns>
		public async Task<DbBoard> GetBoardByCardCommentIdAsync(Guid commentId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Boards
					.Include(b => b.User)
					.Include(b => b.ImageFile)
					.Include(b => b.DesignType)
					.Include(b => b.GeneralAccessLevel)
					.Include(b => b.CardLists)
					.FirstOrDefaultAsync(i => i.CardLists
						.Any(i => i.Cards
							.Any(j => j.CardComments
								.Any(k => k.Id == commentId))));
			}
		}

		/// <summary>
		/// Получить список досок, принадлежащих определенному пользователю.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <returns>Список досок пользователя.</returns>
		public async Task<List<DbBoard>> GetAllByUserIdAsync(Guid userId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Boards
					.Include(b => b.User)
					.Include(b => b.ImageFile)
					.Include(b => b.DesignType)
					.Include(b => b.GeneralAccessLevel)
					.Include(b => b.CardLists)
					.Where(b => b.UserId == userId)
					.ToListAsync();
			}
		}

		/// <summary>
		/// Добавить новую доску.
		/// </summary>
		/// <param name="entity">Добавляемая доска.</param>
		/// <returns>Добавленная доска.</returns>
		public override async Task<DbBoard> AddAsync(DbBoard entity)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				await dbContext.Boards.AddAsync(entity);
				await dbContext.SaveChangesAsync();

				return entity;
			}
		}

		/// <summary>
		/// Обновить существующую доску.
		/// </summary>
		/// <param name="entity">Обновляемая доска.</param>
		/// <returns>Обновленная доска.</returns>
		public virtual async Task<DbBoard> UpdateAsync(DbBoard entity)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				var board = await dbContext.Boards.Include(b => b.User)
												   .Include(b => b.ImageFile)
												   .Include(b => b.DesignType)
												   .Include(b => b.GeneralAccessLevel)
												   .FirstOrDefaultAsync(i => i.Id == entity.Id);

				if (board != null)
				{
					board.Title = entity.Title;
					board.CreatedAt = entity.CreatedAt;
					board.UserId = entity.UserId;
					board.DesignTypeId = entity.DesignTypeId;
					board.ColorCode = entity.ColorCode;
					board.ImageFileId = entity.ImageFileId;
					board.IsPublic = entity.IsPublic;
					board.GeneralAccessLevelId = entity.GeneralAccessLevelId;

					dbContext.Boards.Update(board);
					await dbContext.SaveChangesAsync();
				}

				return board;
			}
		}

		/// <summary>
		/// Удалить доску по идентификатору.
		/// </summary>
		/// <param name="id">Идентификатор удаляемой доски.</param>
		/// <returns>Удаленная доска.</returns>
		public override async Task<DbBoard> DeleteByIdAsync(Guid id)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				var board = await dbContext.Boards
					.Include(i => i.BoardAccessLevelMaps)
					.Include(i => i.BoardViewHistoryMaps)
					.Include(i => i.CardLists)
					.FirstOrDefaultAsync(i => i.Id == id);

				if (board != null)
				{

					if (board.BoardViewHistoryMaps != null)
					{
						dbContext.BoardViewHistoryMaps.RemoveRange(board.BoardViewHistoryMaps);
					}

					if (board.BoardAccessLevelMaps != null)
					{
						dbContext.BoardAccessLevelMaps.RemoveRange(board.BoardAccessLevelMaps);
					}

					dbContext.Boards.Remove(board);
					await dbContext.SaveChangesAsync();
				}

				return board;
			}
		}

		/// <summary>
		/// Получить список досок от владельца, для которых у приглашенного пользователя есть персональный уровень доступа.
		/// </summary>
		/// <param name="invitedUserId">Идентификатор приглашенного пользователя.</param>
		/// <param name="userOwnerId">Идентификатор владельца досок.</param>
		/// <returns>Список досок от владельца с персональным доступом для указанного пользователя.</returns>
		public async Task<List<DbBoard>> GetBoardsFromOwnerWithPersonalAccessLevelForUser(Guid invitedUserId, Guid userOwnerId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Boards
					.Include(b => b.User)
					.Include(b => b.ImageFile)
					.Include(b => b.DesignType)
					.Include(b => b.GeneralAccessLevel)
					.Include(b => b.CardLists)
					.Where(i => i.UserId == userOwnerId)
					.Where(j => j.BoardAccessLevelMaps
						.Any(k => k.User.Id == invitedUserId))
					.ToListAsync();
			}
		}
	}
}

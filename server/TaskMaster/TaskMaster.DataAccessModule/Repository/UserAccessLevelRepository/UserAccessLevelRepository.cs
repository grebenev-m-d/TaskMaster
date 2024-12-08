using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.UserAccessLevelRepository;
using TaskMaster.Validation;

namespace TaskMaster.DataAccessModule.Repository.UserAccessLevelsRepository
{
	/// <summary>
	/// Репозиторий для проверки уровня доступа пользователей к данным доски.
	/// </summary>
	public class UserAccessLevelRepository : IUserAccessLevelRepository
	{
		/// <summary>
		/// Поставщик служб для доступа к сервисам.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Инициализирует новый экземпляр репозитория доступа уровня пользователя.
		/// </summary>
		/// <param name="serviceProvider">Поставщик служб для доступа к сервисам.</param>
		public UserAccessLevelRepository(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Проверяет уровень доступа пользователя к данным доски по их идентификаторам.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Задача, представляющая операцию проверки уровня доступа.</returns>
		public async Task<AccessLevelType?> HasBoardDataAccess(Guid userId, Guid boardId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				var user = await dbContext.Users
					.Include(i => i.Role)
					.FirstOrDefaultAsync(i => i.Id == userId);

				ArgumentValidation.CheckNotNull(user, "Пользователь не найден.");

				if (user.Role.Type == RoleType.admin)
				{
					return AccessLevelType.editor;
				}
				var board = dbContext.Boards
					.Include(b => b.User)
					.Include(b => b.GeneralAccessLevel)
					.Include(b => b.BoardAccessLevelMaps)
						.ThenInclude(m => m.AccessLevel)
					.FirstOrDefault(i => i.Id == boardId);

				ArgumentValidation.CheckNotNull(board, "Доска не найдена.");

				if (board.UserId.Equals(userId))
				{
					return AccessLevelType.owner;
				}

				var userAccessLevelMap = board.BoardAccessLevelMaps.FirstOrDefault(i => i.UserId == userId);
				if (userAccessLevelMap?.AccessLevel != null)
				{
					return userAccessLevelMap.AccessLevel.Type;
				}

				if (board.IsPublic && board.GeneralAccessLevel != null)
				{
					return board.GeneralAccessLevel.Type;
				}

				return null;
			}
		}

		/// <summary>
		/// Проверяет уровень доступа списка пользователей к данным доски.
		/// </summary>
		/// <param name="userIds">Список идентификаторов пользователей.</param>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Задача, представляющая операцию проверки уровня доступа для каждого пользователя.</returns>
		public async Task<Dictionary<Guid, AccessLevelType?>> HasBoardDataAccess(List<Guid> userIds, Guid boardId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				var board = dbContext.Boards
					.Include(b => b.User)
					.Include(b => b.GeneralAccessLevel)
					.Include(b => b.BoardAccessLevelMaps)
						.ThenInclude(m => m.AccessLevel)
					.FirstOrDefault(i => i.Id == boardId);

				ArgumentValidation.CheckNotNull(board, "Доска не найдена.");

				var permissions = new Dictionary<Guid, AccessLevelType?>();

				foreach (var userId in userIds)
				{
					var user = await dbContext.Users
						.Include(i => i.Role)
						.FirstOrDefaultAsync(i => i.Id == userId);
					ArgumentValidation.CheckNotNull(user, "Пользователь не найден.");

					if (user.Role.Type == RoleType.admin)
					{
						permissions.Add(userId, AccessLevelType.editor);
						continue;
					}

					if (board.UserId.Equals(userId))
					{
						permissions.Add(userId, AccessLevelType.owner);
						continue;
					}

					var userAccessLevelMap = board.BoardAccessLevelMaps.FirstOrDefault(i => i.UserId == userId);


					if (userAccessLevelMap?.AccessLevel != null)
					{
						permissions.Add(userId, userAccessLevelMap.AccessLevel.Type);
						continue;
					}

					if (board.IsPublic && board.GeneralAccessLevel != null)
					{
						permissions.Add(userId, board.GeneralAccessLevel.Type);
						continue;
					}

					permissions.Add(userId, null);
				}

				return permissions;
			}
		}
	}
}

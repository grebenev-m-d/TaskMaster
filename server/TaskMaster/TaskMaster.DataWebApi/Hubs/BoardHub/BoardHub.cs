
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BoardRepository;
using TaskMaster.DataAccessModule.Repository.DesignRepository;
using TaskMaster.DataAccessModule.Repository.RoleRepository;
using TaskMaster.DataAccessModule.Repository.UserAccessLevelRepository;
using TaskMaster.DataAccessModule.Repository.UserRepository;
using TaskMaster.DataWebApi.Helpers;
using TaskMaster.DataWebApi.Models;
using TaskMaster.Validation;

namespace TaskMaster.DataWebApi.Hubs.BoardHub
{
	/// <summary>
	/// Хаб для работы с досками.
	/// </summary>
	[Microsoft.AspNetCore.Authorization.Authorize]
	public class BoardHub : Hub<IBoardHubClient>
	{
		/// <summary>
		/// Количество досок в истории просмотров.
		/// </summary>
		private const int NumberBoardsInHistoryList = 10;

		/// <summary>
		/// Словарь для отображения идентификаторов подключений и JwtPayload.
		/// </summary>
		private static readonly ConcurrentDictionary<string, JwtPayload> _connectionIdToJwtTokenMap = new ConcurrentDictionary<string, JwtPayload>();

		/// <summary>
		/// Словарь для отображения идентификаторов подключений и идентификаторов пользователей.
		/// </summary>
		private static readonly ConcurrentDictionary<string, Guid> _connectionsIdToUserIdMap = new ConcurrentDictionary<string, Guid>();

		/// <summary>
		/// Словарь для отображения идентификаторов подключений и идентификаторов досок.
		/// </summary>
		private static readonly ConcurrentDictionary<string, Guid> _connectionsIdToBoardIdMap = new ConcurrentDictionary<string, Guid>();

		/// <summary>
		/// Логгер.
		/// </summary>
		private readonly ILogger<BoardHub> _logger;

		/// <summary>
		/// Репозиторий досок.
		/// </summary>
		private readonly IBoardRepository _boardRepository;

		/// <summary>
		/// Репозиторий пользователей.
		/// </summary>
		private readonly IUserRepository _userRepository;

		/// <summary>
		/// Репозиторий дизайнов.
		/// </summary>
		private readonly IDesignRepository _designRepository;

		/// <summary>
		/// Репозиторий истории просмотров досок.
		/// </summary>
		private readonly IBoardViewHistoryMapRepository _boardViewHistoryMapRepository;

		/// <summary>
		/// Репозиторий уровней доступа пользователей.
		/// </summary>
		private readonly IUserAccessLevelRepository _userAccessLevelRepository;

		/// <summary>
		/// Конструктор класса.
		/// </summary>
		/// <param name="logger">Логгер.</param>
		/// <param name="boardRepository">Репозиторий досок.</param>
		/// <param name="userRepository">Репозиторий пользователей.</param>
		/// <param name="designRepository">Репозиторий дизайнов.</param>
		/// <param name="boardViewHistoryMapRepository">Репозиторий истории просмотров досок.</param>
		/// <param name="userAccessLevelRepository">Репозиторий уровней доступа пользователей.</param>
		public BoardHub(
			ILogger<BoardHub> logger,
			IBoardRepository boardRepository,
			IUserRepository userRepository,
			IDesignRepository designRepository,
			IBoardViewHistoryMapRepository boardViewHistoryMapRepository,
			IUserAccessLevelRepository userAccessLevelRepository)
		{
			_logger = logger;
			_boardRepository = boardRepository;
			_userRepository = userRepository;
			_designRepository = designRepository;
			_boardViewHistoryMapRepository = boardViewHistoryMapRepository;
			_userAccessLevelRepository = userAccessLevelRepository;
		}

		/// <summary>
		/// Метод, вызываемый при подключении к хабу.
		/// </summary>
		/// <returns>Задача</returns>
		public async override Task OnConnectedAsync()
		{
			// Извлекаем данные из jwt токена.  
			if (!Context.GetHttpContext().Request.Query.TryGetValue("access_token", out var tokenValues) ||
				string.IsNullOrEmpty(tokenValues.FirstOrDefault()))
			{
				Context.Abort();
				return;
			}

			// Сохраняем данные из токена в словаре
			var jwtPayload = new JwtPayload(tokenValues.FirstOrDefault());
			_connectionIdToJwtTokenMap[Context.ConnectionId] = jwtPayload;

			// Пробуем получить Id доски.
			if (Context.GetHttpContext().Request.Query.TryGetValue("board_id", out var boardValues) &&
				!string.IsNullOrEmpty(boardValues.FirstOrDefault()) &&
				Guid.TryParse(boardValues.FirstOrDefault(), out var boardId))
			{
				// Добавляем id соединения в группу относящейся к этой доске.
				_connectionsIdToBoardIdMap.TryAdd(Context.ConnectionId, boardId);
				await Groups.AddToGroupAsync(Context.ConnectionId, boardId.ToString());

				await UserBoardViewingStarted(jwtPayload.Sub, boardId);

				return;
			}

			// Иначе добавляем id соединения в группу относящейся к этому пользователю.
			_connectionsIdToUserIdMap.TryAdd(Context.ConnectionId, jwtPayload.Sub);
			await Groups.AddToGroupAsync(Context.ConnectionId, jwtPayload.Sub.ToString());
		}

		/// <summary>
		/// Метод, вызываемый при отключении от хаба.
		/// </summary>
		/// <param name="exception">Исключение.</param>
		/// <returns>Задача</returns>
		public override Task OnDisconnectedAsync(Exception? exception)
		{
			_connectionIdToJwtTokenMap.TryRemove(Context.ConnectionId, out _);
			_connectionsIdToUserIdMap.TryRemove(Context.ConnectionId, out _);
			_connectionsIdToBoardIdMap.TryRemove(Context.ConnectionId, out _);

			return base.OnDisconnectedAsync(exception);
		}

		/// <summary>
		/// Начало просмотра доски пользователем.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Задача</returns>
		public async Task UserBoardViewingStarted(Guid userId, Guid boardId)
		{
			var user = await _userRepository.GetByIdAsyncIncludes(userId);

			var boardsViewHistory = await _boardViewHistoryMapRepository
				.GetAllByUserIdAsync(userId);
			for (var i = 0; boardsViewHistory.Count > NumberBoardsInHistoryList; i++)
			{
				await _boardViewHistoryMapRepository.DeleteLastByUserId(userId);
			}

			var repeatBoardsViewHistory = boardsViewHistory.FirstOrDefault(i => i.BoardId == boardId);

			if (repeatBoardsViewHistory != null)
			{

				repeatBoardsViewHistory.LastViewedAt = DateTime.Now;
				await _boardViewHistoryMapRepository.UpdateAsync(repeatBoardsViewHistory);
			}
			else
			{
				await _boardViewHistoryMapRepository.AddAsync(new DbBoardViewHistoryMap()
				{
					UserId = userId,
					BoardId = boardId,
				});
			}

			var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];

			var newBoardsViewHistory = await _boardViewHistoryMapRepository
				.GetAllByUserIdAsync(payload.Sub);

			var boards = newBoardsViewHistory.OrderByDescending(i => i.LastViewedAt)
				.Select(i => DbModelMappers.MapDbToBoard(i.Board))
				.ToList();


			NotifyGroups((g) => g.ReceiveUpdatedHistoryLastViewedBoards(boards), userId: userId);
		}

		#region Service methods

		/// <summary>
		/// Уведомляет группы.
		/// </summary>
		/// <param name="func">Функция для уведомления групп.</param>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <param name="boardId">Идентификатор доски.</param>
		public void NotifyGroups(Action<IBoardHubClient> func, Guid? userId = null, Guid? boardId = null)
		{
			if (userId != null)
			{
				func(Clients.Group(userId.ToString()));
			}
			if (boardId != null)
			{
				func(Clients.Group(boardId.ToString()));
			}
		}
		#endregion

		#region Создание доски

		/// <summary>
		/// Создает новую доску.
		/// </summary>
		/// <param name="title">Название доски.</param>
		/// <returns>Задача</returns>
		public async Task CreateBoard(string title)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, BoardHub>(async () =>
			{
				ArgumentValidation.CheckNotNullOrEmpty(title,
					"Название доски не должно быть пустым.");

				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];

				var addedBoard = await _boardRepository.AddAsync(
					new DbBoard()
					{
						Title = title,
						UserId = payload.Sub
					});

				NotifyGroups((g) => g.ReceiveBoardCreated(DbModelMappers.MapDbToBoard(addedBoard)), userId: payload.Sub);

				return null;
			}, _logger);
		}

		#endregion

		#region Чтение доски

		/// <summary>
		/// Получает доску по идентификатору.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Доска</returns>
		[Microsoft.AspNetCore.Authorization.Authorize]
		public async Task<Board> GetBoard(Guid boardId)
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Board, BoardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);

				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, boardId), AccessLevelType.reader);

				var board = await _boardRepository.GetByIdAsyncIncludes(boardId);
				ArgumentValidation.CheckNotNull(board, "Доска не найдена.");

				return DbModelMappers.MapDbToBoard(board);
			}, _logger);
		}

		/// <summary>
		/// Возвращает список собственных досок пользователя.
		/// </summary>
		/// <param name="pageNumber">Номер страницы.</param>
		/// <param name="pageSize">Размер страницы.</param>
		/// <returns>Список досок.</returns>
		public async Task<List<Board>> GetOwnBoards(int pageNumber, int pageSize)
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<List<Board>, BoardHub>(async () =>
			{
				ArgumentValidation.CheckPositiveNumber(pageNumber,
					"Такой страницы нет.");
				ArgumentValidation.CheckPositiveNumber(pageSize,
					"Нумерация страниц начинается с первой.");

				int skip = (pageNumber - 1) * pageSize;
				int take = pageSize;
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];

				var boards = await _boardRepository.GetAllByUserIdAsync(payload.Sub);

				return boards
					.Skip(skip)
					.Take(take)
					.OrderBy(i => i.CreatedAt)
					.Select(b => DbModelMappers.MapDbToBoard(b))
					.ToList();
			}, _logger);
		}

		/// <summary>
		/// Возвращает общее количество собственных досок пользователя.
		/// </summary>
		/// <returns>Общее количество собственных досок.</returns>
		public async Task<int> GetTotalNumberOwnBoards()
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<int, BoardHub>(async () =>
			{
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				var boards = await _boardRepository.GetAllByUserIdAsync(payload.Sub);

				return boards.Count();
			}, _logger);
		}

		/// <summary>
		/// Возвращает список пользователей, с которыми пользователь поделился досками.
		/// </summary>
		/// <returns>Список пользователей.</returns>
		public async Task<List<User>> GetUsersWithSharedBoards()
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<List<User>, BoardHub>(async () =>
			{
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				var users = await _userRepository.GetAllUsersWithSharedBoardsByUserIdAsync(payload.Sub);

				return DbModelMappers.MapDbToUser(users);
			}, _logger);
		}

		/// <summary>
		/// Возвращает список досок, с которыми пользователь поделился с указанным пользователем.
		/// </summary>
		/// <param name="inviterUserId">Идентификатор пользователя, с которым поделились досками.</param>
		/// <param name="pageNumber">Номер страницы.</param>
		/// <param name="pageSize">Размер страницы.</param>
		/// <returns>Список досок.</returns>
		public async Task<List<Board>> GetSharedBoardsWithUser(Guid inviterUserId, int pageNumber, int pageSize)
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<List<Board>, BoardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(inviterUserId);
				ArgumentValidation.CheckPositiveNumber(pageNumber,
					"Такой страницы нет.");
				ArgumentValidation.CheckPositiveNumber(pageSize,
					"Нумерация страниц начинается с первой.");

				int skip = (pageNumber - 1) * pageSize;
				int take = pageSize;
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];

				var boards = await _boardRepository.GetBoardsFromOwnerWithPersonalAccessLevelForUser(payload.Sub, inviterUserId);

				return boards
					.Skip(skip)
					.Take(take)
					.OrderBy(i => i.CreatedAt)
					.Select(b => DbModelMappers.MapDbToBoard(b))
					.ToList();

			}, _logger);
		}

		/// <summary>
		/// Возвращает общее количество досок, которыми пользователь поделился с указанным пользователем.
		/// </summary>
		/// <param name="inviterUserId">Идентификатор пользователя, с которым поделились досками.</param>
		/// <returns>Общее количество досок.</returns>
		public async Task<int> GetTotalNumberSharedBoardsWithUser(Guid inviterUserId)
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<int, BoardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(inviterUserId);

				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];

				var boards = await _boardRepository.GetBoardsFromOwnerWithPersonalAccessLevelForUser(payload.Sub, inviterUserId);

				return boards.Count();
			}, _logger);
		}

		/// <summary>
		/// Возвращает список пользователей, которые просматривали указанную доску.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Список пользователей.</returns>
		public async Task<List<User>> GetUsersWhoViewBoard(Guid boardId)
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<List<User>, BoardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);

				var boardViews = _connectionsIdToBoardIdMap.Where(i => i.Value == boardId);

				var users = new List<User>();
				foreach (var item in boardViews)
				{
					var user = await _userRepository.GetByIdAsyncIncludes(_connectionsIdToUserIdMap[item.Key]);
					users.Add(DbModelMappers.MapDbToUser(user));
				}

				return users;
			}, _logger);
		}

		/// <summary>
		/// Возвращает список последних просмотренных пользователем досок.
		/// </summary>
		/// <returns>Список досок.</returns>
		public async Task<List<Board>> GetHistoryLastViewedBoards()
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<List<Board>, BoardHub>(async () =>
			{
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];

				var boardViewHistory = await _boardViewHistoryMapRepository.GetAllByUserIdAsync(payload.Sub);

				return boardViewHistory.OrderByDescending(i => i.LastViewedAt)
					.Select(i => DbModelMappers.MapDbToBoard(i.Board)).ToList();
			}, _logger);
		}

		#endregion

		#region Update board

		/// <summary>
		/// Обновляет заголовок указанной доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="title">Новый заголовок доски.</param>
		/// <returns>Задача обновления.</returns>
		public async Task UpdateTitle(Guid boardId, string title)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, BoardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);
				ArgumentValidation.CheckNotNullOrEmpty(title,
					"Заголовок доски не должен быть пустым.");

				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository
						.HasBoardDataAccess(payload.Sub, boardId), AccessLevelType.editor);

				var board = await _boardRepository.GetByIdAsyncIncludes(boardId);
				board.Title = title;

				await _boardRepository.UpdateAsync(board);

				NotifyGroups((g) => g.ReceiveTitleUpdated(boardId, title), userId: payload.Sub, boardId: board.Id);

				var userIds = board.BoardAccessLevelMaps.Select(map => map.UserId).ToList();
				userIds.ForEach(i =>
				{
					NotifyGroups((g) => g.ReceiveTitleUpdated(boardId, title), userId: i);
				});

				return null;
			}, _logger);
		}

		/// <summary>
		/// Обновляет цвет указанной доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="colorCode">Новый код цвета доски.</param>
		/// <returns>Задача обновления.</returns>
		public async Task UpdateColor(Guid boardId, string colorCode)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, BoardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);
				ArgumentValidation.CheckNotNull(colorCode,
					"Нужно указать код цвета.");

				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository
						.HasBoardDataAccess(payload.Sub, boardId), AccessLevelType.editor);

				var board = await _boardRepository.GetByIdAsyncIncludes(boardId);
				board.ColorCode = colorCode;

				await _boardRepository.UpdateAsync(board);

				NotifyGroups((g) => g.ReceiveColorUpdated(boardId, colorCode), userId: payload.Sub, boardId: board.Id);

				var userIds = board.BoardAccessLevelMaps.Select(map => map.UserId).ToList();
				userIds.ForEach(i =>
				{
					NotifyGroups((g) => g.ReceiveColorUpdated(boardId, colorCode), userId: i);
				});

				return null;
			}, _logger);
		}

		/// <summary>
		/// Обновляет тип дизайна указанной доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="designType">Новый тип дизайна доски.</param>
		/// <returns>Задача обновления.</returns>
		public async Task UpdateDesignType(Guid boardId, DesignType designType)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, BoardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);

				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository
						.HasBoardDataAccess(payload.Sub, boardId), AccessLevelType.editor);

				var board = await _boardRepository.GetByIdAsync(boardId);

				var design = await _designRepository.GetByTypeAsync(designType);
				board.DesignTypeId = design.Id;

				await _boardRepository.UpdateAsync(board);

				NotifyGroups((g) => g.ReceiveDesignTypeUpdated(boardId, designType), userId: payload.Sub, boardId: board.Id);

				var userIds = board.BoardAccessLevelMaps.Select(map => map.UserId).ToList();
				userIds.ForEach(i =>
				{
					NotifyGroups((g) => g.ReceiveDesignTypeUpdated(boardId, designType), userId: i);
				});

				return null;
			}, _logger);
		}

		/// <summary>
		/// Обновляет владельца указанной доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="ownerEmail">Почта нового владельца.</param>
		/// <returns>Задача обновления.</returns>
		public async Task UpdateOwner(Guid boardId, string ownerEmail)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, BoardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);
				ArgumentValidation.CheckNotNullOrEmpty(ownerEmail,
					"Нужно указать почту владельца.");

				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository
						.HasBoardDataAccess(payload.Sub, boardId), AccessLevelType.editor);

				var board = await _boardRepository.GetByIdAsyncIncludes(boardId);
				ArgumentValidation.CheckNotNull(board, "Доски с таким Id нет");

				var newOwner = await _userRepository.GetByEmailAsync(ownerEmail);
				ArgumentValidation.CheckNotNull(newOwner, "Пользователя с таким Id нет");

				board.UserId = newOwner.Id;
				board.User = newOwner;

				await _boardRepository.UpdateAsync(board);

				NotifyGroups((g) => g.ReceiveOwnerUpdated(boardId, DbModelMappers.MapDbToUser(newOwner)), userId: payload.Sub, boardId: board.Id);

				var userIds = board.BoardAccessLevelMaps.Select(map => map.UserId).ToList();
				userIds.ForEach(i =>
				{
					NotifyGroups((g) => g.ReceiveOwnerUpdated(boardId, DbModelMappers.MapDbToUser(newOwner)), userId: i);
				});

				return null;
			}, _logger);
		}

		#endregion

		#region Remove board

		/// <summary>
		/// Удаляет указанную доску.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Задача удаления.</returns>
		public async Task DeleteBoard(Guid boardId)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, BoardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);

				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository
						.HasBoardDataAccess(payload.Sub, boardId), AccessLevelType.editor);

				var board = await _boardRepository.DeleteByIdAsync(boardId);

				NotifyGroups((g) => g.ReceiveBoardDeleted(boardId), userId: payload.Sub, boardId: boardId);

				var userIds = board.BoardAccessLevelMaps.Select(map => map.UserId).ToList();
				userIds.ForEach(i =>
				{
					NotifyGroups((g) => g.ReceiveBoardDeleted(boardId), userId: i);
				});

				return null;
			}, _logger);
		}
		#endregion
	}
}

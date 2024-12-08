using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;
using TaskMaster.DataAccessModule.Repository.BoardRepository;
using TaskMaster.DataAccessModule.Repository.CardListRepository;
using TaskMaster.DataAccessModule.Repository.CardRepository;
using TaskMaster.DataAccessModule.Repository.DesignRepository;
using TaskMaster.DataAccessModule.Repository.AccessLevelRole;
using TaskMaster.DataAccessModule.Repository.RoleRepository;
using TaskMaster.DataAccessModule.Repository.UserAccessLevelRepository;
using TaskMaster.DataAccessModule.Repository.UserRepository;
using TaskMaster.DataWebApi.Helpers;
using TaskMaster.DataWebApi.Hubs.BoardAccessLevelHub;
using TaskMaster.DataWebApi.Models;
using TaskMaster.Validation;
using Microsoft.AspNetCore.Authorization;

namespace TaskMaster.DataWebApi.Hubs.BoardAccessLevelHub
{
	/// <summary>
	/// Хаб уровня доступа к доске.
	/// </summary>
	[Authorize]
	public class BoardAccessLevelHub : Hub<IBoardAccessLevelHubClient>
	{
		/// <summary>
		/// Словарь для отображения идентификатора подключения на идентификатор пользователя.
		/// </summary>
		private static readonly ConcurrentDictionary<string, Guid> _connectionIdToUserIdMap = new ConcurrentDictionary<string, Guid>();

		/// <summary>
		/// Словарь для отображения идентификатора подключения на идентификатор доски.
		/// </summary>
		private static readonly ConcurrentDictionary<string, Guid> _connectionsIdToBoardIdMap = new ConcurrentDictionary<string, Guid>();

		private readonly ILogger<BoardAccessLevelHub> _logger;
		private readonly IBoardRepository _boardRepository;
		private readonly IAccessLevelRepository _permissionRepository;
		private readonly IUserRepository _userRepository;
		private readonly IBoardAccessLevelMapRepository _boardAccessLevelMapRepository;
		private readonly IUserAccessLevelRepository _userAccessLevelRepository;

		public BoardAccessLevelHub(
			ILogger<BoardAccessLevelHub> logger,
			IBoardRepository boardRepository,
			IAccessLevelRepository permissionRepository,
			IUserRepository userRepository,
			IBoardAccessLevelMapRepository boardAccessLevelMapRepository,
			IUserAccessLevelRepository userAccessLevelRepository)
		{
			_logger = logger;
			_boardRepository = boardRepository;
			_permissionRepository = permissionRepository;
			_userRepository = userRepository;
			_boardAccessLevelMapRepository = boardAccessLevelMapRepository;
			_userAccessLevelRepository = userAccessLevelRepository;
		}

		/// <summary>
		/// Вызывается при установке соединения с хабом.
		/// </summary>
		/// <returns>Task</returns>
		public async override Task OnConnectedAsync()
		{
			// Извлекаем данные из JWT токена.  
			if (!Context.GetHttpContext().Request.Query.TryGetValue("access_token", out var tokenValues) ||
				string.IsNullOrEmpty(tokenValues.FirstOrDefault()))
			{
				Context.Abort();
				return;
			}

			// Сохраняем данные из токена в словаре.
			var jwtPayload = new JwtPayload(tokenValues.FirstOrDefault());
			_connectionIdToUserIdMap.TryAdd(Context.ConnectionId, jwtPayload.Sub);

			await Groups.AddToGroupAsync(Context.ConnectionId, jwtPayload.Sub.ToString());

			// Пытаемся получить идентификатор доски.
			if (Context.GetHttpContext().Request.Query.TryGetValue("board_id", out var boardValues) &&
				!string.IsNullOrEmpty(boardValues.FirstOrDefault()) &&
				Guid.TryParse(boardValues.FirstOrDefault(), out var boardId))
			{
				// Добавляем идентификатор соединения в группу, относящуюся к этой доске.
				_connectionsIdToBoardIdMap.TryAdd(Context.ConnectionId, boardId);
				await Groups.AddToGroupAsync(Context.ConnectionId, boardId.ToString());
			}

			await base.OnConnectedAsync();
		}

		/// <summary>
		/// Вызывается при отключении от хаба.
		/// </summary>
		/// <param name="exception">Исключение.</param>
		/// <returns>Task</returns>
		public override Task OnDisconnectedAsync(Exception? exception)
		{
			_connectionsIdToBoardIdMap.TryRemove(Context.ConnectionId, out _);
			_connectionIdToUserIdMap.TryRemove(Context.ConnectionId, out _);

			return base.OnDisconnectedAsync(exception);
		}

		/// <summary>
		/// Уведомляет группы о событии.
		/// </summary>
		/// <param name="func">Действие для уведомления клиентов.</param>
		/// <param name="boardId">Идентификатор доски (необязательный).</param>
		public void NotifyGroups(
			Action<IBoardAccessLevelHubClient> func,
			Guid? boardId = null)
		{
			if (boardId != null)
			{
				func(Clients.Group(boardId.ToString()));
			}
		}

		/// <summary>
		/// Получает уровень доступа к доске.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Уровень доступа к доске.</returns>
		public async Task<BoardAccessLevel> GetBoardAccessLevel(Guid boardId)
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<BoardAccessLevel, BoardAccessLevelHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);

				var connectedUserId = _connectionIdToUserIdMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository
						.HasBoardDataAccess(connectedUserId, boardId), AccessLevelType.reader);

				var board = await _boardRepository.GetByIdAsyncIncludes(boardId);
				ArgumentValidation.CheckNotNull(board, "Доски с таким Id нет");

				return DbModelMappers.MapDbBoardToBoardAccessLevel(board);
			}, _logger);
		}

		/// <summary>
		/// Получает уровень доступа пользователя к доске.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Уровень доступа пользователя к доске.</returns>
		public async Task<AccessLevelType?> GetUserAccessLevel(Guid boardId)
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<AccessLevelType?, BoardAccessLevelHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);

				var connectedUserId = _connectionIdToUserIdMap[Context.ConnectionId];

				return await _userAccessLevelRepository.HasBoardDataAccess(connectedUserId, boardId);
			}, _logger);
		}

		/// <summary>
		/// Обновляет публичный статус доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="isPublic">Публичный статус доски.</param>
		/// <returns>Задача обновления публичного статуса доски.</returns>
		public async Task UpdateBoardPublicStatus(Guid boardId, bool isPublic)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, BoardAccessLevelHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);

				var connectedUserId = _connectionIdToUserIdMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository
					.HasBoardDataAccess(connectedUserId, boardId), AccessLevelType.editor);

				var userIds = _connectionIdToUserIdMap.Select(i => i.Value).Distinct().ToList();

				var oldAccessLevels = await _userAccessLevelRepository.HasBoardDataAccess(userIds, boardId);

				var board = await _boardRepository.GetByIdAsyncIncludes(boardId);
				ArgumentValidation.CheckNotNull(board, "Доски с таким Id нет");

				board.IsPublic = isPublic;

				await _boardRepository.UpdateAsync(board);

				NotifyGroups((g) => g.ReceiveBoardPublicStatusUpdated(boardId, isPublic), boardId: board.Id);


				var newAccessLevels = await _userAccessLevelRepository.HasBoardDataAccess(userIds, boardId);

				foreach (var userId in userIds)
				{
					if (oldAccessLevels[userId] != newAccessLevels[userId])
					{
						Clients.Group(userId.ToString())?.ReceiveUserAccessLevelUpdated(boardId, newAccessLevels[userId]);
					}
				}

				return null;
			}, _logger);
		}








		/// <summary>
		/// Обновляет общий уровень доступа к доске.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="permissionType">Уровень доступа.</param>
		/// <returns>Задача обновления общего уровня доступа к доске.</returns>
		public async Task UpdateBoardGeneralAccessLevel(Guid boardId, AccessLevelType permissionType)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, BoardAccessLevelHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);

				var userIds = _connectionIdToUserIdMap.Select(i => i.Value).Distinct().ToList();

				var connectedUserId = _connectionIdToUserIdMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository
						.HasBoardDataAccess(connectedUserId, boardId), AccessLevelType.editor);


				var oldAccessLevels = await _userAccessLevelRepository.HasBoardDataAccess(userIds, boardId);

				var board = await _boardRepository.GetByIdAsyncIncludes(boardId);
				ArgumentValidation.CheckNotNull(board, "Доски с таким Id нет");

				var permission = await _permissionRepository.GetByNameAsync(permissionType);
				board.GeneralAccessLevelId = permission.Id;
				board.GeneralAccessLevel = permission;


				await _boardRepository.UpdateAsync(board);

				NotifyGroups((g) => g.ReceiveBoardDefaultAccessLevelUpdated(boardId, permissionType), boardId: board.Id);

				var newAccessLevels = await _userAccessLevelRepository.HasBoardDataAccess(userIds, boardId);

				foreach (var userId in userIds)
				{
					if (oldAccessLevels[userId] != newAccessLevels[userId])
					{
						Clients.Group(userId.ToString())?.ReceiveUserAccessLevelUpdated(boardId, newAccessLevels[userId]);
					}
				}

				return null;
			}, _logger);
		}

		/// <summary>
		/// Добавляет персональный уровень доступа к доске для указанного пользователя.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="userEmail">Email пользователя.</param>
		/// <param name="permissionType">Уровень доступа.</param>
		/// <returns>Задача добавления персонального уровня доступа к доске.</returns>
		/// <exception cref="ArgumentException">Выбрасывается, если пользователь с указанным email не найден.</exception>
		public async Task AddPersonalAccessLevel(Guid boardId, string userEmail, AccessLevelType permissionType)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, BoardAccessLevelHub>(async () =>
			{

				ArgumentValidation.CheckNotNull(userEmail, "Нужно указать почту пользователя.");

				var connectedUserId = _connectionIdToUserIdMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository
						.HasBoardDataAccess(connectedUserId, boardId), AccessLevelType.editor);

				var board = await _boardRepository.GetByIdAsyncIncludes(boardId);
				ArgumentValidation.CheckNotNull(board, "Доски с таким Id нет");

				var user = await _userRepository.GetByEmailAsync(userEmail);
				if (user == null)
				{
					throw new ArgumentException("Пользователя с такой почтой не найдено.");
				}

				var permission = await _permissionRepository.GetByNameAsync(permissionType);

				if (board.BoardAccessLevelMaps.Any(i => i.UserId == user.Id))
				{
					throw new ArgumentException("Данному пользователю уже выдан персональный доступ.");
				}


				await _boardAccessLevelMapRepository.AddAsync(new DbBoardAccessLevelMap()
				{
					BoardId = boardId,
					UserId = user.Id,
					AccessLevelId = permission.Id
				});


				await _boardRepository.UpdateAsync(board);

				// Оповещаем себя же 
				Clients.Group(boardId.ToString())?.ReceivePersonalAccessLevelAdded(
					boardId,
					DbModelMappers.MapDbToUser(user),
					permissionType);


				Clients.Group(user.Id.ToString())?.ReceiveUserAccessLevelAdded(
					boardId,
					DbModelMappers.MapDbToUser(await _userRepository.GetByIdAsyncIncludes(connectedUserId)),
					permissionType);

				return null;
			}, _logger);
		}

		/// <summary>
		/// Обновляет персональный уровень доступа к доске для указанного пользователя.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <param name="permissionType">Уровень доступа.</param>
		/// <returns>Задача обновления персонального уровня доступа к доске.</returns>
		/// <exception cref="InvalidOperationException">Выбрасывается, если не удается найти пользователя с указанным Id на доске.</exception>
		public async Task UpdatePersonalAccessLevel(Guid boardId, Guid userId, AccessLevelType permissionType)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, BoardAccessLevelHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId, "Не указано Id доски");
				ArgumentValidation.CheckNotEmptyGuid(userId, "Не указано Id пользователя");


				var connectedUserId = _connectionIdToUserIdMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(connectedUserId, boardId), AccessLevelType.editor);

				var board = await _boardRepository.GetByIdAsyncIncludes(boardId);
				ArgumentValidation.CheckNotNull(board, "Доски с таким Id нет");

				// Изменение права пользователя
				var permission = await _permissionRepository.GetByNameAsync(permissionType);

				if (board.BoardAccessLevelMaps.FirstOrDefault(i => i.User.Id == userId) != null)
				{	
					// Если пользователь уже есть 
					var boardAccessLevelMap = await _boardAccessLevelMapRepository.GetByIdAsync(boardId, userId);
					
					boardAccessLevelMap.AccessLevelId = permission.Id;
					await _boardAccessLevelMapRepository.UpdateAsync(boardAccessLevelMap);
				}
				else
				{// Если пользователя не было
					throw new InvalidOperationException();	
				}

				// Оповещаем себя же 
				Clients.Group(connectedUserId.ToString())?.ReceivePersonalAccessLevelUpdated(boardId, userId, permissionType);
				// Оповещаем пользователя чьи права мы изменили 
				Clients.Group(userId.ToString())?.ReceivePersonalAccessLevelUpdated(boardId, userId, permissionType);

				return null;
			}, _logger);
		}


		/// <summary>
		/// Удаляет персональный уровень доступа к доске для указанного пользователя.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <returns>Задача удаления персонального уровня доступа к доске.</returns>
		public async Task DeletePersonalAccessLevel(Guid boardId, Guid userId)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, BoardAccessLevelHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId, "Не указано Id доски");
				ArgumentValidation.CheckNotEmptyGuid(userId, "Не указано Id пользователя");

				var connectedUserId = _connectionIdToUserIdMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(connectedUserId, boardId), AccessLevelType.editor);

				var board = await _boardRepository.GetByIdAsyncIncludes(boardId);
				ArgumentValidation.CheckNotNull(board, "Доски с таким Id нет");

				var user = await _userRepository.GetByIdAsyncIncludes(userId);

				await _boardAccessLevelMapRepository.DeleteByIdAsync(boardId, user.Id);

				Clients.Group(boardId.ToString())?.ReceivePersonalAccessLevelDeleted(boardId, user.Id);

				Clients.Group(userId.ToString())?.ReceiveUserAccessLevelDeleted(boardId,
					DbModelMappers.MapDbToUser(await _userRepository.GetByIdAsyncIncludes(connectedUserId)));

				return null;
			}, _logger);
		}
	}
}

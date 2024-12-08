using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Asn1.Esf;
using System.Collections.Concurrent;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Extensions;
using TaskMaster.DataAccessModule.Repository.BoardRepository;
using TaskMaster.DataAccessModule.Repository.CardListRepository;
using TaskMaster.DataAccessModule.Repository.CardRepository;
using TaskMaster.DataAccessModule.Repository.DesignRepository;
using TaskMaster.DataAccessModule.Repository.AccessLevelRole;
using TaskMaster.DataAccessModule.Repository.RoleRepository;
using TaskMaster.DataAccessModule.Repository.UserAccessLevelRepository;
using TaskMaster.DataAccessModule.Repository.UserRepository;
using TaskMaster.DataWebApi.Helpers;
using TaskMaster.DataWebApi.Models;
using TaskMaster.Validation;
using Microsoft.AspNetCore.Authorization;
namespace TaskMaster.DataWebApi.Hubs.CardListHub
{
	/// <summary>
	/// Хаб для списка карточек с клиентским интерфейсом <see cref="ICardListHubClient"/>.
	/// </summary>
	
	public partial class CardListHub : Hub<ICardListHubClient>
	{
		/// <summary>
		/// Словарь для сопоставления идентификаторов подключений с данными JWT.
		/// </summary>
		private static readonly ConcurrentDictionary<string, JwtPayload> _connectionIdToJwtTokenMap = new ConcurrentDictionary<string, JwtPayload>();

		/// <summary>
		/// Словарь для сопоставления идентификаторов подключений с идентификаторами досок.
		/// </summary>
		private static readonly ConcurrentDictionary<string, Guid> _connectionsIdToBoardIdMap = new ConcurrentDictionary<string, Guid>();

		/// <summary>
		/// Словарь для сопоставления идентификаторов подключений с идентификаторами списков карточек.
		/// </summary>
		private static readonly ConcurrentDictionary<string, Guid> _connectionsIdToCardListIdMap = new ConcurrentDictionary<string, Guid>();

		private readonly ILogger<CardListHub> _logger;
		private readonly IBoardRepository _boardRepository;
		private readonly ICardListRepository _cardListRepository;
		private readonly IUserAccessLevelRepository _userAccessLevelRepository;

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CardListHub"/>.
		/// </summary>
		/// <param name="logger">Логгер.</param>
		/// <param name="boardRepository">Репозиторий досок.</param>
		/// <param name="cardListRepository">Репозиторий списков карточек.</param>
		/// <param name="userAccessLevelRepository">Репозиторий уровней доступа пользователей.</param>
		public CardListHub(
			ILogger<CardListHub> logger,
			IBoardRepository boardRepository,
			ICardListRepository cardListRepository,
			IUserAccessLevelRepository userAccessLevelRepository)
		{
			_logger = logger;
			_boardRepository = boardRepository;
			_cardListRepository = cardListRepository;
			_userAccessLevelRepository = userAccessLevelRepository;
		}

		/// <summary>
		/// Метод вызывается при установке соединения с хабом.
		/// </summary>
		/// <returns>Асинхронная задача.</returns>
		public async override Task OnConnectedAsync()
		{
			// Извлекаем данные из JWT токена.
			if (!Context.GetHttpContext().Request.Query.TryGetValue("access_token", out var tokenValues) ||
				string.IsNullOrEmpty(tokenValues.FirstOrDefault()))
			{
				Context.Abort();
				return;
			}

			// Сохранить данные из токена в словаре.
			var jwtPayload = new JwtPayload(tokenValues.FirstOrDefault());
			_connectionIdToJwtTokenMap[Context.ConnectionId] = jwtPayload;

			// Пробуем получить идентификатор доски.
			if (Context.GetHttpContext().Request.Query.TryGetValue("board_id", out var boardValues) &&
				!string.IsNullOrEmpty(boardValues.FirstOrDefault()) &&
				Guid.TryParse(boardValues.FirstOrDefault(), out var boardId))
			{
				// Добавляем идентификатор соединения в группу, относящуюся к этой доске.
				_connectionsIdToBoardIdMap.TryAdd(Context.ConnectionId, boardId);
				await Groups.AddToGroupAsync(Context.ConnectionId, boardId.ToString());
				return;
			}
			//// Пробуем получить идентификатор списка карточек.
			//if (Context.GetHttpContext().Request.Query.TryGetValue("card_list_id", out var cardListValues) &&
			//	!string.IsNullOrEmpty(cardListValues.FirstOrDefault()) &&
			//	Guid.TryParse(cardListValues.FirstOrDefault(), out var cardListId))
			//{
			//	// Добавляем идентификатор соединения в группу, относящуюся к этому списку карточек.
			//	_connectionsIdToCardListIdMap.TryAdd(Context.ConnectionId, cardListId);
			//	await Groups.AddToGroupAsync(Context.ConnectionId, cardListId.ToString());
			//	return;
			//}

			Context.Abort();
			return;
		}

		/// <summary>
		/// Метод вызывается при разрыве соединения с хабом.
		/// </summary>
		/// <param name="exception">Исключение.</param>
		/// <returns>Асинхронная задача.</returns>
		public override Task OnDisconnectedAsync(Exception? exception)
		{
			_connectionIdToJwtTokenMap.TryRemove(Context.ConnectionId, out _);
			_connectionsIdToBoardIdMap.TryRemove(Context.ConnectionId, out _);
			_connectionsIdToCardListIdMap.TryRemove(Context.ConnectionId, out _);
			return base.OnDisconnectedAsync(exception);
		}

		/// <summary>
		/// Уведомляет группы клиентов о событиях с карточками.
		/// </summary>
		/// <param name="func">Действие для выполнения в каждой группе.</param>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="cardListId">Идентификатор списка карточек.</param>
		public void NotifyGroups(Action<ICardListHubClient> func, Guid? boardId = null, Guid? cardListId = null)
		{
			if (boardId != null)
			{
				func(Clients.Group(boardId.ToString()));
			}
			if (cardListId != null)
			{
				func(Clients.Group(cardListId.ToString()));
			}
		}

		#region Create card list

		/// <summary>
		/// Создает новый список карточек.
		/// </summary>
		/// <param name="boardId">Идентификатор доски, к которой принадлежит список карточек.</param>
		/// <param name="title">Заголовок нового списка карточек.</param>
		/// <returns>Задача асинхронной операции.</returns>
		public async Task CreateCardList(Guid boardId, string title)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardListHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);
				ArgumentValidation.CheckNotNullOrEmpty(title, "Заголовок списка карточек не должен быть пустым.");

				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, boardId), AccessLevelType.editor);

				var board = await _boardRepository.GetByIdAsyncIncludes(boardId);
				ArgumentValidation.CheckNotNull(board, "Такой доски нет");

				var cardList = new DataAccessModule.Models.DbCardList()
				{
					BoardId = boardId,
					Title = title
				};

				var addedCardList = await _cardListRepository.AddAsync(cardList);

				NotifyGroups((g) => g.ReceiveCardListCreated(boardId, DbModelMappers.MapDbToCardList(addedCardList)), boardId: board.Id);

				return null;
			}, _logger);
		}
		#endregion

		#region Read card list

		/// <summary>
		/// Получает списки карточек для указанной доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Список объектов <see cref="CardList"/>.</returns>
		public async Task<List<CardList>> GetCardLists(Guid boardId)
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<List<CardList>, CardListHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);

				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, boardId), AccessLevelType.reader);

				var boards = await _boardRepository.GetByIdAsyncIncludes(boardId);

				return DbModelMappers.MapDbToCardList(boards.CardLists.SortByRelationship().ToList());
			}, _logger);
		}
		#endregion

		#region Update card list

		/// <summary>
		/// Обновляет заголовок списка карточек.
		/// </summary>
		/// <param name="cardListId">Идентификатор списка карточек.</param>
		/// <param name="title">Новый заголовок списка карточек.</param>
		/// <returns>Задача асинхронной операции.</returns>
		public async Task UpdateTitle(Guid cardListId, string title)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardListHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardListId);
				ArgumentValidation.CheckNotNull(title, "Заголовок списка карточек не должен быть пустым.");

				var board = await _boardRepository.GetBoardByCardListIdAsync(cardListId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

				var cardList = await _cardListRepository.GetByIdAsyncIncludes(cardListId);
				cardList.Title = title;

				await _cardListRepository.UpdateAsync(cardList);

				NotifyGroups((g) => g.ReceiveCardListTitleUpdated(cardListId, title), cardListId: cardListId);

				return null;
			}, _logger);
		}

		/// <summary>
		/// Перемещает список карточек.
		/// </summary>
		/// <param name="movedCardListId">Идентификатор перемещаемого списка карточек.</param>
		/// <param name="prevCardListId">Идентификатор предыдущего списка карточек.</param>
		/// <returns>Задача асинхронной операции.</returns>
		public async Task MoveCardList(Guid movedCardListId, Guid? prevCardListId)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardListHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(movedCardListId);
				if (prevCardListId != null)
				{
					ArgumentValidation.CheckNotEmptyGuid(prevCardListId.Value);
				}

				var board = await _boardRepository.GetBoardByCardListIdAsync(movedCardListId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

				var movedList = await _cardListRepository.GetByIdAsyncIncludes(movedCardListId);
				ArgumentValidation.CheckNotNull(movedList, "Перемещаемый список карточек не найден");

				await _cardListRepository.MoveCardList(movedCardListId, prevCardListId);

				NotifyGroups((g) => g.ReceiveCardListMoved(movedCardListId, prevCardListId), boardId: movedList.BoardId);

				return null;
			}, _logger);
		}


		#endregion

		#region Remove card list
		/// <summary>
		/// Удаляет список карточек.
		/// </summary>
		/// <param name="cardListId">Идентификатор списка карточек.</param>
		/// <returns>Задача асинхронной операции.</returns>
		public async Task DeleteCardList(Guid cardListId)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardListHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardListId);

				var board = await _boardRepository.GetBoardByCardListIdAsync(cardListId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

				var cardList = await _cardListRepository.DeleteByIdAsync(cardListId);

				NotifyGroups((g) => g.ReceiveCardListDeleted(cardListId), boardId: cardList.BoardId, cardListId: cardListId);

				return null;
			}, _logger);
		}
		#endregion
	}
}


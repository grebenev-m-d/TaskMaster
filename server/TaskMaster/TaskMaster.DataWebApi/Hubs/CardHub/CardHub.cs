using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Asn1.Esf;
using System.Collections.Concurrent;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Extensions;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BoardRepository;
using TaskMaster.DataAccessModule.Repository.CardListRepository;
using TaskMaster.DataAccessModule.Repository.CardRepository;
using TaskMaster.DataAccessModule.Repository.DesignRepository;
using TaskMaster.DataAccessModule.Repository.FileRepository;
using TaskMaster.DataAccessModule.Repository.AccessLevelRole;
using TaskMaster.DataAccessModule.Repository.RoleRepository;
using TaskMaster.DataAccessModule.Repository.UserAccessLevelRepository;
using TaskMaster.DataAccessModule.Repository.UserRepository;
using TaskMaster.DataAccessModule.Service.DirectoryService;
using TaskMaster.DataAccessModule.Service.FileService;
using TaskMaster.DataWebApi.Helpers;
using TaskMaster.DataWebApi.Models;
using TaskMaster.Validation;
using Microsoft.AspNetCore.Authorization;

namespace TaskMaster.DataWebApi.Hubs.CardHub
{
	/// <summary>
	/// Хаб для работы с карточками.
	/// </summary>
	[Authorize]
	public partial class CardHub : Hub<ICardHubClient>
	{
		/// <summary>
		/// Словарь для отображения идентификатора соединения на JwtPayload.
		/// </summary>
		private static readonly ConcurrentDictionary<string, JwtPayload> _connectionIdToJwtTokenMap = new ConcurrentDictionary<string, JwtPayload>();

		/// <summary>
		/// Словарь для отображения идентификатора соединения на идентификатор доски.
		/// </summary>
		private static readonly ConcurrentDictionary<string, Guid> _connectionsIdToBoardIdMap = new ConcurrentDictionary<string, Guid>();

		/// <summary>
		/// Словарь для отображения идентификатора соединения на идентификатор карточки.
		/// </summary>
		private static readonly ConcurrentDictionary<string, Guid> _connectionsIdToCardIdMap = new ConcurrentDictionary<string, Guid>();

		/// <summary>
		/// Логгер.
		/// </summary>
		private readonly ILogger<CardHub> _logger;

		/// <summary>
		/// Репозиторий досок.
		/// </summary>
		private readonly IBoardRepository _boardRepository;

		/// <summary>
		/// Репозиторий списков карточек.
		/// </summary>
		private readonly ICardListRepository _cardListRepository;

		/// <summary>
		/// Репозиторий карточек.
		/// </summary>
		private readonly ICardRepository _cardRepository;

		/// <summary>
		/// Репозиторий вложений к карточкам.
		/// </summary>
		private readonly ICardAttachmentRepository _cardAttachmentRepository;

		/// <summary>
		/// Репозиторий файлов.
		/// </summary>
		private readonly IFileRepository _fileRepository;

		/// <summary>
		/// Сервис для работы с файлами.
		/// </summary>
		private readonly IFileService _fileService;

		/// <summary>
		/// Репозиторий уровней доступа пользователей.
		/// </summary>
		private readonly IUserAccessLevelRepository _userAccessLevelRepository;

		/// <summary>
		/// Конструктор класса <see cref="CardHub"/>.
		/// </summary>
		/// <param name="logger">Логгер.</param>
		/// <param name="boardRepository">Репозиторий досок.</param>
		/// <param name="cardListRepository">Репозиторий списков карточек.</param>
		/// <param name="cardRepository">Репозиторий карточек.</param>
		/// <param name="cardAttachmentRepository">Репозиторий вложений к карточкам.</param>
		/// <param name="fileRepository">Репозиторий файлов.</param>
		/// <param name="fileService">Сервис для работы с файлами.</param>
		/// <param name="userAccessLevelRepository">Репозиторий уровней доступа пользователей.</param>
		public CardHub(
			ILogger<CardHub> logger,
			IBoardRepository boardRepository,
			ICardListRepository cardListRepository,
			ICardRepository cardRepository,
			ICardAttachmentRepository cardAttachmentRepository,
			IFileRepository fileRepository,
			IFileService fileService,
			IUserAccessLevelRepository userAccessLevelRepository)
		{
			_logger = logger;
			_boardRepository = boardRepository;
			_cardListRepository = cardListRepository;
			_cardRepository = cardRepository;
			_fileRepository = fileRepository;
			_cardAttachmentRepository = cardAttachmentRepository;
			_fileService = fileService;
			_userAccessLevelRepository = userAccessLevelRepository;
		}

		/// <summary>
		/// Обработчик события отключения клиента.
		/// </summary>
		/// <param name="exception">Исключение.</param>
		/// <returns>Асинхронная задача.</returns>
		public override Task OnDisconnectedAsync(Exception? exception)
		{
			_connectionIdToJwtTokenMap.TryRemove(Context.ConnectionId, out _);
			_connectionsIdToBoardIdMap.TryRemove(Context.ConnectionId, out _);
			_connectionsIdToCardIdMap.TryRemove(Context.ConnectionId, out _);

			return base.OnDisconnectedAsync(exception);
		}

		/// <summary>
		/// Обработчик события подключения клиента.
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

			// Сохраняем данные из токена в словаре.
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
			// Пробуем получить идентификатор карточки.
			if (Context.GetHttpContext().Request.Query.TryGetValue("card_id", out var cardValues) &&
				!string.IsNullOrEmpty(cardValues.FirstOrDefault()) &&
				Guid.TryParse(cardValues.FirstOrDefault(), out var cardId))
			{
				// Добавляем идентификатор соединения в группу, относящуюся к этой карточке.
				_connectionsIdToCardIdMap.TryAdd(Context.ConnectionId, cardId);
				await Groups.AddToGroupAsync(Context.ConnectionId, cardId.ToString());
				return;
			}

			Context.Abort();
			return;
		}

		/// <summary>
		/// Уведомляет группы о событии.
		/// </summary>
		/// <param name="func">Действие для выполнения.</param>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="connectionIds">Список идентификаторов соединений.</param>
		public void NotifyGroups(
			Action<ICardHubClient> func,
			Guid? boardId = null, Guid? cardId = null, IReadOnlyList<string> connectionIds = null)
		{
			if (boardId != null)
			{
				if (connectionIds == null)
				{
					func(Clients.Group(boardId.ToString()));
				}
				else
				{
					func(Clients.GroupExcept(boardId.ToString(), connectionIds));
				}
			}
			if (cardId != null)
			{
				if (connectionIds == null)
				{
					func(Clients.Group(cardId.ToString()));
				}
				else
				{
					func(Clients.GroupExcept(cardId.ToString(), connectionIds));
				}
			}
		}

		/// <summary>
		/// Создает карточку.
		/// </summary>
		/// <param name="cardListId">Идентификатор списка карточек.</param>
		/// <param name="title">Заголовок карточки.</param>
		/// <returns>Асинхронная задача.</returns>
		#region Create card
		public async Task CreateCard(Guid cardListId, string title)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardListId);
				ArgumentValidation.CheckNotNullOrEmpty(title, "Заголовок карточки не должен быть пустым.");

				var board = await _boardRepository.GetBoardByCardListIdAsync(cardListId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

				var addedCard = await _cardRepository.AddAsync(
					new DbCard()
					{
						Title = title,
						CardListId = cardListId
					}
				);

				var cardList = await _cardListRepository.GetByIdAsyncIncludes(cardListId);

				NotifyGroups((g) => g.ReceiveCardCreated(cardListId, DbModelMappers.MapDbToCard(addedCard)), boardId: cardList.BoardId);

				return null;
			}, _logger);
		}
		#endregion


		#region Read card

		/// <summary>
		/// Получает карточку по идентификатору.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <returns>Асинхронная задача, возвращающая карточку.</returns>
		public async Task<Card> GetCard(Guid cardId)
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Card, CardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardId);

				var board = await _boardRepository.GetBoardByCardIdAsync(cardId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.reader);

				var card = await _cardRepository.GetByIdAsyncIncludes(cardId);
				ArgumentValidation.CheckNotNull(card, "Такой карточки не существует");

				return DbModelMappers.MapDbToCard(card);

			}, _logger);
		}

		/// <summary>
		/// Получает список карточек по идентификатору списка.
		/// </summary>
		/// <param name="cardListId">Идентификатор списка карточек.</param>
		/// <returns>Асинхронная задача, возвращающая список карточек.</returns>
		public async Task<List<Card>> GetCards(Guid cardListId)
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<List<Card>, CardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardListId);

				var board = await _boardRepository.GetBoardByCardListIdAsync(cardListId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.reader);

				var cardList = await _cardListRepository.GetByIdAsyncIncludes(cardListId);

				return DbModelMappers.MapDbToCard(cardList.Cards.SortByRelationship().ToList());
			}, _logger);
		}

		/// <summary>
		/// Обновляет заголовок карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="title">Новый заголовок карточки.</param>
		/// <returns>Асинхронная задача.</returns>
		public async Task UpdateCardTitle(Guid cardId, string title)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardId);
				ArgumentValidation.CheckNotNull(title, "Заголовок карточки не должен быть пустым");

				var board = await _boardRepository.GetBoardByCardIdAsync(cardId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

				var card = await _cardRepository.GetByIdAsyncIncludes(cardId);
				card.Title = title;

				await _cardRepository.UpdateAsync(card);

				var cardList = await _cardListRepository.GetByIdAsyncIncludes(card.CardListId.Value);

				NotifyGroups((g) => g.ReceiveCardTitleUpdated(cardId, title), boardId: cardList.BoardId, cardId: cardId);

				return null;
			}, _logger);
		}

		/// <summary>
		/// Обновляет описание карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="description">Новое описание карточки.</param>
		/// <returns>Асинхронная задача.</returns>
		public async Task UpdateCardDescription(Guid cardId, string description)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardId, "Не указано Id карточки");
				ArgumentValidation.CheckNotNull(description, "Описание карточки не должно быть равно null");

				var board = await _boardRepository.GetBoardByCardIdAsync(cardId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

				var card = await _cardRepository.GetByIdAsyncIncludes(cardId);
				card.Description = description;

				await _cardRepository.UpdateAsync(card);

				var cardList = await _cardListRepository.GetByIdAsyncIncludes(card.CardListId.Value);

				NotifyGroups((g) => g.ReceiveCardDescriptionUpdated(cardId, description), boardId: cardList.BoardId, cardId: cardId);

				return null;
			}, _logger);
		}


		/// <summary>
		/// Перемещает карточку в указанный список и позицию.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="currentCardListId">Идентификатор текущего списка карточек.</param>
		/// <param name="movedCardId">Идентификатор перемещаемой карточки.</param>
		/// <param name="prevCardId">Идентификатор карточки, перед которой нужно вставить перемещаемую карточку (если есть).</param>
		/// <returns>Асинхронная задача.</returns>
		public async Task MoveCard(Guid boardId, Guid currentCardListId, Guid movedCardId, Guid? prevCardId)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(boardId);
				ArgumentValidation.CheckNotEmptyGuid(currentCardListId);
				ArgumentValidation.CheckNotEmptyGuid(movedCardId);
				if (prevCardId != null)
				{
					ArgumentValidation.CheckNotEmptyGuid(prevCardId.Value);
				}

				var currentCardList = await _cardListRepository.GetByIdAsyncIncludes(currentCardListId);
				ArgumentValidation.CheckNotNull(currentCardList);

				await _cardRepository.MoveCard(currentCardListId, movedCardId, prevCardId);

				NotifyGroups((g) => g.ReceiveCardMoved(boardId, currentCardListId, movedCardId, prevCardId),
					boardId: boardId,
					connectionIds: new List<string>() { Context.ConnectionId });

				return null;
			}, _logger);
		}

		#endregion

		#region Remove card

		/// <summary>
		/// Удаляет обложку карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <returns>Асинхронная задача.</returns>
		public async Task DeleteCardCoverImage(Guid cardId)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardId);

				var board = await _boardRepository.GetBoardByCardIdAsync(cardId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

				var card = await _cardRepository.GetByIdAsyncIncludes(cardId);
				ArgumentValidation.CheckNotNull(card, "Карточка с указанным Id не найдена.");

				ArgumentValidation.CheckNotNull(card.ImageFile, "Карточка не имеет обложки.");

				var cardList = await _cardListRepository.GetByIdAsyncIncludes(card.CardListId.Value);

				var imageFile = card.ImageFile;

				card.ImageFileId = null;
				card.ImageFile = null;
				await _cardRepository.UpdateAsync(card);

				await _fileRepository.DeleteByIdAsync(imageFile.Id);
				await _fileService.DeleteFile(FilePathHelper.GetFullPath(imageFile.RelativePath));

				NotifyGroups((g) => g.ReceiveCardCoverImageDeleted(cardId), boardId: cardList.BoardId, cardId: card.Id);

				return null;
			}, _logger);
		}

		/// <summary>
		/// Удаляет вложение карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="cardAttachmentId">Идентификатор вложения карточки.</param>
		/// <returns>Асинхронная задача.</returns>
		public async Task DeleteCardAttachment(Guid cardId, Guid cardAttachmentId)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardId);
				ArgumentValidation.CheckNotEmptyGuid(cardAttachmentId);

				var board = await _boardRepository.GetBoardByCardIdAsync(cardId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

				var card = await _cardRepository.GetByIdAsyncIncludes(cardId);
				ArgumentValidation.CheckNotNull(card, "Карточка с указанным Id не найдена.");

				var attachment = await _cardAttachmentRepository.DeleteByIdAsync(cardAttachmentId);
				ArgumentValidation.CheckNotNull(attachment, "Вложение карточки с указанным Id не найдено.");

				await _fileService.DeleteFile(FilePathHelper.GetFullPath(attachment.File.RelativePath));

				NotifyGroups((g) => g.ReceiveCardAttachmentDeleted(cardId, cardAttachmentId), cardId: card.Id);

				return null;
			}, _logger);
		}

		/// <summary>
		/// Удаляет карточку.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <returns>Асинхронная задача.</returns>
		public async Task DeleteCard(Guid cardId)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardId);

				var board = await _boardRepository.GetBoardByCardIdAsync(cardId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

				var card = await _cardRepository.GetByIdAsyncIncludes(cardId);

				await _cardRepository.DeleteByIdAsync(cardId);

				var cardList = await _cardListRepository.GetByIdAsyncIncludes(card.CardListId.Value);

				NotifyGroups((g) => g.ReceiveCardDeleted(cardId), boardId: cardList.BoardId);

				return null;
			}, _logger);
		}
		#endregion


	}
}

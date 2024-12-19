using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BoardRepository;
using TaskMaster.DataAccessModule.Repository.CardCommentRepository;
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

namespace TaskMaster.DataWebApi.Hubs.CardCommentHub
{
	/// <summary>
	/// Хаб комментариев карточек.
	/// </summary>
	[Authorize]
	public class CardCommentHub : Hub<ICardCommentHubClient>
	{
		/// <summary>
		/// Словарь для отображения соответствия идентификатора подключения и JWT-токена.
		/// </summary>
		private static readonly ConcurrentDictionary<string, JwtPayload> _connectionIdToJwtTokenMap = new ConcurrentDictionary<string, Models.JwtPayload>();

		/// <summary>
		/// Словарь для отображения соответствия идентификатора подключения и идентификатора карточки.
		/// </summary>
		private static readonly ConcurrentDictionary<string, Guid> _connectionsIdToCardIdMap = new ConcurrentDictionary<string, Guid>();

		/// <summary>
		/// Словарь для отображения соответствия идентификатора подключения и идентификатора комментария.
		/// </summary>
		private static readonly ConcurrentDictionary<string, Guid> _connectionsIdToCommentIdMap = new ConcurrentDictionary<string, Guid>();

		/// <summary>
		/// Логгер.
		/// </summary>
		private readonly ILogger<CardCommentHub> _logger;

		/// <summary>
		/// Репозиторий досок.
		/// </summary>
		private readonly IBoardRepository _boardRepository;

		/// <summary>
		/// Репозиторий пользователей.
		/// </summary>
		private readonly IUserRepository _userRepository;

		/// <summary>
		/// Репозиторий комментариев карточек.
		/// </summary>
		private readonly ICardCommentRepository _cardCommentRepository;

		/// <summary>
		/// Репозиторий уровней доступа пользователей.
		/// </summary>
		private readonly IUserAccessLevelRepository _userAccessLevelRepository;

		/// <summary>
		/// Конструктор хаба комментариев карточек.
		/// </summary>
		/// <param name="logger">Логгер.</param>
		/// <param name="boardRepository">Репозиторий досок.</param>
		/// <param name="userRepository">Репозиторий пользователей.</param>
		/// <param name="cardCommentRepository">Репозиторий комментариев карточек.</param>
		/// <param name="userAccessLevelRepository">Репозиторий уровней доступа пользователей.</param>
		public CardCommentHub(
			ILogger<CardCommentHub> logger,
			IBoardRepository boardRepository,
			IUserRepository userRepository,
			ICardCommentRepository cardCommentRepository,
			IUserAccessLevelRepository userAccessLevelRepository)
		{
			_logger = logger;
			_boardRepository = boardRepository;
			_userRepository = userRepository;
			_cardCommentRepository = cardCommentRepository;
			_userAccessLevelRepository = userAccessLevelRepository;
		}

		/// <summary>
		/// Вызывается при подключении клиента к хабу.
		/// </summary>
		/// <returns>Асинхронная задача.</returns>
		public async override Task OnConnectedAsync()
		{
			// Извлекаем данные из JWT-токена.  
			if (!Context.GetHttpContext().Request.Query.TryGetValue("access_token", out var tokenValues) ||
				string.IsNullOrEmpty(tokenValues.FirstOrDefault()))
			{
				Context.Abort();
				return;
			}

			// Сохранить данные из токена в словаре.
			var jwtPayload = new Models.JwtPayload(tokenValues.FirstOrDefault());
			_connectionIdToJwtTokenMap[Context.ConnectionId] = jwtPayload;

			// Пробуем получить Id карточки.
			if (Context.GetHttpContext().Request.Query.TryGetValue("card_id", out var cardValues) &&
				!string.IsNullOrEmpty(cardValues.FirstOrDefault()) &&
				Guid.TryParse(cardValues.FirstOrDefault(), out var cardId))
			{
				// Добавляем Id соединения в группу, относящуюся к этой карточке.
				_connectionsIdToCardIdMap.TryAdd(Context.ConnectionId, cardId);
				await Groups.AddToGroupAsync(Context.ConnectionId, cardId.ToString());

				return;
			}

			//// Пробуем получить Id комментария.
			//if (Context.GetHttpContext().Request.Query.TryGetValue("comment_id", out var commentValues) &&
			//	!string.IsNullOrEmpty(commentValues.FirstOrDefault()) &&
			//	Guid.TryParse(commentValues.FirstOrDefault(), out var commentId))
			//{
			//	// Добавляем Id соединения в группу, относящуюся к этому комментарию.
			//	_connectionsIdToCommentIdMap.TryAdd(Context.ConnectionId, commentId);
			//	await Groups.AddToGroupAsync(Context.ConnectionId, commentId.ToString());

			//	return;
			//}

			Context.Abort();
		}

		/// <summary>
		/// Вызывается при отключении клиента от хаба.
		/// </summary>
		/// <param name="exception">Исключение, приведшее к отключению.</param>
		/// <returns>Асинхронная задача.</returns>
		public override Task OnDisconnectedAsync(Exception? exception)
		{
			_connectionIdToJwtTokenMap.TryRemove(Context.ConnectionId, out _);
			_connectionsIdToCardIdMap.TryRemove(Context.ConnectionId, out _);
			_connectionsIdToCommentIdMap.TryRemove(Context.ConnectionId, out _);

			return base.OnDisconnectedAsync(exception);
		}

		/// <summary>
		/// Уведомляет группы о событии.
		/// </summary>
		/// <param name="func">Действие для уведомления.</param>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="commentId">Идентификатор комментария.</param>
		public void NotifyGroups(
			Action<ICardCommentHubClient> func,
			Guid? cardId = null, Guid? commentId = null)
		{
			if (cardId != null)
			{
				func(Clients.Group(cardId.ToString()));
			}
			if (commentId != null)
			{
				func(Clients.Group(commentId.ToString()));
			}
		}

		/// <summary>
		/// Создает комментарий карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="text">Текст комментария.</param>
		/// <returns>Асинхронная задача.</returns>
		public async Task CreateCardComment(Guid cardId, string text)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardCommentHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardId);
				ArgumentValidation.CheckNotNullOrEmpty(text, "Комментарий не должен быть пустым.");

				var board = await _boardRepository.GetBoardByCardIdAsync(cardId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

				var user = await _userRepository.GetByIdAsyncIncludes(payload.Sub);
				var addedComment = await _cardCommentRepository.AddAsync(
					new DbCardComment()
					{
						Text = text,
						CardId = cardId,
						UserId = payload.Sub,
					}
				);

				addedComment.User = user;

				NotifyGroups((g) => g.ReceiveCardCommentCreated(cardId, DbModelMappers.MapDbToComment(addedComment)), cardId: addedComment.CardId);

				return null;
			}, _logger);
		}

		/// <summary>
		/// Получает комментарии карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="pageNumber">Номер страницы.</param>
		/// <param name="pageSize">Размер страницы.</param>
		/// <returns>Список комментариев карточки.</returns>
		public async Task<List<CardComment>> GetCardComments(Guid cardId, int pageNumber, int pageSize)
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<List<CardComment>, CardCommentHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardId);
				ArgumentValidation.CheckPositiveNumber(pageNumber, "Такой страницы нет.");
				ArgumentValidation.CheckPositiveNumber(pageSize, "Нумерация страниц начинается с первой.");

				int skip = (pageNumber - 1) * pageSize;
				int take = pageSize;

				var board = await _boardRepository.GetBoardByCardIdAsync(cardId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id),
					AccessLevelType.reader);

				var cardComments = await _cardCommentRepository.GetAllByCardIdAsync(cardId);

				return DbModelMappers.MapDbToComment(cardComments
					.Skip(skip)
					.Take(take)
					.OrderBy(i => i.CreatedAt)
					.ToList());
			}, _logger);
		}

		/// <summary>
		/// Получает количество комментариев карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <returns>Количество комментариев карточки.</returns>
		public async Task<int> GetCardCommentsCount(Guid cardId)
		{
			return await ExceptionHelper.ExecuteWithExceptionHandlingAsync<int, CardCommentHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardId);

				var board = await _boardRepository.GetBoardByCardIdAsync(cardId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					 await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id),
					AccessLevelType.reader);

				var cardComments = await _cardCommentRepository.GetAllByCardIdAsync(cardId);

				return cardComments.Count();
			}, _logger);
		}

		/// <summary>
		/// Обновляет текст комментария карточки.
		/// </summary>
		/// <param name="cardCommentId">Идентификатор комментария карточки.</param>
		/// <param name="text">Текст комментария.</param>
		/// <returns>Асинхронная задача.</returns>
		public async Task UpdateText(Guid cardCommentId, string text)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardCommentHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardCommentId);
				ArgumentValidation.CheckNotNullOrEmpty(text, "Комментарий не должен быть пустым.");

				var board = await _boardRepository.GetBoardByCardCommentIdAsync(cardCommentId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

				var cardComment = await _cardCommentRepository.GetByIdAsyncIncludes(cardCommentId);
				cardComment.Text = text;
				cardComment.UpdatedAt = DateTime.Now;

				await _cardCommentRepository.UpdateAsync(cardComment);

				NotifyGroups((g) => g.ReceiveTextUpdated(cardCommentId, text), cardId: cardComment.CardId, commentId: cardCommentId);

				return null;
			}, _logger);
		}

		/// <summary>
		/// Удаляет комментарий карточки.
		/// </summary>
		/// <param name="cardCommentId">Идентификатор комментария карточки.</param>
		/// <returns>Асинхронная задача.</returns>
		public async Task DeleteCardComment(Guid cardCommentId)
		{
			await ExceptionHelper.ExecuteWithExceptionHandlingAsync<Task, CardCommentHub>(async () =>
			{
				ArgumentValidation.CheckNotEmptyGuid(cardCommentId);

				var board = await _boardRepository.GetBoardByCardCommentIdAsync(cardCommentId);
				var payload = _connectionIdToJwtTokenMap[Context.ConnectionId];
				AccessControl.CheckAccessLevel(
					await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

				var cardComment = await _cardCommentRepository.DeleteByIdAsync(cardCommentId);
				

				NotifyGroups((g) => g.ReceiveCardCommentDeleted(cardCommentId), cardId: cardComment.CardId, commentId: cardComment.Id);

				return null;
			}, _logger);
		}
	}
}

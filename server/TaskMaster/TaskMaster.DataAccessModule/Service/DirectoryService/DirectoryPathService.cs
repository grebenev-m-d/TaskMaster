using TaskMaster.DataAccessModule.Repository.BoardRepository;
using TaskMaster.DataAccessModule.Repository.CardListRepository;
using TaskMaster.DataAccessModule.Repository.CardRepository;

namespace TaskMaster.DataAccessModule.Service.DirectoryService
{
	/// <summary>
	/// Сервис для работы с путями к директориям.
	/// </summary>
	public class DirectoryPathService : IDirectoryPathService
	{
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
		/// Инициализирует новый экземпляр класса <see cref="DirectoryPathService"/>.
		/// </summary>
		/// <param name="boardRepository">Репозиторий досок.</param>
		/// <param name="cardListRepository">Репозиторий списков карточек.</param>
		/// <param name="cardRepository">Репозиторий карточек.</param>
		public DirectoryPathService(IBoardRepository boardRepository,
			ICardListRepository cardListRepository,
			ICardRepository cardRepository)
		{
			_boardRepository = boardRepository;
			_cardListRepository = cardListRepository;
			_cardRepository = cardRepository;
		}

		/// <summary>
		/// Корневая папка.
		/// </summary>
		private const string RootFolder = "Files";

		/// <summary>
		/// Папка пользователей.
		/// </summary>
		private const string UserFolder = "Users";

		/// <summary>
		/// Папка досок.
		/// </summary>
		private const string BoardFolder = "Boards";

		/// <summary>
		/// Папка списков карточек.
		/// </summary>
		private const string CardListFolder = "CardLists";

		/// <summary>
		/// Папка карточек.
		/// </summary>
		private const string CardFolder = "Cards";

		/// <summary>
		/// Возвращает путь к директории доски по её идентификатору.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Путь к директории доски.</returns>
		/// <exception cref="Exception">Вызывается, если доска не найдена.</exception>
		public async Task<string> GetBoardFolderPath(Guid boardId)
		{
			var board = await _boardRepository.GetByIdAsyncIncludes(boardId);
			if (board == null)
			{
				throw new Exception("Такой доски нет");
			}

			return @$"{RootFolder}\{UserFolder}\{board.UserId}\{BoardFolder}\{board.UserId}";
		}

		/// <summary>
		/// Возвращает путь к директории карточки по её идентификатору.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <returns>Путь к директории карточки.</returns>
		/// <exception cref="Exception">Вызывается, если карточка не найдена.</exception>
		public async Task<string> GetCardFolderPath(Guid cardId)
		{
			var card = await _cardRepository.GetByIdAsyncIncludes(cardId);
			if (card == null)
			{
				throw new Exception("Такой карточки нет");
			}

			var cardList = await _cardListRepository.GetByIdAsyncIncludes(card.CardListId.Value);

			var board = await _boardRepository.GetByIdAsyncIncludes(cardList.BoardId.Value);

			return @$"{RootFolder}\{UserFolder}\{board.UserId}\{BoardFolder}\{board.UserId}\{CardListFolder}\{card.CardListId}\{CardFolder}\{card.Id}";
		}
	}
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BoardRepository;
using TaskMaster.DataAccessModule.Repository.CardListRepository;
using TaskMaster.DataAccessModule.Repository.CardRepository;
using TaskMaster.DataAccessModule.Repository.FileRepository;
using TaskMaster.DataAccessModule.Repository.UserAccessLevelRepository;
using TaskMaster.DataAccessModule.Service.DirectoryService;
using TaskMaster.DataAccessModule.Service.FileService;
using TaskMaster.DataWebApi.Helpers;
using TaskMaster.DataWebApi.Hubs.CardHub;
using TaskMaster.DataWebApi.Models;
using TaskMaster.Validation;

namespace TaskMaster.DataWebApi.Controllers
{
	/// <summary>
	/// Контроллер для управления карточками.
	/// </summary>
	[Route("api/card")]
	[ApiController]
	[Authorize]
	public class CardController : ControllerBase
	{
		/// <summary>
		/// Контекст хаба для управления подключениями карточек.
		/// </summary>
		private readonly IHubContext<CardHub> _hubContext;

		/// <summary>
		/// Репозиторий досок.
		/// </summary>
		private readonly IBoardRepository _boardRepository;

		/// <summary>
		/// Сервис файлов.
		/// </summary>
		private readonly IFileService _fileService;

		/// <summary>
		/// Репозиторий карточек.
		/// </summary>
		private readonly ICardRepository _cardRepository;

		/// <summary>
		/// Сервис для работы с путями каталогов.
		/// </summary>
		private readonly IDirectoryPathService _directoryPathService;

		/// <summary>
		/// Репозиторий файлов.
		/// </summary>
		private readonly IFileRepository _fileRepository;

		/// <summary>
		/// Репозиторий уровней доступа пользователей.
		/// </summary>
		private readonly IUserAccessLevelRepository _userAccessLevelRepository;

		/// <summary>
		/// Репозиторий вложений к карточкам.
		/// </summary>
		private readonly ICardAttachmentRepository _cardAttachmentRepository;

		/// <summary>
		/// Репозиторий списков карточек.
		/// </summary>
		private readonly ICardListRepository _cardListRepository;

		/// <summary>
		/// Конструктор контроллера карточек.
		/// </summary>
		/// <param name="hubContext">Контекст хаба.</param>
		/// <param name="boardRepository">Репозиторий досок.</param>
		/// <param name="fileService">Сервис файлов.</param>
		/// <param name="cardRepository">Репозиторий карточек.</param>
		/// <param name="directoryPathService">Сервис для работы с путями каталогов.</param>
		/// <param name="fileRepository">Репозиторий файлов.</param>
		/// <param name="userAccessLevelRepository">Репозиторий уровней доступа пользователей.</param>
		/// <param name="cardAttachmentRepository">Репозиторий вложений к карточкам.</param>
		/// <param name="cardListRepository">Репозиторий списков карточек.</param>
		public CardController(IHubContext<CardHub> hubContext,
						IBoardRepository boardRepository,
			IFileService fileService,
			ICardRepository cardRepository,
			IDirectoryPathService directoryPathService,
			IFileRepository fileRepository,
			IUserAccessLevelRepository userAccessLevelRepository,
			ICardAttachmentRepository cardAttachmentRepository,
				ICardListRepository cardListRepository)
		{
			_boardRepository = boardRepository;
			_cardListRepository = cardListRepository;
			_hubContext = hubContext;
			_fileRepository = fileRepository;
			_fileService = fileService;
			_cardRepository = cardRepository;
			_directoryPathService = directoryPathService;
			_userAccessLevelRepository = userAccessLevelRepository;
			_cardAttachmentRepository = cardAttachmentRepository;
		}

		/// <summary>
		/// Метод для добавления вложенного файла к карточке.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="file">Файл для добавления.</param>
		/// <returns>Результат операции.</returns>
		[HttpPost("add-attachment-file")]
		[Authorize]
		public async Task<IActionResult> AddAttachmentFile(Guid cardId, IFormFile file)
		{
			// Проверка наличия идентификатора карточки.
			ArgumentValidation.CheckNotEmptyGuid(cardId);
			// Проверка наличия переданного файла.
			ArgumentValidation.CheckNotNull(file, "Файл не был передан.");

			// Получение заголовка авторизации.
			var authorizationHeader = Request.Headers["Authorization"];
			if (authorizationHeader.Count == 0)
			{
				return BadRequest("Отсутствующий заголовок авторизации");
			}

			// Извлечение токена из заголовка авторизации.
			var token = authorizationHeader.FirstOrDefault().Split(' ').Last();

			// Получение доски по идентификатору карточки.
			var board = await _boardRepository.GetBoardByCardIdAsync(cardId);
			var payload = new JwtPayload(token);
			// Проверка уровня доступа к доске.
			AccessControl.CheckAccessLevel(
				await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

			// Получение карточки по идентификатору.
			var card = await _cardRepository.GetByIdAsyncIncludes(cardId);
			// Проверка наличия карточки.
			ArgumentValidation.CheckNotNull(card, "Карточка с таким Id не найдена.");

			// Получение относительного пути к папке карточки.
			var relativePath = await _directoryPathService.GetCardFolderPath(cardId);

			// Получение полного пути сохранения файла.
			var fullPath = FilePathHelper.GetFullPath(relativePath);
			// Сохранение файла.
			fullPath = await _fileService.SaveFile(file, fullPath);
			var fileName = Path.GetFileName(fullPath);

			// Создание вложенного файла для карточки.
			var cardAttachment = await _cardAttachmentRepository.AddAsync(new DbCardAttachment()
			{
				File = new DbFile()
				{
					RelativePath = fullPath,
					FileName = fileName,
				},
				CardId = cardId,
			});

			// Обновление карточки.
			await _cardRepository.UpdateAsync(card);

			// Отправка уведомлений о добавлении вложения карточки.
			await _hubContext.Clients.Group(card.CardListId.ToString()).SendAsync(
				"ReceiveCardAttachmentAdded", cardId, DbModelMappers.MapDbToCardAttachment(cardAttachment));

			await _hubContext.Clients.Group(cardId.ToString()).SendAsync(
				"ReceiveCardAttachmentAdded", cardId, DbModelMappers.MapDbToCardAttachment(cardAttachment));

			return Ok();
		}

		/// <summary>
		/// Метод для получения вложенного файла к карточке.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="attachmentId">Идентификатор вложенного файла.</param>
		/// <returns>Результат операции.</returns>
		[HttpGet("get-attachment-file")]
		[Authorize]
		public async Task<IActionResult> GetAttachmentFile(Guid cardId, Guid attachmentId)
		{
			// Проверка наличия идентификатора карточки и идентификатора вложенного файла.
			ArgumentValidation.CheckNotEmptyGuid(cardId);
			ArgumentValidation.CheckNotEmptyGuid(attachmentId);

			// Получение заголовка авторизации.
			var authorizationHeader = Request.Headers["Authorization"];
			if (authorizationHeader.Count == 0)
			{
				return BadRequest("Missing Authorization header");
			}
			// Извлечение токена из заголовка авторизации.
			var token = authorizationHeader.FirstOrDefault().Split(' ').Last();

			// Получение доски по идентификатору карточки.
			var board = await _boardRepository.GetBoardByCardIdAsync(cardId);
			var payload = new JwtPayload(token);
			// Проверка уровня доступа к доске.
			AccessControl.CheckAccessLevel(
				await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

			// Получение карточки по идентификатору.
			var card = await _cardRepository.GetByIdAsyncIncludes(cardId);
			// Проверка наличия карточки.
			ArgumentValidation.CheckNotNull(card, "Карточка с таким Id не найдена.");

			// Получение вложенного файла карточки по идентификатору.
			var attachment = card.CardAttachments.FirstOrDefault(i => i.Id == attachmentId);
			var fullPath = FilePathHelper.GetFullPath(attachment.File.RelativePath);

			// Проверяем, существует ли файл.
			if (!System.IO.File.Exists(fullPath))
			{
				return NotFound(); // Если файл не найден, возвращаем статус 404 Not Found.
			}

			// Определение MIME-типа файла.
			new FileExtensionContentTypeProvider().TryGetContentType(attachment.File.FileName, out var contentType);
			contentType = contentType ?? "application/octet-stream";

			// Чтение файла и возвращение его как FileStreamResult.
			var fileStream = System.IO.File.OpenRead(fullPath);

			var fileStreamResult = new FileStreamResult(fileStream, contentType);
			fileStreamResult.FileDownloadName = attachment.File.FileName;
			// Добавление заголовка для предоставления доступа к заголовку Content-Disposition.
			Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
			return fileStreamResult;
		}

		/// <summary>
		/// Метод для добавления обложки к карточке.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="file">Файл обложки.</param>
		/// <returns>Результат операции.</returns>
		[HttpPost("add-cover-image")]
		public async Task<IActionResult> AddCoverImage(Guid cardId, IFormFile file)
		{
			// Проверка наличия идентификатора карточки и файла.
			ArgumentValidation.CheckNotEmptyGuid(cardId);
			ArgumentValidation.CheckNotNull(file, "Файл не был передан.");

			// Получение заголовка авторизации.
			var authorizationHeader = Request.Headers["Authorization"];
			if (authorizationHeader.Count == 0)
			{
				return BadRequest("Missing Authorization header");
			}
			// Извлечение токена из заголовка авторизации.
			var token = authorizationHeader.FirstOrDefault().Split(' ').Last();

			// Получение доски, которой принадлежит карточка.
			var board = await _boardRepository.GetBoardByCardIdAsync(cardId);
			var payload = new JwtPayload(token);
			// Проверка уровня доступа к доске.
			AccessControl.CheckAccessLevel(
				await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.editor);

			// Определение MIME-типа файла.
			new FileExtensionContentTypeProvider().TryGetContentType(file.FileName, out var contentType);
			// Проверка MIME-типа файла.
			if (contentType != "image/jpeg")
			{
				return BadRequest("Missing Authorization header");
			}

			// Получение карточки по идентификатору.
			var card = await _cardRepository.GetByIdAsyncIncludes(cardId);
			// Проверка наличия карточки.
			ArgumentValidation.CheckNotNull(card, "Карточка с таким Id не найдена.");
			// Получение списка карточек по идентификатору.
			var cardList = await _cardListRepository.GetByIdAsyncIncludes(card.CardListId.Value);
			var relativePath = await _directoryPathService.GetCardFolderPath(cardId);

			var fullPath = FilePathHelper.GetFullPath(relativePath);
			fullPath = await _fileService.SaveFile(file, fullPath);
			var fileName = Path.GetFileName(fullPath);

			//board.ImageFileId = boardId;

			// Проверка, имеет ли карточка уже обложку.
			if (card.ImageFile == null)
			{
				var newFile = new DbFile()
				{
					RelativePath = fullPath,
					FileName = fileName,
				};

				await _fileRepository.AddAsync(newFile);
				card.ImageFile = newFile;
			}
			else
			{
				card.ImageFile.RelativePath = fullPath;
				card.ImageFile.FileName = fileName;
			}

			await _cardRepository.UpdateAsync(card);

			// Отправка уведомлений об обновлении обложки карточки.
			await _hubContext.Clients.Group(cardList.BoardId.ToString()).SendAsync(
				"ReceiveCardCoverImageAdded", card.Id);

			await _hubContext.Clients.Group(card.Id.ToString()).SendAsync(
				"ReceiveCardCoverImageAdded", card.Id);

			return Ok();
		}


		/// <summary>
		/// Метод для получения обложки карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <returns>Результат операции.</returns>
		[HttpGet("get-cover-image")]
		public async Task<IActionResult> GetCoverImage(Guid cardId)
		{
			// Проверка наличия идентификатора карточки.
			ArgumentValidation.CheckNotEmptyGuid(cardId);

			// Получение заголовка авторизации.
			var authorizationHeader = Request.Headers["Authorization"];
			if (authorizationHeader.Count == 0)
			{
				return BadRequest("Missing Authorization header");
			}
			// Извлечение токена из заголовка авторизации.
			var token = authorizationHeader.FirstOrDefault().Split(' ').Last();

			// Получение доски, которой принадлежит карточка.
			var board = await _boardRepository.GetBoardByCardIdAsync(cardId);
			var payload = new JwtPayload(token);
			// Проверка уровня доступа к доске.
			AccessControl.CheckAccessLevel(
				await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, board.Id), AccessLevelType.reader);

			// Получение карточки по идентификатору.
			var card = await _cardRepository.GetByIdAsyncIncludes(cardId);
			// Проверка наличия карточки.
			ArgumentValidation.CheckNotNull(card, "Карточка с таким Id не найдена.");

			// Проверка наличия обложки к карточке.
			if (card.ImageFile == null)
			{
				return NotFound();
			}

			var imageFullFile = FilePathHelper.GetFullPath(card.ImageFile.RelativePath);

			// Проверяем, существует ли файл.
			if (!System.IO.File.Exists(imageFullFile))
			{
				return NotFound(); // Если файл не найден, возвращаем статус 404 Not Found
			}

			// Определение MIME-типа файла.
			new FileExtensionContentTypeProvider().TryGetContentType(card.ImageFile.FileName, out var contentType);
			contentType = contentType ?? "application/octet-stream";

			// Чтение файла и возврат его как FileStreamResult.
			var fileStream = System.IO.File.OpenRead(imageFullFile);

			var fileStreamResult = new FileStreamResult(fileStream, contentType);
			fileStreamResult.FileDownloadName = card.ImageFile.FileName;
			Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
			return fileStreamResult;
		}
	}
}

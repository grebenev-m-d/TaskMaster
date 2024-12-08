using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BoardRepository;
using TaskMaster.DataAccessModule.Repository.FileRepository;
using TaskMaster.DataAccessModule.Repository.UserAccessLevelRepository;
using TaskMaster.DataAccessModule.Service.DirectoryService;
using TaskMaster.DataAccessModule.Service.FileService;
using TaskMaster.DataWebApi.Helpers;
using TaskMaster.DataWebApi.Hubs.BoardHub;
using TaskMaster.DataWebApi.Models;
using TaskMaster.Validation;

namespace TaskMaster.DataWebApi.Controllers
{
	/// <summary>
	/// Контроллер для управления досками.
	/// </summary>
	[Route("api/board")]
	[ApiController]
	[Authorize]
	public class BoardController : ControllerBase
	{
		/// <summary>
		/// Контекст хаба доски.
		/// </summary>
		private readonly IHubContext<BoardHub> _hubContext;

		/// <summary>
		/// Сервис для работы с файлами.
		/// </summary>
		private readonly IFileService _fileService;

		/// <summary>
		/// Репозиторий досок.
		/// </summary>
		private readonly IBoardRepository _boardRepository;

		/// <summary>
		/// Сервис для работы с путями каталогов и файлов.
		/// </summary>
		private readonly IDirectoryPathService _directoryPathService;

		/// <summary>
		/// Репозиторий файлов.
		/// </summary>
		private readonly IFileRepository _fileRepository;

		/// <summary>
		/// Репозиторий уровней доступа пользователей к доске.
		/// </summary>
		private readonly IUserAccessLevelRepository _userAccessLevelRepository;

		/// <summary>
		/// Конструктор контроллера досок.
		/// </summary>
		/// <param name="hubContext">Контекст хаба доски.</param>
		/// <param name="fileService">Сервис для работы с файлами.</param>
		/// <param name="boardRepository">Репозиторий досок.</param>
		/// <param name="directoryPathService">Сервис для работы с путями каталогов и файлов.</param>
		/// <param name="fileRepository">Репозиторий файлов.</param>
		/// <param name="userAccessLevelRepository">Репозиторий уровней доступа пользователей к доске.</param>
		public BoardController(IHubContext<BoardHub> hubContext,
			IFileService fileService,
			IBoardRepository boardRepository,
			IDirectoryPathService directoryPathService,
			IFileRepository fileRepository,
			IUserAccessLevelRepository userAccessLevelRepository)
		{
			_hubContext = hubContext;
			_fileService = fileService;
			_fileRepository = fileRepository;
			_boardRepository = boardRepository;
			_directoryPathService = directoryPathService;
			_userAccessLevelRepository = userAccessLevelRepository;
		}

		/// <summary>
		/// Добавляет изображение на доску.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="file">Файл изображения.</param>
		/// <returns>Результат добавления изображения.</returns>
		[HttpPost("add-image")]
		[Authorize]
		public async Task<IActionResult> AddImage(Guid boardId, IFormFile file)
		{
			// Проверка корректности идентификатора доски и наличия файла
			ArgumentValidation.CheckNotEmptyGuid(boardId);
			ArgumentValidation.CheckNotNull(file, "Файл не был передан.");

			var authorizationHeader = Request.Headers["Authorization"];
			// Проверка наличия заголовка Authorization
			if (authorizationHeader.Count == 0)
			{
				return BadRequest("Отсутствует заголовок Authorization");
			}
			var token = authorizationHeader.FirstOrDefault().Split(' ').Last();

			var payload = new JwtPayload(token);
			// Проверка уровня доступа к доске
			AccessControl.CheckAccessLevel(
				await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, boardId), AccessLevelType.editor);

			// Определение типа контента файла
			new FileExtensionContentTypeProvider().TryGetContentType(file.FileName, out var contentType);

			// Проверка соответствия типа файла JPEG
			if (contentType != "image/jpeg")
			{
				return BadRequest("Неподдерживаемый формат файла. Изображение должно быть в формате JPEG.");
			}

			// Получение доски
			var board = await _boardRepository.GetByIdAsync(boardId);
			ArgumentValidation.CheckNotNull(board, "Доска с указанным Id не найдена.");

			// Получение относительного пути к директории доски
			var relativePath = await _directoryPathService.GetBoardFolderPath(boardId);

			// Получение полного пути к директории доски
			var fullPath = FilePathHelper.GetFullPath(relativePath);
			// Сохранение файла на сервере
			fullPath = await _fileService.SaveFile(file, fullPath);
			var fileName = Path.GetFileName(fullPath);

			// Проверка наличия изображения у доски
			if (board.ImageFile == null)
			{
				// Создание нового файла и его добавление в базу данных
				var newFile = new DbFile()
				{
					RelativePath = fullPath,
					FileName = fileName,
				};

				await _fileRepository.AddAsync(newFile);
				board.ImageFile = newFile;
				board.ImageFileId = newFile.Id;
			}
			else
			{
				// Обновление информации о файле изображения
				board.ImageFile.RelativePath = fullPath;
				board.ImageFile.FileName = fileName;
			}

			// Обновление доски в базе данных
			await _boardRepository.UpdateAsync(board);

			// Отправка сообщения о обновлении изображения всем подключенным клиентам
			await _hubContext.Clients.Group(payload.Sub.ToString()).SendAsync(
				"ReceiveImageUpdated", board.Id);
			await _hubContext.Clients.Group(board.Id.ToString()).SendAsync(
				"ReceiveImageUpdated", board.Id);

			return Ok();
		}


		/// <summary>
		/// Получает изображение для указанной доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Результат запроса изображения.</returns>
		[HttpGet("get-image")]
		[Authorize]
		public async Task<IActionResult> GetImage(Guid boardId)
		{
			// Проверка корректности идентификатора доски
			ArgumentValidation.CheckNotEmptyGuid(boardId);

			var authorizationHeader = Request.Headers["Authorization"];
			// Проверка наличия заголовка Authorization
			if (authorizationHeader.Count == 0)
			{
				return BadRequest("Отсутствует заголовок Authorization");
			}
			var token = authorizationHeader.FirstOrDefault().Split(' ').Last();

			var payload = new JwtPayload(token);
			// Проверка уровня доступа к доске
			AccessControl.CheckAccessLevel(
				await _userAccessLevelRepository.HasBoardDataAccess(payload.Sub, boardId), AccessLevelType.reader);

			// Получение доски с изображением
			var board = await _boardRepository.GetByIdAsyncIncludes(boardId);
			ArgumentValidation.CheckNotNull(board, "Доска с указанным Id не найдена.");

			// Проверка наличия изображения
			if (board.ImageFile == null)
			{
				return NotFound();
			}

			var imageFullFile = FilePathHelper.GetFullPath(board.ImageFile.RelativePath);

			// Проверка существования файла
			if (!System.IO.File.Exists(imageFullFile))
			{
				return NotFound();
			}

			// Определение типа контента изображения
			new FileExtensionContentTypeProvider().TryGetContentType(board.ImageFile.FileName, out var contentType);
			contentType = contentType ?? "application/octet-stream";

			// Отправка потока файла в ответе
			var fileStream = System.IO.File.OpenRead(imageFullFile);
			var fileStreamResult = new FileStreamResult(fileStream, contentType);
			fileStreamResult.FileDownloadName = board.ImageFile.FileName;
			// Добавление заголовка для доступа к загрузке файла
			Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
			return fileStreamResult;
		}


	}
}

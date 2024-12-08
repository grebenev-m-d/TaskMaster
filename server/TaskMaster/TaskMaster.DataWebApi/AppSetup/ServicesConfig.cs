using Microsoft.EntityFrameworkCore;
using TaskMaster.DataAccessModule;
using TaskMaster.DataAccessModule.Repository.BaseRepository;
using TaskMaster.DataAccessModule.Repository.BoardRepository;
using TaskMaster.DataAccessModule.Repository.CardAttachmentRepository;
using TaskMaster.DataAccessModule.Repository.CardCommentRepository;
using TaskMaster.DataAccessModule.Repository.CardListRepository;
using TaskMaster.DataAccessModule.Repository.CardRepository;
using TaskMaster.DataAccessModule.Repository.DesignRepository;
using TaskMaster.DataAccessModule.Repository.FileRepository;
using TaskMaster.DataAccessModule.Repository.AccessLevelRole;
using TaskMaster.DataAccessModule.Repository.RoleRepository;
using TaskMaster.DataAccessModule.Repository.UserAccessLevelRepository;
using TaskMaster.DataAccessModule.Repository.UserAccessLevelsRepository;
using TaskMaster.DataAccessModule.Repository.UserRepository;
using TaskMaster.DataAccessModule.Service.DirectoryService;
using TaskMaster.DataAccessModule.Service.FileService;

namespace TaskMaster.DataWebApi.AppSetup
{
	/// <summary>
	/// Конфигурационный класс сервисов.
	/// </summary>
	public static class ServicesConfig
	{
		/// <summary>
		/// Добавляет зарегистрированные службы в веб-приложение.
		/// </summary>
		/// <param name="builder">Построитель веб-приложения.</param>
		public static void Add(WebApplicationBuilder builder)
		{
			// Добавление контекста базы данных TaskMasterContext с использованием строки подключения из конфигурации по умолчанию.
			builder.Services.AddDbContext<TaskMasterContext>(options =>
			{
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
			});

			// Регистрация служб репозиториев для всех сущностей.
			builder.Services.AddSingleton<IBoardRepository, BoardRepository>();
			builder.Services.AddSingleton<ICardCommentRepository, CardCommentRepository>();
			builder.Services.AddSingleton<ICardListRepository, CardListRepository>();
			builder.Services.AddSingleton<ICardRepository, CardRepository>();
			builder.Services.AddSingleton<IDesignRepository, DesignRepository>();
			builder.Services.AddSingleton<IFileRepository, FileRepository>();
			builder.Services.AddSingleton<IAccessLevelRepository, AccessLevelRepository>();
			builder.Services.AddSingleton<IDbBoardViewHistoryMapRepository, RoleRepository>();
			builder.Services.AddSingleton<IUserRepository, UserRepository>();
			builder.Services.AddSingleton<IUserAccessLevelRepository, UserAccessLevelRepository>();
			builder.Services.AddSingleton<IBoardAccessLevelMapRepository, BoardAccessLevelMapRepository>();
			builder.Services.AddSingleton<ICardAttachmentRepository, CardAttachmentRepository>();
			builder.Services.AddSingleton<IBoardViewHistoryMapRepository, BoardViewHistoryMapRepository>();

			// Регистрация служб для работы с путями каталогов и файлов.
			builder.Services.AddSingleton<IDirectoryPathService, DirectoryPathService>();
			builder.Services.AddSingleton<IFileService, FileService>();
		}
	}
}

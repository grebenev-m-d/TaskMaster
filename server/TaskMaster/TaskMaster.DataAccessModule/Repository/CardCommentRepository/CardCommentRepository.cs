using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;

namespace TaskMaster.DataAccessModule.Repository.CardCommentRepository
{
	/// <summary>
	/// Репозиторий для работы с комментариями к карточкам.
	/// </summary>
	public class CardCommentRepository : BaseRepository<DbCardComment>, ICardCommentRepository
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Конструктор репозитория комментариев к карточкам.
		/// </summary>
		/// <param name="serviceProvider">Провайдер сервисов.</param>
		public CardCommentRepository(IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Получает все комментарии к карточке.
		/// </summary>
		/// <returns>Список комментариев.</returns>
		public virtual async Task<List<DbCardComment>> GetAllAsync()
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.CardComments
					.Include(x => x.Card)
					.Include(x => x.User)
					.ToListAsync();
			}
		}

		/// <summary>
		/// Получает все комментарии к указанной карточке.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <returns>Список комментариев для указанной карточки.</returns>
		public async Task<List<DbCardComment>> GetAllByCardIdAsync(Guid cardId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.CardComments
					.Include(x => x.Card)
					.Include(x => x.User)
					.Where(i => i.CardId == cardId)
					.ToListAsync();
			}
		}

		/// <summary>
		/// Получает комментарий к карточке по указанному идентификатору с включенными связанными сущностями.
		/// </summary>
		/// <param name="cardCommentId">Идентификатор комментария к карточке.</param>
		/// <returns>Комментарий к карточке с включенными связанными сущностями.</returns>
		public async Task<DbCardComment> GetByIdAsyncIncludes(Guid cardCommentId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.CardComments
					.Include(x => x.Card)
					.Include(x => x.User)
					.FirstOrDefaultAsync(i => i.Id == cardCommentId);
			}
		}
	}

}

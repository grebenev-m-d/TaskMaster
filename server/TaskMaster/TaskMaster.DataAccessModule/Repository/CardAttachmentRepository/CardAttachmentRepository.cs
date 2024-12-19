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
using TaskMaster.DataAccessModule.Repository.DesignRepository;
using TaskMaster.DataAccessModule.Repository.FileRepository;

namespace TaskMaster.DataAccessModule.Repository.CardAttachmentRepository
{
	/// <summary>
	/// Репозиторий для работы с вложениями к карточкам.
	/// </summary>
	public class CardAttachmentRepository : BaseRepository<DbCardAttachment>, ICardAttachmentRepository
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CardAttachmentRepository"/>.
		/// </summary>
		/// <param name="serviceProvider">Провайдер служб.</param>
		public CardAttachmentRepository(IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Добавить новое вложение к карточке.
		/// </summary>
		/// <param name="attachment">Добавляемое вложение.</param>
		/// <returns>Добавленное вложение.</returns>
		public virtual async Task<DbCardAttachment> AddAsync(DbCardAttachment attachment)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();
				await dbContext.CardAttachments.AddAsync(attachment);
				await dbContext.SaveChangesAsync();

				return attachment;
			}
		}

		/// <summary>
		/// Удалить вложение к карточке по его идентификатору.
		/// </summary>
		/// <param name="id">Идентификатор вложения.</param>
		/// <returns>Удаленное вложение, если найдено, иначе null.</returns>
		public virtual async Task<DbCardAttachment> DeleteByIdAsync(Guid id)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();
				var attachment = await dbContext.CardAttachments
					.Include(i => i.File)
					.FirstOrDefaultAsync(i => i.Id == id);

				if (attachment != null)
				{
					dbContext.CardAttachments.Remove(attachment);
					await dbContext.SaveChangesAsync();
				}

				return attachment;
			}
		}
	}

}

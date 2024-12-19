using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TaskMaster.DataAccessModule.Repository.CardRepository;
using TaskMaster.DataAccessModule.Extensions;
using TaskMaster.Validation;

namespace TaskMaster.DataAccessModule.Repository.CardListRepository
{
	/// <summary>
	/// Репозиторий для работы с карточками.
	/// </summary>
	public class CardListRepository : BaseRepository<DbCardList>, ICardListRepository
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CardListRepository"/>.
		/// </summary>
		/// <param name="cardRepository">Репозиторий карточек.</param>
		/// <param name="serviceProvider">Провайдер служб.</param>
		public CardListRepository(ICardRepository cardRepository, IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Получает карточку по идентификатору с включенными связанными данными.
		/// </summary>
		/// <param name="id">Идентификатор карточки.</param>
		/// <returns>Карточка с включенными связанными данными.</returns>
		public override async Task<DbCardList> GetByIdAsyncIncludes(Guid id)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.CardLists
					.Include(cl => cl.Cards)
					.FirstOrDefaultAsync(cl => cl.Id == id);
			}
		}

		/// <summary>
		/// Получает все карточки с включенными связанными данными.
		/// </summary>
		/// <returns>Список всех карточек с включенными связанными данными.</returns>
		public override async Task<List<DbCardList>> GetAllAsync()
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.CardLists
					.Include(cl => cl.Cards)
					.ToListAsync();
			}
		}

		/// <summary>
		/// Добавляет новую карточку.
		/// </summary>
		/// <param name="cardList">Карточка для добавления.</param>
		/// <returns>Добавленная карточка.</returns>
		public override async Task<DbCardList> AddAsync(DbCardList cardList)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				var board = await dbContext.Boards
					.Include(i => i.CardLists)
					.FirstOrDefaultAsync(i => i.Id == cardList.BoardId);

				if (board == null)
				{
					throw new ArgumentException();
				}

				await dbContext.CardLists.AddAsync(cardList);
				await dbContext.SaveChangesAsync();

				if (board.CardLists.Count > 1)
				{
					var cardListsLast = board.CardLists.FirstOrDefault(i => i.NextCardListId == null);

					cardListsLast.NextCardListId = cardList.Id;
					cardList.PrevCardListId = cardListsLast.Id;

					await dbContext.SaveChangesAsync();
				}

				return cardList;
			}
		}

		/// <summary>
		/// Перемещает список карточек.
		/// </summary>
		/// <param name="movedCardListId">Идентификатор перемещаемого списка карточек.</param>
		/// <param name="prevCardListId">Идентификатор предыдущего списка карточек.</param>
		public async Task MoveCardList(Guid movedCardListId, Guid? prevCardListId)
		{
			// Если перемещаемый список и предыдущий список имеют одинаковый идентификатор, возвращаемся.
			if (movedCardListId == prevCardListId)
			{
				return;
			}

			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				// Получаем перемещаемый список карточек с включенными связанными данными.
				var movedCardList = await dbContext.CardLists
					.Include(i => i.PrevCardList)
					.Include(i => i.NextCardList)
					.FirstOrDefaultAsync(i => i.Id == movedCardListId);
				ArgumentValidation.CheckNotNull(movedCardList, "Перемещаемый список карточек не найден");

				// Если предыдущий список перемещаемого списка равен предыдущему списку, возвращаемся.
				if (movedCardList.PrevCardListId == prevCardListId)
				{
					return;
				}

				// Получаем старый предыдущий и следующий список.
				var oldPrevList = movedCardList.PrevCardListId.HasValue ?
					await dbContext.CardLists
					.Include(i => i.PrevCardList)
					.Include(i => i.NextCardList)
					.FirstOrDefaultAsync(i => i.Id == movedCardList.PrevCardListId.Value) :
					null;
				var oldNextList = movedCardList.NextCardListId.HasValue ?
					await dbContext.CardLists
					.Include(i => i.PrevCardList)
					.Include(i => i.NextCardList)
					.FirstOrDefaultAsync(i => i.Id == movedCardList.NextCardListId.Value) :
					null;

				// Обновляем связи для старого предыдущего и следующего списка.
				if (oldPrevList != null && oldPrevList.Id != movedCardList.Id)
				{
					oldPrevList.NextCardListId = movedCardList.NextCardListId;
				}

				if (oldNextList != null && oldNextList.Id != movedCardList.Id)
				{
					oldNextList.PrevCardListId = movedCardList.PrevCardListId;
				}

				// Если предыдущий список отсутствует, то перемещаем список на начало доски.
				if (prevCardListId == null)
				{
					var board = await dbContext.Boards
						.Include(i => i.CardLists)
						.ThenInclude(i => i.PrevCardList)
						.ThenInclude(i => i.NextCardList)
						.FirstOrDefaultAsync(i => i.Id == movedCardList.BoardId.Value);
					var cardListFirst = board.CardLists
						.FirstOrDefault(i => i.PrevCardListId == null);

					// Обновляем связи для перемещаемого списка и первого списка на доске.
					movedCardList.PrevCardListId = null;
					movedCardList.NextCardListId = cardListFirst.Id;
					cardListFirst.PrevCardListId = movedCardList.Id;
				}
				else
				{
					// Получаем новый предыдущий список.
					var newPrevList = await dbContext.CardLists
						.Include(i => i.PrevCardList)
						.Include(i => i.NextCardList)
						.FirstOrDefaultAsync(i => i.Id == prevCardListId.Value);
					ArgumentValidation.CheckNotNull(newPrevList, "Не верно указано Id предыдущего списка карточек");

					// Получаем новый следующий список.
					var newNextList = newPrevList.NextCardListId == null ?
						null :
						await dbContext.CardLists.FirstOrDefaultAsync(i => i.Id == newPrevList.NextCardListId.Value);

					// Обновляем связи для перемещаемого списка, нового предыдущего и следующего списка.
					movedCardList.PrevCardListId = newPrevList.Id;
					movedCardList.NextCardListId = newPrevList.NextCardListId;
					newPrevList.NextCardListId = movedCardList.Id;

					if (newNextList != null)
					{
						newNextList.PrevCardListId = movedCardList.Id;
					}
				}

				await dbContext.SaveChangesAsync();
			}
		}

		/// <summary>
		/// Удаляет список карточек по указанному идентификатору, включая связанные данные.
		/// </summary>
		/// <param name="id">Идентификатор списка карточек.</param>
		/// <returns>Удаленный список карточек.</returns>
		public override async Task<DbCardList> DeleteByIdAsync(Guid id)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				// Получаем список карточек по указанному идентификатору со всеми связанными данными.
				var cardList = await dbContext.CardLists
					.Include(i => i.Cards)
						.ThenInclude(i => i.CardComments)
					.Include(i => i.Cards)
						.ThenInclude(i => i.CardAttachments)
					.FirstOrDefaultAsync(i => i.Id == id);

				if (cardList != null)
				{
					// Получаем предыдущий и следующий список карточек.
					var prevCardList = dbContext.CardLists.FirstOrDefault(i => i.Id == cardList.PrevCardListId);
					var nextCardList = dbContext.CardLists.FirstOrDefault(i => i.Id == cardList.NextCardListId);

					// Обновляем связи для предыдущего и следующего списка карточек.
					if (prevCardList != null)
					{
						prevCardList.NextCardListId = nextCardList == null ? null : nextCardList.Id;
					}
					if (nextCardList != null)
					{
						nextCardList.PrevCardListId = prevCardList == null ? null : prevCardList.Id;
					}

					// Удаляем список карточек из контекста базы данных и сохраняем изменения.
					dbContext.CardLists.Remove(cardList);
					await dbContext.SaveChangesAsync();
				}

				return cardList;
			}
		}

	}
}

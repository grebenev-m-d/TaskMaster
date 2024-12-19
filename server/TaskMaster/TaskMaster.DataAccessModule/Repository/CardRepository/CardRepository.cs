using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskMaster.DataAccessModule.Extensions;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;
using TaskMaster.Validation;

namespace TaskMaster.DataAccessModule.Repository.CardRepository
{
	/// <summary>
	/// Предоставляет репозиторий для работы с карточками.
	/// </summary>
	public class CardRepository : BaseRepository<DbCard>, ICardRepository
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CardRepository"/>.
		/// </summary>
		/// <param name="serviceProvider">Провайдер сервисов.</param>
		public CardRepository(IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Получает карточку по заданному идентификатору с включенными связанными данными.
		/// </summary>
		/// <param name="id">Идентификатор карточки.</param>
		/// <returns>Карточка с включенными связанными данными.</returns>
		public override async Task<DbCard> GetByIdAsyncIncludes(Guid id)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Cards
					.Include(c => c.CardList)
					.Include(c => c.ImageFile)
					.Include(c => c.CardAttachments)
						.ThenInclude(c => c.File)
					.FirstOrDefaultAsync(c => c.Id == id);
			}
		}

		/// <summary>
		/// Получает все карточки с включенными связанными данными.
		/// </summary>
		/// <returns>Список карточек с включенными связанными данными.</returns>
		public override async Task<List<DbCard>> GetAllAsync()
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Cards
					.Include(c => c.CardList)
					.ToListAsync();
			}
		}

		/// <summary>
		/// Добавляет новую карточку.
		/// </summary>
		/// <param name="card">Добавляемая карточка.</param>
		/// <returns>Добавленная карточка.</returns>
		public override async Task<DbCard> AddAsync(DbCard card)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				var cardLists = await dbContext.CardLists
					.Include(i => i.Cards)
					.FirstOrDefaultAsync(i => i.Id == card.CardListId);

				if (cardLists == null)
				{
					throw new ArgumentException();
				}

				if (cardLists.Cards.Count > 0)
				{
					await dbContext.Cards.AddAsync(card);
					await dbContext.SaveChangesAsync();

					var lastCard = cardLists.Cards.FirstOrDefault(i => i.NextCardId == null);

					lastCard.NextCardId = card.Id;
					card.PrevCardId = lastCard.Id; 
					
					await dbContext.SaveChangesAsync();
				}
				else
				{
					await dbContext.Cards.AddAsync(card);
					await dbContext.SaveChangesAsync();
				}

				return card;
			}
		}

		/// <summary>
		/// Перемещает карточку в указанный список после указанной карточки.
		/// </summary>
		/// <param name="currentCardListId">Идентификатор текущего списка карточек.</param>
		/// <param name="movedCardId">Идентификатор перемещаемой карточки.</param>
		/// <param name="prevCardId">Идентификатор карточки, после которой будет перемещена карточка (может быть null).</param>
		/// <returns>Асинхронная задача.</returns>
		/// <exception cref="ArgumentException">Выбрасывается, если указанный идентификатор списка или карточки пуст.</exception>
		public async Task MoveCard(Guid currentCardListId, Guid movedCardId, Guid? prevCardId)
		{
			ArgumentValidation.CheckNotEmptyGuid(currentCardListId);
			ArgumentValidation.CheckNotEmptyGuid(movedCardId);

			if (prevCardId != null)
			{
				ArgumentValidation.CheckNotEmptyGuid(prevCardId.Value);
			}

			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				// Получение доски, содержащей текущий список карточек
				var board = await dbContext.Boards
					.Include(x => x.CardLists)
						.ThenInclude(x => x.Cards)
							.ThenInclude(x => x.PrevCard)
								.ThenInclude(x => x.NextCard)
							.ThenInclude(x => x.PrevCard)
								.ThenInclude(x => x.NextCard)
					.FirstOrDefaultAsync(x => x.CardLists.Any(i => i.Id == currentCardListId));

				// Сортировка списков карточек по их отношениям
				var cardLists = board.CardLists.SortByRelationship();

				// Получение текущего списка карточек и списка, содержащего карточку, которую нужно переместить
				var currentCardList = cardLists.FirstOrDefault(i => i.Id == currentCardListId);
				var previousCardList = cardLists.FirstOrDefault(i => i.Cards.Any(i => i.Id == movedCardId));
				var movedCard = previousCardList.Cards.FirstOrDefault(i => i.Id == movedCardId);

				// Удаление карточки из старого списка
				if (movedCard.PrevCard != null)
				{
					movedCard.PrevCard.NextCardId = movedCard.NextCardId;
				}
				if (movedCard.NextCard != null)
				{
					movedCard.NextCard.PrevCardId = movedCard.PrevCardId;
				}
				movedCard.PrevCard = null;
				movedCard.NextCard = null;
				previousCardList.Cards.Remove(movedCard);

				await dbContext.SaveChangesAsync();

				// Добавление карточки в новый список
				if (prevCardId == null)
				{
					var firstCard = currentCardList.Cards.FirstOrDefault(i => i.PrevCardId == null);

					currentCardList.Cards.Add(movedCard);
					movedCard.PrevCardId = null;
					movedCard.NextCardId = firstCard == null ? null : firstCard.Id;

					if (firstCard != null)
					{
						firstCard.PrevCardId = movedCard.Id;
					}
				}
				else
				{
					var prevCard = currentCardList.Cards.FirstOrDefault(i => i.Id == prevCardId);

					if (prevCard == null)
					{
						throw new ArgumentException();
					}

					currentCardList.Cards.Add(movedCard);
					movedCard.PrevCardId = prevCard.Id;
					movedCard.NextCardId = prevCard.NextCardId;

					if (prevCard.NextCard != null)
					{
						prevCard.NextCard.PrevCardId = movedCard.Id;
					}
					prevCard.NextCardId = movedCard.Id;
				}

				await dbContext.SaveChangesAsync();
			}
		}

		/// <summary>
		/// Удаляет карточку по указанному идентификатору.
		/// </summary>
		/// <param name="id">Идентификатор карточки.</param>
		/// <returns>Удаленная карточка.</returns>
		/// <exception cref="ArgumentException">Выбрасывается, если карточка с указанным идентификатором не найдена.</exception>
		public override async Task<DbCard> DeleteByIdAsync(Guid id)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				// Получение карточки по идентификатору
				var card = await dbContext.Cards
					.Include(i => i.PrevCard)
					.Include(i => i.NextCard)
					.Include(i => i.CardAttachments)
					.Include(i => i.CardComments)
					.FirstOrDefaultAsync(i => i.Id == id);

				// Проверка, найдена ли карточка
				if (card == null)
				{
					throw new ArgumentException();
				}

				// Обновление связей соседних карточек
				if (card.PrevCard != null)
				{
					card.PrevCard.NextCardId = card.NextCardId;
				}
				if (card.NextCard != null)
				{
					card.NextCard.PrevCardId = card.PrevCardId;
				}

				// Удаление вложений и комментариев к карточке
				dbContext.CardAttachments.RemoveRange(card.CardAttachments);
				dbContext.CardComments.RemoveRange(card.CardComments);

				// Удаление карточки из контекста
				dbContext.Cards.Remove(card);

				await dbContext.SaveChangesAsync();

				return card;
			}
		}
	}
}

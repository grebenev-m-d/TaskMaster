using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Extensions
{
	
	public static class SortExtension
	{
		/// <summary>
		/// Метод сортировки списка карточек по отношениям
		/// </summary>
		/// <param name="dbCardLists">Список карточек для сортировки</param>
		/// <returns>Отсортированный список карточек</returns>
		public static IEnumerable<DbCardList> SortByRelationship(this IEnumerable<DbCardList> dbCardLists)
		{
			// Создание словаря для хранения карточек по их идентификаторам
			var dictionary = new Dictionary<Guid, DbCardList>();
			foreach (var cardList in dbCardLists)
			{
				dictionary[cardList.Id] = cardList;
			}

			var sortedList = new List<DbCardList>();
			// Начальная карточка
			var current = dbCardLists.FirstOrDefault(kv => kv.PrevCardListId == null);

			// Проход по списку карточек
			while (current != null)
			{
				sortedList.Add(dictionary[current.Id]);

				// Переход к следующей карточке
				if (current.NextCardListId == null)
					break;

				current = dictionary[current.NextCardListId.Value];
			}

			return sortedList;
		}

		/// <summary>
		/// Метод сортировки карточек по отношениям
		/// </summary>
		/// <param name="dbCards">Список карточек для сортировки</param>
		/// <returns>Отсортированный список карточек</returns>
		public static IEnumerable<DbCard> SortByRelationship(this IEnumerable<DbCard> dbCards)
		{
			// Создание словаря для хранения карточек по их идентификаторам
			var dictionary = new Dictionary<Guid, DbCard>();
			foreach (var card in dbCards)
			{
				dictionary[card.Id] = card;
			}

			var sorted = new List<DbCard>();
			// Начальная карточка
			var current = dbCards.FirstOrDefault(kv => kv.PrevCardId == null);

			// Проход по списку карточек
			while (current != null)
			{
				sorted.Add(dictionary[current.Id]);

				// Переход к следующей карточке
				if (current.NextCardId == null)
					break;

				current = dictionary[current.NextCardId.Value];
			}

			return sorted;
		}
	}
}

namespace TaskMaster.DataWebApi.Models
{
	/// <summary>
	/// Список карточек на доске.
	/// </summary>
	public class CardList
	{
		/// <summary>
		/// Идентификатор списка карточек.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Заголовок списка карточек.
		/// </summary>
		public string Title { get; set; } = null!;
	}

}

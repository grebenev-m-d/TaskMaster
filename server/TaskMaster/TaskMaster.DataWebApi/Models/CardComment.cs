namespace TaskMaster.DataWebApi.Models
{
	/// <summary>
	/// Комментарий к карточке.
	/// </summary>
	public class CardComment
	{
		/// <summary>
		/// Идентификатор комментария.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Текст комментария.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Пользователь, оставивший комментарий.
		/// </summary>
		public User User { get; set; }

		/// <summary>
		/// Дата и время создания комментария.
		/// </summary>
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		/// Дата и время обновления комментария.
		/// </summary>
		public DateTime? UpdatedAt { get; set; }
	}
}

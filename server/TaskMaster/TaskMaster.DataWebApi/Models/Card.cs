using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataWebApi.Models
{
	/// <summary>
	/// Карточка на доске.
	/// </summary>
	public class Card
	{
		/// <summary>
		/// Идентификатор карточки.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Заголовок карточки.
		/// </summary>
		public string Title { get; set; } = null!;

		/// <summary>
		/// Описание карточки.
		/// </summary>
		public string? Description { get; set; }

		/// <summary>
		/// Указывает, есть ли у карточки обложка.
		/// </summary>
		public bool HasCoverImage { get; set; }

		/// <summary>
		/// Список вложений к карточке.
		/// </summary>
		public List<CardAttachment>? Attachments { get; set; }
	}

}

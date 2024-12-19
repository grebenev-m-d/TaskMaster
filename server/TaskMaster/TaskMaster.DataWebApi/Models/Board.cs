using TaskMaster.DataAccessModule.Constants;

namespace TaskMaster.DataWebApi.Models
{
	/// <summary>
	/// Модель доски.
	/// </summary>
	public class Board
	{
		/// <summary>
		/// Идентификатор доски.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Заголовок доски.
		/// </summary>
		public string? Title { get; set; }

		/// <summary>
		/// Дата создания доски.
		/// </summary>
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		/// Дата последнего обновления доски.
		/// </summary>
		public DateTime? LastUpdated { get; set; }

		/// <summary>
		/// Код цвета доски.
		/// </summary>
		public string? ColorCode { get; set; }

		/// <summary>
		/// URL изображения доски.
		/// </summary>
		public string? ImageUrl { get; set; }

		/// <summary>
		/// Тип дизайна доски.
		/// </summary>
		public DesignType DesignType { get; set; }

		/// <summary>
		/// Владелец доски.
		/// </summary>
		public User Owner { get; set; }
	}
}

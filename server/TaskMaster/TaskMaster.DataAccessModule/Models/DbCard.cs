using System;
using System.Collections.Generic;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Models;

/// <summary>
/// Модель карточки в базе данных.
/// </summary>
public partial class DbCard 
	: DbBaseEntity
{
	/// <summary>
	/// Название карточки.
	/// </summary>
	public string Title { get; set; } = null!;

	/// <summary>
	/// Описание карточки.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Идентификатор файла изображения, связанного с карточкой.
	/// </summary>
	public Guid? ImageFileId { get; set; }

	/// <summary>
	/// Идентификатор списка карточек, к которому принадлежит данная карточка.
	/// </summary>
	public Guid? CardListId { get; set; }

	/// <summary>
	/// Идентификатор предыдущей карточки.
	/// </summary>
	public Guid? PrevCardId { get; set; }

	/// <summary>
	/// Идентификатор следующей карточки.
	/// </summary>
	public Guid? NextCardId { get; set; }

	/// <summary>
	/// Вложения к карточке.
	/// </summary>
	public virtual ICollection<DbCardAttachment> CardAttachments { get; set; } = new List<DbCardAttachment>();

	/// <summary>
	/// Комментарии к карточке.
	/// </summary>
	public virtual ICollection<DbCardComment> CardComments { get; set; } = new List<DbCardComment>();

	/// <summary>
	/// Список карточек, к которому принадлежит данная карточка.
	/// </summary>
	public virtual DbCardList? CardList { get; set; }

	/// <summary>
	/// Файл изображения, связанный с карточкой.
	/// </summary>
	public virtual DbFile? ImageFile { get; set; }

	/// <summary>
	/// Обратная связь со следующей карточкой.
	/// </summary>
	public virtual ICollection<DbCard> InverseNextCard { get; set; } = new List<DbCard>();

	/// <summary>
	/// Обратная связь с предыдущей карточкой.
	/// </summary>
	public virtual ICollection<DbCard> InversePrevCard { get; set; } = new List<DbCard>();

	/// <summary>
	/// Следующая карточка.
	/// </summary>
	public virtual DbCard? NextCard { get; set; }

	/// <summary>
	/// Предыдущая карточка.
	/// </summary>
	public virtual DbCard? PrevCard { get; set; }
}

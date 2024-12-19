using System;
using System.Collections.Generic;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Models;

/// <summary>
/// Модель списка карточек в базе данных.
/// </summary>
public partial class DbCardList
	: DbBaseEntity
{
	/// <summary>
	/// Название списка карточек.
	/// </summary>
	public string Title { get; set; } = null!;

	/// <summary>
	/// Идентификатор доски, к которой принадлежит данный список карточек.
	/// </summary>
	public Guid? BoardId { get; set; }

	/// <summary>
	/// Идентификатор предыдущего списка карточек.
	/// </summary>
	public Guid? PrevCardListId { get; set; }

	/// <summary>
	/// Идентификатор следующего списка карточек.
	/// </summary>
	public Guid? NextCardListId { get; set; }

	/// <summary>
	/// Доска, к которой принадлежит данный список карточек.
	/// </summary>
	public virtual DbBoard? Board { get; set; }

	/// <summary>
	/// Коллекция карточек, принадлежащих данному списку.
	/// </summary>
	public virtual ICollection<DbCard> Cards { get; set; } = new List<DbCard>();

	/// <summary>
	/// Коллекция следующих списков карточек.
	/// </summary>
	public virtual ICollection<DbCardList> InverseNextCardList { get; set; } = new List<DbCardList>();

	/// <summary>
	/// Коллекция предыдущих списков карточек.
	/// </summary>
	public virtual ICollection<DbCardList> InversePrevCardList { get; set; } = new List<DbCardList>();

	/// <summary>
	/// Следующий список карточек.
	/// </summary>
	public virtual DbCardList? NextCardList { get; set; }

	/// <summary>
	/// Предыдущий список карточек.
	/// </summary>
	public virtual DbCardList? PrevCardList { get; set; }
}

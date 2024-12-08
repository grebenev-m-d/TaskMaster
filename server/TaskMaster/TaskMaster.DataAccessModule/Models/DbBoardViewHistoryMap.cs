using System;
using System.Collections.Generic;

namespace TaskMaster.DataAccessModule.Models;

/// <summary>
/// Модель отображения истории просмотров доски для пользователя в базе данных.
/// </summary>
public partial class DbBoardViewHistoryMap
{
	/// <summary>
	/// Идентификатор пользователя.
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Идентификатор доски.
	/// </summary>
	public Guid BoardId { get; set; }

	/// <summary>
	/// Дата и время последнего просмотра.
	/// </summary>
	public DateTime? LastViewedAt { get; set; }

	/// <summary>
	/// Доска.
	/// </summary>
	public virtual DbBoard Board { get; set; } = null!;

	/// <summary>
	/// Пользователь.
	/// </summary>
	public virtual DbUser User { get; set; } = null!;
}

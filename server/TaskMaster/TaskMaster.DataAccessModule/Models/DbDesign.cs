using System;
using System.Collections.Generic;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Models;

/// <summary>
/// Модель дизайна в базе данных.
/// </summary>
public partial class DbDesign
	: DbBaseEntity
{
	/// <summary>
	/// Тип дизайна.
	/// </summary>
	public DesignType Type { get; set; }

	/// <summary>
	/// Коллекция досок, связанных с данным дизайном.
	/// </summary>
	public virtual ICollection<DbBoard> Boards { get; set; } = new List<DbBoard>();
}

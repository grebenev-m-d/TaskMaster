using System;
using System.Collections.Generic;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Models;

/// <summary>
/// Определение для работы с уровнями доступа в базе данных.
/// </summary>
public partial class DbAccessLevel
	: DbBaseEntity
{
	/// <summary>
	/// Тип уровня доступа.
	/// </summary>
	public AccessLevelType Type { get; set; }

	/// <summary>
	/// Коллекция отображений уровней доступа к доскам.
	/// </summary>
	public virtual ICollection<DbBoardAccessLevelMap> BoardAccessLevelMaps { get; set; } = new List<DbBoardAccessLevelMap>();

	/// <summary>
	/// Коллекция досок, связанных с этим уровнем доступа.
	/// </summary>
	public virtual ICollection<DbBoard> Boards { get; set; } = new List<DbBoard>();
}
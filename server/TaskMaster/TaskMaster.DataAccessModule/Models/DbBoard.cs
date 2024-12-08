using System;
using System.Collections.Generic;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Models;

/// <summary>
/// Модель для работы с данными доски в базе данных.
/// </summary>
public partial class DbBoard 
	: DbBaseEntity
{
	/// <summary>
	/// Название доски.
	/// </summary>
	public string? Title { get; set; }

	/// <summary>
	/// Время создания доски.
	/// </summary>
	public DateTime? CreatedAt { get; set; }

	/// <summary>
	/// Идентификатор пользователя, создавшего доску.
	/// </summary>
	public Guid? UserId { get; set; }

	/// <summary>
	/// Идентификатор типа дизайна доски.
	/// </summary>
	public Guid? DesignTypeId { get; set; }

	/// <summary>
	/// Код цвета доски.
	/// </summary>
	public string? ColorCode { get; set; }

	/// <summary>
	/// Идентификатор файла изображения доски.
	/// </summary>
	public Guid? ImageFileId { get; set; }

	/// <summary>
	/// Флаг, указывающий, является ли доска общедоступной.
	/// </summary>
	public bool IsPublic { get; set; }

	/// <summary>
	/// Идентификатор общего уровня доступа к доске.
	/// </summary>
	public Guid? GeneralAccessLevelId { get; set; }

	/// <summary>
	/// Коллекция отображений уровней доступа к доске.
	/// </summary>
	public virtual ICollection<DbBoardAccessLevelMap> BoardAccessLevelMaps { get; set; } = new List<DbBoardAccessLevelMap>();

	/// <summary>
	/// Коллекция отображений истории просмотров доски.
	/// </summary>
	public virtual ICollection<DbBoardViewHistoryMap> BoardViewHistoryMaps { get; set; } = new List<DbBoardViewHistoryMap>();

	/// <summary>
	/// Коллекция списков карточек на доске.
	/// </summary>
	public virtual ICollection<DbCardList> CardLists { get; set; } = new List<DbCardList>();

	/// <summary>
	/// Тип дизайна доски.
	/// </summary>
	public virtual DbDesign? DesignType { get; set; }

	/// <summary>
	/// Общий уровень доступа к доске.
	/// </summary>
	public virtual DbAccessLevel? GeneralAccessLevel { get; set; }

	/// <summary>
	/// Файл изображения доски.
	/// </summary>
	public virtual DbFile? ImageFile { get; set; }

	/// <summary>
	/// Пользователь, создавший доску.
	/// </summary>
	public virtual DbUser? User { get; set; }
}

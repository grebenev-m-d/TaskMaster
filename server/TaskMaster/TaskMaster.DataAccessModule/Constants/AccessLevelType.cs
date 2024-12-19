using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMaster.DataAccessModule.Constants
{
	/// <summary>
	/// Перечисление типов уровня доступа
	/// </summary>
	public enum AccessLevelType
	{
		/// <summary>
		/// Читатель
		/// </summary>
		reader,
		/// <summary>
		/// Редактор
		/// </summary>
		editor,
		/// <summary>
		/// Владелец
		/// </summary>
		owner,
	}
}

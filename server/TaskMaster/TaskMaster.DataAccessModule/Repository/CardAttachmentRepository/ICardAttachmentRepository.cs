using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;

namespace TaskMaster.DataAccessModule.Repository.FileRepository
{
	/// <summary>
	/// Интерфейс репозитория для работы с вложениями к карточкам.
	/// </summary>
	public interface ICardAttachmentRepository 
		: IBaseRepository<DbCardAttachment>
	{

	}
}

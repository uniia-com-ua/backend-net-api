namespace UniiaAdmin.Data.Interfaces.FileInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IFileEntity : IEntity
{
	public string? FileId { get; set; }
}

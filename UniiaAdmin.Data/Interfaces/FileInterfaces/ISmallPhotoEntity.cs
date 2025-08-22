namespace UniiaAdmin.Data.Interfaces.FileInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ISmallPhotoEntity : IPhotoEntity
{
	public string? SmallPhotoId { get; set; }
}

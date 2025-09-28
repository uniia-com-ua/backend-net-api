using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.Data.Models;

public class Specialty : IEntity
{
	public int Id { get; set; }

	[Required]
	[MaxLength(50)]
	public string? Name { get; set; }

	[BindNever]
	[JsonIgnore]
	public virtual ICollection<Subject>? Subjects { get; set; }

	[BindNever]
	[JsonIgnore]
	public virtual ICollection<Faculty>? Faculties { get; set; }
}

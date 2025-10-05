namespace UniiaAdmin.Data.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Sorting
{
	[Required]
	public required string FieldName { get; set; }

	[Required]
	public required string SortDirection { get; set; }
}

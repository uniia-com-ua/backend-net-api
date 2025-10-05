namespace UniiaAdmin.Data.Dtos;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UniiaAdmin.Data.Models;

public class SpecialityDto
{
	public int Id { get; set; }

	public string? Name { get; set; }

	public int SubjectCount { get; set; }
}

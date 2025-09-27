namespace UniiaAdmin.Data.Dtos.UserDtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniiaAdmin.Data.Enums;
using UniiaAdmin.Data.Models;

public class UserDto
{
	public string? Id { get; set; }

	[EmailAddress]
	public string? Email { get; set; }

	public string? FirstName { get; set; }

	public string? LastName { get; set; }

	public string? AdditionalName { get; set; }

	public DateTime BirthDate { get; set; }

	public DateTime RegistrationDate { get; set; }

	public DateTime DateOfApprovement { get; set; }

	public string? PreferredLanguage { get; set; }

	public AccountStatus AccountStatus { get; set; }
}

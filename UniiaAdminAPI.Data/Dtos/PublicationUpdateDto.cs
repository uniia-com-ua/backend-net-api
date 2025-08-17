namespace UniiaAdmin.Data.Dtos;

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

public class PublicationUpdateDto
{
	public List<int>? Authors { get; init; }

	public List<int>? Subjects { get; init; }

	public List<int>? Keywords { get; init; }

	public IFormFile? File { get; init; }
}

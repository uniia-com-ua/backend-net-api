using Microsoft.AspNetCore.Authorization;
using UniiaAdmin.Data.Constants;

namespace UniiaAdmin.WebApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class PermissionAttribute : AuthorizeAttribute
{
	public PermissionAttribute(PermissionResource resource, CrudActions action)
	{
		Policy = $"{resource}.{action}";
	}
}

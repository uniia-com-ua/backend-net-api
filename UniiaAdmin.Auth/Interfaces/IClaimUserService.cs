using System.Security.Claims;

namespace UniiaAdmin.Auth.Interfaces
{
	public interface IClaimUserService
	{
		public Task<byte[]> GetUserPictureFromClaims(IEnumerable<Claim> claims, HttpClient httpClient);
	}
}

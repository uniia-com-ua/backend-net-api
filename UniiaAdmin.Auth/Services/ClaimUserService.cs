using System.Security.Claims;
using UniiaAdmin.Auth.Interfaces;

namespace UniiaAdmin.Auth.Services
{
	public class ClaimUserService : IClaimUserService
	{
		public ClaimUserService() { }
		public async Task<byte[]> GetUserPictureFromClaims(IEnumerable<Claim> claims, HttpClient httpClient)
		{
			var pictureUrl = claims.FirstOrDefault(c => c.Type == "Picture")!.Value;

			var response = await httpClient.GetAsync(pictureUrl);

			response.EnsureSuccessStatusCode();

			var imageBytesTask = await response.Content.ReadAsByteArrayAsync();

			return imageBytesTask;
		}
	}
}

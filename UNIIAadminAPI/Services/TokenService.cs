using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;

namespace UNIIAadminAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly string _clientId;

        private readonly string _clientSecret;

        private readonly string _tokenKey;

        private readonly Uri _refreshTokenUri = new("https://oauth2.googleapis.com/token");
        public TokenService(string clientId, string clientSecret, string tokenKey) 
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _tokenKey = tokenKey;
        }

        private async Task<string?> RefreshTokenAsync(string refreshToken, HttpClient httpClient)
        {
            var requestContent = new StringContent(
                $"client_id={_clientId}&client_secret={_clientSecret}&refresh_token={refreshToken}&grant_type=refresh_token",
                Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await httpClient.PostAsync(_refreshTokenUri, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

            return tokenResponse?.access_token;
        }
        public async Task<bool> ValidateGoogleTokenAsync(AdminUser user, UserManager<AdminUser> userManager)
        {
            var httpClient = new HttpClient();

            var accessToken = await GetTokenByUserAsync(user, userManager, "accessToken");

            if (accessToken == null)
            {
                return false;
            }

            var googleTokenInfoUrl = $"https://www.googleapis.com/oauth2/v3/tokeninfo?access_token={accessToken}";

            var response = await httpClient.GetAsync(googleTokenInfoUrl);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var refreshToken = await GetTokenByUserAsync(user, userManager, "refreshToken");

            if (refreshToken == null)
            {
                return false;
            }

            var refreshedAccessToken = await RefreshTokenAsync(refreshToken, httpClient);

            if (!string.IsNullOrEmpty(refreshedAccessToken))
            {
                await userManager.SetAuthenticationTokenAsync(user, "Uniia", "accessToken", EncryptToken(refreshedAccessToken));

                await userManager.UpdateAsync(user);

                return true;
            }

            return false;
        }

        private async Task<string?> GetTokenByUserAsync(AdminUser user, UserManager<AdminUser> userManager, string tokenName)
        {
            var token = await userManager.GetAuthenticationTokenAsync(user, "Uniia", tokenName);

            var decryptedToken = DecryptToken(token);

            return decryptedToken;
        }
        public string EncryptToken(string input)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(_tokenKey);

                aesAlg.GenerateIV();

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                byte[] encryptedBytes;

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(input);
                        }
                        encryptedBytes = msEncrypt.ToArray();
                    }
                }

                return Convert.ToBase64String(aesAlg.IV) + ":" + Convert.ToBase64String(encryptedBytes);
            }
        }

        private string? DecryptToken(string? encryptedInput)
        {
            if (encryptedInput == null)
                return null;

            string[] parts = encryptedInput.Split(':');
            byte[] iv = Convert.FromBase64String(parts[0]);
            byte[] encryptedBytes = Convert.FromBase64String(parts[1]);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(_tokenKey);
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                string decryptedText;

                using (MemoryStream msDecrypt = new(encryptedBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            decryptedText = srDecrypt.ReadToEnd();
                        }
                    }
                }

                return decryptedText;
            }
        }
    }
}

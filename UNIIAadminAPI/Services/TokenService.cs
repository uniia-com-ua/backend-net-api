using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Services
{
    public class TokenService(string clientId, string clientSecret, string tokenKey)
    {
        public async Task<string?> RefreshTokenAsync(string refreshToken, HttpClient httpClient)
        {
            var requestContent = new StringContent(
                $"client_id={clientId}&client_secret={clientSecret}&refresh_token={refreshToken}&grant_type=refresh_token",
                Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", requestContent);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

            return tokenResponse?.access_token;
        }
        public async Task<bool> ValidateGoogleTokenAsync(MongoIdentityUser user, UserManager<MongoIdentityUser> userManager)
        {
            var httpClient = new HttpClient();

            (var accessToken, var refreshToken) = GetTokensByUser(user);

            if (accessToken == null || refreshToken == null)
            {
                return false;
            }

            var googleTokenInfoUrl = $"https://www.googleapis.com/oauth2/v3/tokeninfo?access_token={accessToken}";

            var response = await httpClient.GetAsync(googleTokenInfoUrl);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var refreshedAccessToken = await RefreshTokenAsync(refreshToken, httpClient);

            if (!string.IsNullOrEmpty(refreshedAccessToken))
            {
                user.Tokens = MakeEncryptedTokenList(refreshedAccessToken, refreshToken);

                await userManager.UpdateAsync(user);

                return true;
            }

            return false;
        }
        public List<Token> MakeEncryptedTokenList(string accessToken, string refreshToken)
        {
            List<Token> tokens = [];

            string encryptedAccessToken = EncryptToken(accessToken);

            string encryptedRefreshToken = EncryptToken(refreshToken);

            Token access = new()
            {
                Value = encryptedAccessToken,
                LoginProvider = "UNIIA_admin",
                Name = "accessToken"
            };

            Token refresh = new()
            {
                Value = encryptedRefreshToken,
                LoginProvider = "UNIIA_admin",
                Name = "refreshToken"
            };

            tokens.Add(access);
            tokens.Add(refresh);

            return tokens;
        }
        public (string?, string?) GetTokensByUser(MongoIdentityUser user)
        {
            return (DecryptToken(user.Tokens.FirstOrDefault(n => n.Name == "accessToken")?.Value), DecryptToken(user.Tokens.FirstOrDefault(n => n.Name == "refreshToken")?.Value));
        }
        private string EncryptToken(string input)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(tokenKey);

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
            if(encryptedInput == null)
                return null;

            string[] parts = encryptedInput.Split(':');
            byte[] iv = Convert.FromBase64String(parts[0]);
            byte[] encryptedBytes = Convert.FromBase64String(parts[1]);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(tokenKey); 
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

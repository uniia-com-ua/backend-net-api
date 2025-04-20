using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UniiaAdmin.Data.Models.AuthModels
{
    public class UserTokens
    {
        public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
    }
}

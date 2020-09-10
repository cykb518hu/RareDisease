using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RareDisease.Data.Model
{
    public class LoginModel
    {
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }
}

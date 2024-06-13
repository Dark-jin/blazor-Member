using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Member.Model
{
    public class AuthenticateRequestAndResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}

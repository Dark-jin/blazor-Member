using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Member.Model
{
    public class APIs
    {
        public const string AuthenticateUser = "/api/Login";
        public const string RegisterUser = "/api/Registration";
        public const string RefreshToken = "/api/Login/refresh";
        public const string UserProfile = "/api/Login";
        public const string UserLogout = "/api/Login/logout";
    }
}

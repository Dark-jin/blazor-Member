using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Member.Model
{
    public class RegistrationModel
    {
        [Required]
        public string firstName { get; set; }
        [Required]
        public string lastName { get; set; }
        [Required]
        public string gender { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
        public string userAvatar { get; set; }
    }
}

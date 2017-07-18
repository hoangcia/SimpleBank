using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBank.Web.Utils
{
    public static class Common
    {
        public static string GenerateHashedPassword(string pwd)
        {
            var hashedPassword = string.Empty;

            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.ASCII.GetBytes(pwd));
                var strResult = BitConverter.ToString(result);
                hashedPassword = strResult.Replace("-", "");
            }

            return hashedPassword;
        }

        public static string GenerateNumber(string email, string name)
        {
            string prefix = "11";
            return (prefix + email.Substring(0, 4) + name.Substring(0, 4)).ToLower();
        }
    }
}

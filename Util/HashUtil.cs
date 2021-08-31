using System.Security.Cryptography;
using System.Text;

namespace DataSynchronizor.Util
{
    public static class HashUtil
    {
        public static string Sha1Signature(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = SHA1.Create().ComputeHash(buffer);

            StringBuilder sub = new StringBuilder();
            foreach (var t in data)
            {
                sub.Append(t.ToString("X2"));
            }

            return sub.ToString();
        }
    }
}
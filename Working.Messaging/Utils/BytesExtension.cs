using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Working.Messaging.Utils
{
    public static class BytesExtension
    {
        private static ICryptoTransform _enTrans = GetEnTrans();
        private static ICryptoTransform _deTrans = GetDeTrans();

        private static ICryptoTransform GetEnTrans()
        {
            var bKey = Encoding.UTF8.GetBytes("test");
            Array.Resize(ref bKey, 32);
            var aes = Rijndael.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = bKey;

            var trans = aes.CreateEncryptor();
            return trans;
        }

        private static ICryptoTransform GetDeTrans()
        {
            var bKey = Encoding.UTF8.GetBytes("test");
            Array.Resize(ref bKey, 32);
            var aes = Rijndael.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = bKey;

            var trans = aes.CreateDecryptor();
            return trans;
        }

        public static byte[] EncryptAES(this byte[] data, string key)
        {
            if (String.IsNullOrEmpty(key)) return null;

            return _enTrans.TransformFinalBlock(data, 0, data.Length);
        }

        public static byte[] DecryptAES(this byte[] data, string key)
        {
            if (String.IsNullOrEmpty(key)) return null;

            return _deTrans.TransformFinalBlock(data, 0, data.Length);
        }

        public static bool EndWith(this byte[] bytes, byte[] what, int end)
        {
            if (end > bytes.Length - what.Length)
                return false;
            long start = end - what.Length;

            var j = 0;
            for (long i = start; i < end; i++)
            {
                if (bytes[i] != what[j])
                    return false;
                j += 1;
            }
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Working.Messaging.Utils
{
    class CryptHelper
    {
        public static byte[] EncryptAES(this byte[] data, string key)
        {
            if (String.IsNullOrEmpty(key)) return null;
            var vector = key;
            var bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);
            var aes = Rijndael.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = bKey;

            using (var ms = new MemoryStream())
            {
                using (var encryptor = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    encryptor.Write(data, 0, data.Length);
                    encryptor.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        public static byte[] DecryptAES(this byte[] data, string key)
        {
            if (String.IsNullOrEmpty(key)) return null;
            var vector = key;
            var bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);

            var aes = Rijndael.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = bKey;
            using (var ms = new MemoryStream(data))
            {
                using (var decryptor = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (var originalMemory = new MemoryStream())
                    {
                        var buffer = new Byte[1024];
                        var readBytes = 0;
                        while ((readBytes = decryptor.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            originalMemory.Write(buffer, 0, readBytes);
                        }
                        return originalMemory.ToArray();
                    }
                }
            }
        }
    }
}

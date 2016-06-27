using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Working.Messaging
{
    public class BsonSerializer
    {
        private JsonSerializer _serializer = new JsonSerializer();

        public byte[] Serialize(object value)
        {
            MemoryStream ms = new MemoryStream();
            using (BsonWriter writer = new BsonWriter(ms))
            {
                _serializer.Serialize(writer, value);
            }

            return ms.ToArray();
        }

        public T Deserialize<T>(byte[] data)
        {
            if (data == null || data.Length == 0)
                return default(T);

            MemoryStream ms = new MemoryStream(data);
            using (BsonReader reader = new BsonReader(ms))
            {
                return _serializer.Deserialize<T>(reader);
            }
        }
    }
}

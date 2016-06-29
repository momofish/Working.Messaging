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
    public class BsonSerializer: ISerializer
    {
        private JsonSerializer _serializer = new JsonSerializer();

        public byte[] Serialize(object value)
        {
            var ms = new MemoryStream();
            using (var writer = new BsonWriter(ms))
            {
                _serializer.Serialize(writer, value);
            }

            return ms.ToArray();
        }

        public T Deserialize<T>(byte[] data)
        {
            if (data == null || data.Length == 0)
                return default(T);

            var ms = new MemoryStream(data);
            using (var reader = new BsonReader(ms))
            {
                return _serializer.Deserialize<T>(reader);
            }
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Working.Messaging.Utils;

namespace Working.Messaging
{
    public struct Message
    {
        private static readonly ISerializer _serializer = new BsonSerializer();
        public static readonly byte[] EndTag = new byte[] { 26, 98 };
        private static readonly int _lengthLen = 4;

        public long? Id { get; set; }
        public MsgType? MsgType { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public object Content { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("id: {0}, ", Id);
            builder.AppendFormat("type: {0}, ", MsgType);
            builder.AppendFormat("from: {0}, ", From);
            builder.AppendFormat("to: {0}, ", To);
            builder.AppendFormat("content: {0}", Content);

            return builder.ToString();
        }

        public void Validate()
        {
            if (Id == null)
                throw new ArgumentNullException("Id");
            if (MsgType == null)
                throw new ArgumentNullException("MsgType");
        }

        public byte[] Serialize()
        {
            var data = _serializer.Serialize(this).EncryptAES("test");

            var result = new byte[_lengthLen + data.Length + EndTag.Length];
            var pos = 0;
            byte[] part = null;

            part = BitConverter.GetBytes(data.Length);
            Array.Copy(part, 0, result, pos, part.Length); pos += part.Length;

            part = data;
            Array.Copy(part, 0, result, pos, part.Length); pos += part.Length;

            part = EndTag;
            Array.Copy(part, 0, result, pos, part.Length); pos += part.Length;

            return result;
        }

        public static Message[] ParseData(byte[] data)
        {
            var messages = new List<Message>();

            var pos = 0;
            do
            {
                var lengthData = new byte[_lengthLen];
                Array.Copy(data, pos, lengthData, 0, _lengthLen); pos += _lengthLen;
                var length = BitConverter.ToInt32(lengthData, 0);

                var bodyData = new byte[length];
                Array.Copy(data, pos, bodyData, 0, length); pos += length;
                var message = _serializer.Deserialize<Message>(bodyData.DecryptAES("test"));
                messages.Add(message);

                pos += EndTag.Length;
            } while (pos < data.Length);

            return messages.ToArray();
        }
    }

    public enum MsgType
    {
        Login = 0,
        Content = 1,
        Exception = 9
    }
}

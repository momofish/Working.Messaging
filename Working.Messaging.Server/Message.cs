using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Working.Messaging.Server
{
    public struct Message
    {
        public UInt64? Id { get; set; }
        public MsgType? MsgType { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public object Content { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void Validate()
        {
            if (Id == null)
                throw new ArgumentNullException("Id");
            if (MsgType == null)
                throw new ArgumentNullException("MsgType");
        }
    }

    public enum MsgType
    {
        Login = 0,
        Content = 1,
        Exception = 9
    }
}

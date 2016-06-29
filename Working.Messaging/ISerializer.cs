using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Working.Messaging
{
    public interface ISerializer
    {
        byte[] Serialize(object value);
        T Deserialize<T>(byte[] data);
    }
}

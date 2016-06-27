using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Working.Messaging.Utils
{
    public static class Enumerable
    {
        public static T[] Range<T>(this T[] array, int start, int length)
        {
            T[] range = null;
            Array.Copy(array, start, range, 0, length);

            return range;
        }

        public static int IndexOf<T>(this IEnumerable<T> enumerable, T toFind, int start = 0)
        {
            var index = 0;
            var query = enumerable.Skip(start);
            var count = query.Count();
            foreach (var element in query)
            {
                if (toFind.Equals(element))
                    return start + index;

                index++;
            }

            return -1;
        }
    }
}
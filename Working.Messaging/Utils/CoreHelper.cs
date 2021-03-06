﻿using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Working.Messaging.Utils
{
    public class CoreHelper
    {
        public static T Try<T>(Func<T> expression, out Exception exception, ILog logger = null)
        {
            try
            {
                exception = null;
                return expression.Invoke();
            }
            catch (Exception ex)
            {
                exception = ex;
                if (logger != null)
                    logger.Error(ex.Message);
            }

            return default(T);
        }

        public static void Try(Action expression, out Exception exception, ILog logger = null)
        {
            try
            {
                exception = null;
                expression.Invoke();
            }
            catch (Exception ex)
            {
                exception = ex;
                if (logger != null)
                    logger.Error(ex.Message);
            }
        }
    }
}

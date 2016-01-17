using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShallowNet
{
    public class DebugLog
    {
        public static Action<string> s_printFunc = null;

        internal static void WriteLine(string str)
        {
            if (s_printFunc == null)
            {
                throw new System.InvalidOperationException("DebugLog.s_printFunc has not been set");
            }

            s_printFunc(str);
        }

        internal static void WriteLine(string fmt, params object[] args)
        {
            string str = String.Format(fmt, args);
            WriteLine(str);
        }
    }
}

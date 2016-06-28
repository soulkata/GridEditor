using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    public static class Utils
    {
        public static void Log(string local, string message) { throw new Exception(local + Environment.NewLine + message); }
        public static void Assert(bool condition, string local, string message) { if (!condition) Utils.Log(local, message); }
    }
}

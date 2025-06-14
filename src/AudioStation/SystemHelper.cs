using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AudioStation
{
    internal static class SystemHelper
    {
        public static int GetCurrentDPI()
        {
            return (int)typeof(SystemParameters).GetProperty
                 ("Dpi", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);
        }

        public static double GetCurrentDPIScaleFactor()
        {
            return (double)SystemHelper.GetCurrentDPI() / 96;
        }
    }
}

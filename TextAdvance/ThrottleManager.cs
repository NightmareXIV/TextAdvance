using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdvance
{
    internal static class ThrottleManager
    {
        private static long LastTime = 0;
        public static bool Throttle(Action a, long delay = 500)
        {
            if(Environment.TickCount64 > LastTime + delay)
            {
                LastTime = Environment.TickCount64;
                a();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

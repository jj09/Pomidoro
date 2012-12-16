using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Pomidoro
{
    namespace ThreadPool
    {
        public enum Status
        {
            Unregistered = 0,
            Started = 1,
            Canceled = 2,
            Completed = 3
        }

        class ThreadPoolSample
        {
            public static ThreadPoolTimer PeriodicTimer;
            public static MainPage MainPage;
            public static long PeriodicTimerCount = 1500;
            public static int PeriodicTimerMilliseconds = 1000;
            public static string PeriodicTimerInfo = "";
            public static Status PeriodicTimerStatus = Status.Unregistered;
            public static int PeriodicTimerSelectedIndex = 0;
        }
    }
}

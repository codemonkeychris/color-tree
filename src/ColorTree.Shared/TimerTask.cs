using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ColorTree
{
    public class TimerTask
    {
        public static Task Wait(double milliseconds)
        {
            
            DateTime start = DateTime.Now;
            var t = new TaskCompletionSource<double>();
            if (milliseconds == 0)
            {
                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunIdleAsync(delegate(IdleDispatchedHandlerArgs e)
                {
                    t.SetResult((DateTime.Now - start).TotalMilliseconds);
                });
            }
            else
            {
                DispatcherTimer dt = new DispatcherTimer();
                dt.Interval = TimeSpan.FromMilliseconds(milliseconds);
                dt.Tick += delegate(object sender, object e)
                {
                    dt.Stop();
                    t.SetResult((DateTime.Now - start).TotalMilliseconds);
                };
                dt.Start();
            }
            return t.Task;
        }
    }
}

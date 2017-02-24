using System;
using System.Diagnostics;

namespace HashCode2017
{
    internal class TimerArea : IDisposable
    {
        private Stopwatch timer;

        public TimerArea(string displayText)
        {
            Console.Write(displayText + " ");
            timer = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            Console.WriteLine(timer.Elapsed);
        }
    }
}
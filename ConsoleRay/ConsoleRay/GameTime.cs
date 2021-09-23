using System;
using System.Diagnostics;

namespace ConsoleRay
{
    public class GameTime
    {
        Stopwatch loopSW = new Stopwatch();
        Stopwatch gameSW = new Stopwatch();
        long passedMilliseconds;
        long loopTime;

        public GameTime(long loopTime)
        {
            SetLoopTime(loopTime);
        }

        public void Start()
        {
            loopSW.Start();
            gameSW.Start();
        }

        public void Update()
        {
            if(!loopSW.IsRunning) 
                throw new Exception("TimeDelta stopwatch not running.");

            passedMilliseconds = loopSW.ElapsedMilliseconds;
        }

        public void SetLoopTime(long time)
        {
            this.loopTime = time;
        }

        public bool GameUpdate()
        {
            if(loopTime <= passedMilliseconds)
            {
                loopSW.Restart();
                return true;
            }
            return false;
        }

        public long GetElapsed()
        {
            return gameSW.Elapsed.Seconds;
        }
    }
}

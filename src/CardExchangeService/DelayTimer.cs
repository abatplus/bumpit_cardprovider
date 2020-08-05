using System;
using System.Threading;

namespace CardExchangeService
{
    public class DelayTimer : IDisposable
    {
        Timer _timer;
        TimerCallback _callback;
        object _state;
        long _delay;

        public DelayTimer(TimerCallback callback, object state, long delay)
        {
            _callback = callback;
            _state = state;
            _delay = delay;
        }

        public void Invoke(object state = null)
        {
            StopTimer();
            
            StartTimer(state);
        }

        private void StartTimer(object state = null)
        {
            _timer = new Timer(ExecuteCallback, state ?? _state, _delay, Timeout.Infinite);
        }

        public void InvokeDirect(object state = null) 
        {
            ExecuteCallback(state?? _state);
        }

        private void ExecuteCallback(object state) 
        {
            _callback.Invoke(state);
        }

        public void Stop()
        {
            StopTimer();
        }

        private void StopTimer()
        {
            if(_timer!= null) 
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        public void Dispose()
        {
            StopTimer();
        }
    }
}
using System.Threading;
using System;
using Xunit;
using CardExchangeService;
using FluentAssertions;
using System.Collections.Concurrent;

namespace CardExchangeServiceTests
{
    public class DelayTimerTest
    {
        string _savedMessage;

        private DelayTimer CreateTimer() => new DelayTimer(state => _savedMessage = state as string, "INIT", 100);

        [Fact]
        public void TestDirectInvocation()
        {
            using(DelayTimer dt = CreateTimer())
            {
                dt.InvokeDirect();
                
                _savedMessage.Should().Be("INIT");

                dt.Invoke("TIMED-FAILURE");
                Thread.Sleep(50);
                dt.InvokeDirect("DIRECT");
    
                _savedMessage.Should().Be("DIRECT");
            }
        }

        [Fact]
        public void TestSingleInvocation()
        {
            using(DelayTimer dt = CreateTimer())
            {
                dt.Invoke("SINGLE");

                Thread.Sleep(110);
                
                _savedMessage.Should().Be("SINGLE");
            }
        }

        [Fact]
        public void TestMultipleInvocation()
        {
            using(DelayTimer dt = CreateTimer())
            {            
                dt.Invoke("ONE-FAILURE!");
                Thread.Sleep(50);
                dt.Invoke("TWO-FAILURE!");
                Thread.Sleep(50);
                dt.Invoke("THREE");
                
                Thread.Sleep(110);  
                
                _savedMessage.Should().Be("THREE");
                
                dt.Invoke();
                
                Thread.Sleep(110);

                _savedMessage.Should().Be("INIT");
            }
        }

        [Fact]
        public void TestStop()
        {
            _savedMessage = "TEST_STOP";

            using(DelayTimer dt = CreateTimer())
            {                
                dt.Invoke("STOP-FAILURE!");           
                Thread.Sleep(50);
                dt.Stop();
                
                Thread.Sleep(110); 
            } 
            
            _savedMessage.Should().Be("TEST_STOP");
        }

        [Fact]
        public void TestDispose()
        {
            _savedMessage = "TEST_DISPOSE";

            using(DelayTimer dt = CreateTimer())
            {                
                dt.Invoke("DISPOSE-FAILURE!");
            } 
            
            Thread.Sleep(110); 
            
            _savedMessage.Should().Be("TEST_DISPOSE");
        }

        #region Tests collection of timers

        private readonly ConcurrentDictionary<string, DelayTimer> _deleteTimers = new ConcurrentDictionary<string, DelayTimer>();

        private void AddTimerToCollection(string key)
        {
            if (!_deleteTimers.ContainsKey(key))
            {
                _deleteTimers.TryAdd(key, new DelayTimer(_ => TimerCallback(key), null, 5000));
            }

            _deleteTimers[key].Invoke();
        }

        private void TimerCallback(string key)
        {
            _savedMessage = "CALLBACK";

            if (_deleteTimers.TryRemove(key, out var delay))
            {
                delay?.Dispose();
            }
        }


        [Fact]
        public void TestCollection_Init()
        {
            _savedMessage = "INIT";

            AddTimerToCollection("key1");

            _savedMessage.Should().Be("INIT");

            Thread.Sleep(5010);

            _savedMessage.Should().Be("CALLBACK");
        }

        [Fact]
        public void TestCollection_Change()
        {
            _savedMessage = "INIT";

            AddTimerToCollection("key1");

            _savedMessage.Should().Be("INIT");

            Thread.Sleep(1000);

            AddTimerToCollection("key1");
            
            _savedMessage.Should().Be("INIT");

            Thread.Sleep(5050);

            _savedMessage.Should().Be("CALLBACK");
        }

        #endregion
    }
}
using System.Threading;
using System;
using Xunit;
using CardExchangeService;
using FluentAssertions;

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
    }
}
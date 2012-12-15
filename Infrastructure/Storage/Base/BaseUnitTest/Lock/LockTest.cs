using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Runner.Base.Lock;
using TypeMock.ArrangeActAssert;

namespace BaseUnitTest.Lock
{
    [TestClass]
    public class LockTest
    {
        [TestMethod, Isolated]
        public void Key()
        {
            const string key = "111";
            var fake = Isolate.Fake.Instance<GlobalMutexScope>();
            Isolate.WhenCalled(() => fake.Key).WillReturn(key);

            Assert.AreEqual(fake.Key, key);
        }

        [TestMethod, Isolated]
        [ExpectedException(typeof(MutexScopeException), "Concurrent violation: cannot have multiple mutex which key is '111' at same time.")]
        public void AquireConcurrentLock1()
        {
            const string key = "111";
            GlobalMutexScope.AcquireMutexTimeout = TimeSpan.FromMilliseconds(100);
            using (new GlobalMutexScope(key, LockStyle.Single))
            {
                using (new GlobalMutexScope(key, LockStyle.Single))
                {

                }
            }
        }

        [TestMethod, Isolated]
        [ExpectedException(typeof(MutexScopeException), "Concurrent violation: cannot have multiple mutex which key is '111' at same time.")]
        public void AquireConcurrentLock2()
        {
            const string key = "111";
            GlobalMutexScope.AcquireMutexTimeout = TimeSpan.FromMilliseconds(100);
            using (new GlobalMutexScope(key, LockStyle.Database))
            {
                using (new GlobalMutexScope(key, LockStyle.Database))
                {

                }
            }
        }

        [TestMethod, Isolated]
        [ExpectedException(typeof(MutexScopeException), "Concurrent violation: cannot have multiple mutex which key is '111' at same time.")]
        public void AquireConcurrentLock3()
        {
            const string key = "111";
            GlobalMutexScope.AcquireMutexTimeout = TimeSpan.FromMilliseconds(100);
            using (new GlobalMutexScope(key, LockStyle.MSDTC))
            {
                using (new GlobalMutexScope(key, LockStyle.MSDTC))
                {

                }
            }
        }
    }

}

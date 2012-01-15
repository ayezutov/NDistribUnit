using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NDistribUnit.Common.Common;
using NUnit.Framework;
using System.Linq;

namespace NDistribUnit.Common.Tests.Common
{
    [TestFixture]
    public class CrossThreadPipeTests
    {
        [Test]
        public void EnsureSimpleSubscribePublish()
        {
            var pipe = new CrossThreadPipe<int>(TimeSpan.FromMilliseconds(100));
            Task.Factory.StartNew(()=>
                                      {
                                          Thread.Sleep(50);
                                          pipe.Publish(2);
                                      });

            var results = pipe.GetAvailableResults();

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0], Is.EqualTo(2));
        }

        [Test]
        public void EnsureClosingWithoutPublishingReturnsNull()
        {
            var pipe = new CrossThreadPipe<int>(TimeSpan.FromMilliseconds(100));
            Task.Factory.StartNew(()=>
                                      {
                                          Thread.Sleep(50);
                                          pipe.Close();
                                      });

            var results = pipe.GetAvailableResults();

            Assert.That(results, Is.Null);
        }

        [Test]
        public void EnsureClosingWithNoQueuedItemsReturnsNull()
        {
            var pipe = new CrossThreadPipe<int>(TimeSpan.FromMilliseconds(100));
            Task.Factory.StartNew(()=>
                                      {
                                          Thread.Sleep(50);
                                          pipe.Publish(6);
                                      });
            Task.Factory.StartNew(()=>
                                      {
                                          Thread.Sleep(100);
                                          pipe.Close();
                                      });

            Thread.Sleep(80);
            pipe.GetAvailableResults();

            var results = pipe.GetAvailableResults();

            Assert.That(results, Is.Null);
        }

        [Test]
        public void EnsurePublishingAfterClosingWithNoQueuedItemsReturnsNotNullIfNoReadOccurredInbetween()
        {
            var pipe = new CrossThreadPipe<int>(TimeSpan.FromMilliseconds(100));
            Task.Factory.StartNew(()=>
                                      {
                                          Thread.Sleep(50);
                                          pipe.Publish(6);
                                      });
            Task.Factory.StartNew(()=>
                                      {
                                          Thread.Sleep(100);
                                          pipe.Close();
                                          Thread.Sleep(20);
                                          pipe.Publish(7);
                                      });

            Thread.Sleep(80);
            pipe.GetAvailableResults();

            Thread.Sleep(70);
            var results = pipe.GetAvailableResults();

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0], Is.EqualTo(7));
        }

        [Test]
        public void EnsurePublishingAfterClosingWithNoQueuedItemsReturnsNotNullIfReadOccurredInbetween()
        {
            var pipe = new CrossThreadPipe<int>(TimeSpan.FromMilliseconds(100));
            Task.Factory.StartNew(()=>
                                      {
                                          Thread.Sleep(50);
                                          pipe.Publish(6);
                                      });
            Task.Factory.StartNew(()=>
                                      {
                                          Thread.Sleep(100);
                                          pipe.Close();
                                          Thread.Sleep(50);
                                          pipe.Publish(7);
                                      });

            Thread.Sleep(80);
            pipe.GetAvailableResults();

            Thread.Sleep(30);
            var results1 = pipe.GetAvailableResults();

            Thread.Sleep(80);
            var results2 = pipe.GetAvailableResults();

            Assert.That(results1, Is.Null);
            Assert.That(results2, Is.Null);
        }

        [Test]
        public void EnsureSubscribePublishSuccedsIfPublishedFirst()
        {
            var pipe = new CrossThreadPipe<int>(TimeSpan.FromMilliseconds(100));
            Task.Factory.StartNew(()=> pipe.Publish(2));
            
            Thread.Sleep(50);
            var results = pipe.GetAvailableResults();

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0], Is.EqualTo(2));
        }

        [Test]
        public void EnsureMultiplePublishesResultsInCumulative()
        {
            var pipe = new CrossThreadPipe<int>(TimeSpan.FromMilliseconds(100));
            Task.Factory.StartNew(
                ()=>
                    {
                        pipe.Publish(2);
                        pipe.Publish(4);
                        pipe.Publish(8);
                        pipe.Publish(16);
                    });
            
            Thread.Sleep(50);
            var results = pipe.GetAvailableResults();

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(4));
        }

        [Test]
        public void EnsureMultipleWithPublishesResultsInCumulative()
        {
            var pipe = new CrossThreadPipe<int>(TimeSpan.FromMilliseconds(100), ints =>
                                                                                    {
                                                                                        for (int i = ints.Count - 1; i > 0 ; i--)
                                                                                        {
                                                                                            ints[0] += ints[i];
                                                                                            ints.RemoveAt(i);
                                                                                        }
                                                                                    });
            Task.Factory.StartNew(
                ()=>
                    {
                        pipe.Publish(2);
                        pipe.Publish(4);
                        pipe.Publish(8);
                        pipe.Publish(16);
                    });
            
            Thread.Sleep(50);
            var results = pipe.GetAvailableResults();

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0], Is.EqualTo(2+4+8+16));
        }

        [Test]
        public void EnsureMultiplePublisheResultsInCumulative()
        {
            var pipe = new CrossThreadPipe<int>(TimeSpan.FromMilliseconds(100));

            Action<object> action = (v) => pipe.Publish((int)v);
            Task.Factory.StartNew(action, 2);
            Task.Factory.StartNew(action, 4);
            Task.Factory.StartNew(action, 8);
            Task.Factory.StartNew(action, 16);
            
            Thread.Sleep(50);
            var results = pipe.GetAvailableResults();
            
            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(4));
        }

        [Test]
        public void EnsureMultiplePublishesFromMultipleThreadResultsInCumulative()
        {
            var pipe = new CrossThreadPipe<int>(TimeSpan.FromMilliseconds(100));

            Action<object> action = (v) =>
                                        {
                                            var t = (Tuple<int, int>) v;
                                            Thread.Sleep(t.Item1);
                                            System.Console.WriteLine("Publish {0}", t.Item2);
                                            pipe.Publish(t.Item2);

                                            if (t.Item2 == 16)
                                            {
                                                System.Console.WriteLine("Close");
                                                pipe.Close();
                                            }
                                        };

            Task.Factory.StartNew(action, new Tuple<int, int>(10, 2));
            Task.Factory.StartNew(action, new Tuple<int, int>(20, 4));
            Task.Factory.StartNew(action, new Tuple<int, int>(70, 8));
            Task.Factory.StartNew(action, new Tuple<int, int>(80, 16));
            
            Thread.Sleep(50);
            var count = 0;
            var results = new List<int>();
            IList<int> temp;

            while ((temp = pipe.GetAvailableResults()) != null)
            {
                System.Console.WriteLine("- {0}", string.Join(",", temp.Select(i => i.ToString())));
                results.AddRange(temp);
                count++;
            }

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Has.Count.EqualTo(4));

            Assert.That(count, Is.GreaterThanOrEqualTo(2));
        }


        [Test]
        public void EnsureBufferIsEmptiedAfterCloseTimeout()
        {
            var pipe = new CrossThreadPipe<int>(TimeSpan.FromMilliseconds(100));

            Action<object> action = (v) =>
            {
                var t = (Tuple<int, int>)v;
                Thread.Sleep(t.Item1);
                pipe.Publish(t.Item2);

                if (t.Item2 == 16)
                    pipe.Close();
            };

            Task.Factory.StartNew(action, new Tuple<int, int>(10, 2));
            Task.Factory.StartNew(action, new Tuple<int, int>(20, 4));
            Task.Factory.StartNew(action, new Tuple<int, int>(70, 8));
            Task.Factory.StartNew(action, new Tuple<int, int>(80, 16));

            Thread.Sleep(200); // 80(when close is called) + 100(closeTimeout) + 20(for safety)

            var result = pipe.GetAvailableResults();

            Assert.That(result, Is.Null);
        }


        [Test]
        public void EnsureBufferGetsLatestResultsBeforeCloseTimeoutAfterClose()
        {
            var pipe = new CrossThreadPipe<int>(TimeSpan.FromMilliseconds(100));

            Action<object> action = (v) =>
            {
                var t = (Tuple<int, int>)v;
                Thread.Sleep(t.Item1);
                pipe.Publish(t.Item2);

                if (t.Item2 == 16)
                    pipe.Close();
            };

            Task.Factory.StartNew(action, new Tuple<int, int>(10, 2));
            Task.Factory.StartNew(action, new Tuple<int, int>(20, 4));
            Task.Factory.StartNew(action, new Tuple<int, int>(70, 8));
            Task.Factory.StartNew(action, new Tuple<int, int>(80, 16));

            Thread.Sleep(130);

            var result = pipe.GetAvailableResults();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(4));
        }

        [Test]
        public void ResultsAreReturnedOnlyOnce()
        {
            var pipe = new CrossThreadPipe<int>(TimeSpan.FromMilliseconds(100));

            Task.Factory.StartNew(() =>
                                      {
                                          Thread.Sleep(50);
                                          pipe.Publish(10);
                                          pipe.Close();
                                      });

            var result1 = pipe.GetAvailableResults();
            var result2 = pipe.GetAvailableResults();

            Assert.That(result1, Is.Not.Null);
            Assert.That(result1, Has.Count.EqualTo(1));
            Assert.That(result1[0], Is.EqualTo(10));

            Assert.That(result2, Is.Null);
        }
    }
}
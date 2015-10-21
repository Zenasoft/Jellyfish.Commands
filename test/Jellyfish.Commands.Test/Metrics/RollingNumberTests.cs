// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;
using Jellyfish.Commands.Metrics;
using System.Linq;

namespace Jellyfish.Commands.Tests
{
    public class RollingNumberTests
    {
        [Fact]
        public void testCreatesBuckets()
        {
            var time = new MockedClock();

            var counter = new RollingNumber(time, 200, 10);
            // confirm the initial settings
            Assert.Equal(200, counter.TimeInMs);
            Assert.Equal(10, counter.NumberOfBuckets);
            Assert.Equal(20, counter.BucketSizeInMs);


            // add a Success in each interval which should result in all 10 buckets being created with 1 Success in each
            for (int i = 0; i < counter.NumberOfBuckets; i++)
            {
                counter.Increment(RollingNumberEvent.SUCCESS);
                time.Increment(counter.BucketSizeInMs);
            }

            // confirm we have all 10 buckets
            var buckets = counter.GetBuckets().ToArray();
            Assert.Equal(10, buckets.Length);

            // add 1 more and we should still only have 10 buckets since that's the max
            counter.Increment(RollingNumberEvent.SUCCESS);
            buckets = counter.GetBuckets().ToArray();
            Assert.Equal(10, buckets.Length);
        }

    [Fact]
        public void testResetBuckets()
        {
            MockedClock time = new MockedClock();
            RollingNumber counter = new RollingNumber(time, 200, 10);

  
            // add 1
            counter.Increment(RollingNumberEvent.SUCCESS);

            // confirm we have 1 bucket
            var buckets = counter.GetBuckets().ToArray();
            Assert.Equal(1, buckets.Length);

            // confirm we still have 1 bucket
            Assert.Equal(1, buckets.Length);

            // add 1
            counter.Increment(RollingNumberEvent.SUCCESS);

            // we should now have a single bucket with no values in it instead of 2 or more buckets
            buckets = counter.GetBuckets().ToArray();
            Assert.Equal(1, buckets.Length);
        }


    [Fact]
        public void testIncrementInSingleBucket()
        {
            MockedClock time = new MockedClock();

            RollingNumber counter = new RollingNumber(time, 200, 10);

            // increment
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.FAILURE);
            counter.Increment(RollingNumberEvent.FAILURE);
            counter.Increment(RollingNumberEvent.TIMEOUT);

            // we should have 1 bucket
            var buckets = counter.GetBuckets().ToArray();
            Assert.Equal(1, buckets.Length);

            // the count should be 4
            Assert.Equal(4, counter.GetCurrentBucket().GetAdder(RollingNumberEvent.SUCCESS));
            Assert.Equal(2, counter.GetCurrentBucket().GetAdder(RollingNumberEvent.FAILURE));
            Assert.Equal(1, counter.GetCurrentBucket().GetAdder(RollingNumberEvent.TIMEOUT));
        }


        [Fact]
        public void testTimeout()
        {
            MockedClock time = new MockedClock();

            RollingNumber counter = new RollingNumber(time, 200, 10);

            // increment
            counter.Increment(RollingNumberEvent.TIMEOUT);

            var buckets = counter.GetBuckets().ToArray();
            // we should have 1 bucket
            Assert.Equal(1, buckets.Count());

            // the count should be 1
            Assert.Equal(1, buckets.First().GetAdder(RollingNumberEvent.TIMEOUT));
            Assert.Equal(1, counter.GetRollingSum(RollingNumberEvent.TIMEOUT));

            // sleep to get to a new bucket
            time.Increment(counter.BucketSizeInMs * 3);

            // incremenet again in latest bucket
            counter.Increment(RollingNumberEvent.TIMEOUT);

            // we should have 2 buckets
            buckets = counter.GetBuckets().ToArray();
            Assert.Equal(2, buckets.Length);

            // the counts of the last bucket
            Assert.Equal(1, buckets.First().GetAdder(RollingNumberEvent.TIMEOUT));

            // the total counts
            Assert.Equal(2, counter.GetRollingSum(RollingNumberEvent.TIMEOUT));
        }


    [Fact]
        public void testShortCircuited()
        {
            MockedClock time = new MockedClock();

            RollingNumber counter = new RollingNumber(time, 200, 10);

            // increment
            counter.Increment(RollingNumberEvent.SHORT_CIRCUITED);

            var buckets = counter.GetBuckets().ToArray();
            // we should have 1 bucket
            Assert.Equal(1, buckets.Length);

            // the count should be 1
            Assert.Equal(1, buckets.First().GetAdder(RollingNumberEvent.SHORT_CIRCUITED));
            Assert.Equal(1, counter.GetRollingSum(RollingNumberEvent.SHORT_CIRCUITED));

            // sleep to get to a new bucket
            time.Increment(counter.BucketSizeInMs * 3);

            // incremenet again in latest bucket
            counter.Increment(RollingNumberEvent.SHORT_CIRCUITED);

            // we should have 2 buckets
            buckets = counter.GetBuckets().ToArray();
            Assert.Equal(2, buckets.Length);

            // the counts of the last bucket
            Assert.Equal(1, buckets.First().GetAdder(RollingNumberEvent.SHORT_CIRCUITED));

            // the total counts
            Assert.Equal(2, counter.GetRollingSum(RollingNumberEvent.SHORT_CIRCUITED));

        }


    [Fact]
        public void testThreadPoolRejection()
        {
            testCounterType(RollingNumberEvent.THREAD_POOL_REJECTED);
        }


    [Fact]
        public void testFallbackSuccess()
        {
            testCounterType(RollingNumberEvent.FALLBACK_SUCCESS);
        }


    [Fact]
        public void testFallbackFailure()
        {
            testCounterType(RollingNumberEvent.FALLBACK_FAILURE);
        }


    [Fact]
        public void testExceptionThrow()
        {
            testCounterType(RollingNumberEvent.EXCEPTION_THROWN);
        }

        private void testCounterType(RollingNumberEvent type)
        {
            MockedClock time = new MockedClock();

            RollingNumber counter = new RollingNumber(time, 200, 10);

            // increment
            counter.Increment(type);

            // we should have 1 bucket
            var buckets = counter.GetBuckets().ToArray();
            Assert.Equal(1, buckets.Length);

            // the count should be 1
            Assert.Equal(1, buckets.First().GetAdder(type));
            Assert.Equal(1, counter.GetRollingSum(type));

            // sleep to get to a new bucket
            time.Increment(counter.BucketSizeInMs * 3);

            // increment again in latest bucket
            counter.Increment(type);

            // we should have 2 buckets
            buckets = counter.GetBuckets().ToArray();
            Assert.Equal(2, buckets.Length);

            // the counts of the last bucket
            Assert.Equal(1, buckets.First().GetAdder(type));

            // the total counts
            Assert.Equal(2, counter.GetRollingSum(type));

        }


    [Fact]
        public void testIncrementInMultipleBuckets()
        {
            MockedClock time = new MockedClock();

            RollingNumber counter = new RollingNumber(time, 200, 10);

            // increment
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.FAILURE);
            counter.Increment(RollingNumberEvent.FAILURE);
            counter.Increment(RollingNumberEvent.TIMEOUT);
            counter.Increment(RollingNumberEvent.TIMEOUT);
            counter.Increment(RollingNumberEvent.SHORT_CIRCUITED);

            // sleep to get to a new bucket
            time.Increment(counter.BucketSizeInMs * 3);

            // increment
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.FAILURE);
            counter.Increment(RollingNumberEvent.FAILURE);
            counter.Increment(RollingNumberEvent.FAILURE);
            counter.Increment(RollingNumberEvent.TIMEOUT);
            counter.Increment(RollingNumberEvent.SHORT_CIRCUITED);

            // we should have 2 buckets
            var buckets = counter.GetBuckets().ToArray();
            Assert.Equal(2, buckets.Length);

            // the counts of the last bucket
            Assert.Equal(2, buckets.First().GetAdder(RollingNumberEvent.SUCCESS));
            Assert.Equal(3, buckets.First().GetAdder(RollingNumberEvent.FAILURE));
            Assert.Equal(1, buckets.First().GetAdder(RollingNumberEvent.TIMEOUT));
            Assert.Equal(1, buckets.First().GetAdder(RollingNumberEvent.SHORT_CIRCUITED));

            // the total counts
            Assert.Equal(6, counter.GetRollingSum(RollingNumberEvent.SUCCESS));
            Assert.Equal(5, counter.GetRollingSum(RollingNumberEvent.FAILURE));
            Assert.Equal(3, counter.GetRollingSum(RollingNumberEvent.TIMEOUT));
            Assert.Equal(2, counter.GetRollingSum(RollingNumberEvent.SHORT_CIRCUITED));

            // wait until window passes
            time.Increment(counter.TimeInMs*10);

            // increment
            counter.Increment(RollingNumberEvent.SUCCESS);

            // the total counts should now include only the last bucket after a reset since the window passed
            Assert.Equal(1, counter.GetRollingSum(RollingNumberEvent.SUCCESS));
            Assert.Equal(0, counter.GetRollingSum(RollingNumberEvent.FAILURE));
            Assert.Equal(0, counter.GetRollingSum(RollingNumberEvent.TIMEOUT));
        }


    [Fact]
        public void testCounterRetrievalRefreshesBuckets()
        {
            MockedClock time = new MockedClock();

            RollingNumber counter = new RollingNumber(time, 200, 10);

            // increment
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.SUCCESS);
            counter.Increment(RollingNumberEvent.FAILURE);
            counter.Increment(RollingNumberEvent.FAILURE);

            // sleep to get to a new bucket
            time.Increment(counter.BucketSizeInMs * 3);

            var buckets = counter.GetBuckets().ToArray();
            // we should have 1 bucket since nothing has triggered the update of buckets in the elapsed time
            Assert.Equal(1, buckets.Length);

            // the total counts
            Assert.Equal(4, counter.GetRollingSum(RollingNumberEvent.SUCCESS));
            Assert.Equal(2, counter.GetRollingSum(RollingNumberEvent.FAILURE));

            // we should have 2 buckets as the counter 'gets' should have triggered the buckets being created to fill in time
            buckets = counter.GetBuckets().ToArray();
            Assert.Equal(2, buckets.Length);

            // wait until window passes
            time.Increment(counter.TimeInMs);

            // the total counts should all be 0 (and the buckets cleared by the get, not only increment)
            Assert.Equal(0, counter.GetRollingSum(RollingNumberEvent.SUCCESS));
            Assert.Equal(0, counter.GetRollingSum(RollingNumberEvent.FAILURE));

            // increment
            counter.Increment(RollingNumberEvent.SUCCESS);

            // the total counts should now include only the last bucket after a reset since the window passed
            Assert.Equal(1, counter.GetRollingSum(RollingNumberEvent.SUCCESS));
            Assert.Equal(0, counter.GetRollingSum(RollingNumberEvent.FAILURE));
        }


    [Fact]
        public void testUpdateMax1()
        {
            MockedClock time = new MockedClock();

            RollingNumber counter = new RollingNumber(time, 200, 10);

            // increment
            counter.UpdateRollingMax(RollingNumberEvent.THREAD_MAX_ACTIVE, 10);

            // we should have 1 bucket
            var buckets = counter.GetBuckets().ToArray();
            Assert.Equal(1, buckets.Length);

            // the count should be 10
            Assert.Equal(10, buckets.First().GetMaxUpdater(RollingNumberEvent.THREAD_MAX_ACTIVE));
            Assert.Equal(10, counter.GetRollingMaxValue(RollingNumberEvent.THREAD_MAX_ACTIVE));

            // sleep to get to a new bucket
            time.Increment(counter.BucketSizeInMs * 3);

            // increment again in latest bucket
            counter.UpdateRollingMax(RollingNumberEvent.THREAD_MAX_ACTIVE, 20);

            // we should have 2 buckets
            buckets = counter.GetBuckets().ToArray();
            Assert.Equal(2, buckets.Length);

            // the max
            Assert.Equal(20, buckets.First().GetMaxUpdater(RollingNumberEvent.THREAD_MAX_ACTIVE));

            // counts per bucket
            var values = counter.GetValues(RollingNumberEvent.THREAD_MAX_ACTIVE).ToArray();
            Assert.Equal(10, values[1]); // oldest bucket
            Assert.Equal(20, values[0]); // latest bucket
        }


    [Fact]
        public void testUpdateMax2()
        {
            MockedClock time = new MockedClock();

            RollingNumber counter = new RollingNumber(time, 200, 10);

            // increment
            counter.UpdateRollingMax(RollingNumberEvent.THREAD_MAX_ACTIVE, 10);
            counter.UpdateRollingMax(RollingNumberEvent.THREAD_MAX_ACTIVE, 30);
            counter.UpdateRollingMax(RollingNumberEvent.THREAD_MAX_ACTIVE, 20);

            // we should have 1 bucket
            var buckets = counter.GetBuckets().ToArray();
            Assert.Equal(1, buckets.Length);

            // the count should be 30
            Assert.Equal(30, counter.buckets.First().GetMaxUpdater(RollingNumberEvent.THREAD_MAX_ACTIVE));
            Assert.Equal(30, counter.GetRollingMaxValue(RollingNumberEvent.THREAD_MAX_ACTIVE));

            // sleep to get to a new bucket
            time.Increment(counter.BucketSizeInMs * 3);

            counter.UpdateRollingMax(RollingNumberEvent.THREAD_MAX_ACTIVE, 30);
            counter.UpdateRollingMax(RollingNumberEvent.THREAD_MAX_ACTIVE, 30);
            counter.UpdateRollingMax(RollingNumberEvent.THREAD_MAX_ACTIVE, 50);

            // we should have 2 buckets
            buckets = counter.GetBuckets().ToArray();
            Assert.Equal(2, buckets.Length);
        
            // the count
            Assert.Equal(50, buckets.First().GetMaxUpdater(RollingNumberEvent.THREAD_MAX_ACTIVE));
            Assert.Equal(50, counter.GetValueOfLatestBucket(RollingNumberEvent.THREAD_MAX_ACTIVE));

            // values per bucket
            var values = counter.GetValues(RollingNumberEvent.THREAD_MAX_ACTIVE).ToArray();
            Assert.Equal(30, values[1]); // oldest bucket
            Assert.Equal(50, values[0]); // latest bucket
        }


        public void testMaxValue()
        {
            MockedClock time = new MockedClock();

            RollingNumberEvent type = RollingNumberEvent.THREAD_MAX_ACTIVE;

            RollingNumber counter = new RollingNumber(time, 200, 10);

            counter.UpdateRollingMax(type, 10);

            // sleep to get to a new bucket
            time.Increment(counter.BucketSizeInMs);

            counter.UpdateRollingMax(type, 30);

            // sleep to get to a new bucket
            time.Increment(counter.BucketSizeInMs);

            counter.UpdateRollingMax(type, 40);

            // sleep to get to a new bucket
            time.Increment(counter.BucketSizeInMs);

            counter.UpdateRollingMax(type, 15);

            Assert.Equal(40, counter.GetRollingMaxValue(type));

        }


    [Fact]
        public void testEmptySum()
        {
            MockedClock time = new MockedClock();
            RollingNumberEvent type = RollingNumberEvent.SUCCESS;
            RollingNumber counter = new RollingNumber(time, 200, 10);
            Assert.Equal(0, counter.GetRollingSum(type));
        }


    [Fact]
        public void testEmptyMax()
        {
            MockedClock time = new MockedClock();
            RollingNumberEvent type = RollingNumberEvent.THREAD_MAX_ACTIVE;
            RollingNumber counter = new RollingNumber(time, 200, 10);
            Assert.Equal(0, counter.GetRollingMaxValue(type));
        }


    [Fact]
        public void testEmptyLatestValue()
        {
            MockedClock time = new MockedClock();
            RollingNumberEvent type = RollingNumberEvent.THREAD_MAX_ACTIVE;
            RollingNumber counter = new RollingNumber(time, 200, 10);
            Assert.Equal(0, counter.GetValueOfLatestBucket(type));
        }


    [Fact]
        public void testRolling()
        {
            MockedClock time = new MockedClock();
            RollingNumberEvent type = RollingNumberEvent.THREAD_MAX_ACTIVE;
            RollingNumber counter = new RollingNumber(time, 20, 2);
            // iterate over 20 buckets on a queue sized for 2
            for (int i = 0; i < 20; i++)
            {
                // first bucket
                counter.GetCurrentBucket();
                try
                {
                    time.Increment(counter.BucketSizeInMs);
                }
                catch (Exception)
                {
                    // ignore
                }

                counter.GetValueOfLatestBucket(type);
                Assert.Equal(2, counter.GetValues(type).Count());

                // System.out.println("Head: " + counter.buckets.state.get().head);
                // System.out.println("Tail: " + counter.buckets.state.get().tail);
            }
        }


    [Fact]
        public void testCumulativeCounterAfterRolling()
        {
            MockedClock time = new MockedClock();
            RollingNumberEvent type = RollingNumberEvent.SUCCESS;
            RollingNumber counter = new RollingNumber(time, 20, 2);

            Assert.Equal(0, counter.GetCumulativeSum(type));

            // iterate over 20 buckets on a queue sized for 2
            for (int i = 0; i < 20; i++)
            {
                // first bucket
                counter.Increment(type);
                try
                {
                    time.Increment(counter.BucketSizeInMs);
                }
                catch (Exception)
                {
                    // ignore
                }

                counter.GetValueOfLatestBucket(type);
                Assert.Equal(2, counter.GetValues(type).Count());

            }

            // cumulative count should be 20 (for the number of loops above) regardless of buckets rolling
            Assert.Equal(20, counter.GetCumulativeSum(type));
        }


        //public void testCumulativeCounterAfterRollingAndReset()
        //    {
        //        MockedClock time = new MockedClock();
        //        RollingNumberEvent type = RollingNumberEvent.Success;
        //        RollingNumber counter = new RollingNumber(time, 20, 2);

        //        Assert.Equal(0, counter.GetCumulativeSum(type));

        //        // iterate over 20 buckets on a queue sized for 2
        //        for (int i = 0; i < 20; i++)
        //        {
        //            // first bucket
        //            counter.Increment(type);
        //            try
        //            {
        //                time.Increment(counter.BucketSizeInMs);
        //            }
        //            catch (Exception e)
        //            {
        //                // ignore
        //            }

        //            Assert.Equal(2, counter.GetValues(type).length);

        //            counter.getValueOfLatestBucket(type);

        //            if (i == 5 || i == 15)
        //            {
        //                // simulate a reset occurring every once in a while
        //                // so we ensure the absolute sum is handling it okay
        //                counter.reset();
        //            }
        //        }

        //        // cumulative count should be 20 (for the number of loops above) regardless of buckets rolling
        //        Assert.Equal(20, counter.GetCumulativeSum(type));
        //    }


        //public void testCumulativeCounterAfterRollingAndReset2()
        //    {
        //        MockedClock time = new MockedClock();
        //        RollingNumberEvent type = RollingNumberEvent.Success;
        //        RollingNumber counter = new RollingNumber(time, 20, 2);

        //        Assert.Equal(0, counter.GetCumulativeSum(type));

        //        counter.Increment(type);
        //        counter.Increment(type);
        //        counter.Increment(type);

        //        // iterate over 20 buckets on a queue sized for 2
        //        for (int i = 0; i < 20; i++)
        //        {
        //            try
        //            {
        //                time.Increment(counter.BucketSizeInMs);
        //            }
        //            catch (Exception e)
        //            {
        //                // ignore
        //            }

        //            if (i == 5 || i == 15)
        //            {
        //                // simulate a reset occurring every once in a while
        //                // so we ensure the absolute sum is handling it okay
        //                counter.reset();
        //            }
        //        }

        //        // no increments during the loop, just some before and after
        //        counter.Increment(type);
        //        counter.Increment(type);

        //        // cumulative count should be 5 regardless of buckets rolling
        //        Assert.Equal(5, counter.GetCumulativeSum(type));
        //    }


    [Fact]
        public void testCumulativeCounterAfterRollingAndReset3()
        {
            MockedClock time = new MockedClock();
            RollingNumberEvent type = RollingNumberEvent.SUCCESS;
            RollingNumber counter = new RollingNumber(time, 20, 2);

            Assert.Equal(0, counter.GetCumulativeSum(type));

            counter.Increment(type);
            counter.Increment(type);
            counter.Increment(type);

            // iterate over 20 buckets on a queue sized for 2
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    time.Increment(counter.BucketSizeInMs);
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            // since we are rolling over the buckets it should reset naturally

            // no increments during the loop, just some before and after
            counter.Increment(type);
            counter.Increment(type);

            // cumulative count should be 5 regardless of buckets rolling
            Assert.Equal(5, counter.GetCumulativeSum(type));
        }
    }
}

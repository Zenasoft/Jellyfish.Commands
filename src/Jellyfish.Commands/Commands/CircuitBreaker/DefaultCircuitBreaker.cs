// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Jellyfish.Commands.Metrics;
using Jellyfish.Commands.Utils;
using Microsoft.Framework.Internal;
using System.Threading;

namespace Jellyfish.Commands.CircuitBreaker
{
    public class DefaultCircuitBreaker : ICircuitBreaker
    {
        private long _circuitOpen;
        private CommandMetrics _metrics;
        private CommandProperties _properties;
        private long _circuitOpenedOrLastTestedTime;
        private IClock _clock;


        internal DefaultCircuitBreaker([NotNull]CommandProperties properties, [NotNull]CommandMetrics metrics, [NotNull]IClock clock)
        {
            _clock = clock;
            _properties = properties;
            _metrics = metrics;
        }

        public bool AllowRequest
        {
            get
            {
                if (_properties.CircuitBreakerForceOpen.Value)
                    return false;

                if (_properties.CircuitBreakerForceClosed.Value)
                {
                    // we still want to allow isOpen() to perform it's calculations so we simulate normal behavior
                    IsOpen();
                    // properties have asked us to ignore errors so we will ignore the results of isOpen and just allow all traffic through
                    return true;
                }
                return !IsOpen() || AllowSingleTest();
            }
        }

        public bool AllowSingleTest()
        {
            long timeCircuitOpenedOrWasLastTested = Interlocked.Read(ref _circuitOpenedOrLastTestedTime);
            // 1) if the circuit is open
            // 2) and it's been longer than 'sleepWindow' since we opened the circuit
            if (Interlocked.Read(ref _circuitOpen) == 1 && _clock.EllapsedTimeInMs > timeCircuitOpenedOrWasLastTested + _properties.CircuitBreakerSleepWindowInMilliseconds.Value)
            {
                // We push the 'circuitOpenedTime' ahead by 'sleepWindow' since we have allowed one request to try.
                // If it succeeds the circuit will be closed, otherwise another singleTest will be allowed at the end of the 'sleepWindow'.
                if ( Interlocked.CompareExchange(ref _circuitOpenedOrLastTestedTime, _clock.EllapsedTimeInMs, timeCircuitOpenedOrWasLastTested) == timeCircuitOpenedOrWasLastTested)
                {
                    // if this returns true that means we set the time so we'll return true to allow the singleTest
                    // if it returned false it means another thread raced us and allowed the singleTest before we did
                    return true;
                }
            }
            return false;
        }

        public bool IsOpen()
        {
            if (Interlocked.Read(ref _circuitOpen ) == 1)
            {
                // if we're open we immediately return true and don't bother attempting to 'close' ourself as that is left to allowSingleTest and a subsequent successful test to close
                return true;
            }

            // we're closed, so let's see if errors have made us so we should trip the circuit open
            HealthCounts health = _metrics.GetHealthCounts();

            // check if we are past the statisticalWindowVolumeThreshold
            if (health.TotalRequests < _properties.CircuitBreakerRequestVolumeThreshold.Value)
            {
                // we are not past the minimum volume threshold for the statisticalWindow so we'll return false immediately and not calculate anything
                return false;
            }

            if (health.ErrorPercentage < _properties.CircuitBreakerErrorThresholdPercentage.Value)
            {
                return false;
            }
            else
            {
                // our failure rate is too high, trip the circuit
                if (Interlocked.CompareExchange(ref _circuitOpen, 1L, 0L) == 0L)
                {
                    // if the previousValue was false then we want to set the currentTime
                    Interlocked.Exchange(ref _circuitOpenedOrLastTestedTime, _clock.EllapsedTimeInMs);
                    return true;
                }
                else
                {
                    // How could previousValue be true? If another thread was going through this code at the same time a race-condition could have
                    // caused another thread to set it to true already even though we were in the process of doing the same
                    // In this case, we know the circuit is open, so let the other thread set the currentTime and report back that the circuit is open
                    return true;
                }
            }
        }

        public void MarkSuccess()
        {
           if( Interlocked.CompareExchange(ref _circuitOpen, 0L, 1L) == 1L)
            {
                _metrics.Reset();
            }
        }
    }
}

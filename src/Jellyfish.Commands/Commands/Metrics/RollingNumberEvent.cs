// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Jellyfish.Commands.Metrics
{
    /// <summary>
    /// <para>
    /// Various states/events that can be captured in the <see cref="RollingNumber"/>.
    /// </para>
    /// <para>
    /// Events can be type of Counter or MaxUpdater, which can be determined using the
    /// <see cref="RollingNumberEventExtensions.IsCounter()"/> or
    /// <see cref="RollingNumberEventExtensions.IsMaxUpdater()"/> extension methods.
    /// </para>
    /// <para>
    /// The Counter type events can be used with <see cref="RollingCounter.Increment()"/>, <see cref="RollingCounter.Add()"/>,
    /// <see cref="RollingCounter.GetRollingSum()"/> methods.
    /// </para>
    /// <para>
    /// The MaxUpdater type events can be used with <see cref="RollingCounter.UpdateRollingMax()"/> and <see cref="RollingCounter.GetRollingMax()"/> methods.
    /// </para>
    /// </summary>
    /// 

    //
    // WARNING : If you want to add new max counters, insert value after ThreadMaxActive and normal counter before.
    //

    public enum RollingNumberEvent
    {
        /// <summary>
        /// When a <see cref="Command" /> successfully completes.
        /// </summary>
        SUCCESS=0,

        /// <summary>
        /// When a <see cref="Command" /> fails to complete.
        /// </summary>
        FAILURE,

        /// <summary>
        /// When a <see cref="Command" /> times out (fails to complete).
        /// </summary>
        TIMEOUT,

        /// <summary>
        /// When a <see cref="Command" /> performs a short-circuited fallback.
        /// </summary>
        SHORT_CIRCUITED,

        /// <summary>
        /// When a <see cref="Command" /> is unable to queue up (thread pool rejection).
        /// </summary>
        THREAD_POOL_REJECTED,

        /// <summary>
        /// When a <see cref="Command" /> is unable to execute due to reaching the semaphore limit.
        /// </summary>
        SEMAPHORE_REJECTED,

        BAD_REQUEST,

        /// <summary>
        /// When a <see cref="Command" /> returns a Fallback successfully.
        /// </summary>
        FALLBACK_SUCCESS,

        /// <summary>
        /// When a <see cref="Command" /> attempts to retrieve a fallback but fails.
        /// </summary>
        FALLBACK_FAILURE,

        /// <summary>
        /// When a <see cref="Command" /> attempts to retrieve a fallback but it is rejected due to too many concurrent executing fallback requests.
        /// </summary>
        FALLBACK_REJECTION,

        /// <summary>
        /// When a <see cref="Command" /> throws an exception.
        /// </summary>
        EXCEPTION_THROWN,

        /// <summary>
        /// When a thread is executed.
        /// </summary>
        THREAD_EXECUTION,

        /// <summary>
        /// When a response is coming from a cache. The cache-hit ratio can be determined by dividing this number by the total calls.
        /// </summary>
        RESPONSE_FROM_CACHE,

        // From here all counters are max counters
        // Don't move this virtual value and create max counter after it
        MAX_COUNTER,

        /// <summary>
        /// A MaxUpdater event which is used to determine the maximum number of concurrent threads.
        /// </summary>
        THREAD_MAX_ACTIVE,

        COMMAND_MAX_ACTIVE

    }
}
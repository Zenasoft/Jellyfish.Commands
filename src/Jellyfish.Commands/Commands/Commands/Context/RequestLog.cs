using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jellyfish.Commands
{
    public class RequestLog
    {
        /**
         * Upper limit on RequestLog before ignoring further additions and logging warnings.
         * 
         * Intended to help prevent memory leaks when someone isn't aware of the
         * RequestContext lifecycle or enabling/disabling RequestLog.
         */
        internal static int MAX_STORAGE = 1000;

        /**
         * History of ServiceCommandInfo executed in this request.
         */
        private Lazy<ConcurrentQueue<ServiceCommandInfo>> allExecutedCommands = new Lazy<ConcurrentQueue<ServiceCommandInfo>>(() => new ConcurrentQueue<ServiceCommandInfo>());

        // prevent public instantiation
        internal RequestLog()
        {
        }

        /**
         * Retrieve {@link ServiceCommand} instances that were executed during this {@link JellyfishContext}.
         * 
         * @return {@code Collection<ServiceCommand<?>>}
         */
        public IEnumerable<ServiceCommandInfo> GetAllExecutedCommands()
        {
            return allExecutedCommands.IsValueCreated ? allExecutedCommands.Value.ToArray() : Enumerable.Empty<ServiceCommandInfo>();
        }

        /**
         * Add {@link ServiceCommand} instance to the request log.
         * 
         * @param command
         *            {@code ServiceCommand<?>}
         */
        public void AddExecutedCommand(ServiceCommandInfo command)
        {
            if (allExecutedCommands.Value.Count >= MAX_STORAGE)
            {
                return;
            }
            allExecutedCommands.Value.Enqueue(command);
        }

        /**
         * Formats the log of executed commands into a string usable for logging purposes.
         * <p>
         * Examples:
         * <ul>
         * <li>TestCommand[SUCCESS][1ms]</li>
         * <li>TestCommand[SUCCESS][1ms], TestCommand[SUCCESS, RESPONSE_FROM_CACHE][1ms]x4</li>
         * <li>TestCommand[TIMEOUT][1ms]</li>
         * <li>TestCommand[FAILURE][1ms]</li>
         * <li>TestCommand[THREAD_POOL_REJECTED][1ms]</li>
         * <li>TestCommand[THREAD_POOL_REJECTED, FALLBACK_SUCCESS][1ms]</li>
         * <li>TestCommand[EMIT, SUCCESS][1ms]</li>
         * <li>TestCommand[EMITx5, SUCCESS][1ms]</li>
         * <li>TestCommand[EMITx5, FAILURE, FALLBACK_EMITx6, FALLBACK_FAILURE][100ms]</li>
         * <li>TestCommand[FAILURE, FALLBACK_SUCCESS][1ms], TestCommand[FAILURE, FALLBACK_SUCCESS, RESPONSE_FROM_CACHE][1ms]x4</li>
         * <li>GetData[SUCCESS][1ms], PutData[SUCCESS][1ms], GetValues[SUCCESS][1ms], GetValues[SUCCESS, RESPONSE_FROM_CACHE][1ms], TestCommand[FAILURE, FALLBACK_FAILURE][1ms], TestCommand[FAILURE,
         * FALLBACK_FAILURE, RESPONSE_FROM_CACHE][1ms]</li>
         * </ul>
         * <p>
         * If a command has a multiplier such as <code>x4</code>, that means this command was executed 4 times with the same events. The time in milliseconds is the sum of the 4 executions.
         * <p>
         * For example, <code>TestCommand[SUCCESS][15ms]x4</code> represents TestCommand being executed 4 times and the sum of those 4 executions was 15ms. These 4 each executed the run() method since
         * <code>RESPONSE_FROM_CACHE</code> was not present as an event.
         *
         * <p>
         * For example, <code>TestCommand[EMITx5, FAILURE, FALLBACK_EMITx6, FALLBACK_FAILURE][100ms]</code> represents TestCommand executing observably, emitted 5 <code>OnNext</code>s, then an <code>OnError</code>.
         * This command also has an Observable fallback, and it emits 6 <code>OnNext</code>s, then an <code>OnCompleted</code>.
         *
         * @return String request log or "Unknown" if unable to instead of throwing an exception.
         */
        public string GetExecutedCommandsAsString()
        {
            if (allExecutedCommands.IsValueCreated == false)
                return String.Empty;
            try
            {
                var aggregatedCommandsExecuted = new Dictionary<string, int>();
                var aggregatedCommandExecutionTime = new Dictionary<string, int>();

                StringBuilder builder = new StringBuilder();
                int estimatedLength = 0;

                foreach (var command in allExecutedCommands.Value)
                {
                    builder.Clear();
                    builder.Append(command.CommandName);

                    var events = command.ExecutionEvents.ToArray();
                    //if (events.Length > 0)
                    //{
                    //    Array.Sort(events);

                    //    //replicate functionality of Arrays.toString(events.toArray()) to append directly to existing StringBuilder
                    //    builder.Append("[");
                    //    foreach (var evt in events)
                    //    {
                    //        switch (evt) {
                    //            case EMIT:
                    //                int numEmissions = command.getNumberEmissions();
                    //                if (numEmissions > 1)
                    //                {
                    //                    builder.append(event).append("x").append(numEmissions).append(", ");
                    //                }
                    //                else
                    //                {
                    //                    builder.append(event).append(", ");
                    //                }
                    //                break;
                    //            case FALLBACK_EMIT:
                    //                int numFallbackEmissions = command.getNumberFallbackEmissions();
                    //                if (numFallbackEmissions > 1)
                    //                {
                    //                    builder.append(event).append("x").append(numFallbackEmissions).append(", ");
                    //                }
                    //                else
                    //                {
                    //                    builder.append(event).append(", ");
                    //                }   
                    //                break;
                    //            default:
                    //                builder.append(event).append(", ");
                    //        }
                    //    }
                    //    builder.setCharAt(builder.length() - 2, ']');
                    //    builder.setLength(builder.length() - 1);
                    //} else {
                    builder.Append("[Executed]");
                    //}

                    var display = builder.ToString();
                    estimatedLength += display.Length + 12; //add 12 chars to display length for appending totalExecutionTime and count below
                    int counter;
                    if (aggregatedCommandsExecuted.TryGetValue(display, out counter))
                    {
                        aggregatedCommandsExecuted[display] = counter + 1;
                    }
                    else
                    {
                        // add it
                        aggregatedCommandsExecuted.Add(display, 1);
                    }

                    int executionTime = command.ExecutionTimeInMilliseconds;
                    if (executionTime < 0)
                    {
                        // do this so we don't create negative values or subtract values
                        executionTime = 0;
                    }
                   
                    if (aggregatedCommandExecutionTime.TryGetValue(display, out counter) && executionTime > 0)
                    {
                        // add to the existing executionTime (sum of executionTimes for duplicate command displayNames)
                        aggregatedCommandExecutionTime[display] = counter + executionTime;
                    }
                    else
                    {
                        // add it
                        aggregatedCommandExecutionTime.Add(display, executionTime);
                    }

                }

                builder.Clear();
                builder.EnsureCapacity(estimatedLength);
                foreach (var kv in aggregatedCommandsExecuted)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(kv.Key);

                    int totalExecutionTime;
                    aggregatedCommandExecutionTime.TryGetValue(kv.Key, out totalExecutionTime);
                    builder.Append("[").Append(totalExecutionTime).Append("ms]");

                    int count = kv.Value;
                    if (count > 1)
                    {
                        builder.Append("x").Append(count);
                    }
                }
                return builder.ToString();
            }
            catch (Exception )
            {
                //logger.error("Failed to create RequestLog response header string.", e);
                // don't let this cause the entire app to fail so just return "Unknown"
                return "Unknown";
            }
        }
    }
}

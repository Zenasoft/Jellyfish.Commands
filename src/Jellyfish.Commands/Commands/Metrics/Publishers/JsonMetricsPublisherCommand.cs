// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Jellyfish.Commands.CircuitBreaker;
using System.IO;
using System.Text;
using System.Globalization;

namespace Jellyfish.Commands.Metrics.Publishers
{
    public class JsonMetricsPublisherCommand : IMetricsPublisherCommand
    {
        private ICircuitBreaker circuitBreaker;
        private string commandName;
        private CommandMetrics metrics;
        private CommandProperties properties;

        public JsonMetricsPublisherCommand(CommandMetrics metrics, ICircuitBreaker circuitBreaker=null)
        {
            this.commandName = metrics.CommandName;
            this.metrics = metrics;
            this.circuitBreaker = circuitBreaker ?? CircuitBreaker.CircuitBreakerFactory.GetInstance(commandName);
            this.properties = metrics.Properties;
        }

        /// <summary>
        /// Very simple json writer 
        /// </summary>
        class JsonWriter
        {
            private TextWriter _writer;
            private bool _insertComma;
            private int _level;

            public JsonWriter(TextWriter writer)
            {
                this._writer = writer;
            }

            internal void writeStartObject()
            {
                _writer.Write("{");
                _insertComma = false;
                _level++;
            }

            internal void writeStringField(string name, string value)
            {
                if (_insertComma)
                    _writer.Write(", ");
                _writer.Write("\"{0}\": \"{1}\"", name, value);
                _insertComma = true;
            }

            internal void writeNumberField(string name, long value)
            {
                if (_insertComma)
                    _writer.Write(", ");
                _writer.Write("\"{0}\": {1}", name, value);
                _insertComma = true;
            }

            internal void writeBooleanField(string name, bool value)
            {
                if (_insertComma)
                    _writer.Write(", ");
                _writer.Write("\"{0}\": {1}", name, value ? "true" : " false");
                _insertComma = true;
            }

            internal void writeEndObject()
            {
                _writer.Write("}");
                _level--;
                _insertComma = _level > 0;
            }

            internal void writeObjectFieldStart(string name)
            {
                if (_insertComma)
                    _writer.Write(", ");
                _writer.Write("\"{0}\": {{", name);
                _insertComma = false;
                _level++;
            }
        }

        public virtual void Run(Action<string> handler)
        {
            try
            {
                foreach (var commandMetrics in CommandMetricsFactory.GetInstances())
                {
                    var jsonString = GetCommandJson(commandMetrics);
                    handler(jsonString);
                }

                //foreach (var collapserMetrics in CollapserMetrics.getInstances())
                //{
                //    String jsonString = getCollapserJson(collapserMetrics);
                //    listener.handleJsonMetric(jsonString);
                //}

            }
            catch (Exception)
            {
                //logger.warn("Failed to output metrics as JSON", e);
                // shutdown
                return;
            }
        }

        private String GetCommandJson(CommandMetrics commandMetrics)
        {
            var circuitBreaker = this.circuitBreaker ?? CircuitBreakerFactory.GetInstance(commandMetrics.CommandName);

            var sb = new StringBuilder(1024);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            var json = new JsonWriter(sw);

            json.writeStartObject();
            json.writeStringField("type", "ServiceCommand");
            json.writeStringField("name", commandMetrics.CommandName);
            json.writeStringField("group", commandMetrics.CommandGroup);
            json.writeNumberField("currentTime", DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond); // TODO check this

            // circuit breaker
            if (circuitBreaker == null)
            {
                // circuit breaker is disabled and thus never open
                json.writeBooleanField("isCircuitBreakerOpen", false);
            }
            else
            {
                json.writeBooleanField("isCircuitBreakerOpen", circuitBreaker.IsOpen());
            }
            HealthCounts healthCounts = commandMetrics.GetHealthCounts();
            json.writeNumberField("errorPercentage", healthCounts.ErrorPercentage);
            json.writeNumberField("errorCount", healthCounts.ErrorCount);
            json.writeNumberField("requestCount", healthCounts.TotalRequests);

            // rolling counters
            json.writeNumberField("rollingCountBadRequests", commandMetrics.GetRollingCount(RollingNumberEvent.BAD_REQUEST));
            json.writeNumberField("rollingCountExceptionsThrown", commandMetrics.GetRollingCount(RollingNumberEvent.EXCEPTION_THROWN));
            json.writeNumberField("rollingCountFailure", commandMetrics.GetRollingCount(RollingNumberEvent.FAILURE));
            json.writeNumberField("rollingCountFallbackFailure", commandMetrics.GetRollingCount(RollingNumberEvent.FALLBACK_FAILURE));
            json.writeNumberField("rollingCountFallbackRejection", commandMetrics.GetRollingCount(RollingNumberEvent.FALLBACK_REJECTION));
            json.writeNumberField("rollingCountFallbackSuccess", commandMetrics.GetRollingCount(RollingNumberEvent.FALLBACK_SUCCESS));
            json.writeNumberField("rollingCountResponsesFromCache", commandMetrics.GetRollingCount(RollingNumberEvent.RESPONSE_FROM_CACHE));
            json.writeNumberField("rollingCountSemaphoreRejected", commandMetrics.GetRollingCount(RollingNumberEvent.SEMAPHORE_REJECTED));
            json.writeNumberField("rollingCountShortCircuited", commandMetrics.GetRollingCount(RollingNumberEvent.SHORT_CIRCUITED));
            json.writeNumberField("rollingCountSuccess", commandMetrics.GetRollingCount(RollingNumberEvent.SUCCESS));
            json.writeNumberField("rollingCountThreadPoolRejected", commandMetrics.GetRollingCount(RollingNumberEvent.THREAD_POOL_REJECTED));
            json.writeNumberField("rollingCountTimeout", commandMetrics.GetRollingCount(RollingNumberEvent.TIMEOUT));

            json.writeNumberField("currentConcurrentExecutionCount", commandMetrics.CurrentConcurrentExecutionCount);
            json.writeNumberField("rollingMaxConcurrentExecutionCount", commandMetrics.GetRollingMaxConcurrentExecutions());

            // latency percentiles
            json.writeNumberField("latencyExecute_mean", commandMetrics.GetExecutionTimeMean());
            json.writeObjectFieldStart("latencyExecute");
            json.writeNumberField("0", commandMetrics.GetExecutionTimePercentile(0));
            json.writeNumberField("25", commandMetrics.GetExecutionTimePercentile(25));
            json.writeNumberField("50", commandMetrics.GetExecutionTimePercentile(50));
            json.writeNumberField("75", commandMetrics.GetExecutionTimePercentile(75));
            json.writeNumberField("90", commandMetrics.GetExecutionTimePercentile(90));
            json.writeNumberField("95", commandMetrics.GetExecutionTimePercentile(95));
            json.writeNumberField("99", commandMetrics.GetExecutionTimePercentile(99));
            json.writeNumberField("99.5", commandMetrics.GetExecutionTimePercentile(99.5));
            json.writeNumberField("100", commandMetrics.GetExecutionTimePercentile(100));
            json.writeEndObject();
            //
            json.writeNumberField("latencyTotal_mean", commandMetrics.GetTotalTimeMean());
            json.writeObjectFieldStart("latencyTotal");
            json.writeNumberField("0", commandMetrics.GetTotalTimePercentile(0));
            json.writeNumberField("25", commandMetrics.GetTotalTimePercentile(25));
            json.writeNumberField("50", commandMetrics.GetTotalTimePercentile(50));
            json.writeNumberField("75", commandMetrics.GetTotalTimePercentile(75));
            json.writeNumberField("90", commandMetrics.GetTotalTimePercentile(90));
            json.writeNumberField("95", commandMetrics.GetTotalTimePercentile(95));
            json.writeNumberField("99", commandMetrics.GetTotalTimePercentile(99));
            json.writeNumberField("99.5", commandMetrics.GetTotalTimePercentile(99.5));
            json.writeNumberField("100", commandMetrics.GetTotalTimePercentile(100));
            json.writeEndObject();

            // property values for reporting what is actually seen by the command rather than what was set somewhere
            var commandProperties = commandMetrics.Properties;

            json.writeNumberField("propertyValue_circuitBreakerRequestVolumeThreshold", commandProperties.CircuitBreakerRequestVolumeThreshold.Value);
            json.writeNumberField("propertyValue_circuitBreakerSleepWindowInMilliseconds", commandProperties.CircuitBreakerSleepWindowInMilliseconds.Value);
            json.writeNumberField("propertyValue_circuitBreakerErrorThresholdPercentage", commandProperties.CircuitBreakerErrorThresholdPercentage.Value);
            json.writeBooleanField("propertyValue_circuitBreakerForceOpen", commandProperties.CircuitBreakerForceOpen.Value);
            json.writeBooleanField("propertyValue_circuitBreakerForceClosed", commandProperties.CircuitBreakerForceClosed.Value);
            json.writeBooleanField("propertyValue_circuitBreakerEnabled", commandProperties.CircuitBreakerEnabled.Value);
        
            json.writeStringField("propertyValue_executionIsolationStrategy", commandProperties.ExecutionIsolationStrategy.Value.ToString());
            json.writeNumberField("propertyValue_executionIsolationThreadTimeoutInMilliseconds", commandProperties.ExecutionIsolationThreadTimeoutInMilliseconds.Value);
            json.writeNumberField("propertyValue_executionTimeoutInMilliseconds", commandProperties.ExecutionIsolationThreadTimeoutInMilliseconds.Value);
            //json.writeBooleanField("propertyValue_executionIsolationThreadInterruptOnTimeout", commandProperties.executionIsolationThreadInterruptOnTimeout().get());
            //json.writeStringField("propertyValue_executionIsolationThreadPoolKeyOverride", commandProperties.executionIsolationThreadPoolKeyOverride().get());
            json.writeNumberField("propertyValue_executionIsolationSemaphoreMaxConcurrentRequests", commandProperties.ExecutionIsolationSemaphoreMaxConcurrentRequests.Value);
            json.writeNumberField("propertyValue_fallbackIsolationSemaphoreMaxConcurrentRequests", commandProperties.FallbackIsolationSemaphoreMaxConcurrentRequests.Value);

            json.writeNumberField("propertyValue_metricsRollingStatisticalWindowInMilliseconds", commandProperties.MetricsRollingStatisticalWindowInMilliseconds.Value);
            
            json.writeBooleanField("propertyValue_requestCacheEnabled", commandProperties.RequestCacheEnabled.Value);
            json.writeBooleanField("propertyValue_requestLogEnabled", commandProperties.RequestLogEnabled.Value);

            json.writeNumberField("reportingHosts", 1); // this will get summed across all instances in a cluster
                                                        //json.writeStringField("threadPool", commandMetrics.getThreadPoolKey().name());

            // Hystrix specific
            json.writeNumberField("rollingCountCollapsedRequests", 0);
            json.writeBooleanField("propertyValue_executionIsolationThreadInterruptOnTimeout", false);

            json.writeEndObject();

            return sb.ToString();
        }
    }
}
using Lib.data.elasticsearch;
using Lib.helper;
using log4net.Core;
using Nest;
using System;

namespace Lib.extra.log
{
    [ElasticsearchType(IdProperty = nameof(UID), Name = nameof(ESLogLine))]
    public class ESLogLine : IElasticSearchIndex
    {
        private static readonly DateTime EpochStart = new DateTime(1970, 1, 1);

        [Obsolete("不要调用")]
        public ESLogLine() { }

        public ESLogLine(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null) { throw new Exception("loggingEvent不能为null"); }
            HostName = loggingEvent.LookupProperty(LoggingEvent.HostNameProperty).ToString();
            Identity = loggingEvent.Identity;
            UserName = loggingEvent.UserName;
            Domain = loggingEvent.Domain;
            TimeStamp = (long)(loggingEvent.TimeStamp.ToUniversalTime() - EpochStart).TotalMilliseconds;
            Level = loggingEvent.Level.DisplayName;
            LoggerName = loggingEvent.LoggerName;
            Thread = loggingEvent.ThreadName;
            Message = loggingEvent.RenderedMessage;
            Throwable = loggingEvent.GetExceptionString();
            //location
            Class = loggingEvent.LocationInformation?.ClassName;
            Method = loggingEvent.LocationInformation?.MethodName;
            File = loggingEvent.LocationInformation?.FileName;
            Line = loggingEvent.LocationInformation?.LineNumber;

            this.UID = Com.GetUUID();
            this.CreateTime = DateTime.Now;
            this.UpdateTime = this.CreateTime;
        }

        [Text(Name = nameof(UID), Index = false)]
        public string UID { get; set; }

        [Text(Name = nameof(HostName), Index = false)]
        public string HostName { get; set; }

        [Text(Name = nameof(Identity), Index = false)]
        public string Identity { get; set; }

        [Text(Name = nameof(UserName), Index = false)]
        public string UserName { get; set; }

        [Text(Name = nameof(Domain), Index = false)]
        public string Domain { get; set; }

        [Text(Name = nameof(LoggerName), Index = false)]
        public string LoggerName { get; set; }

        [Number(Name = nameof(TimeStamp), Index = false)]
        public long TimeStamp { get; set; }

        [Text(Name = nameof(Level), Index = false)]
        public string Level { get; set; }

        [Text(Name = nameof(Thread), Index = false)]
        public string Thread { get; set; }

        [Text(Name = nameof(Message), Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
        public string Message { get; set; }

        [Text(Name = nameof(Throwable), Index = false)]
        public string Throwable { get; set; }

        [Text(Name = nameof(Class), Index = false)]
        public string Class { get; set; }

        [Text(Name = nameof(Method), Index = false)]
        public string Method { get; set; }

        [Text(Name = nameof(File), Index = false)]
        public string File { get; set; }

        [Text(Name = nameof(Line), Index = false)]
        public string Line { get; set; }

        [Date(Name = nameof(CreateTime), Index = false)]
        public DateTime CreateTime { get; set; }

        [Date(Name = nameof(UpdateTime), Index = false)]
        public DateTime UpdateTime { get; set; }
    }
}

﻿using System;
using Glimpse.Core2.Extensibility;

namespace Glimpse.Mvc.Message
{
    public class TimerResultMessage
    {
        public TimerResultMessage(TimerResult timerResult, string eventName, string eventCategory)
        {
            Result = timerResult;
            EventName = eventName;
            EventCategory = eventCategory;
        }

        public string EventName { get; set; }
        public string EventCategory { get; set; }
        private TimerResult Result { get; set; }

        public long Offset
        {
            get { return Result.Offset; }
        }

        public TimeSpan Duration
        {
            get { return Result.Duration; }
        }
    }
}
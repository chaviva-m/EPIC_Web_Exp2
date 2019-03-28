using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExperimentApp.Infrastructure
{
    public class Emotions
    {
        public static List<string> SelfReportEmotions { get; set; } = new List<string>
        {
            "אדישות Apathy", "עצב Sadness", "רוגע Calm",
            "בידור Amusement","צער Grief", "שמחה Happiness",
        };
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExperimentApp.Infrastructure
{
    public enum EmotionalContentEnum
    {
        Happy,
        Sad,
        Neutral
    }

    public enum ExperimentOptionEnum
    {
        A, B, C
    }

    public class EmotionInducingContent
    {
        public static string VideoRootDir = Directories.ContentVideos + "//";

        public static Dictionary<EmotionalContentEnum, string> VideoByContent = new Dictionary<EmotionalContentEnum, string>
        {
            {EmotionalContentEnum.Happy, VideoRootDir + "Friends.mp4" },
            {EmotionalContentEnum.Sad, VideoRootDir + "LionKingSubtitles1.mp4" },
            {EmotionalContentEnum.Neutral, VideoRootDir + "GreatBarrierReef.mp4" }
        };

    }
}
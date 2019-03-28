using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExperimentApp.Infrastructure;

namespace ExperimentApp.Models
{
    public class Participant
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        //induced emotion
        private EmotionalContentEnum emotionalContent;
        public EmotionalContentEnum EmotionalContent { get { return emotionalContent; } set { emotionalContent = value; ExpOption = (ExperimentOptionEnum)Convert.ToInt32(emotionalContent); } }
        //experiment option
        private ExperimentOptionEnum expOption;
        [Display(Name = "Experiment Option")]
        public ExperimentOptionEnum ExpOption { get { return expOption; } set { expOption = value; emotionalContent = (EmotionalContentEnum)Convert.ToInt32(expOption); } }

        //writing assignment timing
        public float WritingAssignmentTime { get; set; }

        //emotions from baseline video
        public string VideoBaselinePath { get; set; }
        public string VideoBaselineWithLabelsPath { get; set; }
        public string VideoBaselineDataPath { get; set; }
        public virtual List<VideoBaselineEmotion> VideoBaselineEmotions { get; set; } //change this to dif Model...?

        //emotions from video
        public string VideoPath { get; set; }
        public string VideoWithLabelsPath { get; set; }
        public string VideoDataPath { get; set; }
        public virtual List<VideoEmotion> VideoEmotions { get; set; }

        //emotions from baseline audio
        public string AudioBaselinePath { get; set; }
        public string AudioBaselineDataPath { get; set; }
        public virtual List<AudioBaselineEmotion> AudioBaselineEmotions { get; set; } = new List<AudioBaselineEmotion>();

        //emotions from audio
        public string AudioPath { get; set; }
        public string AudioDataPath { get; set; }
        public virtual List<AudioEmotion> AudioEmotions { get; set; } = new List<AudioEmotion>();

        //emotions - self report
        public SelfReportQuestionnaire SelfReportQuestionnaire { get; set; }

        //Ultimatum Game
        public int UltimatumReceivedSum { get; } = 10;
        private int ultimatumGaveSum;
        [Required(ErrorMessage = "שדה חובה")]
        [Range(0, 10, ErrorMessage = "יש להכניס מספר שלם בין 0 ל 10")]
        [Display(Name = "הצעה")]
        public int UltimatumGaveSum { get { return ultimatumGaveSum; } set { ultimatumGaveSum = value; UltimatumGavePercent = (ultimatumGaveSum / (float)UltimatumReceivedSum) * 100;} }
        public float UltimatumGavePercent { get; private set; }
        public float UltimatumInstructionReadTime { get; set; }
        public float UltimatumDMtime { get; set; }

        //Trust Game
        public int TrustReceivedSum { get; } = 10;
        private int trustGaveSum;
        [Required(ErrorMessage = "שדה חובה")]
        [Range(0, 10, ErrorMessage = "יש להכניס מספר שלם בין 0 ל 10")]
        [Display(Name = "הצעה")]
        public int TrustGaveSum { get { return trustGaveSum; }  set { trustGaveSum = value; TrustGavePercent = (trustGaveSum / (float)TrustReceivedSum) * 100; this.getRewardVal(trustGaveSum); } }
        public float TrustGavePercent { get; private set; }


        private int trustGotSum = 0;
        public int TrustGotSum { get { return trustGotSum; } private set { trustGotSum = value; } }

        public float TrustInstructionReadTime { get; set; }
        public float TrustDMtime { get; set; }

        private int totalAward;
        public int TotalAward { get { return totalAward; } private set { totalAward = value; } }

        public void getRewardVal(int trustGave)
        {
            trustGave = trustGave * 3;
            int hasUltimatum = this.UltimatumReceivedSum - this.ultimatumGaveSum;
            int hasTrust = this.TrustReceivedSum - this.TrustGaveSum;
            int has = hasUltimatum + hasTrust;
            Random random = new Random();
            int n = random.Next(15 - has, 26 - has);
            if (n > trustGave)
            {
                this.TotalAward = has + trustGave;
                this.TrustGotSum = trustGave;
                return;
            }
            this.TrustGotSum = n;
            this.TotalAward = has + n;
            return;
        }

    }
}
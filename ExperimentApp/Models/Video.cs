using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ExperimentApp.Infrastructure;

namespace ExperimentApp.Models
{
    public class Video
    {
        private readonly string dataRelDir;
        private EventWaitHandle ewh = new EventWaitHandle(false, EventResetMode.AutoReset);
        private static readonly Object obj = new Object();
        private bool ProcessSuccess = true;
        private readonly string python_window;
        private readonly string webcam_window;
        private readonly bool baseline_flag;

        public Video(string dataRelDir, string python_window, string webcam_window, bool baseline_flag)
        {
            this.dataRelDir = dataRelDir;
            this.python_window = python_window;
            this.webcam_window = webcam_window;
            this.baseline_flag = baseline_flag;
        }


        public void StopRecording()
        {
            //signal to stop process
            ewh.Set();
            //wait for process to finish
            do
            {
                Thread.Sleep(2000);
            } while (WindowIsOpen(webcam_window));
        }

        public bool RecordVideo(Participant participant)
        {
            string file;
            string video;
            string labeledVideo;

            if (baseline_flag)
            {
                file = "VideoBaselineData" + participant.ID;
                participant.VideoBaselineDataPath = file;
                video = "VideoBaseline" + participant.ID;
                participant.VideoBaselinePath = video + ".avi";
                labeledVideo = "VideoBaselineLabeled" + participant.ID;
                participant.VideoBaselineWithLabelsPath = labeledVideo + ".avi";
            } else
            {
                file = "VideoData" + participant.ID;
                participant.VideoDataPath = file;
                video = "Video" + participant.ID;
                participant.VideoPath = video + ".avi";
                labeledVideo = "VideoLabeled" + participant.ID;
                participant.VideoWithLabelsPath = labeledVideo + ".avi";
            }

            string dataRootDir = Directories.Data + "\\" + dataRelDir + "\\";

            string processRelPath = "Scripts\\VideoEmotionDetector\\run_emotions.bat";
            string processPath = HttpContext.Current.Server.MapPath(Path.Combine("~", processRelPath));

            string codeDir = Directories.VideoEmotionDetector;


            ProcessSuccess = true;
            Task t = new Task(() =>
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(processPath)
                    {
                        WindowStyle = ProcessWindowStyle.Minimized,
                        Arguments = String.Format("{0} {1} {2} {3} {4} {5}", codeDir, python_window,
                        dataRootDir + file, dataRootDir + video, dataRootDir + labeledVideo, webcam_window)
                    };
                    //start process
                    using (Process myProcess = Process.Start(startInfo))
                    {
                        //wait for signal
                        ewh.WaitOne();
                        // Close Emotion by sending a close message to its window.
                        SearchAndClose(python_window);
                    }
                }
                catch (Exception e)
                {
                    lock (obj)
                    {
                        ProcessSuccess = false;
                    }
                    Console.WriteLine("The following exception was raised: ");
                    Console.WriteLine(e.Message);
                }
            });
            t.Start();
            //check if camera window opened
            Thread.Sleep(10000);
            while (!WindowIsOpen(webcam_window))
            {
                if (!WindowIsOpen(python_window))
                {
                    lock (obj)
                    {
                        ProcessSuccess = false;
                    }
                    break;
                }
                Thread.Sleep(3000);
            }
            return ProcessSuccess;            

        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string className, string windowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int WM_CLOSE = 0x10;
        private const int WM_QUIT = 0x12;

        private bool WindowIsOpen(string windowName)
        {
            IntPtr hWnd = FindWindow(null, windowName);
            if (hWnd == IntPtr.Zero)
                return false;
            return true;
        }

        private void SearchAndClose(string windowName)
        {
            IntPtr hWnd = FindWindow(null, windowName);
            if (hWnd == IntPtr.Zero)
                throw new Exception("Couldn't find window!");
            SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public void GetEmotionsFromFile(Participant participant)
        {
            string fileRelPath;
            if (baseline_flag)
            {
                fileRelPath = participant.VideoBaselineDataPath;
            } else
            {
                fileRelPath = participant.VideoDataPath;
            }
            string dataRootDir = Directories.Data;
            string filePath = dataRootDir + "\\" + dataRelDir + "\\" + fileRelPath;
            List<string> emotionsLabels;
            try
            { 

            using (StreamReader sr = File.OpenText(filePath))
            {
                emotionsLabels = sr.ReadLine().Split('\t').ToList();
            }        
            emotionsLabels = emotionsLabels.GetRange(1, emotionsLabels.Count() - 1); //remove "prediction" (1st word)
            var emotionsVector = File.ReadLines(filePath).Select(line => line.Split('\t')[0]).ToList();
            emotionsVector = emotionsVector.GetRange(1, emotionsVector.Count() - 1); //remove "prediction" (1st line)
            var groups = emotionsVector.ToLookup(i => i);
            int vectorLength = emotionsVector.Count;
            //calculate emotion frequencies in video
            double count;
            double freq;
            foreach (string emotion in emotionsLabels)
            {
                if (groups[emotion].Any()) { count = groups[emotion].Count(); }
                else { count = 0; }
                if (vectorLength == 0)
                {
                    freq = 0;
                } else
                {
                    freq = count / vectorLength;
                }
                //add emotion frequency to participant
                if (baseline_flag)
                    {
                        participant.VideoBaselineEmotions.Add(new VideoBaselineEmotion
                        {
                            ParticipantID = participant.ID,
                            Name = emotion,
                            Strength = freq
                        });
                    }
                    else
                    {
                        participant.VideoEmotions.Add(new VideoEmotion
                        {
                            ParticipantID = participant.ID,
                            Name = emotion,
                            Strength = freq
                        });
                    }
            }
            } catch (Exception e)
            {
                return;
            }

        }
    }
}

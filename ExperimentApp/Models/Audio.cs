using ExperimentApp.Infrastructure;
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

namespace ExperimentApp.Models
{
    public class Audio
    {
        private readonly string dataRelDir;
        private readonly string window_title;
        private readonly bool baseline_flag;

        public Audio(string dataRelDir, string window_title, bool baseline_flag)
        {
            this.dataRelDir = dataRelDir;
            this.window_title = window_title;
            this.baseline_flag = baseline_flag;
        }

        public bool RunVokaturi(Participant participant)
        {
            string output_file;
            string input_file;

            if (baseline_flag)
            {
                output_file = "AudioBaselineData" + participant.ID;
                participant.AudioBaselineDataPath = output_file;
                input_file = participant.AudioBaselinePath;

            }
            else
            {
                output_file = "AudioData" + participant.ID;
                participant.AudioDataPath = output_file;
                input_file = participant.AudioPath;
            }

            string inputFullPath = Directories.AudioFile + '\\' + input_file;
            string dataRootDir = Directories.Data + "\\" + dataRelDir + "\\";
            string outputFullPath = dataRootDir + output_file;


            string processRelPath = "Scripts\\AudioEmotionDetector\\run_vokaturi.bat";
            string processPath = HttpContext.Current.Server.MapPath(Path.Combine("~", processRelPath));

            string codeDir = Directories.AudioEmotionDetector;


            Task<bool> t = new Task<bool>(() =>
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(processPath)
                    {
                        WindowStyle = ProcessWindowStyle.Minimized,
                        Arguments = String.Format("{0} {1} {2} {3}", codeDir, window_title, inputFullPath, outputFullPath),
                        UseShellExecute = true
                        //UseShellExecute = false
                    };
                    //start process
                    using (Process myProcess = Process.Start(startInfo)) { }
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("The following exception was raised: ");
                    Console.WriteLine(e.Message);
                    return false;
                }
            });
            t.Start();
            //wait for process to finish
            do
            {
                Thread.Sleep(2000);
            } while (WindowIsOpen(window_title));

            return t.Result;
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string className, string windowName);

        private bool WindowIsOpen(string windowName)
        {
            IntPtr hWnd = FindWindow(null, windowName);
            if (hWnd == IntPtr.Zero)
                return false;
            return true;
        }

        public bool GetEmotionsFromFile(Participant participant)
        {

            //audio file
            string audioRelPathDest;
            if (baseline_flag)
            {
                audioRelPathDest = participant.AudioBaselinePath;
            }
            else
            {
                audioRelPathDest = participant.AudioPath;
            }
            string audioPathDest = Directories.Data + "\\" + dataRelDir + "\\" + audioRelPathDest;
            string audioPathSource = Directories.AudioFile + '\\' + audioRelPathDest;
            //move audio file in Data folder
            bool result;
            MoveFile(audioPathSource, audioPathDest, out result);

            //audio data file
            string fileRelPathDest;
            if (baseline_flag)
            {
                fileRelPathDest = participant.AudioBaselineDataPath;
            }
            else
            {
                fileRelPathDest = participant.AudioDataPath;
            }
            string filePath = Directories.Data + "\\" + dataRelDir + '\\' + fileRelPathDest;
            try
            {
                //save emotions from audio data file
                var lines = File.ReadLines(filePath);
                string[] data;
                foreach (var line in lines)
                {
                    data = line.Split('\t');

                    if (baseline_flag)
                    {
                        participant.AudioBaselineEmotions.Add(new AudioBaselineEmotion
                        {
                            ParticipantID = participant.ID,
                            Name = data[0],
                            Strength = Convert.ToDouble(data[1])
                        });
                    }
                    else
                    {
                        participant.AudioEmotions.Add(new AudioEmotion
                        {
                            ParticipantID = participant.ID,
                            Name = data[0],
                            Strength = Convert.ToDouble(data[1])
                        });
                    }
                }
                return true;
            } catch
            {
                return false;
            }
        }


        /// <summary>
        /// Moves file from sourceFile to destinationFile.
        /// </summary>
        /// <param name="sourceFile">source path of file</param>
        /// <param name="destinationFile">destination path of file</param>
        /// <param name="result">result of action: true = success, false = failure</param>
        /// <returns>string describing the result of function's action.</returns>
        public string MoveFile(string sourceFile, string destinationFile, out bool result)
        {
            try
            {
                //overwrite existing file with same name
                if (File.Exists(destinationFile))
                {
                    File.Delete(destinationFile);
                }
                File.Move(sourceFile, destinationFile);
                result = true;
                return "Moved file from " + sourceFile + " to " + destinationFile;
            }
            catch (Exception e)
            {
                result = false;
                return "Could not move file " + sourceFile + " to " + destinationFile + ".\nProblem: " + e.Message;
            }

        }
    }
}
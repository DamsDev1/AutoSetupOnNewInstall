using Newtonsoft.Json;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;


namespace AutoSetupOnNewInstall
{
    internal class Program
    {
        public static ProgressBarOptions ProgressBarOptions = new ProgressBarOptions
        {
            ProgressBarOnBottom = true,
            EnableTaskBarProgress = true,
            
        };
        public static ProgressBar bar = new ProgressBar(100, "Auto Setup", ProgressBarOptions);
        public static List<DownloadFiles> downloadList = new List<DownloadFiles>();
        public static int filesExecuted = 0;
        public static int filesDownloaded = 0;
        static void Main(string[] args)
        {
            setDownloadLists();
            using (ChildProgressBar allDownloads = bar.Spawn(100, "Téléchargement des fichiers"))
            {
                ChildProgressBar executeFiles = bar.Spawn(100, "Installation des fichiers");
                DownloadAllFiles(allDownloads, executeFiles);
                Console.ReadKey();
            }
        }

        static void DownloadAllFiles(ChildProgressBar allDownloads, ChildProgressBar executeFilesBar)
        {
            int totalFiles = downloadList.Count;

            foreach (DownloadFiles downloadFile in downloadList)
            {
                if(downloadFile.isNotDownloadable())
                {
                    Process.Start(downloadFile.url);

                    // stuff for bar
                    filesDownloaded = filesDownloaded + 1;
                    allDownloads.Tick((int)(Math.Round((double)filesDownloaded / totalFiles, 2) * 100));
                    calculateTotalProgress();
                } else {
                    using (ChildProgressBar currentDownload = allDownloads.Spawn(100, downloadFile.name))
                    {
                        {
                            WebClient webClient = new WebClient();
                            webClient.DownloadProgressChanged += (s, e) =>
                            {
                                currentDownload.Tick(e.ProgressPercentage);
                            };
                            webClient.DownloadFileCompleted += (s, e) =>
                            {
                                filesDownloaded = filesDownloaded + 1;
                                allDownloads.Tick((int)(Math.Round((double)filesDownloaded / totalFiles, 2) * 100));
                                if (downloadFile.command != null)
                                {
                                    calculateTotalProgress();
                                    executeCommand(downloadFile, downloadFile.admin, executeFilesBar);
                                }
                                else
                                {
                                    filesExecuted += 1;
                                    executeFilesBar.Tick((int)(Math.Round((double)filesExecuted / downloadList.Count, 2) * 100));
                                    calculateTotalProgress();
                                }
                            };
                            try
                            {

                                webClient.DownloadFileTaskAsync(new Uri(downloadFile.url), downloadFile.name).Wait();

                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
                        }
                    }
                }
              
            }
        }

        static void executeCommand(DownloadFiles file, Boolean admin, ChildProgressBar executeFilesBar)
        {
            try
            {
                string[] commandSplited = file.command.Split(" ".ToCharArray());
                string filename = commandSplited[0];
                string arguments;
                if (commandSplited.Length > 1)
                {
                    arguments = string.Join(" ", commandSplited.Skip(1).ToArray());
                }
                else
                {
                    arguments = "";
                }
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                process.EnableRaisingEvents = true;
                startInfo.FileName = filename;
                startInfo.Arguments = arguments;

                if (admin)
                {
                    startInfo.Verb = "runas";
                }

                process.StartInfo = startInfo;
                process.Start();
                process.Exited += new EventHandler((s, e) => fileFinishedExecution(s, e, executeFilesBar));
            } catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

        }

        static void fileFinishedExecution(object sender, EventArgs e, ChildProgressBar executeFilesBar)
        {
            filesExecuted = filesExecuted + 1;
            executeFilesBar.Tick((int)(Math.Round((double)filesExecuted / downloadList.Count, 2) * 100));
            calculateTotalProgress();
        }
        static int countFilesToExecute()
        {
            int total = 0;
            downloadList.ForEach(file =>
            {
                if (file.command != null)
                {
                    total += 1;
                }
            });
            return total;
        }
        static void calculateTotalProgress()
        {
            double progress = (double) (filesDownloaded + filesExecuted) / (downloadList.Count + countFilesToExecute());
            bar.Tick((int)(Math.Round(progress, 2) * 100));
            Debug.WriteLine(progress);
            if(progress == 1)
            {
                bar.Dispose();
                Console.WriteLine("\n\n\n\n\n\nFini !");
                Console.WriteLine("\n\nAppuyez sur une touche pour fermer le programme...");
            }
        }
        static void setDownloadLists()
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                    wc.Headers.Add("Cache-Control", "no-cache");
                    string json = wc.DownloadString("file:///C:/Users/dams/source/repos/AutoSetupOnNewInstall/files.json");
                    downloadList = JsonConvert.DeserializeObject<List<DownloadFiles>>(json);
                } catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }
    }
}

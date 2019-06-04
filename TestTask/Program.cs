using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace TestTask
{
    class Program
    {
        private static string _htmlExtension = ".html";
        private static string _txtExtension = ".txt";
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the adress link:");
            String address = Console.ReadLine();
            DownloadHTML(address, _htmlExtension);
            Pause();

        }
        private static string[] WorkingDirectory() {
            //Specail array of keeping two main pathes the first for 'root' of task directory, the second for images dir 
            string[] pathArr = new string[2];
            // Specify the directory you want to manipulate
            //The Directory "AidarAhmetshinTestTask"('root' dir of the task) will occur in your desktop 
            string path = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + @"\AidarAhmetshinTestTask";
            string pathForImg = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + @"\AidarAhmetshinTestTask"+@"\images";
            try
            {
                // Determine whether the directory exists
                if (Directory.Exists(path) && Directory.Exists(pathForImg))
                {
                    Console.WriteLine("That folder/path exists already.");
                }
                else
                {
                    // Try to create the directory
                    Directory.CreateDirectory(path);
                    Directory.CreateDirectory(pathForImg);
                    Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(path));
                    Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(pathForImg));
                }    
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            pathArr[0] = path;
            pathArr[1] = pathForImg;

            return pathArr;
        }

        private static void DownloadHTML(string address, string extension) {
            WebClient webClient = new WebClient
            {
                Encoding = System.Text.Encoding.UTF8
            };
            //Getting directory where all download images and another stuff will save 
            string path = WorkingDirectory()[0];
            Console.WriteLine(path);
            //Getting html page like string for futher parsing
            string htmlText = webClient.DownloadString(address);
            //Path for downloaded page -> new name for it TestPage.html
            string absolutePath = path + @"\TestPage" + extension;
            //Saving .html file
            File.WriteAllText(absolutePath, htmlText);
            Console.WriteLine(absolutePath);
            
            StreamWriter streamWriter = new StreamWriter(path+@"\ImgAddresses.txt", false, System.Text.Encoding.Default);
            DownloadSources(Parse(htmlText), webClient, WorkingDirectory()[1], streamWriter);
        }
        private static void Pause() {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static List<string> Parse(string htmlText) {
            //the list of all image's resourses
            List<string> images = new List<string>();
            //I use htmlAgilityPack library for easy parsing htmlDoc
            var htmlDoc = new HtmlDocument()
            {
                OptionFixNestedTags = true,
                OptionAutoCloseOnEnd = true
            };

            htmlDoc.LoadHtml(htmlText);
            //searching all tag img and their attributs src
            foreach (HtmlNode img in htmlDoc.DocumentNode.SelectNodes("//img"))
            {
                HtmlAttribute att = img.Attributes["src"];
                if (att != null)
                {
                    images.Add(att.Value);
                }
            }
            
            Console.WriteLine("Total amount of images on the page: " + images.Count);

            return images;
        }

        private static void DownloadSources(List<string> images, WebClient webClient, string path,  StreamWriter streamWriter) {
            int k = 0;
            int imgCount = images.Count;
            
            foreach (string str in images)
            {
                try
                {
                    webClient.DownloadFile(str, path + @"\testImg" + k.ToString() + ".png");
                    Console.WriteLine(k.ToString()+") "+str);
                    try
                    {
                        streamWriter.WriteLine(str);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    k++;
                }
                catch(Exception e) {
                    //if source is not available or another problem occur, we will see the 'bad' source and Exception message
                    Console.WriteLine("There is no such source image: "+ str);
                    Console.WriteLine(e.Message);
                }

                //This not pretty block for simple checking that we can download only not more 5 or such amout images that page has  
                if (k>4)
                {
                    break;
                }
                else if(k>imgCount)
                {
                    break;
                }
            }
            streamWriter.Close();
        }
    }
}

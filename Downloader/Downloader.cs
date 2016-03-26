using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;
using System.Drawing;
namespace Downloader
{
    public class Downloader
    {
        private string CACHE_DIRECTORY_PATH;

        /// <summary>Constructor</summary>
        /// <param name="cacheFilePath">The system file path to the directory to store cached images</param>
        public Downloader(string cacheDirectoryPath)
        {
            this.CACHE_DIRECTORY_PATH = cacheDirectoryPath;
            if (!System.IO.Directory.Exists(CACHE_DIRECTORY_PATH))
            {
                System.IO.Directory.CreateDirectory(CACHE_DIRECTORY_PATH);
            }
        }

        /// <summary>
        /// Gets the full 360* panorama from cache, or downloads all the tiles and compiles into the full image.
        /// </summary>
        /// <param name="panoId"></param>
        /// <param name="zoomLevel"></param>
        /// <returns></returns>
        public System.Drawing.Image GetFullImage(string panoId, int zoomLevel)
        {
            int horizontalSlices = GetHorizontalSlicesPerLevel(zoomLevel);
            int verticalSlices = GetVerticalSlicesPerLevel(zoomLevel);

            string panoCacheDirectory = CACHE_DIRECTORY_PATH + panoId + @"\";

            if (!System.IO.Directory.Exists(panoCacheDirectory))
            {
                System.IO.Directory.CreateDirectory(panoCacheDirectory);
            }

            string fullImageName = zoomLevel + "Complete.jpg";
            if (!File.Exists(panoCacheDirectory + fullImageName))
            {
                DownloadTiles(panoId, zoomLevel);
                CompileTilesToImage(panoId, zoomLevel, panoCacheDirectory + fullImageName);
            }

            // The full image is stored in cache
            return Image.FromFile(panoCacheDirectory + fullImageName);
        }

        public System.Drawing.Image GetThumbnail(string panoId)
        {
            string panoCacheDirectory = CACHE_DIRECTORY_PATH + panoId + @"\";
            if (!System.IO.Directory.Exists(panoCacheDirectory))
            {
                System.IO.Directory.CreateDirectory(panoCacheDirectory);
            }

            string fullImageName = "Thumbnail.jpg";
            if (!File.Exists(panoCacheDirectory + fullImageName))
            {
                string URL = "http://cbk0.google.com/cbk?output=thumbnail&panoid=" + panoId;
                Download(URL, panoCacheDirectory + fullImageName);
            }

            return Image.FromFile(panoCacheDirectory + fullImageName);
        }

        // Compile all tiles into a single image, save the image and clear up cached tiles
        void CompileTilesToImage(string panoId, int zoomLevel, string savePath)
        {
            int horizontalSlices = GetHorizontalSlicesPerLevel(zoomLevel);
            int verticalSlices = GetVerticalSlicesPerLevel(zoomLevel);

            int imageWidth = 512 * (verticalSlices + 1);
            int imageHeight = 512 * (horizontalSlices + 1);
            if (zoomLevel == 3)
            {
                imageWidth = 3328;
                imageHeight = 1664;
            }
            if (zoomLevel == 4)
            {
                imageWidth = 6656;
                imageHeight = 3329;
            }

            using (System.Drawing.Image grandImage = new Bitmap(imageWidth, imageHeight))
            {
                int drawX = 0;
                int drawY = 0;
                for (int y = 0; y <= horizontalSlices; y++)
                {
                    for (int x = 0; x <= verticalSlices; x++)
                    {
                        string cacheName = zoomLevel + zero(y) + zero(x) + ".jpg";
                        System.Drawing.Image tile = new Bitmap(CACHE_DIRECTORY_PATH + panoId + @"\" + cacheName);

                        using (Graphics g = Graphics.FromImage(grandImage))
                        {
                            g.DrawImage(tile, drawX, drawY, 512, 512);
                        }

                        // Calculate where to place the next image in the grid
                        drawX += 512;
                        if (drawX >= grandImage.Width)
                        {
                            drawX = 0;
                            drawY += 512;
                        }
                    }
                }

                //Save image to disk
                grandImage.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }

            // Clean up the tiles
            /*for (int y = 0; y <= horizontalSlices; y++)
            {
                for (int x = 0; x <= verticalSlices; x++)
                {
                    string cacheName = zoomLevel + zero(y) + zero(x) + ".jpg";
                    if (File.Exists(CACHE_DIRECTORY_PATH + cacheName))
                    {
                        File.Delete(CACHE_DIRECTORY_PATH + cacheName);
                    }
                }
            }*/

        }

        void DownloadTiles(string panoId, int zoomLevel)
        {
            int horizontalSlices = GetHorizontalSlicesPerLevel(zoomLevel);
            int verticalSlices = GetVerticalSlicesPerLevel(zoomLevel);

            //Download the tiles based on the current zoom level
            string basepath = "http://cbk0.google.com/cbk?output=tile&zoom=" + zoomLevel;
            List<Thread> threads = new List<Thread>();
            for (int y = 0; y <= horizontalSlices; y++)
            {
                for (int x = 0; x <= verticalSlices; x++)
                {
                    string Url = basepath + "&panoid=" + panoId + "&x=" + x + "&y=" + y;
                    string cacheName = zoomLevel + zero(y) + zero(x) + ".jpg";
                    string filePathAndCacheName = CACHE_DIRECTORY_PATH + panoId + @"\" + cacheName;

                    if (!File.Exists(filePathAndCacheName) || new FileInfo(filePathAndCacheName).Length == 0)
                    {
                        Thread downloaderThread = new Thread(() => Download(Url, filePathAndCacheName));
                        downloaderThread.Start();
                        Console.Write("^");
                        threads.Add(downloaderThread);
                    }
                    else
                    {
                        Console.Write("*");
                    }
                }
            }

            //Wait for all threads to complete
            foreach (Thread thread in threads)
            { thread.Join(); }
        }

        public void Download(string url, string filepath)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, filepath);
            }
        }

        private int GetHorizontalSlicesPerLevel(int zoomLevel)
        {
            switch (zoomLevel)
            {
                case 3:
                    return 3; //4 rows; 0,1,2,3
                case 4:
                    return 6;
                default:
                    return 0;
            }
        }

        private int GetVerticalSlicesPerLevel(int zoomLevel)
        {
            switch (zoomLevel)
            {
                case 3:
                    return 6;
                case 4:
                    return 12;
                default:
                    return 0;
            }
        }

        public static string zero(int number)
        {
            if (number < 10)
            {
                return "0" + number;
            }

            return number.ToString();
        }
    }

}

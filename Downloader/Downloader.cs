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
        private bool keepTiles = true;

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
		public System.Drawing.Image GetFullImage(string panoId, int zoomLevel, IProgress<Tuple<int, int, string>> progress)
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
                DownloadTiles(panoId, zoomLevel, progress);
                CompileTilesToImage(panoId, zoomLevel, panoCacheDirectory + fullImageName, progress);
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
		void CompileTilesToImage(string panoId, int zoomLevel, string savePath, IProgress<Tuple<int, int, string>> progress)
        {
            int horizontalSlices = GetHorizontalSlicesPerLevel(zoomLevel);
            int verticalSlices = GetVerticalSlicesPerLevel(zoomLevel);

            int imageWidth = 512 * (verticalSlices + 1);
            int imageHeight = 512 * (horizontalSlices + 1);

            if (zoomLevel == 2)
            {
                imageWidth = 1664;
                imageHeight = 832;
            } else if (zoomLevel == 3) {
                imageWidth = 3328;
                imageHeight = 1664;
            } else if (zoomLevel == 4) {
                imageWidth = 6656;
                imageHeight = 3328;
            } else if (zoomLevel == 5) {
                imageWidth = 3328;
                imageHeight = 6656;
            }

            using (System.Drawing.Image grandImage = new Bitmap(imageWidth, imageHeight))
            {
                int drawX = 0;
                int drawY = 0;
                for (int y = 0; y <= horizontalSlices; y++)
                {
                    for (int x = 0; x <= verticalSlices; x++)
                    {
                        string cacheName = zoomLevel + Zero(y) + Zero(x) + ".jpg";
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
					if (progress != null) {
						progress.Report(new Tuple<int, int, string>(y, horizontalSlices, "Aligning tiles..."));
					}
                }

                //Save image to disk
                grandImage.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }

            // Clean up the tiles
            if (!keepTiles) { 
                for (int y = 0; y <= horizontalSlices; y++)
                {
                    for (int x = 0; x <= verticalSlices; x++)
                    {
                        string cacheName = zoomLevel + Zero(y) + Zero(x) + ".jpg";
                        if (File.Exists(CACHE_DIRECTORY_PATH + cacheName))
                        {
                            File.Delete(CACHE_DIRECTORY_PATH + cacheName);
                        }
                    }
                }
            }

        }

		void DownloadTiles(string panoId, int zoomLevel, IProgress<Tuple<int, int, string>> progress)
        {
			if (progress != null) {
				progress.Report(new Tuple<int, int, string>(0, 100, "Downloading..."));
			}

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
                    string cacheName = zoomLevel + Zero(y) + Zero(x) + ".jpg";
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
			int threadCounter = 0;
            foreach (Thread thread in threads){
				if (progress != null) {
					progress.Report(new Tuple<int, int, string>(threadCounter++, threads.Count, "Downloading..."));
				}
				thread.Join();
			}
        }

        public void Download(string url, string filepath)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, filepath);
            }
        }

        /// <summary>
        /// Change the viewing size and viewing direction (Yaw) of a 360 image. For example change the heading to be 0 degrees (North) and restrict the view port to 180 degrees
        /// </summary>
        /// <param name="originalImage"></param>
        /// <param name="originalHeading">Centre of the image. In degrees</param>
        /// <param name="desiredHeading">Desired new centre of the image. In degrees</param>
        /// <param name="desiredSize">In degrees</param>
        /// <returns></returns>
        public Image ManipulateImage(Image originalImage, decimal originalHeading, decimal desiredHeading, decimal desiredSize)
        {
            decimal headingDelta = (Mod((desiredHeading - originalHeading + 180), 360)) - 180; //The angle difference between panoH and desired H
            Image maniuplatedImage = new Bitmap(originalImage);
            int imageWidth = maniuplatedImage.Width;
            int imageHeight = maniuplatedImage.Height;
            decimal pixelDegreeSize = imageWidth / 360;

            if (originalHeading != desiredHeading && (headingDelta > 2 || headingDelta < -2))
            {
                //Make the desired heading the new centre of the image
                decimal headingPixelPosition = (pixelDegreeSize * headingDelta) + (imageWidth / 2);
                using (Image intermediate = new Bitmap(imageWidth, imageHeight))
                {
                    using (Graphics g = Graphics.FromImage(intermediate))
                    {
                        if ((Mod((originalHeading - desiredHeading), 360)) > 180)
                        {
                            //Desired heading is on the right hand side of centre
                            int newLeftHandPixelPosition = decimal.ToInt32(headingPixelPosition - (180 * pixelDegreeSize)); //This position will become far left.
                            Rectangle newLeftHandSide = new Rectangle(newLeftHandPixelPosition, 0, imageWidth - newLeftHandPixelPosition, imageHeight);
                            Rectangle newRightHandSide = new Rectangle(0, 0, newLeftHandPixelPosition, imageHeight);

                            g.DrawImage(originalImage, 0, 0, newLeftHandSide, GraphicsUnit.Pixel);
                            g.DrawImage(originalImage, imageWidth - newLeftHandPixelPosition, 0, newRightHandSide, GraphicsUnit.Pixel);
                        }
                        else
                        {
                            //Desired heading is on the left hand side of the centre
                            int newRightHandPixelPosition = decimal.ToInt32(headingPixelPosition + (180 * pixelDegreeSize));
                            Rectangle newRightHandSide = new Rectangle(0, 0, newRightHandPixelPosition, imageHeight);
                            Rectangle newLeftHandSide = new Rectangle(newRightHandPixelPosition, 0, imageWidth - newRightHandPixelPosition, imageHeight);

                            g.DrawImage(originalImage, 0, 0, newLeftHandSide, GraphicsUnit.Pixel);
                            g.DrawImage(originalImage, imageWidth - newRightHandPixelPosition, 0, newRightHandSide, GraphicsUnit.Pixel);
                        }
                    }

                    //Update the image
                    using (Graphics g = Graphics.FromImage(maniuplatedImage))
                    {
                        g.DrawImage(intermediate, 0, 0);
                    }
                }
            }

            if (desiredSize < 360)
            {
                int leftHandCropX = decimal.ToInt32((180 - (desiredSize / 2)) * pixelDegreeSize);
                int imageCropWidth = decimal.ToInt32(pixelDegreeSize * desiredSize);

                //Go with a 16:9 ratio aka 1.77:1
                int imageCropHeight = decimal.ToInt32(imageCropWidth / 1.7777777m);
                if (imageCropHeight > imageHeight)
                {
                    imageCropHeight = imageHeight;
                }
                int leftHandCropY = (imageHeight / 2) - (imageCropHeight / 2);

                using (Image resizedImage = new Bitmap(imageCropWidth, imageCropHeight))
                {
                    using (Graphics g = Graphics.FromImage(resizedImage))
                    {
                        Rectangle newImageBounds = new Rectangle(leftHandCropX, leftHandCropY, imageCropWidth, imageCropHeight);
                        g.DrawImage(maniuplatedImage, 0, 0, newImageBounds, GraphicsUnit.Pixel);
                    }

                    maniuplatedImage = new Bitmap(resizedImage);
                }

            }

            return maniuplatedImage;
        }

        private int GetHorizontalSlicesPerLevel(int zoomLevel)
        {
            switch (zoomLevel)
            {
                case 2:
                    return 1;
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
                case 2:
                    return 3;
                case 3:
                    return 6;
                case 4:
                    return 12;
                default:
                    return 0;
            }
        }

        public static string Zero(int number)
        {
            if (number < 10)
            {
                return "0" + number;
            }

            return number.ToString();
        }

        public static int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        public static decimal Mod(decimal x, decimal m)
        {
            decimal r = x % m;
            return r < 0 ? r + m : r;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace StreetviewDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dictionary<int, string> keyboardPanoLinkList = new Dictionary<int, string>();
        public int ZoomLevel = 2;
        private System.Drawing.Image displayedImage;

        private bool continueTimelapse = false;

        // All images will be stored here
        string CachePathBase = Directory.GetCurrentDirectory() + @"\cache\";

        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
        }


        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (panoIdTextBox.IsFocused)
            {
                return;
            }

            switch (e.Key){
                case Key.NumPad1:
                case Key.D1:
                    thumbnailKeyPressed(1);
                    break;
                case Key.NumPad2:
                case Key.D2:
                    thumbnailKeyPressed(2);
                    break;
                case Key.NumPad3:
                case Key.D3:
                    thumbnailKeyPressed(3);
                    break;
                case Key.NumPad4:
                case Key.D4:
                    thumbnailKeyPressed(4);
                    break;
                case Key.NumPad5:
                case Key.D5:
                    thumbnailKeyPressed(5);
                    break;
                case Key.NumPad6:
                case Key.D6:
                    thumbnailKeyPressed(6);
                    break;
                case Key.OemPlus:
                case Key.Add:
                    ZoomIn_Click(sender, e);
                    break;
                case Key.OemMinus:
                case Key.Subtract:
                    ZoomOut_Click(sender, e);
                    break;
            }
        }

        private void thumbnailKeyPressed(int key)
        {
            if (keyboardPanoLinkList.ContainsKey(key))
            {
                string newPanoId = string.Empty;
                keyboardPanoLinkList.TryGetValue(key, out newPanoId);
                panoIdTextBox.Text = newPanoId;
				RetrieveAndDisplayPanorama(newPanoId);
            }
        }

		private void ReportProgress(Tuple<int, int, string> value)
		{
			ProgressBar.Value = value.Item1;
			ProgressBar.Maximum = value.Item2;
			ProgressLabel.Content = value.Item3;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
        {
            locationLabel.Content = "Loading...";
            string panoId = panoIdTextBox.Text;

			RetrieveAndDisplayPanorama(panoId);
        }

		private async void RetrieveAndDisplayPanorama(string panoId)
		{
            try
            {
                // Show progress bar
                ProgressBar.Visibility = Visibility.Visible;
                ProgressLabel.Visibility = Visibility.Visible;

                // Holds technical meta information about the pano
                panorama panoObject = await Task.Run(() =>
                {
                    return DownloadPanoramaInfo(panoId);
                });

                if (panoObject.data_properties == null)
                {
                    locationLabel.Content = "Error: Panorama has invalid XML data...";
                    return;
                }

                locationLabel.Content = GetPanoramaLocationText(panoObject);

                if (panoObject.data_properties.image_width < 3330 && ZoomLevel > 3)
                {
                    ZoomLevel = 3; //We're dealing with a low res panorama here
                    LabelZoomLevel.Content = ZoomLevel;
                }

                // Progress value, max value, status text
                IProgress<Tuple<int, int, string>> progressIndicator = new Progress<Tuple<int, int, string>>(ReportProgress);

                var image = await Task.Run(() =>
                {
                    return DownloadBigPano(panoId, progressIndicator);
                });

                // Show the image width and height on screen
                UpdateDimenions(image.Width, image.Height);

                // Download progress complete
                progressIndicator.Report(new Tuple<int, int, string>(0, 100, "Displaying image..."));

                if (sliderFieldOfView.Value == 360)
                {
                    // Load the image from the cache and display it. This has faster performance than loading it via MemorySteam.
                    mainImage.Source = new BitmapImage(new Uri(CachePathBase + panoId + @"\" + ZoomLevel + "Complete.jpg", UriKind.RelativeOrAbsolute));
                    displayedImage = image;
                }
                else
                {
                    Downloader.Downloader imageDownloader = new Downloader.Downloader(CachePathBase);
                    var manipulatedImage = imageDownloader.ManipulateImage(image, 0, 0, (int)sliderFieldOfView.Value);
                    mainImage.Source = ImageConverter(manipulatedImage);
                    // Show the image width and height on screen
                    UpdateDimenions(manipulatedImage.Width, manipulatedImage.Height);
                    displayedImage = manipulatedImage;
                }

                progressIndicator.Report(new Tuple<int, int, string>(100, 100, "Displaying image..."));

                // The Save As... dialog in the file menu
                FileMenuSaveAs.IsEnabled = true;
                FileMenuTimelapse.IsEnabled = true;

                // Show the thumbnail images looking towards linked panoramas
                DisplayThumbnails(panoObject, image);

                // Hide Progress Bar
                ProgressBar.Visibility = Visibility.Hidden;
                ProgressLabel.Visibility = Visibility.Hidden;
            } catch (Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.InnerException, "An error occured downloading the image");
            }
		}

		private System.Drawing.Image DownloadBigPano(string panoId, IProgress<Tuple<int, int, string>> progress)
        {
			// Start Progress Report
			progress.Report(new Tuple<int, int, string>(0, 100, "Loading..."));

            //Set up downloader...
            Downloader.Downloader imageDownloader = new Downloader.Downloader(CachePathBase);

			// Download big pano
			System.Drawing.Image image = imageDownloader.GetFullImage(panoId.ToString(), ZoomLevel, progress);

			return image;
        }

		private panorama DownloadPanoramaInfo(string panoId)
		{
			//Set up downloader...
			Downloader.Downloader imageDownloader = new Downloader.Downloader(CachePathBase);

			//Download XML or Load from cache
			if (!System.IO.File.Exists(CachePathBase + panoId + @"\" + panoId + ".xml") || new FileInfo(CachePathBase + panoId + @"\" + panoId + ".xml").Length == 0)
			{
				if (!System.IO.Directory.Exists(CachePathBase + panoId))
				{
					System.IO.Directory.CreateDirectory(CachePathBase + panoId);
				}
				imageDownloader.Download("http://cbk0.google.com/cbk?output=xml&panoid=" + panoId, CachePathBase + panoId + @"\" + panoId + ".xml");
			}

			XmlSerializer serializer = new XmlSerializer(typeof(panorama));
			StreamReader reader = new StreamReader(CachePathBase + panoId + @"\" + panoId + ".xml");
			panorama panoObject = (panorama)serializer.Deserialize(reader);
			reader.Close();

			return panoObject;
		}

        private string GetPanoramaLocationText(panorama panoObject)
        {
            string location = string.Empty;
            
            string country = panoObject.data_properties.country ?? string.Empty;
            string region = panoObject.data_properties.region ?? string.Empty;
            string text = panoObject.data_properties.text ?? string.Empty;
            string copyright = panoObject.data_properties.copyright ?? string.Empty;

            location = text + ", " + region + ", " + country + " " + copyright;

            return location;
        }

        private void DisplayThumbnails(panorama panoObject, System.Drawing.Image bigImage)
        {
            // Clear previous items
            thumbnails.Children.Clear();
            keyboardPanoLinkList.Clear();

            //Set up downloader...
            string cachePathBase = @"C:\StreetSwoop\cache\";
            Downloader.Downloader imageDownloader = new Downloader.Downloader(cachePathBase);

            int thumbnailKey = 1;
            decimal panoYaw = panoObject.projection_properties.pano_yaw_deg;

            System.Drawing.Image smallImage = new System.Drawing.Bitmap(720, 360);
            using (Graphics g = Graphics.FromImage(smallImage))
            {
                g.DrawImage(bigImage, 0, 0, smallImage.Width, smallImage.Height);
            }

            // Get all the thumbnails
            foreach (var annotation in panoObject.annotation_properties.OrderBy(item => item.yaw_deg))
            {
                var button = new System.Windows.Controls.Button();
                var thumbImg = new System.Windows.Controls.Image();

                //headingDelta >= 2 || headingDelta <= -2
                System.Drawing.Image thumbnail = imageDownloader.ManipulateImage(smallImage, panoYaw, annotation.yaw_deg, 90);
                
                thumbImg.Source = ImageConverter(thumbnail);
                thumbImg.Width = 125;

                button.Content = thumbImg;
                button.Click += (object sender, RoutedEventArgs e) =>
                {
					foreach (Button thumbButton in thumbnails.Children) {
						thumbButton.IsEnabled = false;
					}

                    panoIdTextBox.Text = annotation.pano_id;
                    RetrieveAndDisplayPanorama(annotation.pano_id);
                };
                thumbnails.Children.Add(button);

                keyboardPanoLinkList.Add(thumbnailKey++, annotation.pano_id);
            }


        }



        private ImageSource ImageConverter(System.Drawing.Image inputImage) {
            MemoryStream ms = new MemoryStream();
            inputImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            return bi;
        }

        public static string zero(int number)
        {
            if (number < 10)
            {
                return "0" + number;
            }

            return number.ToString();
        }

        public static string fiveZero(int number)
        {
            if (number < 10)
            {
                return "0000" + number;
            }
            if (number < 100)
            {
                return "000" + number;
            }
            if (number < 1000)
            {
                return "00" + number;
            }
            if (number < 10000)
            {
                return "0" + number;
            }

            return number.ToString();
        }

        public static string RemoveInvalidFilePathCharacters(string filename, string replaceChar)
        {
            string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(filename, replaceChar);
        }

        public static int mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        public static decimal mod(decimal x, decimal m)
        {
            decimal r = x % m;
            return r < 0 ? r + m : r;
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut.IsEnabled = true;

            if (ZoomLevel < 4)
            {
                ZoomLevel++;
            }

            if (ZoomLevel >= 4)
            {
                ZoomIn.IsEnabled = false;
            }

            LabelZoomLevel.Content = ZoomLevel;
            Button_Click(sender, e);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn.IsEnabled = true;

            if (ZoomLevel > 2)
            {
                ZoomLevel--;
            }

            if (ZoomLevel <= 2)
            {
                ZoomOut.IsEnabled = false;
            }

            LabelZoomLevel.Content = ZoomLevel;
            Button_Click(sender, e);
        }

		private void OpenFromURL_Click(object sender, RoutedEventArgs e)
		{
            // Instantiate window
            OpenDialog dialogBox = new OpenDialog();

            // Show window modally
            // NOTE: Returns only when window is closed
            Nullable<bool> dialogResult = dialogBox.ShowDialog();

            if(dialogResult == true)
            {
                panoIdTextBox.Text = dialogBox.PanoId;
                RetrieveAndDisplayPanorama(panoIdTextBox.Text);

            }
        }

		private void SaveAs_Click(object sender, RoutedEventArgs e)
		{
			//Save as dialog box
			Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
			saveDialog.FileName = ""; // Default file name
			saveDialog.DefaultExt = ".jpg"; // Default file extension
			saveDialog.Filter = "JPG (.jpg)|*.jpg"; // Filter files by extension
			saveDialog.OverwritePrompt = true;

			// Show save file dialog box
			Nullable<bool> result = saveDialog.ShowDialog();

			// Process save file dialog box results
			if (result == true) {
                displayedImage.Save(saveDialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
			}
		}

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

        private void UpdateDimenions(int width, int height)
        {
            string dimensions = width + " x " + height;
            panoDimensions.Text = dimensions;
        }

        private void OpenCacheFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(CachePathBase);
        }

        private void CreateTimelapse_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("The timelapser is a work in progress. Create a new folder in the next popup and give the timelapse files a name. They will have an incrementing number added into the file name you choose eg. Timelapse.jpg will save as Timelapse001.jpg, Timelapse002.jpg... etc", "Timelapse Instructions");


            //Save as dialog box
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.FileName = "StreetviewTimelapse"; // Default file name
            saveDialog.DefaultExt = ".jpg"; // Default file extension
            saveDialog.Filter = "JPG (.jpg)|*.jpg"; // Filter files by extension
            saveDialog.OverwritePrompt = true;
            saveDialog.Title = "Time lapse files will be saved here with the provided name";

            // Show save file dialog box
            Nullable<bool> result = saveDialog.ShowDialog();

            if (result == true)
            {
                string saveFilePath = saveDialog.FileName;
                saveFilePath = saveFilePath.Substring(0, saveFilePath.Length - 3); // remove file extension
                TimeLapse(saveFilePath, panoIdTextBox.Text);
            }
        }

        private async void TimeLapse(string fileSavePath, string startPanoId)
        {
            int timelapseFileCounter = 1;
            continueTimelapse = true;
            string nextPanoId = startPanoId;

            try
            {
                while (continueTimelapse)
                {
                    // Get the pano info to find linked panoramas
                    panorama panoObject = await Task.Run(() =>
                    {
                        return DownloadPanoramaInfo(nextPanoId);
                    });

                    if (panoObject.annotation_properties.Length > 0)
                    {
                        // Just pick the first linked pano, TODO : make this user selectable
                        nextPanoId = panoObject.annotation_properties.OrderBy(item => item.yaw_deg).First().pano_id;
                    } else
                    {
                        //No linked panoramas (it can happen)
                        continueTimelapse = false;
                        break;
                    }

                    RetrieveAndDisplayPanorama(nextPanoId);

                    Thread.Sleep(200);

                    // Save the current image
                    displayedImage.Save(fileSavePath + fiveZero(timelapseFileCounter++) + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.InnerException, "An error occured creating timelapse images");
            }
        }


    }
}

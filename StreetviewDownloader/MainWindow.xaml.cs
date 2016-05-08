using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StreetviewDownloader {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public Dictionary<int, string> keyboardPanoLinkList = new Dictionary<int, string>();

		private System.Drawing.Image displayedImage;

		private bool continueTimelapse = false;

		StreetviewDownloaderModel _model;

		public MainWindow() {
			InitializeComponent();
			this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
			_model = new StreetviewDownloaderModel();
			this.DataContext = _model;
		}


		void MainWindow_KeyDown(object sender, KeyEventArgs e) {
			if (panoIdTextBox.IsFocused) {
				return;
			}

			switch (e.Key) {
				case Key.NumPad1:
				case Key.D1:
					ThumbnailKeyPressed(1);
					break;
				case Key.NumPad2:
				case Key.D2:
					ThumbnailKeyPressed(2);
					break;
				case Key.NumPad3:
				case Key.D3:
					ThumbnailKeyPressed(3);
					break;
				case Key.NumPad4:
				case Key.D4:
					ThumbnailKeyPressed(4);
					break;
				case Key.NumPad5:
				case Key.D5:
					ThumbnailKeyPressed(5);
					break;
				case Key.NumPad6:
				case Key.D6:
					ThumbnailKeyPressed(6);
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

		private void ThumbnailKeyPressed(int key) {
			if (keyboardPanoLinkList.ContainsKey(key)) {
				string newPanoId = string.Empty;
				keyboardPanoLinkList.TryGetValue(key, out newPanoId);
				_model.PanoID = newPanoId;
				RetrieveAndDisplayPanorama(newPanoId);
			}
		}

		private void ReportProgress(Tuple<int, int, string> value) {
			_model.ProgressBarValue = value.Item1;
			_model.ProgressBarMaximum = value.Item2;
			_model.ProgressBarLabel = value.Item3;
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e) {
			_model.Location = "Loading...";

			RetrieveAndDisplayPanorama(_model.PanoID);
		}

		private async void RetrieveAndDisplayPanorama(string panoId) {
			await RetrieveAndDisplayPanoramaAsync(panoId);
		}

		private async Task RetrieveAndDisplayPanoramaAsync(string panoId) {
			try {
				// Show progress bar
				_model.ProgressBarVisibility = Visibility.Visible;

				// Holds technical meta information about the pano
				panorama panoObject = await Task.Run(() =>
				{
					return _model.DownloadPanoramaInfo(panoId);
				});

				if (panoObject.data_properties == null) {
					_model.Location = "Error: Panorama has invalid XML data...";
					return;
				}

				_model.Location = GetPanoramaLocationText(panoObject);

				if (panoObject.data_properties.image_width < 3330 && _model.ZoomLevel > 3) {
					_model.ZoomLevel = 3; //We're dealing with a low res panorama here
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

				if (_model.FieldOfView == 360) {
					// Load the image from the cache and display it. This has faster performance than loading it via MemorySteam.
					_model.MainImageSource = new BitmapImage(new Uri(_model.CachePathBase + panoId + @"\" + _model.ZoomLevel + "Complete.jpg", UriKind.RelativeOrAbsolute));
					displayedImage = image;
				} else {
					Downloader.Downloader imageDownloader = new Downloader.Downloader(_model.CachePathBase);
					var manipulatedImage = imageDownloader.ManipulateImage(image, panoObject.projection_properties.pano_yaw_deg, _model.Heading, _model.FieldOfView);
					_model.MainImageSource = ImageConverter(manipulatedImage);
					// Show the image width and height on screen
					UpdateDimenions(manipulatedImage.Width, manipulatedImage.Height);
					displayedImage = manipulatedImage;
				}

				progressIndicator.Report(new Tuple<int, int, string>(100, 100, "Displaying image..."));

				// The Save As... dialog in the file menu
				_model.FileMenuSaveAsEnabled = true;
				_model.FileMenuTimelapseEnabled = true;

				// Show the thumbnail images looking towards linked panoramas
				if (!continueTimelapse) {
					DisplayThumbnails(panoObject, image);
				}

				// Hide Progress Bar
				_model.ProgressBarVisibility = Visibility.Hidden;
			} catch (Exception e) {
				MessageBox.Show(e.Message + Environment.NewLine + e.InnerException, "An error occured downloading the image");
			}
		}

		private System.Drawing.Image DownloadBigPano(string panoId, IProgress<Tuple<int, int, string>> progress) {
			// Start Progress Report
			progress.Report(new Tuple<int, int, string>(0, 100, "Loading..."));

			//Set up downloader...
			Downloader.Downloader imageDownloader = new Downloader.Downloader(_model.CachePathBase);

			// Download big pano
			System.Drawing.Image image = imageDownloader.GetFullImage(panoId.ToString(), _model.ZoomLevel, progress);

			return image;
		}



		private string GetPanoramaLocationText(panorama panoObject) {
			string location = string.Empty;

			string country = panoObject.data_properties.country ?? string.Empty;
			string region = panoObject.data_properties.region ?? string.Empty;
			string text = panoObject.data_properties.text ?? string.Empty;
			string copyright = panoObject.data_properties.copyright ?? string.Empty;

			location = text + ", " + region + ", " + country + " " + copyright;

			return location;
		}

		private void DisplayThumbnails(panorama panoObject, System.Drawing.Image bigImage) {
			// Clear previous items
			thumbnails.Children.Clear();
			keyboardPanoLinkList.Clear();

			//Set up downloader...
			Downloader.Downloader imageDownloader = new Downloader.Downloader(_model.CachePathBase);

			int thumbnailKey = 1;
			decimal panoYaw = panoObject.projection_properties.pano_yaw_deg;

			System.Drawing.Image smallImage = new System.Drawing.Bitmap(720, 360);
			using (Graphics g = Graphics.FromImage(smallImage)) {
				g.DrawImage(bigImage, 0, 0, smallImage.Width, smallImage.Height);
			}

			// Get all the thumbnails
			foreach (var annotation in panoObject.annotation_properties.OrderBy(item => item.yaw_deg)) {
				var button = new System.Windows.Controls.Button();
				var thumbImg = new System.Windows.Controls.Image();

				//headingDelta >= 2 || headingDelta <= -2
				System.Drawing.Image thumbnail = imageDownloader.ManipulateImage(smallImage, panoYaw, annotation.yaw_deg, 90);

				thumbImg.Source = ImageConverter(thumbnail);
				thumbImg.Width = 125;

				button.Content = thumbImg;
				button.ToolTip = "Click to move to next image. Heading = " + annotation.yaw_deg + " PanoID = " + annotation.pano_id;
				button.Click += (object sender, RoutedEventArgs e) =>
				{
					foreach (Button thumbButton in thumbnails.Children) {
						thumbButton.IsEnabled = false;
					}

					_model.PanoID = annotation.pano_id;
					_model.Heading = annotation.yaw_deg;
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

		public static string Zero(int number) {
			if (number < 10) {
				return "0" + number;
			}

			return number.ToString();
		}

		public static string FiveZero(int number) {
			if (number < 10) {
				return "0000" + number;
			}
			if (number < 100) {
				return "000" + number;
			}
			if (number < 1000) {
				return "00" + number;
			}
			if (number < 10000) {
				return "0" + number;
			}

			return number.ToString();
		}

		public static string RemoveInvalidFilePathCharacters(string filename, string replaceChar) {
			string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
			Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
			return r.Replace(filename, replaceChar);
		}

		public static int Mod(int x, int m) {
			int r = x % m;
			return r < 0 ? r + m : r;
		}

		public static decimal Mod(decimal x, decimal m) {
			decimal r = x % m;
			return r < 0 ? r + m : r;
		}

		private void ZoomIn_Click(object sender, RoutedEventArgs e) {
			ZoomOut.IsEnabled = true;

			if (_model.ZoomLevel < 4) {
				_model.ZoomLevel++;
			}

			if (_model.ZoomLevel >= 4) {
				ZoomIn.IsEnabled = false;
			}

			RefreshButton_Click(sender, e);
		}

		private void ZoomOut_Click(object sender, RoutedEventArgs e) {
			ZoomIn.IsEnabled = true;

			if (_model.ZoomLevel > 2) {
				_model.ZoomLevel--;
			}

			if (_model.ZoomLevel <= 2) {
				ZoomOut.IsEnabled = false;
			}

			RefreshButton_Click(sender, e);
		}

		private void OpenFromURL_Click(object sender, RoutedEventArgs e) {
			// Instantiate window
			OpenDialog dialogBox = new OpenDialog();

			// Show window modally
			// NOTE: Returns only when window is closed
			Nullable<bool> dialogResult = dialogBox.ShowDialog();

			if (dialogResult == true) {
				_model.PanoID = dialogBox.PanoId;
				RetrieveAndDisplayPanorama(_model.PanoID);

			}
		}

		private void SaveAs_Click(object sender, RoutedEventArgs e) {
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

		private void Exit_Click(object sender, RoutedEventArgs e) {
			Application.Current.Shutdown();
		}

		private void UpdateDimenions(int width, int height) {
			string dimensions = width + " x " + height;
			_model.PanoDimentions = dimensions;
		}

		private void OpenCacheFolder_Click(object sender, RoutedEventArgs e) {
			System.Diagnostics.Process.Start(_model.CachePathBase);
		}

		private void CreateTimelapse_Click(object sender, RoutedEventArgs e) {
			// Instantiate window
			TimelapseSetting dialogBox = new TimelapseSetting(_model.PanoID, _model.CachePathBase);

			// Show window modally
			// NOTE: Returns only when window is closed
			Nullable<bool> dialogResult = dialogBox.ShowDialog();

			if (dialogResult == true) {
				if (!System.IO.Directory.Exists(dialogBox.FilePath)) {
					System.IO.Directory.CreateDirectory(dialogBox.FilePath);
				}

				string saveFilePath = dialogBox.FilePath + @"\" + dialogBox.FileName;
				_model.Heading = dialogBox.DesiredHeading;
				TimeLapseAsync(saveFilePath, _model.PanoID);
			}
		}

		private async void TimeLapseAsync(string fileSavePath, string startPanoId) {
			int timelapseFileCounter = 1;
			continueTimelapse = true;
			_model.StopTimelapseVisibility = Visibility.Visible;
			string nextPanoId = startPanoId;

			try {
				while (continueTimelapse) {
					Task retrievePanoramaTask = RetrieveAndDisplayPanoramaAsync(nextPanoId);
					await retrievePanoramaTask;

					// Save the current image
					displayedImage.Save(fileSavePath + FiveZero(timelapseFileCounter++) + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

					// Get the pano info to find linked panoramas
					panorama panoObject = _model.DownloadPanoramaInfo(nextPanoId);

					if (panoObject.annotation_properties.Length > 0) {
						// Just pick the first linked pano, TODO : make this user selectable
						var nextPano = panoObject.annotation_properties.OrderBy(item => item.yaw_deg).First();
						nextPanoId = nextPano.pano_id;
						_model.Heading = nextPano.yaw_deg;
					} else {
						//No linked panoramas (it can happen)
						continueTimelapse = false;
						break;
					}

				}

			} catch (Exception e) {
				MessageBox.Show(e.Message + Environment.NewLine + e.InnerException, "An error occured creating timelapse images");
			} finally {
				// Open the folder the images are saved to
				System.Diagnostics.Process.Start(fileSavePath.Substring(0, fileSavePath.LastIndexOf(@"\")));
			}
		}

		private void StopTimelapse_Click(object sender, RoutedEventArgs e) {
			continueTimelapse = false;
			_model.StopTimelapseVisibility = Visibility.Hidden;
		}

		private void Website_Click(object sender, RoutedEventArgs e) {
			System.Diagnostics.Process.Start("https://github.com/TomWasHere/StreetViewDownloader");
		}

		private void FOVSlider_Changed(object sender, RoutedEventArgs e) {
			RetrieveAndDisplayPanorama(_model.PanoID);
		}

	}
}

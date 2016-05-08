using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StreetviewDownloader {
	/// <summary>
	/// Interaction logic for TimelapseSetting.xaml
	/// </summary>
	public partial class TimelapseSetting : Window {
		public string FilePath { get; set; } //Save to this path
		public string FileName { get; set; } //Prepend saved images with this name
		public bool Zero360Timelapses { get; set; }  // Set the heading to zero for 360 panos
		public decimal DesiredHeading { get; set; } // The compass heading/direction to move
		public int FieldOfView { get; set; }

		public TimelapseSetting(string panoID, string cachePathBase, bool zero360, int FOV) {
			FilePath = string.Empty;
			Zero360Timelapses = zero360;
			DesiredHeading = 0;
			FileName = "MyTimelapse";
			FieldOfView = FOV;

			InitializeComponent();

			DisplayThumbnails(panoID, cachePathBase);

		}

		private void DisplayThumbnails(string panoId, string cachePathBase) {
			StreetviewDownloaderModel _model = new StreetviewDownloaderModel
			{
				PanoID = panoId
			};

			// Clear previous items
			thumbnails.Children.Clear();

			//Set up downloader...
			Downloader.Downloader imageDownloader = new Downloader.Downloader(cachePathBase);

			System.Drawing.Image bigImage = imageDownloader.GetFullImage(panoId, 2, null);
			panorama panoObject = _model.DownloadPanoramaInfo(panoId);

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
				thumbImg.Width = 180;

				button.Content = thumbImg;
				button.Click += (object sender, RoutedEventArgs e) =>
				{
					foreach (Button thumbButton in thumbnails.Children) {
						thumbButton.BorderThickness = new Thickness(1);
						thumbButton.BorderBrush = System.Windows.Media.Brushes.Gray;
					}

					button.BorderThickness = new Thickness(2);
					button.BorderBrush = System.Windows.Media.Brushes.Yellow;

					DesiredHeading = annotation.yaw_deg;

				};
				thumbnails.Children.Add(button);
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

		private void ChooseDirectory_Click(object sender, RoutedEventArgs e) {
			//Save as dialog box
			Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
			saveDialog.FileName = "Timelapse images will save here..."; // Default file name
			saveDialog.DefaultExt = ""; // Default file extension
			saveDialog.OverwritePrompt = false;
			saveDialog.Title = "Time lapse files will be saved here with the provided name";

			// Show save file dialog box
			Nullable<bool> result = saveDialog.ShowDialog();

			if (result == true) {
				string saveFilePath = saveDialog.FileName;
				saveFilePath = saveFilePath.Substring(0, saveFilePath.LastIndexOf(@"\")); // remove file extension
				FilePath = saveFilePath;
				directory.Text = FilePath;
			}
		}

		private void Cancel_Click(object sender, RoutedEventArgs e) {
			DialogResult = false;
			this.Close();
		}

		private void StartTimelapse_Click(object sender, RoutedEventArgs e) {
			if (FilePath == string.Empty || FileName == string.Empty) {
				MessageBox.Show("Filename and directory required.");
				return;
			}

			DialogResult = true;
			this.Close();
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StreetviewDownloader {
	class StreetviewDownloaderModel : ObservableObject {
		// All images will be stored here
		public string CachePathBase = Directory.GetCurrentDirectory() + @"\cache\";

		private int _zoomLevel;
		public int ZoomLevel { get { return _zoomLevel; } set { _zoomLevel = value; RaisePropertyChanged("ZoomLevel"); } }

		private string _panoID;
		public string PanoID { get { return _panoID; } set { _panoID = value; RaisePropertyChanged("PanoID"); } }

		private string _location;
		public string Location { get { return _location; } set { _location = value; RaisePropertyChanged("Location"); } }

		private string _panoDimentions;
		public string PanoDimentions { get { return _panoDimentions; } set { _panoDimentions = value; RaisePropertyChanged("PanoDimentions"); } }

		private System.Windows.Visibility _progressBarVisibility;
		public System.Windows.Visibility ProgressBarVisibility { get { return _progressBarVisibility; } set { _progressBarVisibility = value; RaisePropertyChanged("ProgressBarVisibility"); } }

		private double _progressBarValue;
		public double ProgressBarValue { get { return _progressBarValue; } set { _progressBarValue = value; RaisePropertyChanged("ProgressBarValue"); } }

		private double _progressBarMaximum;
		public double ProgressBarMaximum { get { return _progressBarMaximum; } set { _progressBarMaximum = value; RaisePropertyChanged("ProgressBarMaximum"); } }

		private string _progressBarLabel;
		public string ProgressBarLabel { get { return _progressBarLabel; } set { _progressBarLabel = value; RaisePropertyChanged("ProgressBarLabel"); } }

		private int _fieldOfView;
		public int FieldOfView {  get { return _fieldOfView; } set { _fieldOfView = value; RaisePropertyChanged("FieldOfView"); } }

		private bool _saveAsEnabled;
		public bool FileMenuSaveAsEnabled { get { return _saveAsEnabled; } set { _saveAsEnabled = value; RaisePropertyChanged("FileMenuSaveAsEnabled"); } }

		private bool _fileMenuTimelapseEnabled;
		public bool FileMenuTimelapseEnabled { get { return _fileMenuTimelapseEnabled; } set { _fileMenuTimelapseEnabled = value; RaisePropertyChanged("FileMenuTimelapseEnabled"); } }

		private System.Windows.Media.ImageSource _mainImageSource;
		public System.Windows.Media.ImageSource MainImageSource { get { return _mainImageSource; } set { _mainImageSource = value; RaisePropertyChanged("MainImageSource"); } }

		public StreetviewDownloaderModel() {
			ZoomLevel = 2;
			PanoID = "28hi5tiRI9Sl6xylktw2TA";
			ProgressBarVisibility = System.Windows.Visibility.Hidden;
			FieldOfView = 360;

			FileMenuSaveAsEnabled = false;
			FileMenuTimelapseEnabled = false;
		}


		public panorama DownloadPanoramaInfo(string panoId) {
			//Set up downloader...
			Downloader.Downloader imageDownloader = new Downloader.Downloader(CachePathBase);

			//Download XML or Load from cache
			if (!System.IO.File.Exists(CachePathBase + panoId + @"\" + panoId + ".xml") || new FileInfo(CachePathBase + panoId + @"\" + panoId + ".xml").Length == 0) {
				if (!System.IO.Directory.Exists(CachePathBase + panoId)) {
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

	}
}

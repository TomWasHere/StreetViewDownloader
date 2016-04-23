using System;
using System.Collections.Generic;
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

namespace StreetviewDownloader
{
    /// <summary>
    /// Interaction logic for OpenDialog.xaml
    /// </summary>
    public partial class OpenDialog : Window
    {

        public String PanoId = string.Empty;

        public OpenDialog()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string url = urlInput.Text;

            try
            {
                PanoId = GetPanoIdFromUrl(url);
                DialogResult = true;
                this.Close();
            } catch 
            {
                PanoId = string.Empty;
            }

            if (PanoId == string.Empty)
            {
                MessageBox.Show("Could not determine panorama ID from this URL.");
            }
        }

        private string GetPanoIdFromUrl(string url)
        {
            int startIndex = url.IndexOf("!1s") + 3;

            string partial = url.Substring(startIndex);

            int endIndex = partial.IndexOf("!");

            return partial.Substring(0, endIndex);
        }
    }
}

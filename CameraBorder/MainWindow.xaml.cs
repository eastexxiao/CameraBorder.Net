using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
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

using MetadataExtractor;
using MetadataExtractor.Formats.Jpeg;

namespace CameraBorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var file = ImageFile.FromFile("sony_a7r5_mine.JPG");
            //var file = ImageFile.FromFile("sony_a7r5_test.JPG");
            //var file = ImageFile.FromFile("sony_a7r5_gps.JPG");
            //var file = ImageFile.FromFile("sony_a7r5_argb.JPG");
            //var file = ImageFile.FromFile("nk_z8.JPG");
            //var file = ImageFile.FromFile("sony_a1.JPG");
            //var file = ImageFile.FromFile("sony_a7c2.JPG");
            //var file = ImageFile.FromFile("canon_r5.JPG");
            //var file = ImageFile.FromFile("nk_z9_export.JPG");
            //var file = ImageFile.FromFile("nk_z9.JPG");
            //var file = ImageFile.FromFile("nk_z9_1.JPG");
            

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //var file = ImageFile.FromFile("sony_a7r5_mine.JPG");
            //var file = ImageFile.FromFile("sony_a7r5_test.JPG");
            //var file = ImageFile.FromFile("sony_a7r5_gps.JPG");
            //var file = ImageFile.FromFile("nk_z8.JPG");
            //var file = ImageFile.FromFile("sony_a1.JPG");
            //var file = ImageFile.FromFile("sony_a7c2.JPG");
            //var file = ImageFile.FromFile("canon_r5.JPG");
            //var file = ImageFile.FromFile("nk_z9.JPG");

            //var t = ExifInfo.Parse("sony_a7r5_gps.JPG");
            //var t = ExifInfo.Parse("sony_a7m4_tl.JPG");

            //var t = ExifInfo.Parse("nk_z8.JPG");
            //var t = ExifInfo.Parse("sony_a1.JPG");
            //var t = ExifInfo.Parse("nk_z9_export.JPG");
            //var t = ExifInfo.Parse("sony_a7c2.JPG");
            //var t = ExifInfo.Parse("canon_r5.JPG");
            var t = ExifInfo.Parse("sony_a7r5_argb.JPG");
            //var t = ExifInfo.Parse("sony_a7r5_test.JPG");
            //var t = ExifInfo.Parse("sony_a7r5_gps.JPG");
            Type q = t.GetType();
            PropertyInfo[] pList = q.GetProperties();
            foreach (var propertyInfo in pList)
            {
                Trace.WriteLine(propertyInfo.Name + " => " + propertyInfo.GetValue(t, null));
            }

            

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //string filePath = "nk_z9_export.jpg";
            //string filePath = "sony_a7r5_export.jpg";
            //string filePath = "sony_a7m4_tl.jpg";
            string filePath = "sony_a7r5_argb.JPG";

            var metas = JpegMetadataReader.ReadMetadata(filePath);
            foreach (var meta in metas)
            {
                //exifsub
                //Trace.WriteLine(@$"=====>{meta.GetString(MetadataExtractor.Formats.Jpeg.JpegDirectory.TagImageWidth)}"); 
                foreach (var tag in meta.Tags)
                {
                    Trace.WriteLine($"{meta.Name} - {tag.Name} = {tag.Description}");
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            WatermarkGenerator q = new WatermarkGenerator();
            q.Test();
        }
    }
}

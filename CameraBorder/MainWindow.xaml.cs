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
using ExifLibrary;
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
            var file = ImageFile.FromFile("sony_a7r5_export.JPG");





            var model = file.Properties.Get<ExifAscii>(ExifTag.Model);
            Trace.WriteLine("Model => "+ model?.Value);
            var make = file.Properties.Get<ExifAscii>(ExifTag.Make);
            Trace.WriteLine("Make => " + make?.Value);
            var cameraOwnerName = file.Properties.Get<ExifAscii>(ExifTag.CameraOwnerName);
            Trace.WriteLine("CameraOwnerName => " + cameraOwnerName?.Value);
            var artist = file.Properties.Get<ExifAscii>(ExifTag.Artist);
            Trace.WriteLine("Artist => " + artist?.Value);
            var cellLength = file.Properties.Get<ExifUShort>(ExifTag.CellLength);
            Trace.WriteLine("CellLength => " + cellLength?.Value);
            var cellWidth = file.Properties.Get<ExifUShort>(ExifTag.CellWidth);
            Trace.WriteLine("CellWidth => " + cellWidth?.Value);
            var dateTime = file.Properties.Get<ExifDateTime>(ExifTag.DateTime);
            Trace.WriteLine("DateTime => " + dateTime?.Value);
            var dateTimeDigitized = file.Properties.Get<ExifDateTime>(ExifTag.DateTimeDigitized);
            Trace.WriteLine("DateTimeDigitized => " + dateTimeDigitized?.Value);
            var dateTimeOriginal = file.Properties.Get<ExifDateTime>(ExifTag.DateTimeOriginal);
            Trace.WriteLine("DateTimeOriginal => " + dateTimeOriginal?.Value);
            var documentName = file.Properties.Get<ExifAscii>(ExifTag.DocumentName);
            Trace.WriteLine("DocumentName => " + documentName?.Value);
            var exposureTime = file.Properties.Get<ExifURational>(ExifTag.ExposureTime);
            Trace.WriteLine("ExposureTime => " + exposureTime?.Value);
            if (exposureTime != null)
            {
                var numerator = exposureTime.Value.Numerator;
                var denominator = exposureTime.Value.Denominator;
                if (numerator > denominator)
                {
                    Trace.WriteLine("ExposureTime => " + (((numerator) / (double)denominator)).ToString() + " s");
                }
                else
                {
                    Trace.WriteLine("ExposureTime => " + numerator.ToString() + "/" + denominator.ToString() + " s");
                }
            }
            var gpsAltitude = file.Properties.Get<ExifURational>(ExifTag.GPSAltitude);
            Trace.WriteLine("GPSAltitude => " + gpsAltitude?.Value);
            var gpsLatitude = file.Properties.Get<GPSLatitudeLongitude>(ExifTag.GPSLatitude);
            Trace.WriteLine("GPSLatitude => " + gpsLatitude?.Value[0] + " " + gpsLatitude?.Value[1] + " " + gpsLatitude?.Value[2]);
            var gpsLongitude = file.Properties.Get<GPSLatitudeLongitude>(ExifTag.GPSLongitude);
            Trace.WriteLine("GPSLongitude => " + gpsLongitude?.Value[0] + " " + gpsLongitude?.Value[1] + " " + gpsLongitude?.Value[2]);
            var imageLength = file.Properties.Get<ExifUShort>(ExifTag.RelatedImageHeight);
            Trace.WriteLine("ImageLength => " + imageLength?.Value);
            var imageWidth = file.Properties.Get<ExifUInt>(ExifTag.XDensity);
            Trace.WriteLine("ImageWidth => " + imageWidth?.Value);
            var isoSpeedRatings = file.Properties.Get<ExifUShort>(ExifTag.ISOSpeedRatings);
            Trace.WriteLine("ISOSpeedRatings => " + isoSpeedRatings?.Value);
            var bodySerialNumber = file.Properties.Get<ExifAscii>(ExifTag.BodySerialNumber);
            Trace.WriteLine("BodySerialNumber => " + bodySerialNumber?.Value);
            var fNumber = file.Properties.Get<ExifURational>(ExifTag.FNumber);
            Trace.WriteLine("FNumber => f/" + fNumber?.ToFloat().ToString(".0"));
            var focalLength = file.Properties.Get<ExifURational>(ExifTag.FocalLength);
            Trace.WriteLine("FocalLength => " + focalLength?.ToFloat().ToString()+"mm");
            var focalLengthIn35mmFilm = file.Properties.Get<ExifUShort>(ExifTag.FocalLengthIn35mmFilm);
            Trace.WriteLine("FocalLengthIn35mmFilm => " + focalLengthIn35mmFilm?.Value);
            var lensMake = file.Properties.Get<ExifAscii>(ExifTag.LensMake);
            Trace.WriteLine("LensMake => " + lensMake?.Value);
            var lensModel = file.Properties.Get<ExifAscii>(ExifTag.LensModel);
            Trace.WriteLine("LensModel => " + lensModel?.Value);
            var lensSerialNumber = file.Properties.Get<ExifAscii>(ExifTag.LensSerialNumber);
            Trace.WriteLine("LensSerialNumber => " + lensSerialNumber?.Value);
            var lensSpecification = file.Properties.Get<LensSpecification>(ExifTag.LensSpecification);
            Trace.WriteLine("LensSpecification => " + lensSpecification?.Value[0] + " " + lensSpecification?.Value[1] + " " + lensSpecification?.Value[2] + " " + lensSpecification?.Value[3]);
            var exposureProgram = file.Properties.Get<ExifEnumProperty<ExposureProgram>>(ExifTag.ExposureProgram);
            Trace.WriteLine("ExposureProgram => " + exposureProgram?.Value);
            var exposureBiasValue = file.Properties.Get<ExifSRational>(ExifTag.ExposureBiasValue);
            Trace.WriteLine("ExposureBiasValue => " + exposureBiasValue?.ToFloat().ToString());
            var colorSpace = file.Properties.Get<ExifEnumProperty<ColorSpace>>(ExifTag.ColorSpace);
            Trace.WriteLine("ColorSpace => " + colorSpace?.Value);

            var testData = file.Properties.Get<ExifUInt>(ExifTag.ImageLength);
            Trace.WriteLine("TestResult => " + testData?.Value);
            var imageWidthQ = file.Properties.Get<ExifUInt>(ExifTag.PixelXDimension); 
            Trace.WriteLine("TestResult => " + imageWidthQ?.Value);

            var qqq = file.Properties.Get(ExifTag.PixelXDimension);
            //Trace.WriteLine(qqq.Name);
            //Trace.WriteLine(qqq.Value);

            foreach (var property in file.Properties)
            {
                Trace.WriteLine(property.Name + " => " + property.Value + " @ " + property.Tag);
            }

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
            //var t = ExifInfo.Parse("nk_z8.JPG");
            //var t = ExifInfo.Parse("sony_a1.JPG");
            var t = ExifInfo.Parse("nk_z9_export.JPG");
            //var t = ExifInfo.Parse("sony_a7c2.JPG");
            //var t = ExifInfo.Parse("canon_r5.JPG");
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
            string filePath = "sony_a7r5_export.jpg";


            var metas = JpegMetadataReader.ReadMetadata(filePath);
            foreach (var meta in metas)
            {
                //exifsub
                Trace.WriteLine(@$"=====>{meta.GetString(MetadataExtractor.Formats.Jpeg.JpegDirectory.TagImageWidth)}"); 
                foreach (var tag in meta.Tags)
                {
                    Trace.WriteLine($"{meta.Name} - {tag.Name} = {tag.Description}");
                }
            }
        }
    }
}

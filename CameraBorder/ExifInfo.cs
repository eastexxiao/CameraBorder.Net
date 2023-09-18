using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using System.Windows;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;
using static MetadataExtractor.Formats.Exif.Makernotes.CanonMakernoteDirectory;

namespace CameraBorder
{
    internal enum DateFormat
    {
        Short,
        Long,
    }

    internal enum TimeFormat
    {
        TwentyFour,
        Twelve,
    }

    internal enum LensSize
    {
        FullFrame,
        ApscFrame,
        M43Frame,
        MF617,
        MF612,
        MF69,
        MF68,
        MF67,
        MF66,
        MF645,
        MF5440,
        MF4937,
        MF4836,
        MF4433,
        MF3737,
    }

    internal class ExifInfo
    {
        public string? CameraManufacturer { get; private set; } = "";
        public string? CameraModel { get; private set; } = "";
        public string? CameraSerialNumber { get; private set; } = "";
        public string? LensManufacturer { get; private set; } = "";
        public string? LensModel { get; private set; } = "";
        public string? LensSerialNumber { get; private set; } = "";
        public string? ExposureMode { get; private set; } = "";
        public string? ApertureFNumber { get; private set; } = "";
        public string? ExposureTime { get; private set; } = "";
        public string? IsoInfo { get; private set; } = "";
        public string? FocalLength { get; private set; } = "";
        public string? EquivalentFocalLength{ get; private set; } = "";
        public string? ExposureCompensation { get; private set; } = "";
        public string? Artist { get; private set; } = "";
        public string? ShootingDate { get; private set; } = "";
        public string? ShootingTime { get; private set; } = "";
        public string? CopyrightInfo { get; private set; } = "";
        public string? ShootingLocationLatitude { get; private set; } = "";
        public string? ShootingLocationLongitude { get; private set; } = "";
        public string? ShootingLocation { get; private set; } = "";
        public bool IsSRGB { get; private set; } = true;
        public int PhotoHeight { get; private set; } = -1;
        public int PhotoWidth { get; private set; } = -1;
        public bool Using35mm = false;
        public int Rotate { get; private set; } = 1;
        
        private DateFormat _dateFormat = DateFormat.Long;
        private TimeFormat _timeFormat = TimeFormat.TwentyFour;
        private bool _using35 = false;
        private LensSize _lensSize = LensSize.FullFrame;
        public ExifInfo()
        {
            
        }

        public bool Decode(string filename,bool using35Focal = false, LensSize lensSize = LensSize.FullFrame, string userName = "", bool cover = false, string userCopy = "",bool coverCopy = false, DateFormat dateFormat = DateFormat.Long, TimeFormat timeFormat = TimeFormat.TwentyFour)
        {
            Using35mm = using35Focal;
            _dateFormat = dateFormat;
            _timeFormat = timeFormat;
            _using35 = using35Focal;
            _lensSize = lensSize;
            var metas = JpegMetadataReader.ReadMetadata(filename);

            var jpegInfo = metas.OfType<JpegDirectory>().FirstOrDefault();
            if (jpegInfo == null)
            {
                return false;
            }

            var exifIfdInfo = metas.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (exifIfdInfo == null)
            {
                return false;
            }

            var exifSubIfdInfo = metas.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (exifSubIfdInfo == null)
            {
                return false;
            }


            PhotoWidth = jpegInfo.GetInt32(JpegDirectory.TagImageWidth);
            PhotoHeight = jpegInfo.GetInt32(JpegDirectory.TagImageHeight);

            var rotR = exifIfdInfo.TryGetInt32(ExifDirectoryBase.TagOrientation, out var rot);
            if (rotR)
            {
                Rotate = rot;
            }

            if (Rotate is 8 or 7 or 6 or 5)
            {
                (PhotoHeight, PhotoWidth) = (PhotoWidth, PhotoHeight);
            }

            CameraManufacturer = exifIfdInfo.GetString(ExifDirectoryBase.TagMake);
            CameraModel = exifIfdInfo.GetString(ExifDirectoryBase.TagModel);
            CameraSerialNumber = exifSubIfdInfo.GetString(ExifDirectoryBase.TagBodySerialNumber);

            LensManufacturer = exifSubIfdInfo.GetString(ExifDirectoryBase.TagLensMake);
            LensModel = exifSubIfdInfo.GetString(ExifDirectoryBase.TagLensModel);
            LensSerialNumber = exifSubIfdInfo.GetString(ExifDirectoryBase.TagLensSerialNumber);

            var ep = exifSubIfdInfo.GetInt32(ExifDirectoryBase.TagExposureProgram);
            ExposureMode = ep switch
            {
                1 => "M",
                2 => "P",
                3 => "A",
                4 => "S",
                5 => "Ps",
                6 => "Pa",
                7 => "H",
                8 => "L",
                _ => "UNKNOWN"
            };

            ExposureTime = exifSubIfdInfo.GetString(ExifDirectoryBase.TagExposureTime) + "s";
            ApertureFNumber = "f/"+exifSubIfdInfo.GetString(ExifDirectoryBase.TagFNumber);
            IsoInfo = "iso" + exifSubIfdInfo.GetString(ExifDirectoryBase.TagIsoEquivalent);
            var focalLength = exifSubIfdInfo.GetDouble(ExifDirectoryBase.TagFocalLength);
            FocalLength = exifSubIfdInfo.GetString(ExifDirectoryBase.TagFocalLength) + "mm";
            EquivalentFocalLength = exifSubIfdInfo.GetString(ExifDirectoryBase.Tag35MMFilmEquivFocalLength);
            if (EquivalentFocalLength == null)
            {
                EquivalentFocalLength = _lensSize switch
                {
                    LensSize.FullFrame => focalLength.ToString() + "mm",
                    LensSize.ApscFrame => (focalLength * 1.5).ToString() + "mm",
                    LensSize.M43Frame => (focalLength * 2).ToString() + "mm",
                    LensSize.MF3737 => (focalLength / 0.83).ToString() + "mm",
                    LensSize.MF4433 => (focalLength / 0.79).ToString() + "mm",
                    LensSize.MF4836 => (focalLength/ 0.72).ToString() + "mm",
                    LensSize.MF4937 => (focalLength / 0.71).ToString() + "mm",
                    LensSize.MF5440 => (focalLength / 0.64).ToString() + "mm",
                    LensSize.MF645 => (focalLength / 0.58).ToString() + "mm",
                    LensSize.MF66 => (focalLength / 0.51).ToString() + "mm",
                    LensSize.MF67 => (focalLength / 0.47).ToString() + "mm",
                    LensSize.MF68 => (focalLength/ 0.43).ToString() + "mm",
                    LensSize.MF69 => (focalLength / 0.40).ToString() + "mm",
                    LensSize.MF612 => (focalLength / 0.32).ToString() + "mm",
                    LensSize.MF617 => (focalLength / 0.24).ToString() + "mm",
                    _ => "N/A"
                };
            }
            else
            {
                EquivalentFocalLength = EquivalentFocalLength + "mm";
            }

            ExposureCompensation = exifSubIfdInfo.GetString(ExifDirectoryBase.TagExposureBias)+"EV";
            Artist = exifIfdInfo.GetString(ExifDirectoryBase.TagArtist);
            if (string.IsNullOrEmpty(Artist))
            {
                Artist = userName;
            }
            CopyrightInfo = exifIfdInfo.GetString(ExifDirectoryBase.TagCopyright);

            var shootTime = exifSubIfdInfo.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);
            ShootingDate = shootTime.ToString(_dateFormat == DateFormat.Long ? "d" : "D");
            ShootingTime = shootTime.ToString("T");

            var gpsInfo = metas.OfType<GpsDirectory>().FirstOrDefault();
            if (gpsInfo != null)
            {
                ShootingLocationLatitude = gpsInfo.GetGeoLocation()?.Latitude.ToString("#.0000");
                ShootingLocationLongitude = gpsInfo.GetGeoLocation()?.Longitude.ToString("#.0000");
                ShootingLocation = gpsInfo.GetGeoLocation()?.ToDmsString();
            }

            var colorSpace = exifSubIfdInfo.GetInt32(ExifDirectoryBase.TagColorSpace);
            IsSRGB = colorSpace == 1;

            return true;

        }

        public static ExifInfo? Parse(string filename, bool using35Focal = false, LensSize lensSize = LensSize.FullFrame, string? userName = "", bool cover = false, string? userCopy = "", bool coverCopy = false, DateFormat dateFormat = DateFormat.Long, TimeFormat timeFormat = TimeFormat.TwentyFour)
        {
            ExifInfo info = new();
            return info.Decode(filename, using35Focal, lensSize, userName, cover, userCopy,  coverCopy, dateFormat, timeFormat) ? info : null;
        }

        

        public void GetJpgSizeByImage(string path, out int x, out int y)
        {
           Image newImage = Image.FromFile(path);
           x = newImage.Width;
           y = newImage.Height;
           //Trace.WriteLine(@$"X={x} Y={y}");
           newImage.Dispose();
        }

    }
}

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
using ExifLibrary;

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
        public string CameraManufacturer { get; private set; } = "";
        public string CameraModel { get; private set; } = "";
        public string CameraSerialNumber { get; private set; } = "";
        public string LensManufacturer { get; private set; } = "";
        public string LensModel { get; private set; } = "";
        public string LensSerialNumber { get; private set; } = "";
        public string ExposureMode { get; private set; } = "";
        public string ApertureFNumber { get; private set; } = "";
        public string ExposureTime { get; private set; } = "";
        public string IsoInfo { get; private set; } = "";
        public string FocalLength { get; private set; } = "";
        public string EquivalentFocalLength{ get; private set; } = "";
        public string ExposureCompensation { get; private set; } = "";
        public string Artist { get; private set; } = "";
        public string ShootingDate { get; private set; } = "";
        public string ShootingTime { get; private set; } = "";
        public string CopyrightInfo { get; private set; } = "";
        public string ShootingLocationLatitude { get; private set; } = "";
        public string ShootingLocationLongitude { get; private set; } = "";
        public bool IsSRGB { get; private set; } = true;
        public int PhotoHeight { get; private set; } = -1;
        public int PhotoWidth { get; private set; } = -1;
        
        private DateFormat _dateFormat = DateFormat.Long;
        private TimeFormat _timeFormat = TimeFormat.TwentyFour;
        private bool _using35 = false;
        private LensSize _lensSize = LensSize.FullFrame;
        public ExifInfo()
        {
            
        }

        public bool Decode(string filename,bool using35Focal = false, LensSize lensSize = LensSize.FullFrame, string userName = "", bool cover = false, string userCopy = "", DateFormat dateFormat = DateFormat.Long, TimeFormat timeFormat = TimeFormat.TwentyFour)
        {
            var file = ImageFile.FromFile(filename);
            if (file == null) return false;

            _dateFormat = dateFormat;
            _timeFormat = timeFormat;
            _using35 = using35Focal;
            _lensSize = lensSize;

            var make = file.Properties.Get<ExifAscii>(ExifTag.Make);
            CameraManufacturer = make == null ? "None" : make.Value;

            var model = file.Properties.Get<ExifAscii>(ExifTag.Model);
            CameraModel = model == null ? "None" : model.Value;

            var bodySerialNumber = file.Properties.Get<ExifAscii>(ExifTag.BodySerialNumber);
            CameraSerialNumber = bodySerialNumber == null ? "None" : bodySerialNumber.Value;

            CopyrightInfo = userCopy;

            if (cover)
            {
                Artist = userName;
            }
            else
            {
                var artist = file.Properties.Get<ExifAscii>(ExifTag.Artist);
                Artist = artist == null ? (userName == "" ? "None" : userName) : artist.Value;
            }

            var lensMake = file.Properties.Get<ExifAscii>(ExifTag.LensMake);
            LensManufacturer = lensMake == null ? "None" :  lensMake.Value;

            var lensModel = file.Properties.Get<ExifAscii>(ExifTag.LensModel);
            LensModel = lensModel == null ? "None" : lensModel.Value;

            var lensSerialNumber = file.Properties.Get<ExifAscii>(ExifTag.LensSerialNumber);
            LensSerialNumber = lensSerialNumber == null ? "None" : lensSerialNumber.Value;

            var fNumber = file.Properties.Get<ExifURational>(ExifTag.FNumber);
            if (fNumber == null)
            {
                ApertureFNumber = "N/A";
            }
            else
            {
                ApertureFNumber = "f/" + fNumber.ToFloat().ToString(".0");
            }

            var exposureTime = file.Properties.Get<ExifURational>(ExifTag.ExposureTime);
            if (exposureTime == null)
            {
                ExposureTime = "N/A";
            }
            else
            {
                var eTime = exposureTime.ToFloat();
                if (eTime > 1)
                {
                    ExposureTime = eTime.ToString() + "s";
                }
                else
                {
                    ExposureTime = exposureTime.Value.Numerator.ToString()+"/"+exposureTime.Value.Denominator.ToString()+"s";
                }
            }

            var isoSpeedRatings = file.Properties.Get<ExifUShort>(ExifTag.ISOSpeedRatings);
            if (isoSpeedRatings == null)
            {
                IsoInfo = "N/A";
            }
            else
            {
                IsoInfo = "ISO" + isoSpeedRatings.ToString();
            }

            var focalLength = file.Properties.Get<ExifURational>(ExifTag.FocalLength);
            if (focalLength == null)
            {
                FocalLength = "N/A";
            }
            else
            {
                if (using35Focal)
                {
                    var focalLengthIn35MmFilm = file.Properties.Get<ExifUShort>(ExifTag.FocalLengthIn35mmFilm);
                    if (focalLengthIn35MmFilm == null)
                    {
                        FocalLength = _lensSize switch
                        {
                            LensSize.FullFrame => focalLength.ToFloat().ToString() + "mm",
                            LensSize.ApscFrame => (focalLength.ToFloat() * 1.5).ToString() + "mm",
                            LensSize.M43Frame => (focalLength.ToFloat() * 2).ToString() + "mm",
                            LensSize.MF3737 => (focalLength.ToFloat() / 0.83).ToString() + "mm",
                            LensSize.MF4433 => (focalLength.ToFloat() / 0.79).ToString() + "mm",
                            LensSize.MF4836 => (focalLength.ToFloat() / 0.72).ToString() + "mm",
                            LensSize.MF4937 => (focalLength.ToFloat() / 0.71).ToString() + "mm",
                            LensSize.MF5440 => (focalLength.ToFloat() / 0.64).ToString() + "mm",
                            LensSize.MF645 => (focalLength.ToFloat() / 0.58).ToString() + "mm",
                            LensSize.MF66 => (focalLength.ToFloat() / 0.51).ToString() + "mm",
                            LensSize.MF67 => (focalLength.ToFloat() / 0.47).ToString() + "mm",
                            LensSize.MF68 => (focalLength.ToFloat() / 0.43).ToString() + "mm",
                            LensSize.MF69 => (focalLength.ToFloat() / 0.40).ToString() + "mm",
                            LensSize.MF612 => (focalLength.ToFloat() / 0.32).ToString() + "mm",
                            LensSize.MF617 => (focalLength.ToFloat() / 0.24).ToString() + "mm",
                            _ => "N/A"
                        };
                    }
                    else
                    {
                        FocalLength = focalLengthIn35MmFilm.Value.ToString() + "mm";
                    }
                }
                else
                {
                    FocalLength = focalLength.ToFloat().ToString() + "mm";
                }
            }

            var exposureBiasValue = file.Properties.Get<ExifSRational>(ExifTag.ExposureBiasValue);
            if (exposureBiasValue == null)
            {
                ExposureCompensation = "N/A";
            }
            else
            {
                if (exposureBiasValue.ToFloat() > 0)
                {
                    ExposureCompensation = "+" + exposureBiasValue.ToFloat().ToString("0.0") + "EV";
                }
                else if (exposureBiasValue.ToFloat() == 0)
                {
                    ExposureCompensation = "0EV";
                }
                else
                {
                    ExposureCompensation = exposureBiasValue.ToFloat().ToString(".0") + "EV";
                }
            }

            var dateTime = file.Properties.Get<ExifDateTime>(ExifTag.DateTime);
            if (dateTime == null)
            {
                ShootingDate = "N/A";
                ShootingTime = "N/A";
            }
            else
            {
                ShootingDate = _dateFormat == DateFormat.Long ? dateTime.Value.ToLongDateString() : dateTime.Value.ToShortDateString();
                ShootingTime = _timeFormat == TimeFormat.TwentyFour
                    ? dateTime.Value.ToString("HH:mm:ss")
                    : dateTime.Value.ToString("hh:mm:ss tt");
            }

            var gpsLatitude = file.Properties.Get<GPSLatitudeLongitude>(ExifTag.GPSLatitude);
            if (gpsLatitude == null)
            {
                ShootingLocationLatitude = "N/A";
            }
            else
            {
                ShootingLocationLatitude = (gpsLatitude.Degrees.Numerator / gpsLatitude.Degrees.Denominator).ToString() + "\u00b0";
                ShootingLocationLatitude +=
                    (gpsLatitude.Minutes.Numerator / gpsLatitude.Minutes.Denominator).ToString() + "'";
                ShootingLocationLatitude +=
                    (gpsLatitude.Seconds.Numerator / (double)gpsLatitude.Seconds.Denominator).ToString(".0") + "\"";
            }

            var gpsLongitude = file.Properties.Get<GPSLatitudeLongitude>(ExifTag.GPSLongitude);
            if (gpsLongitude == null)
            {
                ShootingLocationLongitude = "N/A";
            }
            else
            {
                ShootingLocationLongitude = (gpsLongitude.Degrees.Numerator / gpsLongitude.Degrees.Denominator).ToString() + "\u00b0";
                ShootingLocationLongitude +=
                    (gpsLongitude.Minutes.Numerator / gpsLongitude.Minutes.Denominator).ToString() + "'";
                ShootingLocationLongitude +=
                    (gpsLongitude.Seconds.Numerator / (double)gpsLongitude.Seconds.Denominator).ToString(".0") + "\"";
            }

            var exposureProgram = file.Properties.Get<ExifEnumProperty<ExposureProgram>>(ExifTag.ExposureProgram);
            if (exposureProgram == null)
            {
                ExposureMode = "N/A";
            }
            else
            {
                ExposureMode = exposureProgram.Value switch
                {
                    ExposureProgram.ShutterPriority => "S",
                    ExposureProgram.AperturePriority => "A",
                    ExposureProgram.Manual => "M",
                    ExposureProgram.Normal => "Auto",
                    _ => "N/A"
                };
            }

            var colorSpace = file.Properties.Get<ExifEnumProperty<ColorSpace>>(ExifTag.ColorSpace);
            if (colorSpace == null)
            {
                IsSRGB = true;
            }
            else
            {
                IsSRGB = colorSpace.Value == ColorSpace.sRGB;
            }

            var imageWidth = file.Properties.Get(ExifTag.PixelXDimension);
            if (imageWidth != null)
            {
                PhotoWidth = Convert.ToInt32(imageWidth.Value);
            }
            else
            {
                GetJpgSizeByImage(filename, out int x, out int y);
                PhotoWidth = x;
                PhotoHeight = y;
            }

            var imageHeight = file.Properties.Get(ExifTag.PixelYDimension);
            if (imageHeight != null)
            {
                PhotoHeight = Convert.ToInt32(imageHeight.Value);
            }

            return true;

        }

        public static ExifInfo? Parse(string filename)
        {
            ExifInfo info = new();
            return info.Decode(filename) ? info : null;
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

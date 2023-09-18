using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace CameraBorder
{
    public delegate void UpdateStage(string filename,string stage, int process);

    public delegate void GenerateFail(string filename, string reason);

    public delegate void GenerateSuccess(string filename);
    public enum WatermarkLayout
    {
        Left,
        Right,
        Middle,
        None,
    }

    internal class CharacterConfig
    {
        public string? Format { get; set; }
        public int FontSize { get; set; } = 1;
        public bool FontBold { get; set; } = false;
        public Color FontColor { get; set; } = Color.Black;
        public string? FontFamily { get; set; } = "微软雅黑";

    }

    internal class WatermarkConfig
    {
        public WatermarkLayout Layout { get; set; }
        public List<CharacterConfig> LeftCharacterConfigs { get; set; } = new();
        public List<CharacterConfig> MiddleCharacterConfigs { get; set; } = new();
        public List<CharacterConfig> RightCharacterConfigs { get; set; } = new();
        public string? CopyrightInfo { get; set; } = "";
        public string? ArtistName { get; set; } = "";
        public bool OverwriteCopyright { get; set; } = false;
        public bool OverwriteArtistName { get; set; } = false;
        public string? OutputPath { get; set; } = "./output/";
        public bool WhiteFill { get; set; } = true;
        public Color BackColor { get; set; } = Color.White;
        public bool Using35 { get; set; } = false;
        public LensSize LensSize { get; set; } = LensSize.FullFrame;
    }

    internal class WatermarkGenerator
    {
        private List<string>? fileList;

        //private WatermarkConfig? config;
        private PrivateFontCollection? pfc;

        private void AddPrivateFont()
        {
            pfc = new PrivateFontCollection();
            string[] fontNames = { "./fonts/AlibabaPuHuiTi-2-45-Light.otf", "./fonts/AlibabaPuHuiTi-2-85-Bold.otf" };
            foreach (var fontName in fontNames)
            {
                pfc.AddFontFile(fontName);
            }
        }

        public WatermarkGenerator()
        {
            AddPrivateFont();
        }

        //private Dictionary<string, string?>? tmpDict = null;

        private double CalcWatermarkRatio(int x, int y)
        {
            if (x / y >= 1)
            {
                return 0.08;
            }
            else
            {
                return 0.13;
            }
        }

        private bool ConcatenateSrcImageAndWatermark(ref Bitmap org, ref Bitmap watermark, Color backColor, int whiteBoardWidth = 0)
        {
            var targetWidth = org.Width + 2 * whiteBoardWidth;
            var targetHeight = org.Height + watermark.Height + 1 * whiteBoardWidth;
            Bitmap img = new Bitmap(targetWidth, targetHeight);
            Graphics g = Graphics.FromImage(img);
            g.FillRectangle(new SolidBrush(backColor), new Rectangle(0, 0, targetWidth, targetHeight));
            g.DrawImage(org, whiteBoardWidth, whiteBoardWidth, org.Width, org.Height);
            g.DrawImage(watermark, (targetWidth - watermark.Width) / 2, org.Height + whiteBoardWidth, watermark.Width,
                watermark.Height);
            org.Dispose();
            org = img;
            g.Dispose();
            return true;
        }

        private bool GenerateWatermark(string filename, ref string reason, UpdateStage update,
            WatermarkConfig watermarkConfig, string outputFilePath)
        {
            var fileNameEasy = System.IO.Path.GetFileName(filename);
            var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filename);
            var outputFileName = outputFilePath + "\\" + fileNameWithoutExtension + "_C.jpg";
            update($@"{fileNameEasy}", @"Starting", 0);
            update($@"{fileNameEasy}", @"Load Info", 0);
            var t = ExifInfo.Parse(filename: filename, using35Focal: watermarkConfig.Using35,
                userName: watermarkConfig.ArtistName,
                cover: watermarkConfig.OverwriteArtistName, lensSize: watermarkConfig.LensSize,
                userCopy: watermarkConfig.CopyrightInfo,
                coverCopy: watermarkConfig.OverwriteCopyright);
            if (t == null)
            {
                update($@"{fileNameEasy}", @"Failed", 0);
                return false;
            }

            Dictionary<string, string?> tmpDict = new();
            Type q = t.GetType();
            PropertyInfo[] pList = q.GetProperties();
            foreach (var propertyInfo in pList)
            {
                //Trace.WriteLine(propertyInfo.Name + " => " + propertyInfo.GetValue(t, null));
                tmpDict[propertyInfo.Name] = propertyInfo.GetValue(t, null)?.ToString();
            }

            var ratio = CalcWatermarkRatio(t.PhotoWidth, t.PhotoHeight);
            var watermarkWidth = t.PhotoWidth;
            var watermarkHeight = (int)(watermarkWidth * ratio);
            update($@"{fileNameEasy}", $"{t.CameraManufacturer}", 0);
            var logoFilePath = t.CameraManufacturer switch
            {
                { } a when a.Contains("NIKON") => "./logos/nikon.png",
                { } a when a.Contains("SONY") => "./logos/sony.png",
                { } a when a.Contains("Canon") => "./logos/canon.png",
                { } a when a.Contains("APPLE") => "./logos/apple.png",
                { } a when a.Contains("DJI") => "./logos/DJI.png",
                { } a when a.Contains("FUJIFILM") => "./logos/fujifilm.png",
                { } a when a.Contains("HASSELBLAD") => "./logos/hasselblad.png",
                { } a when a.Contains("HUAWEI") => "./logos/xmage.png",
                { } a when a.Contains("leica") => "./logos/leica_logo.png",
                { } a when a.Contains("HASSELBLAD") => "./logos/hasselblad.png",
                { } a when a.Contains("Olympus") => "./logos/olympus_blue_gold.png",
                { } a when a.Contains("Panasonic") => "./logos/panasonic.png",
                { } a when a.Contains("PENTAX") => "./logos/pentax.png",
                { } a when a.Contains("RICOH") => "./logos/ricoh.png",
                _ => "./logos/empty.png"
            };

            var logoImage = Image.FromFile(logoFilePath);
            Bitmap logoBitmap = new(logoImage);
            ZoomBitmap(ref logoBitmap, (double)watermarkHeight / logoBitmap.Height);

            Bitmap result = new(1, 1);
            update($@"{fileNameEasy}", "Convert Strings", 5);
            Regex reg = new Regex(@"\{\{(.*?)\}\}");

            List<Bitmap> leftBitmaps = new();
            foreach (var leftConfig in watermarkConfig.LeftCharacterConfigs)
            {
                var str = reg.Replace(leftConfig.Format!,
                    m => tmpDict[m.Groups[1].Value] ?? "N/A");
                GenerateImageByString(str, out Bitmap? tmpBitmap, leftConfig, watermarkConfig.BackColor);
                if (tmpBitmap != null)
                {
                    leftBitmaps.Add(tmpBitmap);
                }
            }

            Bitmap leftResult = new Bitmap(1, 1);
            if (leftBitmaps.Count > 0)
            {
                _ = ConcatenateImageListColumn(leftBitmaps, out leftResult, watermarkConfig.Layout, watermarkConfig.BackColor);
                ZoomBitmap(ref leftResult!, (double)watermarkHeight / leftResult!.Height);
            }

            List<Bitmap> middleBitmaps = new();
            foreach (var middleConfig in watermarkConfig.MiddleCharacterConfigs)
            {
                var str = reg.Replace(middleConfig.Format!,
                    m => tmpDict[m.Groups[1].Value] ?? "N/A");
                GenerateImageByString(str, out Bitmap? tmpBitmap, middleConfig, watermarkConfig.BackColor);
                if (tmpBitmap != null)
                {
                    middleBitmaps.Add(tmpBitmap);
                }
            }

            Bitmap middleResult;
            if (middleBitmaps.Count > 0)
            {
                _ = ConcatenateImageListColumn(middleBitmaps, out middleResult, watermarkConfig.Layout,watermarkConfig.BackColor);
                ZoomBitmap(ref middleResult!, (double)watermarkHeight / middleResult!.Height);
            }

            List<Bitmap> rightBitmaps = new();
            foreach (var rightConfig in watermarkConfig.RightCharacterConfigs)
            {
                var str = reg.Replace(rightConfig.Format!,
                    m => tmpDict[m.Groups[1].Value] ?? "N/A");
                GenerateImageByString(str, out Bitmap? tmpBitmap, rightConfig, watermarkConfig.BackColor);
                if (tmpBitmap != null)
                {
                    rightBitmaps.Add(tmpBitmap);
                }
            }

            Bitmap rightResult = new(1, 1);
            if (rightBitmaps.Count > 0)
            {
                _ = ConcatenateImageListColumn(rightBitmaps, out rightResult, watermarkConfig.Layout, watermarkConfig.BackColor);
                ZoomBitmap(ref rightResult!, (double)watermarkHeight / rightResult!.Height);
            }

            int borderWidth = 0;
            double left_scale = 0.25 * leftBitmaps.Count;
            double right_scale = 0.25 * rightBitmaps.Count;
            switch (watermarkConfig.Layout)
            {
                case WatermarkLayout.Left:
                    borderWidth = (int)(logoBitmap.Height *  (t.PhotoWidth>t.PhotoHeight ? 0.28 : 0.25));
                    
                    ZoomBitmap(ref leftResult, left_scale);
                    ZoomBitmap(ref rightResult, right_scale);
                    ZoomBitmap(ref logoBitmap, 0.8);
                    _ = ConcatenateImageListRow(ref logoBitmap, ref leftResult, ref rightResult, watermarkConfig.Layout,
                        watermarkWidth,
                        watermarkHeight, out result,watermarkConfig.BackColor);
                    break;
                case WatermarkLayout.Right:


                    borderWidth = (int)(logoBitmap.Height * (t.PhotoWidth > t.PhotoHeight ? 0.28 : 0.25));
                    ZoomBitmap(ref leftResult, left_scale);
                    ZoomBitmap(ref logoBitmap, 0.6);
                    ZoomBitmap(ref rightResult, right_scale);
                    _ = ConcatenateImageListRow(ref leftResult, ref logoBitmap, ref rightResult, watermarkConfig.Layout,
                        watermarkWidth, watermarkHeight, out result,watermarkConfig.BackColor ,(watermarkConfig.WhiteFill) ? 0 : borderWidth);
                    break;
                case WatermarkLayout.Middle:
                    break;
                case WatermarkLayout.None:
                    break;
                default:
                    break;
            }


            var orgImg = Image.FromFile(filename);
            Bitmap orgBitmap = new Bitmap(orgImg);
            switch (t.Rotate)
            {
                case 5:
                    orgBitmap.RotateFlip(RotateFlipType.Rotate180FlipY);
                    orgBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 6:
                    orgBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 7:
                    orgBitmap.RotateFlip(RotateFlipType.Rotate180FlipY);
                    orgBitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
                case 8:
                    orgBitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
                case 1:
                    break;
                case 2:
                    orgBitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
                    break;
                case 3:
                    orgBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 4:
                    orgBitmap.RotateFlip(RotateFlipType.Rotate180FlipY);
                    break;
                default:
                    break;

            }

            update($@"{fileNameEasy}", $"Saving", 0);
            ConcatenateSrcImageAndWatermark(ref orgBitmap, ref result, watermarkConfig.BackColor,(watermarkConfig.WhiteFill) ? borderWidth : 0);

            orgBitmap.Save(outputFileName);

            foreach (var left in leftBitmaps)
            {
                left.Dispose();
            }

            foreach (var right in rightBitmaps)
            {
                right.Dispose();
            }

            foreach (var midd in middleBitmaps)
            {
                midd.Dispose();
            }

            orgImg.Dispose();
            orgBitmap.Dispose();
            update($@"{fileNameEasy}", $@"Done!", 0);
            return false;
        }
        
        public async void StartGenerateAsync(GenerateSuccess success, GenerateFail fail, UpdateStage update,
            List<string> fileList, WatermarkConfig config, string outputPath, int count = 4)
        {
            List<Task<bool>> tasks = new();
            foreach (var filename in fileList)
            {
                var reason = "";
                tasks.Add(Task.Run(() => GenerateWatermark(filename, ref reason, update, config, outputPath)));
            }
        }

        private bool GenerateImageByString(string text, out Bitmap? image, CharacterConfig? fontConfig, Color backColor)
        {
            if (fontConfig == null)
            {
                image = null;
                return false;
            }

            var stringFont = new Font((fontConfig.FontBold) ? pfc!.Families[1] : pfc!.Families[0], fontConfig.FontSize,
                (fontConfig.FontBold) ? FontStyle.Bold : FontStyle.Regular);
            Graphics g = Graphics.FromImage(new Bitmap(1, 1));
            SizeF sizeF = g.MeasureString(text, stringFont);
            Brush brush = new SolidBrush(fontConfig.FontColor);
            PointF pf = new PointF(0, 0);
            Bitmap img = new Bitmap(Convert.ToInt32(sizeF.Width) + 1, Convert.ToInt32(sizeF.Height) + 1);
            g = Graphics.FromImage(img);
            var rect = new Rectangle(0, 0, Convert.ToInt32(sizeF.Width) + 1, Convert.ToInt32(sizeF.Height) + 1);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.FillRectangle(new SolidBrush(backColor), rect);
            g.DrawString(text, stringFont, brush, rect);
            image = img;
            return true;
        }

        private bool ConcatenateImageListColumn(List<Bitmap> bitmaps, out Bitmap result, WatermarkLayout layout, Color backColor)
        {
            int widthMax = 0;
            int heightSum = 0;
            foreach (var bitmap in bitmaps)
            {
                if (bitmap.Width > widthMax)
                {
                    widthMax = bitmap.Width;
                }

                heightSum += bitmap.Height;
            }

            Bitmap img = new Bitmap(widthMax + 1, heightSum + 1);
            Graphics g = Graphics.FromImage(img);
            g.FillRectangle(new SolidBrush(backColor), new Rectangle(0, 0, widthMax + 1, heightSum + 1));
            var lastHeight = 0;
            foreach (var bitmap in bitmaps)
            {
                var currWidth = layout switch
                {
                    WatermarkLayout.Left => 0,
                    //WatermarkLayout.Right => widthMax - bitmap.Width,
                    WatermarkLayout.Right => 0,
                    WatermarkLayout.Middle => widthMax / 2 - bitmap.Width / 2,
                    _ => 1,
                };
                g.DrawImage(bitmap, currWidth, lastHeight, bitmap.Width, bitmap.Height);
                lastHeight += bitmap.Height;
            }

            g.Dispose();
            result = img;
            return true;

        }

        private bool ConcatenateImageListRow(ref Bitmap left, ref Bitmap mid, ref Bitmap right,
            WatermarkLayout layout, int width, int height, out Bitmap? result, Color backColor, int whiteBorderWidth = 0)
        {
            Bitmap img = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(img);
            g.FillRectangle(new SolidBrush(backColor), new Rectangle(0, 0, width, height));
            int lineBlankWidth = (int)(width * 0.015);
            if (layout == WatermarkLayout.Middle)
            {
                g.DrawImage(mid, (width - mid.Width) / 2, (height - mid.Height) / 2, mid.Width, mid.Height);

                result = img;
                return true;
            }
            else if (layout == WatermarkLayout.Right)
            {
                g.DrawImage(left, 0 + whiteBorderWidth, (height - left.Height) / 2, left.Width, left.Height);
                g.DrawImage(right, (width - right.Width - whiteBorderWidth), (height - right.Height) / 2, right.Width,
                    right.Height);
                g.DrawImage(mid, width - right.Width - mid.Width - lineBlankWidth - whiteBorderWidth,
                    (height - mid.Height) / 2, mid.Width, mid.Height);
                g.DrawLine(new Pen(Color.Gray, 5), width - right.Width - lineBlankWidth / 2 - whiteBorderWidth,
                    (height - right.Height) / 2, width - right.Width - lineBlankWidth / 2 - whiteBorderWidth,
                    (height - right.Height) / 2 + right.Height);

                result = img;
                return true;
            }
            else if (layout == WatermarkLayout.Left)
            {
                g.DrawImage(left, 0 + whiteBorderWidth, (height - left.Height) / 2, left.Width, left.Height);
                g.DrawImage(mid, left.Width + whiteBorderWidth + 20, (height - mid.Height) / 2, mid.Width, mid.Height);
                g.DrawImage(right, (width - right.Width - whiteBorderWidth), (height - right.Height) / 2, right.Width,
                    right.Height);

                result = img;
                return true;
            }
            else
            {
                result = null;
                return false;
            }

            return true;
        }

        private bool ZoomBitmap(ref Bitmap bitmap, double scale)
        {
            //Trace.WriteLine($@"====>Scale = {scale}");
            var srcWidth = bitmap.Width;
            var srcHeight = bitmap.Height;
            var dstWidth = (int)(srcWidth * scale);
            var dstHeight = (int)(srcHeight * scale);
            Bitmap destBitmap = new Bitmap((int)dstWidth, (int)dstHeight);
            Graphics g = Graphics.FromImage(destBitmap);
            g.Clear(Color.Transparent);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(bitmap, new Rectangle(0, 0, dstWidth, dstHeight), 0, 0, bitmap.Width, bitmap.Height,
                GraphicsUnit.Pixel);
            bitmap.Dispose();
            g.Dispose();
            bitmap = destBitmap;
            return true;
        }

    }
}

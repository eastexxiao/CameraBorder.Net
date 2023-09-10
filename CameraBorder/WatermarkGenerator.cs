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
    public delegate void UpdateStage(string stage, int process);

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
        private WatermarkConfig? config;
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

        private Dictionary<string, string?>? tmpDict = null;

        private double CalcWatermarkRatio(int x, int y)
        {
            if (x / y >= 1)
            {
                return 0.04;
            }
            else
            {
                return 0.09;
            }
        }

        private bool ConcatenateSrcImageAndWatermark(ref Bitmap org, ref Bitmap watermark, int whiteBoardWidth = 0)
        {
            var targetWidth = org.Width + 2 * whiteBoardWidth;
            var targetHeight = org.Height+watermark.Height + 2 * whiteBoardWidth + whiteBoardWidth;
            Bitmap img = new Bitmap(targetWidth, targetHeight);
            Graphics g = Graphics.FromImage(img);
            g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, targetWidth, targetHeight));
            g.DrawImage(org, whiteBoardWidth, whiteBoardWidth, org.Width, org.Height);
            g.DrawImage(watermark, whiteBoardWidth, org.Height+whiteBoardWidth+whiteBoardWidth, watermark.Width, watermark.Height);
            org.Dispose();
            org = img;
            g.Dispose();
            return true;
        }

        private bool GenerateWatermark(string filename, ref string reason, UpdateStage update,  WatermarkConfig watermarkConfig)
        {
            update(@$"Start Processing File{filename}", 1);
            update("Reading Exif Info", 2);
            var t = ExifInfo.Parse(filename: filename, using35Focal: watermarkConfig.Using35, userName: watermarkConfig.ArtistName,
                cover: watermarkConfig.OverwriteArtistName, lensSize: watermarkConfig.LensSize, userCopy: watermarkConfig.CopyrightInfo,
                coverCopy: watermarkConfig.OverwriteCopyright);
            if (t == null)
            {
                update("Failed!", 2);
                return false;
            }
            tmpDict = new();
            Type q = t.GetType();
            PropertyInfo[] pList = q.GetProperties();
            foreach (var propertyInfo in pList)
            {
                Trace.WriteLine(propertyInfo.Name + " => " + propertyInfo.GetValue(t, null));
                tmpDict[propertyInfo.Name] = propertyInfo.GetValue(t, null)?.ToString();
            }

            var ratio = CalcWatermarkRatio(t.PhotoWidth, t.PhotoHeight);
            var watermarkWidth = t.PhotoWidth;
            var watermarkHeight = (int)(watermarkWidth * ratio);

            var logoFilePath = t.CameraManufacturer switch
            {
                { } a when a.Contains("NIKON") => "./logos/nikon.png",
                { } b when b.Contains("SONY") => "./logos/sony.png",
                _ => "./logos/empty.png"
            };

            var logoImage = Image.FromFile(logoFilePath);
            Bitmap logoBitmap = new(logoImage);
            ZoomBitmap(ref logoBitmap, (double)watermarkHeight / logoBitmap.Height);
            
            Bitmap result = new(1, 1);
            
            Regex reg = new Regex(@"\{\{(.*?)\}\}");

            List<Bitmap> leftBitmaps = new();
            foreach (var leftConfig in watermarkConfig.LeftCharacterConfigs)
            {
                var str = reg.Replace(leftConfig.Format!,
                    m => tmpDict[m.Groups[1].Value] ?? "N/A");
                GenerateImageByString(str, out Bitmap? tmpBitmap, leftConfig);
                if (tmpBitmap != null)
                {
                    leftBitmaps.Add(tmpBitmap);
                }
            }

            Bitmap leftResult = new Bitmap(1, 1);
            if (leftBitmaps.Count > 0)
            {
                _ = ConcatenateImageListColumn(leftBitmaps, out leftResult, watermarkConfig.Layout);
                leftResult.Save("leftResult.jpg");
                ZoomBitmap(ref leftResult!, (double)watermarkHeight / leftResult!.Height);
                leftResult.Save("leftResultS.jpg");
            }

            List<Bitmap> middleBitmaps = new();
            foreach (var middleConfig in watermarkConfig.MiddleCharacterConfigs)
            {
                var str = reg.Replace(middleConfig.Format!,
                    m => tmpDict[m.Groups[1].Value] ?? "N/A");
                GenerateImageByString(str, out Bitmap? tmpBitmap, middleConfig);
                if (tmpBitmap != null)
                {
                    middleBitmaps.Add(tmpBitmap);
                }
            }

            Bitmap middleResult;
            if (middleBitmaps.Count > 0)
            {
                _ = ConcatenateImageListColumn(middleBitmaps, out middleResult, watermarkConfig.Layout);
                ZoomBitmap(ref middleResult!, (double)watermarkHeight / middleResult!.Height);
            }

            List<Bitmap> rightBitmaps = new();
            foreach (var rightConfig in watermarkConfig.RightCharacterConfigs)
            {
                var str = reg.Replace(rightConfig.Format!,
                    m => tmpDict[m.Groups[1].Value] ?? "N/A");
                GenerateImageByString(str, out Bitmap? tmpBitmap, rightConfig);
                if (tmpBitmap != null)
                {
                    rightBitmaps.Add(tmpBitmap);
                }
            }

            Bitmap rightResult = new(1, 1);
            if (rightBitmaps.Count > 0)
            {
                _ = ConcatenateImageListColumn(rightBitmaps, out rightResult, watermarkConfig.Layout);
                rightResult.Save("rightSave.jpg");
                ZoomBitmap(ref rightResult!, (double)watermarkHeight / rightResult!.Height);
                rightResult.Save("rightSaveS.jpg");
            }


            if (watermarkConfig.Layout == WatermarkLayout.Left)
            {
                _ = ConcatenateImageListRow(ref logoBitmap, ref leftResult, ref rightResult, watermarkConfig.Layout, watermarkWidth,
                    watermarkHeight, out result);
            } else if (watermarkConfig.Layout == WatermarkLayout.Right)
            {

            } else if (watermarkConfig.Layout == WatermarkLayout.Middle)
            {

            } else if (watermarkConfig.Layout == WatermarkLayout.None)
            {

            }
            else
            {

            }


            var orgImg = Image.FromFile(filename);
            Bitmap orgBitmap = new Bitmap(orgImg);

            ConcatenateSrcImageAndWatermark(ref orgBitmap, ref result, 100);
            orgBitmap.Save(filename+"_done.jpg");

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
            update($@"Success!", 2);
            return false;
        }

        public void StartGenerateAsync(GenerateSuccess success, GenerateFail fail, UpdateStage update, List<string> fileList, WatermarkConfig config)
        {
            
        }

        private bool GenerateImageByString(string text, out Bitmap? image, CharacterConfig? fontConfig)
        {
            if (fontConfig == null)
            {
                image = null;
                return false;
            }

            var stringFont = new Font(pfc!.Families[0], fontConfig.FontSize, (fontConfig.FontBold) ? FontStyle.Bold : FontStyle.Regular);
            Graphics g = Graphics.FromImage(new Bitmap(1, 1));
            SizeF sizeF = g.MeasureString(text, stringFont);
            Brush brush = new SolidBrush(fontConfig.FontColor);
            PointF pf = new PointF(0, 0);
            Bitmap img = new Bitmap(Convert.ToInt32(sizeF.Width)+1, Convert.ToInt32(sizeF.Height)+1);
            g = Graphics.FromImage(img);
            var rect = new Rectangle(0,0,Convert.ToInt32(sizeF.Width)+1, Convert.ToInt32(sizeF.Height) + 1);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.FillRectangle(new SolidBrush(Color.White), rect);
            g.DrawString(text, stringFont, brush, rect);
            img.Save("test.jpg");
            image = img;
            return true;
        }

        private bool ConcatenateImageListColumn(List<Bitmap> bitmaps, out Bitmap result, WatermarkLayout layout)
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
            g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, widthMax + 1, heightSum + 1));
            var lastHeight = 0;
            foreach (var bitmap in bitmaps)
            {
                var currWidth = layout switch
                {
                    WatermarkLayout.Left => 0,
                    WatermarkLayout.Right => widthMax - bitmap.Width,
                    WatermarkLayout.Middle=>widthMax/2 - bitmap.Width/2,
                    _ => 1,
                };
                g.DrawImage(bitmap, currWidth, lastHeight,  bitmap.Width, bitmap.Height);
                lastHeight += bitmap.Height;
            }
            g.Dispose();
            result = img;
            //img.Save("test1.jpg");
            return true;

        }

        private bool ConcatenateImageListRow(ref Bitmap left, ref Bitmap mid, ref Bitmap right,
            WatermarkLayout layout, int width, int height, out Bitmap? result)
        {
            Bitmap img = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(img);
            g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, width, height));
            if (layout == WatermarkLayout.Middle)
            {
                g.DrawImage(mid, (width-mid.Width)/2, (height-mid.Height)/2, mid.Width, mid.Height);

                result = img;
                return true;
            } else if (layout == WatermarkLayout.Right)
            {
                g.DrawImage(left, 0, (height-left.Height)/2, left.Width, left.Height);
                g.DrawImage(right, (width-right.Width), (height-right.Height)/2, right.Width, right.Height);
                g.DrawImage(mid, width-right.Width-mid.Width, (height - right.Height) / 2, mid.Width, mid.Height);

                result = img;
                return true;
            } else if (layout == WatermarkLayout.Left)
            {
                g.DrawImage(left, 0, (height - left.Height) / 2, left.Width, left.Height);
                g.DrawImage(mid, left.Width, (height-mid.Height)/2, mid.Width, mid.Height);
                g.DrawImage(right, (width - right.Width), (height - right.Height) / 2, right.Width, right.Height);

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
            Trace.WriteLine($@"====>Scale = {scale}");
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
            g.DrawImage(bitmap, new Rectangle(0, 0, dstWidth, dstHeight), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel);
            bitmap.Dispose();
            g.Dispose();
            bitmap = destBitmap;
            return true;
        }

        public void Test()
        {
            
            GenerateImageByString("Hello World", out Bitmap? image,
                new CharacterConfig()
                    { FontBold = true, FontFamily = @"微软雅黑", FontColor = Color.Black, FontSize = 122, Format = "" });
            GenerateImageByString("Hello World1", out Bitmap? image1,
                new CharacterConfig()
                    { FontBold = true, FontFamily = @"微软雅黑", FontColor = Color.Black, FontSize = 122, Format = "" });
            GenerateImageByString("Hello World2", out Bitmap? image2,
                new CharacterConfig()
                    { FontBold = true, FontFamily = @"微软雅黑", FontColor = Color.Black, FontSize = 122, Format = "" });
            var bitmapList = new List<Bitmap>
            {
                image,
                image1,
                image2,
            };
            ConcatenateImageListColumn(bitmapList, out Bitmap? result1, WatermarkLayout.Middle);

            GenerateImageByString("Hello World3", out Bitmap? image3,
                new CharacterConfig()
                    { FontBold = false, FontFamily = @"微软雅黑", FontColor = Color.Black, FontSize = 122, Format = "" });
            GenerateImageByString("Hello World4", out Bitmap? image4,
                new CharacterConfig()
                    { FontBold = false, FontFamily = @"微软雅黑", FontColor = Color.Black, FontSize = 122, Format = "" });
            GenerateImageByString("Hello World5", out Bitmap? image5,
                new CharacterConfig()
                    { FontBold = false, FontFamily = @"微软雅黑", FontColor = Color.Black, FontSize = 122, Format = "" });
            var bitmapList1 = new List<Bitmap>
            {
                image3,
                image4,
                image5,
            };
            ConcatenateImageListColumn(bitmapList1, out Bitmap? result2, WatermarkLayout.Middle);

            GenerateImageByString("Hello World6", out Bitmap? image6,
                new CharacterConfig()
                    { FontBold = false, FontFamily = @"微软雅黑", FontColor = Color.Black, FontSize = 122, Format = "" });
            GenerateImageByString("Hello World7", out Bitmap? image7,
                new CharacterConfig()
                    { FontBold = false, FontFamily = @"微软雅黑", FontColor = Color.Black, FontSize = 122, Format = "" });
            GenerateImageByString("Hello World8", out Bitmap? image8,
                new CharacterConfig()
                    { FontBold = false, FontFamily = @"微软雅黑", FontColor = Color.Black, FontSize = 122, Format = "" });
            var bitmapList2 = new List<Bitmap>
            {
                image3,
                image4,
                //image5,
            };
            ConcatenateImageListColumn(bitmapList2, out Bitmap? result3, WatermarkLayout.Middle);

            var logoImage = Image.FromFile("./logos/nikon.png");
            Bitmap logoBitmap = new Bitmap(logoImage);
            ZoomBitmap(ref logoBitmap, 0.3);
            result2.Save("22222.jpg");
            Trace.WriteLine($@"result2.Height = > {result2.Height} logo.Height = > {logoBitmap.Height}");
            ZoomBitmap(ref result2, (double)logoBitmap.Height/result2.Height);
            result2.Save("22222z.jpg");
            Trace.WriteLine($@"result3.Height = > {result3.Height} logo.Height = > {logoBitmap.Height}");
            ZoomBitmap(ref result3, (double)logoBitmap.Height/result3.Height) ;

            ConcatenateImageListRow(ref logoBitmap, ref result2, ref result3, WatermarkLayout.Left, logoBitmap.Width+result2.Width+result3.Width +300, logoBitmap.Height, out Bitmap? result);
            result?.Save("hhhh.jpg");
            
            logoBitmap.Save("newlogo.jpg");
            logoBitmap.Dispose();
            

        }

        public void Test1()
        {
            //string filePath = "nk_z9_export.jpg";
            //string filePath = "sony_a7r5_export.jpg";
            //string filePath = "sony_a7m4_tl.jpg";

            //string filePath = ("sony_a7r5_mine.JPG");
            //string filePath = ("sony_a7r5_test.JPG");
            string filePath = ("sony_a7r5_gps.JPG");
            //string filePath = ("nk_z8.JPG");
            //string filePath = ("sony_a1.JPG");
            //string filePath = ("sony_a7c2.JPG");
            //string filePath = ("canon_r5.JPG");
            //string filePath = ("nk_z9.JPG");

            var leftConfigs = new List<CharacterConfig>();


            var config = new WatermarkConfig()
            {
                ArtistName = null,
                BackColor = Color.White,
                CopyrightInfo = "c",
                Layout = WatermarkLayout.Left,
                LeftCharacterConfigs = new List<CharacterConfig>()
                {
                    new()
                    {
                        FontFamily = "a",
                        FontBold = true,
                        FontColor = Color.Red,
                        FontSize = 122,
                        Format = "{{CameraModel}}",
                    },
                    new()
                    {
                        FontFamily = "a",
                        FontBold = true,
                        FontColor = Color.Blue,
                        FontSize = 122,
                        Format = "汪淼是小受的{{LensModel}}",
                    },
                    new()
                    {
                        FontFamily = "a",
                        FontBold = true,
                        FontColor = Color.Black,
                        FontSize = 122,
                        Format = "{{FocalLength}} {{ApertureFNumber}} {{ExposureTime}} {{IsoInfo}} {{ExposureCompensation}}",
                    },

                },
                RightCharacterConfigs = new List<CharacterConfig>()
                {
                    new()
                    {
                        FontFamily = "a",
                        FontBold = true,
                        FontColor = Color.Black,
                        FontSize = 122,
                        Format = "{{ShootingDate}} {{ShootingTime}}",
                    },
                    new()
                    {
                        FontFamily = "a",
                        FontBold = true,
                        FontColor = Color.Black,
                        FontSize = 122,
                        Format = "{{ShootingLocation}}",
                    },
                    new()
                    {
                        FontFamily = "a",
                        FontBold = true,
                        FontColor = Color.Green,
                        FontSize = 122,
                        Format = "{{Artist}}",
                    },

                },
                MiddleCharacterConfigs = new List<CharacterConfig>()
                {
                    new()
                    {
                        FontFamily = "a",
                        FontBold = true,
                        FontColor = Color.Black,
                        FontSize = 122,
                        Format = "{{CameraManufacturer}} {{CameraModel}}",
                    },
                    new()
                    {
                        FontFamily = "a",
                        FontBold = true,
                        FontColor = Color.Black,
                        FontSize = 122,
                        Format = "{{CameraSerialNumber}} {{LensModel}}",
                    },
                    new()
                    {
                        FontFamily = "a",
                        FontBold = true,
                        FontColor = Color.Black,
                        FontSize = 122,
                        Format = "{{ExposureTime}} {{ApertureFNumber}}",
                    },

                },
                OverwriteArtistName = false,
                OverwriteCopyright = false,
            };
            string reason = "";
            GenerateWatermark(filePath, ref reason, ((stage, process) =>
            {

            }), config);
        }
    }
}

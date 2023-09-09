using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
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
        public WatermarkGenerator(List<string> fileList, WatermarkConfig config)
        {
            this.fileList = fileList;
            this.config = config;
            AddPrivateFont();
        }

        public WatermarkGenerator()
        {
            AddPrivateFont();

        }
        private bool GenerateWatermark(string filename, ref string reason)
        {
            return false;
        }

        public void StartGenerateAsync(GenerateSuccess success, GenerateFail fail, UpdateStage update)
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

        private bool ConcatenateImageListColumn(List<Bitmap> bitmaps, out Bitmap? result, WatermarkLayout layout)
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

            result = img;
            img.Save("test1.jpg");
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

        }
    }
}

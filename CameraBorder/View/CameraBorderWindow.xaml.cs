using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using CameraBorder.ViewModel;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Path = System.IO.Path;

namespace CameraBorder
{
    /// <summary>
    /// CameraBorderWindow.xaml 的交互逻辑
    /// </summary>

    
    
    public partial class CameraBorderWindow : Window
    {
        
        private WatermarkLayout choice = WatermarkLayout.Left;
        private Dictionary<string, string> _fileStatus = new();
        public CameraBorderWindow()
        {
            InitializeComponent();
            DataContext = new CameraBorderWindowViewModel();

            this.LeftRadio.IsChecked = true;

            CbxBackColor.ItemsSource = typeof(Colors).GetProperties();
            CbxBackColor.SelectedIndex = typeof(Colors).GetProperties().Length-4;
            CbxBackColor.IsEditable = false;

            CbxLeftLine1Color.ItemsSource = typeof(Colors).GetProperties();
            CbxLeftLine1Color.SelectedIndex = 7;
            CbxLeftLine1Color.IsEditable = false;

            CbxLeftLine2Color.ItemsSource = typeof(Colors).GetProperties();
            CbxLeftLine2Color.SelectedIndex = 7;
            CbxLeftLine2Color.IsEditable = false;

            CbxLeftLine3Color.ItemsSource = typeof(Colors).GetProperties();
            CbxLeftLine3Color.SelectedIndex = 7;
            CbxLeftLine3Color.IsEditable = false;

            CbxRightLine1Color.ItemsSource = typeof(Colors).GetProperties();
            CbxRightLine1Color.SelectedIndex = 7;
            CbxRightLine1Color.IsEditable = false;

            CbxRightLine2Color.ItemsSource = typeof(Colors).GetProperties();
            CbxRightLine2Color.SelectedIndex = 7;
            CbxRightLine2Color.IsEditable = false;

            CbxRightLine3Color.ItemsSource = typeof(Colors).GetProperties();
            CbxRightLine3Color.SelectedIndex = 7;
            CbxRightLine3Color.IsEditable = false;

            CbxMiddleLine1Color.ItemsSource = typeof(Colors).GetProperties();
            CbxMiddleLine1Color.SelectedIndex = 7;
            CbxMiddleLine1Color.IsEditable = false;

            CbxMiddleLine2Color.ItemsSource = typeof(Colors).GetProperties();
            CbxMiddleLine2Color.SelectedIndex = 7;
            CbxMiddleLine2Color.IsEditable = false;

            CbxMiddleLine3Color.ItemsSource = typeof(Colors).GetProperties();
            CbxMiddleLine3Color.SelectedIndex = 7;
            CbxMiddleLine3Color.IsEditable = false;
        }

        private void LeftRadio_Checked(object sender, RoutedEventArgs e)
        {
            choice = WatermarkLayout.Left;
            UpdateDisplay();
        }

        private void RightRadio_Checked(object sender, RoutedEventArgs e)
        {
            choice = WatermarkLayout.Right;
            UpdateDisplay();
        }

        private void RadioMiddle_Checked(object sender, RoutedEventArgs e)
        {
            choice = WatermarkLayout.Middle;
            UpdateDisplay();
        }

        private void BtnSetInputPath_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            string m_Dir = m_Dialog.SelectedPath.Trim();
            this.TxtInputFilePath.Text = m_Dir;
            _fileStatus = new();
            DirectoryInfo folder = new DirectoryInfo(m_Dir);
            FileSystemInfo fileInfo = folder as FileSystemInfo;
            FileSystemInfo[] files = folder.GetFileSystemInfos();
            foreach (FileSystemInfo file in files)
            {
                var t = file as FileInfo;
                string extension = Path.GetExtension(file.Name);
                if (extension.ToUpper() == ".JPG")
                {
                    _fileStatus.Add(file.Name, "Wait");
                }
            }
            LstFileList.ItemsSource = _fileStatus;
        }

        private void UpdateLeftLayout()
        {
            ChkMiddleLine1Enable.IsChecked = false;
            ChkMiddleLine2Enable.IsChecked = false;
            ChkMiddleLine3Enable.IsChecked = false;
            ChkMiddleLine1Enable.IsEnabled = false;
            ChkMiddleLine2Enable.IsEnabled = false;
            ChkMiddleLine3Enable.IsEnabled = false;

            ChkLeftLine1Enable.IsEnabled = true;
            ChkLeftLine2Enable.IsEnabled = true;
            ChkLeftLine3Enable.IsEnabled = true;

            ChkRightLine1Enable.IsEnabled = true;
            ChkRightLine2Enable.IsEnabled = true;
            ChkRightLine3Enable.IsEnabled = true;

        }

        private void UpdateMiddleLayout()
        {
            ChkLeftLine1Enable.IsEnabled = false;
            ChkLeftLine2Enable.IsEnabled = false;
            ChkLeftLine3Enable.IsEnabled = false;
            ChkLeftLine1Enable.IsChecked = false;
            ChkLeftLine2Enable.IsChecked = false;
            ChkLeftLine3Enable.IsChecked = false;

            ChkRightLine1Enable.IsEnabled = false;
            ChkRightLine2Enable.IsEnabled = false;
            ChkRightLine3Enable.IsEnabled = false;
            ChkRightLine1Enable.IsChecked = false;
            ChkRightLine2Enable.IsChecked = false;
            ChkRightLine3Enable.IsChecked = false;

            ChkMiddleLine1Enable.IsEnabled = true;
            ChkMiddleLine2Enable.IsEnabled = true;
            ChkMiddleLine3Enable.IsEnabled = true;
        }

        private void UpdateRightLayout()
        {
            ChkMiddleLine1Enable.IsChecked = false;
            ChkMiddleLine2Enable.IsChecked = false;
            ChkMiddleLine3Enable.IsChecked = false;
            ChkMiddleLine1Enable.IsEnabled = false;
            ChkMiddleLine2Enable.IsEnabled = false;
            ChkMiddleLine3Enable.IsEnabled = false;

            ChkLeftLine1Enable.IsEnabled = true;
            ChkLeftLine2Enable.IsEnabled = true;
            ChkLeftLine3Enable.IsEnabled = true;

            ChkRightLine1Enable.IsEnabled = true;
            ChkRightLine2Enable.IsEnabled = true;
            ChkRightLine3Enable.IsEnabled = true;
        }

        private void UpdateDisplay()
        {
            switch (choice)
            {
                case WatermarkLayout.Left:
                    UpdateLeftLayout();
                    break;
                case WatermarkLayout.Middle:
                    UpdateMiddleLayout();
                    break;
                case WatermarkLayout.Right: 
                    UpdateRightLayout();
                    break;
                case WatermarkLayout.None:
                default:
                    break;
            }
        }

        private void BtnSetOutputPath_OnClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            string m_Dir = m_Dialog.SelectedPath.Trim();
            this.TxtOutputFilePath.Text = m_Dir;
            Trace.WriteLine(m_Dir);
        }

        private void BtnStartStop_Click(object sender, RoutedEventArgs e)
        {
            WatermarkGenerator g = new WatermarkGenerator();
            var config = new WatermarkConfig();

            Color backColor = (Color)((CbxBackColor.SelectedItem as PropertyInfo)?.GetValue(null, null) ?? Color.FromRgb(255,255,255));
            config.BackColor = System.Drawing.Color.FromArgb(backColor.A, backColor.R, backColor.G, backColor.B);
            config.WhiteFill = EnableWhiteBorder.IsChecked??true;
            config.Using35 = Enable35mm.IsChecked??false;
            config.Layout = choice;
            config.ArtistName = TxtArtist.Text;
            config.OverwriteArtistName = false;

            if (choice == WatermarkLayout.Middle)
            {
                var middleInfoList = new List<CharacterConfig>();
                if (ChkLeftLine1Enable.IsChecked is true)
                {
                    var middleInfo = new CharacterConfig();
                    var fontColor = (Color)((CbxMiddleLine1Color.SelectedItem as PropertyInfo)?.GetValue(null, null) ?? Color.FromRgb(0, 0, 0));
                    middleInfo.FontBold = ChkMiddleLine1Bold.IsChecked ?? false;
                    middleInfo.FontColor = System.Drawing.Color.FromArgb(fontColor.A, fontColor.R, fontColor.G, fontColor.B);
                    middleInfo.Format = TextMiddleLine1.Text;
                    middleInfo.FontSize = 122;
                    middleInfo.FontFamily = "a";
                    middleInfoList.Add(middleInfo);
                }

                if (ChkLeftLine2Enable.IsChecked is true)
                {
                    var middleInfo = new CharacterConfig();
                    var fontColor = (Color)((CbxMiddleLine2Color.SelectedItem as PropertyInfo)?.GetValue(null, null) ?? Color.FromRgb(0, 0, 0));
                    middleInfo.FontBold = ChkMiddleLine2Bold.IsChecked ?? false;
                    middleInfo.FontColor = System.Drawing.Color.FromArgb(fontColor.A, fontColor.R, fontColor.G, fontColor.B);
                    middleInfo.Format = TextMiddleLine2.Text;
                    middleInfo.FontSize = 122;
                    middleInfo.FontFamily = "a";
                    middleInfoList.Add(middleInfo);
                }

                if (ChkLeftLine3Enable.IsChecked is true)
                {
                    var middleInfo = new CharacterConfig();
                    var fontColor = (Color)((CbxMiddleLine3Color.SelectedItem as PropertyInfo)?.GetValue(null, null) ?? Color.FromRgb(0, 0, 0));
                    middleInfo.FontBold = ChkMiddleLine3Bold.IsChecked ?? false;
                    middleInfo.FontColor = System.Drawing.Color.FromArgb(fontColor.A, fontColor.R, fontColor.G, fontColor.B);
                    middleInfo.Format = TextMiddleLine3.Text;
                    middleInfo.FontSize = 122;
                    middleInfo.FontFamily = "a";
                    middleInfoList.Add(middleInfo);
                }
                config.MiddleCharacterConfigs= middleInfoList; 
            }
            else
            {
                var leftInfoList = new List<CharacterConfig>();
                if (ChkLeftLine1Enable.IsChecked is true)
                {
                    var leftInfo = new CharacterConfig();
                    var fontColor = (Color)((CbxLeftLine1Color.SelectedItem as PropertyInfo)?.GetValue(null, null) ?? Color.FromRgb(0, 0, 0));
                    leftInfo.FontBold = ChkLeftLine1Bold.IsChecked ?? false;
                    leftInfo.FontColor = System.Drawing.Color.FromArgb(fontColor.A, fontColor.R, fontColor.G, fontColor.B);
                    leftInfo.Format = TextLeftLine1.Text;
                    leftInfo.FontSize = 122;
                    leftInfo.FontFamily = "a";
                    leftInfoList.Add(leftInfo);
                }

                if (ChkLeftLine2Enable.IsChecked is true)
                {
                    var leftInfo = new CharacterConfig();
                    var fontColor = (Color)((CbxLeftLine2Color.SelectedItem as PropertyInfo)?.GetValue(null, null) ?? Color.FromRgb(0, 0, 0));
                    leftInfo.FontBold = ChkLeftLine2Bold.IsChecked ?? false;
                    leftInfo.FontColor = System.Drawing.Color.FromArgb(fontColor.A, fontColor.R, fontColor.G, fontColor.B);
                    leftInfo.Format = TextLeftLine2.Text;
                    leftInfo.FontSize = 122;
                    leftInfo.FontFamily = "a";
                    leftInfoList.Add(leftInfo);
                }

                if (ChkLeftLine3Enable.IsChecked is true)
                {
                    var leftInfo = new CharacterConfig();
                    var fontColor = (Color)((CbxLeftLine3Color.SelectedItem as PropertyInfo)?.GetValue(null, null) ?? Color.FromRgb(0, 0, 0));
                    leftInfo.FontBold = ChkLeftLine3Bold.IsChecked ?? false;
                    leftInfo.FontColor = System.Drawing.Color.FromArgb(fontColor.A, fontColor.R, fontColor.G, fontColor.B);
                    leftInfo.Format = TextLeftLine3.Text;
                    leftInfo.FontSize = 122;
                    leftInfo.FontFamily = "a";
                    leftInfoList.Add(leftInfo);
                }

                var rightInfoList = new List<CharacterConfig>();
                if (ChkRightLine1Enable.IsChecked is true)
                {
                    var rightInfo = new CharacterConfig();
                    var fontColor = (Color)((CbxRightLine1Color.SelectedItem as PropertyInfo)?.GetValue(null, null) ?? Color.FromRgb(0, 0, 0));
                    rightInfo.FontBold = ChkRightLine1Bold.IsChecked ?? false;
                    rightInfo.FontColor = System.Drawing.Color.FromArgb(fontColor.A, fontColor.R, fontColor.G, fontColor.B);
                    rightInfo.Format = TextRightLine1.Text;
                    rightInfo.FontSize = 122;
                    rightInfo.FontFamily = "a";
                    rightInfoList.Add(rightInfo);
                }

                if (ChkRightLine2Enable.IsChecked is true)
                {
                    var rightInfo = new CharacterConfig();
                    var fontColor = (Color)((CbxRightLine2Color.SelectedItem as PropertyInfo)?.GetValue(null, null) ?? Color.FromRgb(0, 0, 0));
                    rightInfo.FontBold = ChkRightLine2Bold.IsChecked ?? false;
                    rightInfo.FontColor = System.Drawing.Color.FromArgb(fontColor.A, fontColor.R, fontColor.G, fontColor.B);
                    rightInfo.Format = TextRightLine2.Text;
                    rightInfo.FontSize = 122;
                    rightInfo.FontFamily = "a";
                    rightInfoList.Add(rightInfo);
                }

                if (ChkRightLine3Enable.IsChecked is true)
                {
                    var rightInfo = new CharacterConfig();
                    var fontColor = (Color)((CbxRightLine3Color.SelectedItem as PropertyInfo)?.GetValue(null, null) ?? Color.FromRgb(0, 0, 0));
                    rightInfo.FontBold = ChkRightLine3Bold.IsChecked ?? false;
                    rightInfo.FontColor = System.Drawing.Color.FromArgb(fontColor.A, fontColor.R, fontColor.G, fontColor.B);
                    rightInfo.Format = TextRightLine3.Text;
                    rightInfo.FontSize = 122;
                    rightInfo.FontFamily = "a";
                    rightInfoList.Add(rightInfo);
                }
                config.LeftCharacterConfigs = leftInfoList;
                config.RightCharacterConfigs = rightInfoList;
            }

            var fileList = new List<string>();
            foreach (var node in _fileStatus)
            {
                fileList.Add(TxtInputFilePath.Text+"\\"+node.Key);
            }

            g.StartGenerateAsync((filename =>
            {
                Trace.WriteLine("All Success!");
            }), (filename, reason) =>
            {
                Trace.WriteLine($@"File {filename} Failed");
            }, (filename, stage, process) =>
            {
                Trace.WriteLine($@"File {filename} {stage}");
                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    _fileStatus[filename] = stage;
                });
            },fileList,config,TxtOutputFilePath.Text);


        }
    }
}

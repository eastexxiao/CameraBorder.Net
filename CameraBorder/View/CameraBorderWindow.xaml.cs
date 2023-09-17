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
using CameraBorder.ViewModel;

namespace CameraBorder
{
    /// <summary>
    /// CameraBorderWindow.xaml 的交互逻辑
    /// </summary>

    public enum LayoutChoice
    {
        Left,
        Right,
        Middle,
    }
    public partial class CameraBorderWindow : Window
    {
        
        private LayoutChoice choice = LayoutChoice.Left;
        public CameraBorderWindow()
        {
            InitializeComponent();
            DataContext = new CameraBorderWindowViewModel();

            this.LeftRadio.IsChecked = true;

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
            choice = LayoutChoice.Left;
        }

        private void RightRadio_Checked(object sender, RoutedEventArgs e)
        {
            choice = LayoutChoice.Right;
        }

        private void RadioMiddle_Checked(object sender, RoutedEventArgs e)
        {
            choice = LayoutChoice.Middle;
        }

    }
}

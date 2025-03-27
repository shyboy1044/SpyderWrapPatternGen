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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a message box!", "Message Box Title", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
      //      Generate_GCode();

            MessageBox.Show($"Save Button Clicked! {Generate_GCode()[2]} dfadfd", "Message Box Title", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void BtnSaveNew_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a message box!", "Message Box Title", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Allow numbers and only one demical point
        private void NumberOnlyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            // Regex: Allow numbers and single demical point
            e.Handled = !Regex.IsMatch(fullText, @"^(\d+\.?\d*)?$");
        }

        // Prevent pasting invalid value
        private void NumberOnlyTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));

                //Regex: Check if past data is a valid double
                if (!Regex.IsMatch(pastedText, @"^(\d+\.?\d*)?$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        
        private string[] Generate_GCode()
        {
            
            int DDiameter = int.Parse(NumMeasuredSize.Text);
            float DDiameterPcg = float.Parse(NumDiameterPcg.Text)/100;
            float CirclePlus = float.Parse(NumCircPlus.Text);

            float TotalKickPcg = float.Parse(NumTotalKick.Text)/100;
            float KickRatio = float.Parse(NumKickRatioPcg.Text)/100;

            float OverWrapPcg = float.Parse(NumOverWrapPcg.Text)/100;
            int WrapPerLayer = int.Parse(NumWrapsPerLayer.Text);
            int TotalLayers = int.Parse(NumTotalLayers.Text);
            /*    
                int DDiameter = 6;
                float DDiameterPcg = 0.91f;
                float CirclePlus = 0.03f;

                float TotalKickPcg = 0.12f;
                float KickRatio = 0.3f;

                float OverWrapPcg = 100;
                int WrapPerLayer = 5;
                int TotalLayers = 8;
              */
              // Ask client About this value!
            float YAxisPcg = 0.98f;

            double XOffSet = DDiameter * 3.1415 * DDiameterPcg * TotalKickPcg;
            double XAxisCircle = (DDiameter * 3.1415 * DDiameterPcg) - XOffSet;
            double YAxisCircle = XAxisCircle * YAxisPcg;
            double YOffSet = XOffSet * KickRatio;

            int LayerCounter = 0;
            int WrapCounter = 1;

            double WrapXAxisTravel, WrapYAxisTravel, KickXAxisTravel, KickYAxisTravel;
            string[] mainWrapGCode = new string[TotalLayers * WrapPerLayer * 2];

            while (LayerCounter <= TotalLayers)
            {
                while (WrapCounter <= WrapPerLayer)
                {
                    WrapXAxisTravel = XAxisCircle + CirclePlus;
                    WrapYAxisTravel = YAxisCircle + CirclePlus;

                    mainWrapGCode[WrapPerLayer * LayerCounter + WrapCounter] = $"X{WrapXAxisTravel:F3}Y{WrapYAxisTravel:F3}";
                    WrapCounter++;

                    if (WrapCounter == WrapPerLayer)
                    {
                        KickXAxisTravel = WrapXAxisTravel + XOffSet + (DDiameter * OverWrapPcg);
                        KickYAxisTravel = WrapYAxisTravel + YOffSet + (DDiameter * OverWrapPcg);
                    }
                    else
                    {
                        KickXAxisTravel = WrapXAxisTravel + XOffSet;
                        KickYAxisTravel = WrapYAxisTravel + YOffSet;
                    }

                    mainWrapGCode[WrapPerLayer * LayerCounter + WrapCounter] = $"X{KickXAxisTravel:F3}Y{KickYAxisTravel:F3}";

                    WrapXAxisTravel = KickXAxisTravel + XAxisCircle + CirclePlus;
                    WrapYAxisTravel = KickYAxisTravel + YAxisCircle + CirclePlus;

                    WrapCounter++;
                }
                WrapCounter = 1;
                LayerCounter++;
            }

            return mainWrapGCode;
        }
        
    }
}



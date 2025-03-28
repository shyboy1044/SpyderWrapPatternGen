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
            CheckInputValidation(NumShellSize); 
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            //         MessageBox.Show($"Save Button Clicked! {Generate_GCode()[2]} dfadfd", "Message Box Title", MessageBoxButton.OK, MessageBoxImage.Information);





            TxtGcodeOutput.Text = "";
            string[] GCode = Generate_GCode();



            string[] Pump = new string[GCode.Length];
            string PumpOnCode = StrPumpOnCode.Text;
            string PumpOffCode = StrPumpOffCode.Text;
            int CycPerShell = int.Parse(NumCyclesPerShell.Text);
            int Duration = int.Parse(NumDuration.Text);

            if (Pump.Length > CycPerShell * Duration)
            {
                int StartPos = Pump.Length / CycPerShell;
                for (int i = 0; i * StartPos < Pump.Length; i++)
                {
                    Pump[i * StartPos] = PumpOnCode;
                    Pump[i * StartPos + Duration - 1] = PumpOffCode;
                }
            }
            else
            {
                for (int i = 0; i < Pump.Length; i += Duration)
                {
                    Pump[i] = PumpOnCode;
                    if (i + Duration - 1 >= Pump.Length)
                        Pump[Pump.Length - 1] = PumpOffCode;
                    else
                        Pump[i + Duration - 1] = PumpOffCode;
                }
            }

            // Add Main part of GCode
            TxtGcodeOutput.Text = "%Startup_GCODE%\n" + TxtStartGcode.Text + "\n";
            for (int i = 0; i < GCode.Length; i ++)
            {
                if (i == 0)
                    TxtGcodeOutput.Text = TxtGcodeOutput.Text + GCode[i] + "\t\tF" + NumWrapFeedRate.Text + "\t\t" + Pump[i] + "\n";
                else
                    TxtGcodeOutput.Text = TxtGcodeOutput.Text + GCode[i] + "\t\t" + Pump[i] + "\n";
            }
            TxtGcodeOutput.Text = TxtGcodeOutput.Text + "%End_of_main_WrapGCODE%\n" + TxtEndMWrap.Text + "\n";

            // Add Completed GCode
            TxtGcodeOutput.Text = TxtGcodeOutput.Text + @"\\Burnish Selection";

            // Here implement completed GCode

            TxtGcodeOutput.Text = TxtGcodeOutput.Text + "\n" + "%End_of_Completed_Wrap%" + "\n" + TxtEndCWrap.Text;
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
            int RowCounter = 0;

            double WrapXAxisTravel, WrapYAxisTravel, KickXAxisTravel, KickYAxisTravel;
            string[] mainWrapGCode = new string[(TotalLayers + 1) * WrapPerLayer * 2];
           
            WrapXAxisTravel = XAxisCircle + CirclePlus;
            WrapYAxisTravel = YAxisCircle + CirclePlus;

            while (LayerCounter <= TotalLayers)
            {
                while (WrapCounter <= WrapPerLayer)
                {

                    mainWrapGCode[RowCounter] = $"X{WrapXAxisTravel:F3}Y{WrapYAxisTravel:F3}";

                    double VarA = XAxisCircle + CirclePlus;
                    double VarB = YAxisCircle + CirclePlus;

                    RowCounter++;

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

                    mainWrapGCode[RowCounter] = $"X{KickXAxisTravel:F3}Y{KickYAxisTravel:F3}";

                    VarA += CirclePlus;
                    VarB += CirclePlus;

                    WrapXAxisTravel = KickXAxisTravel + VarA + CirclePlus;
                    WrapYAxisTravel = KickYAxisTravel + VarA + CirclePlus;

                    RowCounter++;
                    WrapCounter++;
                }
                WrapCounter = 1;
                LayerCounter++;
            }

            return mainWrapGCode;
        }
        
        private void CheckInputValidation(TextBox textbox)
        {
            if (string.IsNullOrWhiteSpace(textbox.Text))
            {
                if (textbox.Parent is Border border)
                    border.BorderBrush = Brushes.Red;
            }
            else
            {
                if (textbox.Parent is Border border)
                    border.BorderBrush = Brushes.Transparent;
            }
        }

        private void Pump_Enabled(object sender, RoutedEventArgs e)
        {
            StrPumpOnCode.IsEnabled = true;
            StrPumpOffCode.IsEnabled = true;
            NumCyclesPerShell.IsEnabled = true;
            NumDuration.IsEnabled = true;
        }

        private void Pump_Disabled(object sender, RoutedEventArgs e)
        {
            StrPumpOnCode.IsEnabled = false;
            StrPumpOffCode.IsEnabled = false;
            NumCyclesPerShell.IsEnabled = false;
            NumDuration.IsEnabled = false;
        }
    }
}



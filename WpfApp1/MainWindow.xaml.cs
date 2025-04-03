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
using System.IO;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Path = System.IO.Path;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        string openedFilePath = string.Empty;
        public MainWindow()
        {
            InitializeComponent();

            StrPumpOffCode.IsEnabled = false;
            StrPumpOnCode.IsEnabled = false;
            NumCyclesPerShell.IsEnabled = false;
            NumDuration.IsEnabled = false;
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "MUM files (*.mum)|*.mum|All files (*.*)|*.*",
                    DefaultExt = ".mum",
                    Title = "Select a MUM File"
                };

                if (openFileDialog.ShowDialog() == true)
                {

                    try
                    {
                        // Check if file exists (additional safety check)
                        if (!File.Exists(openFileDialog.FileName))
                        {
                            MessageBox.Show("The selected file does not exist.");
                            return;
                        }
                        // Read data from file
                        string filePath = openFileDialog.FileName;
                        string fileContent = File.ReadAllText(filePath);
                        dynamic mumContent = JsonConvert.DeserializeObject(fileContent);

                        // Fill Form using Data
                        PopulateFormFromJson(mumContent);
                        openedFilePath = filePath;

                        TxtGcodeOutput.Text = string.Empty;

                        // Set window title as opened file name
                        SetWindowsTitle(openedFilePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reading file: {ex.Message}", "This is Error");
                        return;
                    }
                }
                return;
            }
            catch(Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckParameterValidation() == false)
                {
                    MessageBox.Show("Enter All Parameters", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(StrPatternName.Text))
                {
                    MessageBox.Show("Enter File Name", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                //    GenerateGCode();

                if (openedFilePath.Length == 0)
                {
                    SaveAsNewFile();
                }
                else
                {
                    string savingData = CollectFormData();
                    File.WriteAllText(openedFilePath, savingData);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void BtnSaveNew_Click(object sender, RoutedEventArgs e)
        {
            if (CheckParameterValidation() == false)
            {
                MessageBox.Show("Enter All Parameters", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(StrPatternName.Text))
            {
                MessageBox.Show("Enter File Name", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            SaveAsNewFile();
        }

        private void SaveAsNewFile()
        {
            try
            {
                string savingData = CollectFormData();

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "MUM files (*.mum)|*.mum|All files (*.*)|*.*",
                    DefaultExt = "mum",
                    FileName = $"{StrPatternName.Text}.mum"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, savingData);
                    openedFilePath = saveFileDialog.FileName;

                    SetWindowsTitle(openedFilePath);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void SetWindowsTitle(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            this.Title = fileName + " - Spyder Pattern Generator";
        }
        private void BtnGenGCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NumCyclesPerShell.Text == "0")
                {
                    MessageBox.Show("Cycles per shell must be greater than zero to proceed");
                    return;
                }
                if (CheckParameterValidation() == false)
                {
                    MessageBox.Show("Enter All Parameters", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(StrPatternName.Text))
                {
                    MessageBox.Show("Enter File Name", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                TxtGcodeOutput.Text = "";

                string[] GCode = Generate_GCode(out double EstTapeFeet);

                if (ValidatePumpCodeParameter() == false)
                {
                    MessageBox.Show("Enter All Pump Parameters", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                string[] pumpCode = GetPumpCode(GCode.Length);

                // Add Main part of GCode
                TxtGcodeOutput.Text = "%Startup_GCODE%\n" + TxtStartGcode.Text + "\n";
                for (int i = 0; i < GCode.Length; i++)
                {
                    if (i == 0)
                        TxtGcodeOutput.Text = TxtGcodeOutput.Text + GCode[i] + "\t\tF" + NumWrapFeedRate.Text + " " + pumpCode[i] + "\n";
                    else
                        TxtGcodeOutput.Text = TxtGcodeOutput.Text + GCode[i] + "\t\t" + pumpCode[i] + "\n";
                }
                TxtGcodeOutput.Text = TxtGcodeOutput.Text + "%End_of_main_WrapGCODE%\n" + TxtEndMWrap.Text + "\n";

                // Add Completed GCode
                TxtGcodeOutput.Text = TxtGcodeOutput.Text + @"\\Burnish Selection" + "\n";

                int startSpeed = int.Parse(NumBurStartSpeed.Text);
                int finalSpeed = int.Parse(NumBurFinalSpeed.Text);
                int rampStep = int.Parse(NumBurRampSteps.Text);
                int[] burnishSpeed = GenerateBurnishSpeed(startSpeed, finalSpeed, rampStep);

                for (int i = 0; i < GCode.Length; i++)
                {
                    if (IsEven(i) && (i < burnishSpeed.Length * 2))
                        TxtGcodeOutput.Text = TxtGcodeOutput.Text + GCode[i] + "\tF" + burnishSpeed[i / 2] + "\n";
                    else
                        TxtGcodeOutput.Text = TxtGcodeOutput.Text + GCode[i] + "\n";
                }
                // Here implement completed GCode

                TxtGcodeOutput.Text = TxtGcodeOutput.Text + "%End_of_Completed_Wrap%" + "\n" + TxtEndCWrap.Text;

                NumTotalEstFeet.Text = Math.Round(EstTapeFeet, 2).ToString();

            }
            catch(Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void BtnExportGCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtGcodeOutput.Text))
                {
                    MessageBox.Show("Generate GCode First");
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"{StrPatternName.Text}.txt"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, TxtGcodeOutput.Text);
                    openedFilePath = saveFileDialog.FileName;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
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
        
        private string[] Generate_GCode(out double EstTapeFeet)
        {
            
            float DDiameter = float.Parse(NumMeasuredSize.Text);
            float DDiameterPcg = float.Parse(NumDiameterPcg.Text)/100;
            float CirclePlus = float.Parse(NumCircPlus.Text);

            float TotalKickPcg = float.Parse(NumTotalKick.Text)/100;
            float KickRatio = float.Parse(NumKickRatioPcg.Text)/100;

            float OverWrapPcg = float.Parse(NumOverWrapPcg.Text)/100;
            int WrapPerLayer = int.Parse(NumWrapsPerLayer.Text);
            int TotalLayers = int.Parse(NumTotalLayers.Text);

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

            EstTapeFeet = 0;
            while (LayerCounter <= TotalLayers)
            {
                while (WrapCounter <= WrapPerLayer)
                {

                    EstTapeFeet = ((WrapXAxisTravel + WrapYAxisTravel) / 2) / 12; 
                    mainWrapGCode[RowCounter] = $"X{WrapXAxisTravel:F3}Y{WrapYAxisTravel:F3}";

                    double VarA = XAxisCircle + CirclePlus;
                    double VarB = YAxisCircle + CirclePlus;

                    RowCounter++;

                    if (WrapCounter == WrapPerLayer)
                    {
                        KickXAxisTravel = WrapXAxisTravel + XOffSet + (DDiameter * OverWrapPcg);
                        KickYAxisTravel = WrapYAxisTravel + YOffSet + (DDiameter * OverWrapPcg);

                        EstTapeFeet = ((KickXAxisTravel + KickYAxisTravel) / 2) / 12;
                    }
                    else
                    {
                        KickXAxisTravel = WrapXAxisTravel + XOffSet;
                        KickYAxisTravel = WrapYAxisTravel + YOffSet;

                        EstTapeFeet = ((KickXAxisTravel + KickYAxisTravel) / 2) / 12;
                    }

                    mainWrapGCode[RowCounter] = $"X{KickXAxisTravel:F3}Y{KickYAxisTravel:F3}";

                    VarA += CirclePlus;
                    VarB += CirclePlus;

                    WrapXAxisTravel = KickXAxisTravel + VarA + CirclePlus;
                    WrapYAxisTravel = KickYAxisTravel + VarA + CirclePlus;
                    
           //         if ((LayerCounter < TotalLayers) && (WrapCounter < WrapPerLayer))
            //            EstTapeFeet = ((WrapXAxisTravel + WrapYAxisTravel) / 2) / 12;

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

        private bool CheckParameterValidation()
        {
            if (!string.IsNullOrWhiteSpace(NumMeasuredSize.Text) )
                if (!string.IsNullOrWhiteSpace(NumDiameterPcg.Text))
                    if (!string.IsNullOrWhiteSpace(NumCircPlus.Text))
                        if (!string.IsNullOrWhiteSpace(NumTotalKick.Text))
                            if (!string.IsNullOrWhiteSpace(NumKickRatioPcg.Text))
                                if (!string.IsNullOrWhiteSpace(NumWrapFeedRate.Text))
                                    if (!string.IsNullOrWhiteSpace(NumOverWrapPcg.Text))
                                        if (!string.IsNullOrWhiteSpace(NumTotalLayers.Text))
                                            if (!string.IsNullOrWhiteSpace(NumWrapsPerLayer.Text))
                                                if (!string.IsNullOrWhiteSpace(TxtStartGcode.Text))
                                                    if (!string.IsNullOrWhiteSpace(TxtEndMWrap.Text))
                                                        if (!string.IsNullOrWhiteSpace(TxtEndCWrap.Text))
                                                            if (!string.IsNullOrWhiteSpace(NumShellSize.Text))
                                                                return true;
            return false;
        }

        private string[] GetPumpCode(int codeLength)
        {
            string[] Pump = new string[codeLength];
            if (pumpState.IsChecked == true)
            {
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
            }
            return Pump;
        }

        private bool ValidatePumpCodeParameter()
        {
            if (pumpState.IsChecked == true)    
            {
                string PumpOnCode = StrPumpOnCode.Text;
                string PumpOffCode = StrPumpOffCode.Text;
                string CycPerShell = NumCyclesPerShell.Text;
                string Duration = NumDuration.Text;
                if ((!string.IsNullOrWhiteSpace(PumpOnCode) && !string.IsNullOrWhiteSpace(PumpOffCode)
                    && !string.IsNullOrWhiteSpace(CycPerShell) && !string.IsNullOrWhiteSpace(Duration)) == true)
                    return true;
                else
                    return false;
            }
            return true;
        }

        private string CollectFormData()
        {
            var data = new
            {
                Pattern_Name = StrPatternName.Text,
                Shell_Description = StrShellDescription.Text,
                Shell_Size = NumShellSize.Text,
                Measured_Size = NumMeasuredSize.Text,
                Diameter_Percentage = NumDiameterPcg.Text,
                Circ_Plus = NumCircPlus.Text,
                Total_Kick = NumTotalKick.Text,
                Kick_Ratio = NumKickRatioPcg.Text,

                Wrap_Feedrate = NumWrapFeedRate.Text,
                Overwrap_Percentage = NumOverWrapPcg.Text,
                Total_Layers = NumTotalLayers.Text,
                Wraps_Per_Layer = NumWrapsPerLayer.Text,
               
                Burnish_Start_Speed = NumBurStartSpeed.Text,
                Burnish_Ramp_Steps = NumBurRampSteps.Text,
                Burnish_Final_Steps = NumBurFinalSpeed.Text,

                Startup_Gcode = TxtStartGcode.Text,
                End_Of_Main_Wrap = TxtEndMWrap.Text,
                End_Of_Complete_Wrap = TxtEndCWrap.Text,

                Total_Estimated_Feet = NumTotalEstFeet.Text,

                Pump_Enable = pumpState.IsChecked,
                Pump_On_Code = StrPumpOnCode.Text,
                Pump_Off_Code = StrPumpOffCode.Text,
                Pump_Cycles = NumCyclesPerShell.Text,
                Pump_Duration = NumDuration.Text
            };

            // Convert to String
            string formData = JsonConvert.SerializeObject(data, Formatting.Indented);

            return formData;
        }
        
        private void PopulateFormFromJson(dynamic mumContent)
        {
            StrPatternName.Text = mumContent.Pattern_Name;
            StrShellDescription.Text = mumContent.Shell_Description;
            NumShellSize.Text = mumContent.Shell_Size;
            NumMeasuredSize.Text = mumContent.Measured_Size;
            NumDiameterPcg.Text = mumContent.Diameter_Percentage;
            NumCircPlus.Text = mumContent.Circ_Plus;
            NumTotalKick.Text = mumContent.Total_Kick;
            NumKickRatioPcg.Text = mumContent.Kick_Ratio;
            NumWrapFeedRate.Text = mumContent.Wrap_Feedrate;
            NumOverWrapPcg.Text = mumContent.Overwrap_Percentage;
            NumTotalLayers.Text = mumContent.Total_Layers;
            NumWrapsPerLayer.Text = mumContent.Wraps_Per_Layer;
            NumBurStartSpeed.Text = mumContent.Burnish_Start_Speed;
            NumBurRampSteps.Text = mumContent.Burnish_Ramp_Steps;
            NumBurFinalSpeed.Text = mumContent.Burnish_Final_Steps;
            TxtStartGcode.Text = mumContent.Startup_Gcode;
            TxtEndMWrap.Text = mumContent.End_Of_Main_Wrap;
            TxtEndCWrap.Text = mumContent.End_Of_Complete_Wrap;
            NumTotalEstFeet.Text = mumContent.Total_Estimated_Feet;
            pumpState.IsChecked = mumContent.Pump_Enable;
            StrPumpOnCode.Text = mumContent.Pump_On_Code;
            StrPumpOffCode.Text = mumContent.Pump_Off_Code;
            NumCyclesPerShell.Text = mumContent.Pump_Cycles;
            NumDuration.Text = mumContent.Pump_Duration;
        }

        private static int RoundToNearest25(double inputValue)
        {
            int value = (int)Math.Round(inputValue);
            int remainder = value % 25;

            if (remainder == 0)
            {
                // Already a multiple of 25
                return value;
            }
            else if (remainder < 13)
            {
                // Round down
                return value - remainder;
            }
            else
            {
                // Round up
                return value + (25 - remainder);
            }
        }

        private int[] GenerateBurnishSpeed(int startSpeed, int finalSpeed, int rampSteps)
        {
            try
            {
                double dblRampSpeed = (finalSpeed - startSpeed) / (rampSteps - 1);
                int rampSpeed = RoundToNearest25(dblRampSpeed);
                int[] burnishSpeed = new int[rampSteps];

                burnishSpeed[0] = startSpeed;
                for (int i = 1; i < rampSteps; i++)
                {
                    if (i == 1)
                    {
                        if (finalSpeed - startSpeed >= 25)
                            burnishSpeed[1] = RoundToNearest25(burnishSpeed[0] + 13 + rampSpeed);
                        else
                            burnishSpeed[1] = startSpeed;
                    }
                    else if (i != 1)
                    {
                        burnishSpeed[i] = burnishSpeed[i - 1] + rampSpeed;
                        if (burnishSpeed[i] > finalSpeed)
                            burnishSpeed[i] = finalSpeed;
                    }

                    if (i == rampSteps - 1)
                        burnishSpeed[i] = finalSpeed;
                }
                return burnishSpeed;
            }
            catch
            {
                return new int[] { startSpeed, finalSpeed };
            }
        }

        private bool IsEven(int number)
        {
            if (number % 2 == 0)
                return true;
            return false;
        }

    }
}



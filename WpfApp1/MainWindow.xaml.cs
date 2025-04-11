using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
            Loaded += MainWindow_Loaded;

            StrPumpOffCode.IsEnabled = false;
            StrPumpOnCode.IsEnabled = false;
            NumCyclesPerShell.IsEnabled = false;
            NumDuration.IsEnabled = false;
        }

        private void MainWindow_Loaded(Object sender, RoutedEventArgs e)
        {
            // Get the screen size
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            if ((screenWidth < 1000) || (screenHeight < 700))
            {
                this.Width = 800;
                this.Height = 570;
            }
            else
            {
                this.Width = 1024;
                this.Height = 700;
            }

            // Get the screen working area
            var workingArea = SystemParameters.WorkArea;

            // Calculate center position
            this.Left = (workingArea.Width - this.ActualWidth) / 2 + workingArea.Left;
            this.Top = (workingArea.Height - this.ActualHeight) / 2 + workingArea.Top;
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
                    MessageBox.Show("Please ensure all required parameters are entered before generating.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(StrPatternName.Text))
                {
                    MessageBox.Show("Please enter a valid Pattern Name before saving the file.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (IsIncludeExtension(StrPatternName.Text) == "non")
                {
                    MessageBox.Show(
                    "The Pattern Name must end with one of the following valid extensions: " +
                    "\".tap\", \".gco\", or \".gcode\".\n\n" +
                    "Please update the Pattern Name and try again.",
                    "Invalid Pattern Name",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
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
                MessageBox.Show("Please ensure all required parameters are entered before generating.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(StrPatternName.Text))
            {
                MessageBox.Show("Please enter a valid Pattern Name before saving the file.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (IsIncludeExtension(StrPatternName.Text) == "non")
            {
                MessageBox.Show(
                "The Pattern Name must end with one of the following valid extensions: " +
                "\".tap\", \".gco\", or \".gcode\".\n\n" +
                "Please update the Pattern Name and try again.",
                "Invalid Pattern Name",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
                );
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
                    FileName = $"{GetFileNameWithoutExtension(StrPatternName.Text)}.mum"
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
                    MessageBox.Show("Cycles per shell must be greater than zero to proceed", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (CheckParameterValidation() == false)
                {
                    MessageBox.Show("Please ensure all required parameters are entered before generating.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(StrPatternName.Text))
                {
                    MessageBox.Show("Please enter a valid Pattern Name before saving the file.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (IsIncludeExtension(StrPatternName.Text) == "non")
                {
                    MessageBox.Show(
                    "The Pattern Name must end with one of the following valid extensions: " +
                    "\".tap\", \".gco\", or \".gcode\".\n\n" +
                    "Please update the Pattern Name and try again.",
                    "Invalid Pattern Name",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
                    return;
                }

                TxtGcodeOutput.Text = "";

                string[] GCode = Generate_GCode(out double EstTapeFeet);

                if (ValidatePumpCodeParameter() == false)
                {
                    MessageBox.Show("Please ensure all Pump Parameters are entered before proceeding.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                string[] pumpCode = GetPumpCode(GCode.Length);

                // Add Main part of GCode
 //               TxtGcodeOutput.Text = "%Startup_GCODE%\n" + TxtStartGcode.Text + "\n";
                TxtGcodeOutput.Text = "%Startup_GCode%\n";
                for (int i = 0; i < GCode.Length; i++)
                {
                    if (i == 0)
                        TxtGcodeOutput.Text = TxtGcodeOutput.Text + GCode[i] + "  F" + NumWrapFeedRate.Text + " " + pumpCode[i] + "\n";
                    else
                        TxtGcodeOutput.Text = TxtGcodeOutput.Text + GCode[i] + "  " + pumpCode[i] + "\n";
                }
  //              TxtGcodeOutput.Text = TxtGcodeOutput.Text + "%End_of_main_WrapGCODE%\n" + TxtEndMWrap.Text + "\n";
                TxtGcodeOutput.Text = TxtGcodeOutput.Text + "%End_of_Main_Wrap%\n";

                // Add Completed GCode
                TxtGcodeOutput.Text = TxtGcodeOutput.Text + @"\\Burnish Selection" + "\n";

                if (string.IsNullOrWhiteSpace(NumBurStartSpeed.Text) || string.IsNullOrWhiteSpace(NumBurFinalSpeed.Text) || string.IsNullOrWhiteSpace(NumBurRampSteps.Text))
                {
                    float burLayerPcg = float.Parse(NumBurLayerPcg.Text) / 100;
                    for (int i = 0; i < Math.Floor(GCode.Length * burLayerPcg); i++)
                    {
                        TxtGcodeOutput.Text = TxtGcodeOutput.Text + GCode[i] + "\n";
                    }
                }
                else
                {
                    float burLayerPcg = float.Parse(NumBurLayerPcg.Text) / 100;
                    int startSpeed = int.Parse(NumBurStartSpeed.Text);
                    int finalSpeed = int.Parse(NumBurFinalSpeed.Text);
                    int rampStep = int.Parse(NumBurRampSteps.Text);
                    int[] burnishSpeed = GenerateBurnishSpeed(startSpeed, finalSpeed, rampStep);

                    for (int i = 0; i < Math.Floor(GCode.Length * burLayerPcg); i++)
                    {
                        if (IsEven(i) && (i < burnishSpeed.Length * 2))
                            TxtGcodeOutput.Text = TxtGcodeOutput.Text + GCode[i] + "  F" + burnishSpeed[i / 2] + "\n";
                        else
                            TxtGcodeOutput.Text = TxtGcodeOutput.Text + GCode[i] + "\n";
                    }
                }

                // Here implement completed GCode

                //               TxtGcodeOutput.Text = TxtGcodeOutput.Text + "%End_of_Completed_Wrap%" + "\n" + TxtEndCWrap.Text;
                TxtGcodeOutput.Text = TxtGcodeOutput.Text + "%End_of_Complete_Wrap%" + "\n";

                NumTotalEstFeet.Text = Math.Round(EstTapeFeet, 2).ToString();

            }
            catch(Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private string IsIncludeExtension(string filename)
        {
            if (filename.EndsWith(".tap"))
                return "tap";
            else if (filename.EndsWith(".gco"))
                return "gco";
            else if (filename.EndsWith(".gcode"))
                return "gcode";
            return "non";
        }

        private string GetFileNameWithoutExtension(string fullFilename)
        {
            if (fullFilename.EndsWith(".tap"))
                return fullFilename.Replace(".tap", "");
            else if (fullFilename.EndsWith(".gco"))
                return fullFilename.Replace(".gco", "");
            else if (fullFilename.EndsWith(".gcode"))
                return fullFilename.Replace(".gcode", "");
            return fullFilename;
        }

        private void BtnExportGCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtGcodeOutput.Text))
                {
                    MessageBox.Show(
                     "Please generate the GCode before proceeding.", // Message text
                    "GCode Not Generated",                          // Caption (title of the message box)
                    MessageBoxButton.OK,                           // Button(s) to display
                    MessageBoxImage.Information                    // Icon to display
                    );
                    return;
                }

                if (IsIncludeExtension(StrPatternName.Text) == "non")
                {
                    MessageBox.Show(
                    "The Pattern Name must end with one of the following valid extensions: " +
                    "\".tap\", \".gco\", or \".gcode\".\n\n" +
                    "Please update the Pattern Name and try again.",
                    "Invalid Pattern Name",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                    );
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "gcode files (*.gcode)|*.gcode|gco files (*.gco)|*.gco|tap files (*.tap)|*.tap|All files (*.*)|*.*",
                    //         DefaultExt = (IsIncludeExtension(StrPatternName.Text) == "non") ? "gco" : IsIncludeExtension(StrPatternName.Text),
                    DefaultExt = "gco",
                    FileName = (IsIncludeExtension(StrPatternName.Text) == "non") ? $"{StrPatternName.Text}.gco" : StrPatternName.Text
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string replacedOutput = ReplaceVariables(TxtGcodeOutput.Text);
                    string finalOutput = ReplaceVariables(replacedOutput);
                    File.WriteAllText(saveFileDialog.FileName, finalOutput);
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

        // Allow integers and not demical and string
        private void IntegerOnlyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = !Regex.IsMatch(fullText, @"^[0-9]+$");
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
            float YAxisPcg = float.Parse(NumYAixsPcg.Text) / 100;

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
                                                                if (!string.IsNullOrWhiteSpace(NumYAixsPcg.Text))
                                                                    if (!string.IsNullOrWhiteSpace(NumBurLayerPcg.Text))
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
                    for (int i = 0; i * StartPos + Duration - 1 < Pump.Length; i++)
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
                YAixs_Percentage = NumYAixsPcg.Text,
                Total_Kick = NumTotalKick.Text,
                Kick_Ratio = NumKickRatioPcg.Text,

                Wrap_Feedrate = NumWrapFeedRate.Text,
                Overwrap_Percentage = NumOverWrapPcg.Text,
                Total_Layers = NumTotalLayers.Text,
                Wraps_Per_Layer = NumWrapsPerLayer.Text,

                Burnish_Layer_Percentage = NumBurLayerPcg.Text,
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
            NumYAixsPcg.Text = mumContent.YAixs_Percentage;
            NumTotalKick.Text = mumContent.Total_Kick;
            NumKickRatioPcg.Text = mumContent.Kick_Ratio;
            NumWrapFeedRate.Text = mumContent.Wrap_Feedrate;
            NumOverWrapPcg.Text = mumContent.Overwrap_Percentage;
            NumTotalLayers.Text = mumContent.Total_Layers;
            NumWrapsPerLayer.Text = mumContent.Wraps_Per_Layer;
            NumBurLayerPcg.Text = mumContent.Burnish_Layer_Percentage;
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

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height <= 550)
                return;
            // Get the new height of the window
            double newWindowHeight = e.NewSize.Height;

            // Calculate the new height for the TextBox 
            double textBoxHeight = newWindowHeight - 435;
            double GcodeBoxHight = newWindowHeight - 520;

            // Apply the new height to the TextBox
            TxtGcodeOutput.Height = textBoxHeight;
            TxtStartGcode.Height = GcodeBoxHight;
            TxtEndCWrap.Height = GcodeBoxHight;
            TxtEndMWrap.Height = GcodeBoxHight;
  //          TxtReloadCommand.Height = GcodeBoxHight;
        }

        private string ReplaceVariables(string template)
        {
            // Find all %Variable% patterns
            Regex regex = new Regex(@"%([^\s%]+)%");

            // Replace each match with its corresponding value
            return regex.Replace(template, match =>
            {
                string variableName = match.Groups[1].Value;

                Dictionary<string, string> variables = new Dictionary<string, string>();

                variables["Startup_GCode"] = TxtStartGcode.Text;
                //     variables["Pattern_Name"] = (Path.GetFileName(openedFilePath).Length == 0) ? StrPatternName.Text : Path.GetFileName(openedFilePath);
                variables["Pattern_Name"] = (Path.GetFileName(openedFilePath).Length == 0) ? StrPatternName.Text : Path.GetFileName(openedFilePath);
                variables["End_of_Main_Wrap"] = TxtEndMWrap.Text;
                variables["End_of_Complete_Wrap"] = TxtEndCWrap.Text;

                // Check if the variable exists in our dictionary   
                if (variables.TryGetValue(variableName, out string value))
                {
                    return value;
                }

                // If variable doesn't exist, leave it as is (or you could return an empty string)
                return match.Value;
            });
        }
    }
}



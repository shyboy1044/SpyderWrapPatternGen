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
using System.Text;
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
        private Window gCodeEditorWindow;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            StrPumpOffCode.IsEnabled = false;
            StrPumpOnCode.IsEnabled = false;
            NumCyclesPerShell.IsEnabled = false;
            NumDuration.IsEnabled = false;




            // Initialize GCode output preview with Empty string
            TxtGcodeOutput.Text = "";

            // Set initial state for collapse toggle
            CollapseGCodeToggle.IsChecked = false;
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
                this.Height = 605;
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
            this.Title = fileName + " - Spyder Pattern Generator v1.2.1";
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
                /*
                TxtGcodeOutput.Text = "";

                string[] GCode = Generate_GCode(out double EstTapeFeet);

                if (ValidatePumpCodeParameter() == false)
                {
                    MessageBox.Show("Please ensure all Pump Parameters are entered before proceeding.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                string[] pumpCode = GetPumpCode(GCode.Length);

                // Add Main part of GCode
                TxtGcodeOutput.Text = "%Startup_GCode%\n" + ReplaceVariables(TxtStartGcode.Text) + "\n";
  //              TxtGcodeOutput.Text = "%Startup_GCode%\n";
                for (int i = 0; i < GCode.Length; i++)
                {
                    if (i == 0)
                        TxtGcodeOutput.Text = TxtGcodeOutput.Text + GCode[i] + "  F" + NumWrapFeedRate.Text + " " + pumpCode[i] + "\n";
                    else
                        TxtGcodeOutput.Text = TxtGcodeOutput.Text + GCode[i] + "  " + pumpCode[i] + "\n";
                }
                TxtGcodeOutput.Text = TxtGcodeOutput.Text + "%End_of_Main_Wrap%\n" + ReplaceVariables(TxtEndMWrap.Text) + "\n";
       //         TxtGcodeOutput.Text = TxtGcodeOutput.Text + "%End_of_Main_Wrap%\n";

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

                        TxtGcodeOutput.Text = TxtGcodeOutput.Text + "%End_of_Completed_Wrap%" + "\n" + ReplaceVariables(TxtEndCWrap.Text);
        //        TxtGcodeOutput.Text = TxtGcodeOutput.Text + "%End_of_Complete_Wrap%" + "\n";

                NumTotalEstFeet.Text = Math.Round(EstTapeFeet, 2).ToString();
                */

                
                TxtGcodeOutput.Text = "";
                string[] GCode = Generate_GCode(out double EstTapeFeet);
                if (!ValidatePumpCodeParameter())
                {
                    MessageBox.Show("Please ensure all Pump Parameters are entered before proceeding.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                string[] pumpCode = GetPumpCode(GCode.Length);

                // Use StringBuilder instead of string concatenation for better performance
                StringBuilder gcodeBuilder = new StringBuilder();

                // Add Startup GCode
                gcodeBuilder.AppendLine(ReplaceVariables(TxtStartGcode.Text));

                // Add Main part of GCode
                string feedRate = "F" + NumWrapFeedRate.Text;
                for (int i = 0; i < GCode.Length; i++)
                {
                    gcodeBuilder.Append(GCode[i]);

                    // Only add feed rate to first line
                    if (i == 0)
                        gcodeBuilder.Append("  ").Append(feedRate).Append(" ");
                    else
                        gcodeBuilder.Append("  ");

                    gcodeBuilder.AppendLine(pumpCode[i]);
                }

                // Add End of Main Wrap
                gcodeBuilder.AppendLine(ReplaceVariables(TxtEndMWrap.Text));

                // Add Burnish Selection
                gcodeBuilder.AppendLine(@"\\Burnish Selection");

                // Calculate burnish layer percentage once
                float burLayerPcg = float.Parse(NumBurLayerPcg.Text) / 100;
                int burnishLayerCount = (int)Math.Floor(GCode.Length * burLayerPcg);

                bool hasBurnishParams = !string.IsNullOrWhiteSpace(NumBurStartSpeed.Text) &&
                                        !string.IsNullOrWhiteSpace(NumBurFinalSpeed.Text) &&
                                        !string.IsNullOrWhiteSpace(NumBurRampSteps.Text);

                if (!hasBurnishParams)
                {
                    // Simple burnish without speed ramping
                    for (int i = 0; i < burnishLayerCount; i++)
                    {
                        gcodeBuilder.AppendLine(GCode[i]);
                    }
                }
                else
                {
                    // Burnish with speed ramping
                    int startSpeed = int.Parse(NumBurStartSpeed.Text);
                    int finalSpeed = int.Parse(NumBurFinalSpeed.Text);
                    int rampStep = int.Parse(NumBurRampSteps.Text);
                    int[] burnishSpeed = GenerateBurnishSpeed(startSpeed, finalSpeed, rampStep);

                    for (int i = 0; i < burnishLayerCount; i++)
                    {
                        if (IsEven(i) && (i < burnishSpeed.Length * 2))
                        {
                            gcodeBuilder.Append(GCode[i]).Append("  F").AppendLine(burnishSpeed[i / 2].ToString());
                        }
                        else
                        {
                            gcodeBuilder.AppendLine(GCode[i]);
                        }
                    }
                }

                // Add End of Completed Wrap
                gcodeBuilder.Append(ReplaceVariables(TxtEndCWrap.Text));

                // Set the final output text at once
                TxtGcodeOutput.Text = gcodeBuilder.ToString();

                // Update estimated feet display
                NumTotalEstFeet.Content = Math.Round(EstTapeFeet, 2).ToString();
                


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


//                    ShowModernMessageBox("Information", "This is a modern message box!");
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
            float YAxisPcg = float.Parse(NumYAixsPcg.Text);
     //       float YAxisPcg = float.Parse(NumYAixsPcg.Text) / 100;

            double XOffSet = DDiameter * Math.PI * DDiameterPcg * TotalKickPcg;
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

                Total_Estimated_Feet = NumTotalEstFeet.Content,

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
            NumTotalEstFeet.Content = mumContent.Total_Estimated_Feet;
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
    //        if (e.NewSize.Height <= 550)
    //            return;
            // Get the new height of the window
            double newWindowHeight = e.NewSize.Height;
           
            // Apply the new height to the TextBox
  //          TxtGcodeOutput.Height = textBoxHeight;

            if (e.NewSize.Height >= 600)
                TxtGcodeOutput.Height = 110;
            else
                TxtGcodeOutput.Height = newWindowHeight - 485;
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
                variables["startup_gcode"] = TxtStartGcode.Text;
                variables["Startup_Gcode"] = TxtStartGcode.Text;
                variables["Startup_GCODE"] = TxtStartGcode.Text;
                variables["Startup_gcode"] = TxtStartGcode.Text;
                variables["startup_GCode"] = TxtStartGcode.Text;
                variables["startup_Gcode"] = TxtStartGcode.Text;

                /*
                variables["Pattern_Name"] = (Path.GetFileName(openedFilePath).Length == 0) ? StrPatternName.Text : Path.GetFileName(openedFilePath);
                variables["pattern_name"] = (Path.GetFileName(openedFilePath).Length == 0) ? StrPatternName.Text : Path.GetFileName(openedFilePath);
                variables["Pattern_name"] = (Path.GetFileName(openedFilePath).Length == 0) ? StrPatternName.Text : Path.GetFileName(openedFilePath);
                variables["pattern_Name"] = (Path.GetFileName(openedFilePath).Length == 0) ? StrPatternName.Text : Path.GetFileName(openedFilePath);
                */
                variables["Pattern_Name"] = StrPatternName.Text;

                variables["End_of_Main_Wrap"] = TxtEndMWrap.Text;
                variables["end_of_main_wrap"] = TxtEndMWrap.Text;
                variables["End_Of_Main_Wrap"] = TxtEndMWrap.Text;

                variables["End_of_Complete_Wrap"] = TxtEndCWrap.Text;
                variables["End_Of_Complete_Wrap"] = TxtEndCWrap.Text;
                variables["end_of_complete_wrap"] = TxtEndCWrap.Text;

                // Check if the variable exists in our dictionary   
                if (variables.TryGetValue(variableName, out string value))
                {
                    return value;
                }

                // If variable doesn't exist, leave it as is (or you could return an empty string)
                return match.Value;
            });
        }








        /*
        private void GCodeSection_Click(object sender, RoutedEventArgs e)
        {
            if (CollapseGCodeToggle.IsChecked == true)
            {
                CollapseGCodeToggle_Unchecked(sender, e);
        //        CollapseGCodeToggle.IsChecked = false;
            }
            else
            {
                CollapseGCodeToggle_Checked(sender, e);
          //      CollapseGCodeToggle.IsChecked = true;
            }
        }
        */
        private void CollapseGCodeToggle_Checked(object sender, RoutedEventArgs e)
        {
            // Show GCode section
            GCodeEditorSection.Visibility = Visibility.Visible;
            CollapseGCodeToggle.Content = "▲";
        }

        private void CollapseGCodeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            // Hide GCode section
            GCodeEditorSection.Visibility = Visibility.Collapsed;
            CollapseGCodeToggle.Content = "▼";
        }

        private void ExpandGCodeButton_Click(object sender, RoutedEventArgs e)
        {
            // Create and show the GCode editor dialog
            ShowGCodeEditorWindow();
        }



        /*
        private void ShowGCodeEditorWindow()
        {
            // Close existing dialog if open
            if (gCodeEditorWindow != null && gCodeEditorWindow.IsVisible)
            {
                gCodeEditorWindow.Close();
            }

            // Create a new dialog window
            gCodeEditorWindow = new Window
            {
                Title = "Machine G-Code Variables",
                Width = 900,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.CanResize,
                Background = new SolidColorBrush(Colors.WhiteSmoke)
            };

            // Create the content
            Grid mainGrid = new Grid
            {
                Margin = new Thickness(20)
            };

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // GCode editors grid
            Grid editorsGrid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 15)
            };

            editorsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            editorsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            editorsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            editorsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            editorsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Create and add the labels
            TextBlock startupLabel = new TextBlock
            {
                Text = "Startup GCode",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666")),
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(startupLabel, 0);
            Grid.SetColumn(startupLabel, 0);

            TextBlock mainWrapLabel = new TextBlock
            {
                Text = "End of Main Wrap",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666")),
                Margin = new Thickness(5, 0, 5, 5)
            };
            Grid.SetRow(mainWrapLabel, 0);
            Grid.SetColumn(mainWrapLabel, 1);

            TextBlock completeWrapLabel = new TextBlock
            {
                Text = "End of Complete Wrap",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666")),
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(completeWrapLabel, 0);
            Grid.SetColumn(completeWrapLabel, 2);

            // Create and add the text boxes
            TextBox startupTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Margin = new Thickness(0, 0, 5, 0),
                Padding = new Thickness(8),
                Text = TxtStartGcode.Text
            };
            Grid.SetRow(startupTextBox, 1);
            Grid.SetColumn(startupTextBox, 0);

            TextBox mainWrapTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Margin = new Thickness(5, 0, 5, 0),
                Padding = new Thickness(8),
                Text = TxtEndMWrap.Text
            };
            Grid.SetRow(mainWrapTextBox, 1);
            Grid.SetColumn(mainWrapTextBox, 1);

            TextBox completeWrapTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Margin = new Thickness(5, 0, 0, 0),
                Padding = new Thickness(8),
                Text = TxtEndCWrap.Text
            };
            Grid.SetRow(completeWrapTextBox, 1);
            Grid.SetColumn(completeWrapTextBox, 2);

            // Add all elements to the editors grid
            editorsGrid.Children.Add(startupLabel);
            editorsGrid.Children.Add(mainWrapLabel);
            editorsGrid.Children.Add(completeWrapLabel);
            editorsGrid.Children.Add(startupTextBox);
            editorsGrid.Children.Add(mainWrapTextBox);
            editorsGrid.Children.Add(completeWrapTextBox);

            // Button panel
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Button closeButton = new Button
            {
                Content = "OK",
                Padding = new Thickness(20, 8, 20, 8),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A56A0")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 0, 0),
                Style = (Style)FindResource("SidebarButtonStyle")
            };

            closeButton.Click += SetGCode_Variables_Click;

            void SetGCode_Variables_Click(object sender, RoutedEventArgs e)
            {
                TxtStartGcode.Text = startupTextBox.Text;
                TxtEndMWrap.Text = mainWrapTextBox.Text;
                TxtEndCWrap.Text = completeWrapTextBox.Text;

                gCodeEditorWindow.Close();
            }

            buttonPanel.Children.Add(closeButton);

            // Add components to main grid
            Grid.SetRow(editorsGrid, 0);
            Grid.SetRow(buttonPanel, 1);

            mainGrid.Children.Add(editorsGrid);
            mainGrid.Children.Add(buttonPanel);

            // Set the content and show
            gCodeEditorWindow.Content = mainGrid;
            gCodeEditorWindow.ShowDialog();
        }

        */





        private void ShowGCodeEditorWindow()
        {
            // Close existing dialog if open
            if (gCodeEditorWindow != null && gCodeEditorWindow.IsVisible)
            {
                gCodeEditorWindow.Close();
            }

            // Create a new dialog window
            gCodeEditorWindow = new Window
            {
                Title = "Machine G-Code Variables",
                Width = 900,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.CanResize,
                Background = new SolidColorBrush(Colors.WhiteSmoke)
            };

            // Create the content
            Grid mainGrid = new Grid
            {
                Margin = new Thickness(20)
            };

            // Add RowDefinitions to mainGrid
       //     mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // For the new section
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // For the new section
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // For editorsGrid
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // For buttonPanel

            // Create the new section grid
            Grid pumpSectionGrid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 30)
            };

            pumpSectionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            pumpSectionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            pumpSectionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }); // For CheckBox
            pumpSectionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // For TextBoxes
            pumpSectionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // For TextBoxes
            pumpSectionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // For TextBoxes
            pumpSectionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // For TextBoxes

            // CheckBox for "Enable Pump"
            CheckBox enablePumpCheckBox = new CheckBox
            {
                Content = "Enable Pump",
                Margin = new Thickness(0, 10, 10, 0),
                IsChecked = pumpState.IsChecked,
            };
            
            Grid.SetRow(enablePumpCheckBox, 0);
            Grid.SetRowSpan(enablePumpCheckBox, 2);
            Grid.SetColumn(enablePumpCheckBox, 0);


            // Labels and TextBoxes for "Pump on code," "Pump off code," "Cycles per shell," and "Duration"
            TextBlock pumpOnLabel = new TextBlock
            {
                Text = "Pump on code",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666")),
                Margin = new Thickness(5, 0, 0, 5)
            };
            Grid.SetRow(pumpOnLabel, 0);
            Grid.SetColumn(pumpOnLabel, 1);

            TextBox pumpOnTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Margin = new Thickness(5, 0, 0, 0),
                Padding = new Thickness(8),
                Text = StrPumpOnCode.Text, // Assuming TxtPumpOn is the source for initial text
                IsEnabled = enablePumpCheckBox.IsChecked == true
            };
            Grid.SetRow(pumpOnTextBox, 1);
            Grid.SetColumn(pumpOnTextBox, 1);
            
            TextBlock pumpOffLabel = new TextBlock
            {
                Text = "Pump off code",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666")),
                Margin = new Thickness(5, 0, 0, 5)
            };
            Grid.SetRow(pumpOffLabel, 0);
            Grid.SetColumn(pumpOffLabel, 2);

            TextBox pumpOffTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Margin = new Thickness(5, 0, 0, 0),
                Padding = new Thickness(8),
                Text = StrPumpOffCode.Text, // Assuming TxtPumpOff is the source for initial text
                IsEnabled = enablePumpCheckBox.IsChecked == true
            };
            Grid.SetRow(pumpOffTextBox, 1);
            Grid.SetColumn(pumpOffTextBox, 2);

            TextBlock cyclesLabel = new TextBlock
            {
                Text = "Cycles per shell",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666")),
                Margin = new Thickness(5, 0, 0, 5)
            };
            Grid.SetRow(cyclesLabel, 0);
            Grid.SetColumn(cyclesLabel, 3);

            TextBox cyclesTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Margin = new Thickness(5, 0, 0, 0),
                Padding = new Thickness(8),
                Text = NumCyclesPerShell.Text, // Assuming TxtCyclesPerShell is the source for initial text
                IsEnabled = enablePumpCheckBox.IsChecked == true
            };
            Grid.SetRow(cyclesTextBox, 1);
            Grid.SetColumn(cyclesTextBox, 3);

            TextBlock durationLabel = new TextBlock
            {
                Text = "Duration",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666")),
                Margin = new Thickness(5, 0, 0, 5)
            };
            Grid.SetRow(durationLabel, 0);
            Grid.SetColumn(durationLabel, 4);

            TextBox durationTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Margin = new Thickness(5, 0, 0, 0),
                Padding = new Thickness(8),
                Text = NumDuration.Text, // Assuming TxtDuration is the source for initial text
                IsEnabled = enablePumpCheckBox.IsChecked == true
            };
            Grid.SetRow(durationTextBox, 1);
            Grid.SetColumn(durationTextBox, 4);


            enablePumpCheckBox.Checked += Section_Pump_Enabled;
            enablePumpCheckBox.Unchecked += Section_Pump_Disabled;


            void Section_Pump_Enabled(object sender, RoutedEventArgs e)
            {
                pumpOnTextBox.IsEnabled = true;
                pumpOffTextBox.IsEnabled = true;
                cyclesTextBox.IsEnabled = true;
                durationTextBox.IsEnabled = true;
            }


            void Section_Pump_Disabled(object sender, RoutedEventArgs e)
            {
                pumpOnTextBox.IsEnabled = false;
                pumpOffTextBox.IsEnabled = false;
                cyclesTextBox.IsEnabled = false;
                durationTextBox.IsEnabled = false;
            }


            // Add all elements to the pumpSectionGrid
            pumpSectionGrid.Children.Add(enablePumpCheckBox);
            pumpSectionGrid.Children.Add(pumpOnLabel);
            pumpSectionGrid.Children.Add(pumpOnTextBox);
            pumpSectionGrid.Children.Add(pumpOffLabel);
            pumpSectionGrid.Children.Add(pumpOffTextBox);
            pumpSectionGrid.Children.Add(cyclesLabel);
            pumpSectionGrid.Children.Add(cyclesTextBox);
            pumpSectionGrid.Children.Add(durationLabel);
            pumpSectionGrid.Children.Add(durationTextBox);

            // Add pumpSectionGrid to mainGrid
            Grid.SetRow(pumpSectionGrid, 0);
            mainGrid.Children.Add(pumpSectionGrid);

            // Existing code for editorsGrid
            Grid editorsGrid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 15)
            };

            editorsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            editorsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            editorsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            editorsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            editorsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Create and add the labels
            TextBlock startupLabel = new TextBlock
            {
                Text = "Startup GCode",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666")),
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(startupLabel, 0);
            Grid.SetColumn(startupLabel, 0);

            TextBlock mainWrapLabel = new TextBlock
            {
                Text = "End of Main Wrap",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666")),
                Margin = new Thickness(5, 0, 5, 5)
            };
            Grid.SetRow(mainWrapLabel, 0);
            Grid.SetColumn(mainWrapLabel, 1);

            TextBlock completeWrapLabel = new TextBlock
            {
                Text = "End of Complete Wrap",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666")),
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(completeWrapLabel, 0);
            Grid.SetColumn(completeWrapLabel, 2);

            // Create and add the text boxes
            TextBox startupTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Margin = new Thickness(0, 0, 5, 0),
                Padding = new Thickness(8),
                Text = TxtStartGcode.Text
            };
            Grid.SetRow(startupTextBox, 1);
            Grid.SetColumn(startupTextBox, 0);

            TextBox mainWrapTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Margin = new Thickness(5, 0, 5, 0),
                Padding = new Thickness(8),
                Text = TxtEndMWrap.Text
            };
            Grid.SetRow(mainWrapTextBox, 1);
            Grid.SetColumn(mainWrapTextBox, 1);

            TextBox completeWrapTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Margin = new Thickness(5, 0, 0, 0),
                Padding = new Thickness(8),
                Text = TxtEndCWrap.Text
            };
            Grid.SetRow(completeWrapTextBox, 1);
            Grid.SetColumn(completeWrapTextBox, 2);

            // Add all elements to the editors grid
            editorsGrid.Children.Add(startupLabel);
            editorsGrid.Children.Add(mainWrapLabel);
            editorsGrid.Children.Add(completeWrapLabel);
            editorsGrid.Children.Add(startupTextBox);
            editorsGrid.Children.Add(mainWrapTextBox);
            editorsGrid.Children.Add(completeWrapTextBox);

            // Add editorsGrid to mainGrid
            Grid.SetRow(editorsGrid, 1);
            mainGrid.Children.Add(editorsGrid);

            // Button panel
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Button closeButton = new Button
            {
                Content = "OK",
                Padding = new Thickness(20, 8, 20, 8),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A56A0")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 0, 0),
                Style = (Style)FindResource("SidebarButtonStyle")
            };

            closeButton.Click += SetGCode_Variables_Click;

            void SetGCode_Variables_Click(object sender, RoutedEventArgs e)
            {
                TxtStartGcode.Text = startupTextBox.Text;
                TxtEndMWrap.Text = mainWrapTextBox.Text;
                TxtEndCWrap.Text = completeWrapTextBox.Text;

                // Update values from the new section
                StrPumpOnCode.Text = pumpOnTextBox.Text;
                StrPumpOffCode.Text = pumpOffTextBox.Text;
                NumCyclesPerShell.Text = cyclesTextBox.Text;
                NumDuration.Text = durationTextBox.Text;

                pumpState.IsChecked = enablePumpCheckBox.IsChecked;

                gCodeEditorWindow.Close();
            }

            buttonPanel.Children.Add(closeButton);

            // Add buttonPanel to mainGrid
            Grid.SetRow(buttonPanel, 2);
            mainGrid.Children.Add(buttonPanel);

            // Set the content and show
            gCodeEditorWindow.Content = mainGrid;
            gCodeEditorWindow.ShowDialog();
        }





        /*

        public static void ShowModernMessageBox(string title, string message)
    {
        // Create the main window
        var window = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = false,
            Background = Brushes.Gray,
            Content = CreateContent(title, message)
        };

        // Enable drag-move functionality
        window.MouseLeftButtonDown += (sender, e) => window.DragMove();

        // Show the dialog
        window.ShowDialog();
    }

    private static Grid CreateContent(string title, string message)
    {
        // Main Grid
        var grid = new Grid
        {
            Background = new SolidColorBrush(Color.FromRgb(222, 222, 222)), // Gray background
            Margin = new Thickness(1)
        };

        // Define rows for the layout
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content area
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });                      // Button area

        // Define columns for the button alignment
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Left space
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });                     // Button column

        // Border for rounded corners
        var border = new Border
        {
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(Color.FromRgb(66, 66, 66)), // Slightly lighter gray
            Padding = new Thickness(20)
        };

        // StackPanel for the content (title and message)
        var stackPanel = new StackPanel();

        // Title TextBlock
        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 10)
        };
        stackPanel.Children.Add(titleText);

        // Message TextBlock
        var messageText = new TextBlock
        {
            Text = message,
            FontSize = 14,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20)
        };
        stackPanel.Children.Add(messageText);

        // Add the content StackPanel to the first row
        Grid.SetRow(stackPanel, 0);
        Grid.SetColumnSpan(stackPanel, 2); // Span across both columns
        border.Child = stackPanel;

        // OK Button
        var okButton = new Button
        {
            Content = "OK",
            Width = 80,
            Height = 30,
            Background = new SolidColorBrush(Color.FromRgb(0, 120, 215)), // Blue button
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 10, 20, 20) // Add some margin above the button
        };

        // Close the dialog when the OK button is clicked
        okButton.Click += (sender, args) =>
        {
            var button = sender as Button;
            var window = Window.GetWindow(button);
            window?.Close();
        };

        // Place the button in the bottom-right corner
 //       Grid.SetRow(okButton, 1);       // Second row
 //       Grid.SetColumn(okButton, 1);    // Second column

        // Add the border and button to the grid
        grid.Children.Add(border);
        grid.Children.Add(okButton);

        return grid;
    }
    */
}
}



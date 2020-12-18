using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Presto.PRC;
using Presto.PRC.EventHandlers;
//using PRC.Manager.Dialogs;
using System.Diagnostics;
using WpfOpenGlControl;
using System.Collections.Generic;
using Presto.PRC.Types;
using Microsoft.Win32;
using System.Security;
using System.Xml;

namespace PRC_Phatv_3DView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Robot> lRobot = new List<Robot>();
        private OpenFileDialog OpenXmlFileDialog;
        private OpenFileDialog OpenSTLFileDialog;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private static int flagUpdateCtrl = 0;
        private string folderName;

        enum Theme
        {
            Default = 0,
            Theme1,
            Theme2
        }


        void SetTheme(Theme theme)
        {
            if (theme == Theme.Theme1)
            {
                Application.Current.Resources["AppBg"] = Brushes.CadetBlue;
                Application.Current.Resources["AppTextColor"] = Brushes.Gold;
                Application.Current.Resources["PanelBg"] = Brushes.DarkGray;
                Application.Current.Resources["PanelTextColor"] = Brushes.Yellow;
            }
            else //Default
            {
                Application.Current.Resources["AppBg"] = Brushes.LightGray;
                Application.Current.Resources["AppTextColor"] = Brushes.Black;
                Application.Current.Resources["PanelBg"] = Brushes.WhiteSmoke;
                Application.Current.Resources["PanelTextColor"] = Brushes.Black;
            }
        }

        ~MainWindow()
        {
            Global.controller.Disconnect();
        }

        public MainWindow()
        {
            //Library initialization
            Global.controller = new Controller();     //Create controller

            SetTheme(Theme.Default);

            InitializeComponent();

            // Init parameter
            //Global.gLMMCRobot = new LMMC_Info();

            // Init Open Xml file dialog
            OpenXmlFileDialog = new OpenFileDialog()
            {
                FileName = "Select a xml file",
                Filter = "Xml files (*.xml)|*.xml",
                Title = "Open Xml file"
            };

            // Init Open STL file dialog
            OpenSTLFileDialog = new OpenFileDialog()
            {
                FileName = "Select a STL file",
                Filter = "STL files (*.stl)|*.stl",
                Title = "Open STL file"
            };

            // Init Open Xml folder diglog

            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            // Set the help text description for the FolderBrowserDialog.
            this.folderBrowserDialog1.Description =
                "Select the directory that you want to use as the default.";

            // Do not allow the user to create new files via the FolderBrowserDialog.
            this.folderBrowserDialog1.ShowNewFolderButton = false;

            // Default to the My Documents folder.
            //this.folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Personal;

            // Handle event
            HandleEvents();
        }

        private void HandleEvents()
        {
            Global.controller.ConnectEventHandler += new ConnectEventHandler(() =>
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() =>
                {
                    txtIp.IsEnabled = false;
                    btnConnect.Content = "Disconnect";
                    btnConnect.IsEnabled = true;
                    Debug.WriteLine("connection ok");
                }));
            }
            );

            Global.controller.DisconnectEventHandler += new DisconnectEventHandler(() =>
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() =>
                {
                    txtIp.IsEnabled = true;
                    btnConnect.Content = "Connect";
                    btnConnect.IsEnabled = true;
                }));
            });

            Global.controller.LmmcGetCarrierInfoEventHandler += new LGetCarrierInfoEventHandler((List<SCarrierInfo> lstCarr) =>
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() =>
                {
                    if (flagUpdateCtrl == 0)
                    {
                        Global.gListCarrierInfo = lstCarr;
                        flagUpdateCtrl = 1;
                        if ((uc3DView.bLoadedRoboot == true) && (Global.controller.IsConnected))
                        {
                            txtCarrPos0.Text = lstCarr[0].Pos.Pos.ToString();
                            txtCarrPos1.Text = lstCarr[1].Pos.Pos.ToString();
                            txtCarrPos2.Text = lstCarr[2].Pos.Pos.ToString();
                            txtCarrPos3.Text = lstCarr[3].Pos.Pos.ToString();
                        }
                    }
                }));
            });

            Global.controller.LmmcGetShuttleInfoEventHandler += new LGetShuttleInfoEventHandler((List<SShuttleInfo> lstShuttle) =>
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() =>
                {
                    if (flagUpdateCtrl == 1)
                    {
                        Global.gListShuttleInfo = lstShuttle;
                        // call update Carrier
                        Global.gLMMCSimulateRealSystem.LMMC_UpdatePos();
                        // Draw 3D view
                        if ((uc3DView.bLoadedRoboot == true) && (Global.controller.IsConnected))
                        {
                            txtLeftPosZ.Text = lstShuttle[0].Pos.ToString();
                            txtLeftPosX.Text = lstShuttle[1].Pos.ToString();
                            txtLeftPosY.Text = lstShuttle[2].Pos.ToString();

                            txtRightPosZ.Text = lstShuttle[3].Pos.ToString();
                            txtRightPosX.Text = lstShuttle[4].Pos.ToString();
                            txtRightPosY.Text = lstShuttle[5].Pos.ToString();
                        }
                        uc3DView.glControl.Invalidate();
                        flagUpdateCtrl = 0;
                    }
                }));
            });

            Global.UpdateCarrInfo += UpdateCarrierInfo;
            Global.UpdateLMTrackInfo += UpdateLMTrackInfo;
        }

        private void UpdateLMTrackInfo(double[] arrLm)
        {
            if (arrLm == null)
                return;
            if (arrLm.Length != 6)
                return;
            txtLeftPosX.Text = string.Format("{0:0.0000}", arrLm[0]);
            txtLeftPosY.Text = string.Format("{0:0.0000}", arrLm[1]);
            txtLeftPosZ.Text = string.Format("{0:0.0000}", arrLm[2]);
            txtRightPosX.Text = string.Format("{0:0.0000}", arrLm[3]);
            txtRightPosY.Text = string.Format("{0:0.0000}", arrLm[4]);
            txtRightPosZ.Text = string.Format("{0:0.0000}", arrLm[5]);
        }

        private void UpdateCarrierInfo(double[] arrCarr)
        {
            if (arrCarr == null)
                return;
            if (arrCarr.Length != 4)
                return;
            txtCarrPos0.Text = string.Format("{0:0.0000}", arrCarr[0]);
            txtCarrPos1.Text = string.Format("{0:0.0000}", arrCarr[1]);
            txtCarrPos2.Text = string.Format("{0:0.0000}", arrCarr[2]);
            txtCarrPos3.Text = string.Format("{0:0.0000}", arrCarr[3]);
        }


        private void Btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            Debug.Write("Connect to LMMC controller");

            if (btnConnect.Content.ToString() == "Connect")
            {
                Global.controller.Connect(txtIp.Text);
                btnConnect.Content = "Connecting...";
                btnConnect.IsEnabled = false;
            }
            else
            {
                Global.controller.Disconnect();
            }
        }

        private void btnSysParaLoad_Click(object sender, RoutedEventArgs e)
        {
            if (OpenXmlFileDialog.ShowDialog() == true)
            {
                try
                {
                    Global.gLMMCRobot.stLSysParaPath = OpenXmlFileDialog.FileName;
                    var filename = OpenXmlFileDialog.SafeFileName;

                    if (Global.gLMMCRobot.ParsingLSysPara(Global.gLMMCRobot.stLSysParaPath, Global.gLMMCRobot) == 1)
                    {
                        textSysParaFile.Text = filename;
                        textSysParaFile.IsReadOnly = true;
                    }
                    else
                    {
                        textSysParaFile.Text = null;
                        textSysParaFile.IsReadOnly = true;
                        Global.gLMMCRobot.stLSysParaPath = null;
                    }

                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private void btnMMTrackLoad_Click(object sender, RoutedEventArgs e)
        {
            if (OpenXmlFileDialog.ShowDialog() == true)
            {
                try
                {
                    XmlDataDocument xmldoc = new XmlDataDocument();
                    Global.gLMMCRobot.stMMTrackPath = OpenXmlFileDialog.FileName;
                    var filename = OpenXmlFileDialog.SafeFileName;

                    if (Global.gLMMCRobot.ParsingMMTrack(Global.gLMMCRobot.stMMTrackPath, Global.gLMMCRobot) == 1)
                    {
                        textMMTrackFile.Text = filename;
                        textMMTrackFile.IsReadOnly = true;
                    }
                    else
                    {
                        textMMTrackFile.Text = null;
                        textMMTrackFile.IsReadOnly = true;
                        Global.gLMMCRobot.stMMTrackPath = null;
                    }

                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private void btnLMTrackLoad_Click(object sender, RoutedEventArgs e)
        {
            if (OpenXmlFileDialog.ShowDialog() == true)
            {
                try
                {
                    XmlDataDocument xmldoc = new XmlDataDocument();
                    Global.gLMMCRobot.stLMTrackPath = OpenXmlFileDialog.FileName;
                    var filename = OpenXmlFileDialog.SafeFileName;
                    if (Global.gLMMCRobot.ParsingLMTrack(Global.gLMMCRobot.stLMTrackPath, Global.gLMMCRobot) == 1)
                    {
                        textLMTrackFile.Text = filename;
                        textLMTrackFile.IsReadOnly = true;
                    }
                    else
                    {
                        textLMTrackFile.Text = null;
                        textLMTrackFile.IsReadOnly = true;
                        Global.gLMMCRobot.stLMTrackPath = null;
                    }
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }

        }

        private void btnMMRailLoad_Click(object sender, RoutedEventArgs e)
        {
            if (OpenXmlFileDialog.ShowDialog() == true)
            {
                try
                {
                    var filePath = OpenXmlFileDialog.FileName;
                    using (Stream str = OpenXmlFileDialog.OpenFile())
                    {
                        // Process.Start("notepad.exe", filePath);
                    }
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }

        }

        private void btn3DView_Click(object sender, RoutedEventArgs e)
        {

            string sRobotType = this.cbxRobotTypes.SelectionBoxItem.ToString();
            Robot tmpRobot = new Robot();
            uc3DView.UnloadRobot();

            if (lRobot.Count != 0)
                lRobot.Clear();

            switch (sRobotType)
            {
                case "LPK_SCARA":
                    Global.gLMMCRobot.rbtRobotType = RobotType.LPK_SP_2525_200;
                    tmpRobot.Name = sRobotType;
                    tmpRobot.RbType = 4000;
                    lRobot.Add(tmpRobot);

                    Global.controller.LoadedAllRobotEventHandler?.Invoke(lRobot);
                    uc3DView.glControl.Invalidate();

                    break;
                case "SINGLE_RB":
                    break;
                case "LMMC_TYPE_01":
                    Global.gLMMCRobot.rbtRobotType = RobotType.LMMC_TYPE_01;
                    tmpRobot.Name = sRobotType;
                    tmpRobot.RbType = (ushort)RobotType.LMMC_TYPE_01;
                    lRobot.Add(tmpRobot);

                    if (Global.gLMMCRobot.stArrMMTrackInf != null)
                    {
                        Global.gLMMCVisualization.InitTargetPos();
                        Global.controller.LoadedAllRobotEventHandler?.Invoke(lRobot);
                        uc3DView.glControl.Invalidate();
                    }
                    else
                    {
                        MessageBox.Show("Plese select LSysPara.xml, LMTrack.xml, MMTrack.xml files");
                    }
                    break;
                case "LMMC_TYPE_02":
                    Global.gLMMCRobot.rbtRobotType = RobotType.LMMC_TYPE_02;
                    tmpRobot.Name = sRobotType;
                    tmpRobot.RbType = (ushort)RobotType.LMMC_TYPE_02;
                    lRobot.Add(tmpRobot);

                    if (Global.gLMMCRobot.stArrMMTrackInf != null)
                    {
                        Global.gLMMCVisualization.InitTargetPos();
                        Global.controller.LoadedAllRobotEventHandler?.Invoke(lRobot);
                        uc3DView.glControl.Invalidate();
                    }
                    else
                    {
                        MessageBox.Show("Plese select LSysPara.xml, LMTrack.xml, MMTrack.xml files");
                    }
                    break;

                case "LMMC_TYPE_03":
                    Global.gLMMCRobot.rbtRobotType = RobotType.LMMC_TYPE_03;
                    tmpRobot.Name = sRobotType;
                    tmpRobot.RbType = (ushort)RobotType.LMMC_TYPE_03;
                    lRobot.Add(tmpRobot);

                    if (Global.gLMMCRobot.stArrMMTrackInf != null)
                    {
                        Global.gLMMCVisualization.InitTargetPos();
                        Global.controller.LoadedAllRobotEventHandler?.Invoke(lRobot);
                        uc3DView.glControl.Invalidate();
                    }
                    else
                    {
                        MessageBox.Show("Plese select LSysPara.xml, LMTrack.xml, MMTrack.xml files");
                    }
                    break;

                case "CUBE":
                    tmpRobot.Name = sRobotType;
                    tmpRobot.RbType = (ushort)RobotType.CUBE;
                    lRobot.Add(tmpRobot);
                    break;
                case "LMS_ASSY_TYPE":
                    Global.gLMMCRobot.rbtRobotType = RobotType.LMS_ASSY_TYPE;
                    tmpRobot.Name = sRobotType;
                    tmpRobot.RbType = (ushort)RobotType.LMS_ASSY_TYPE;
                    lRobot.Add(tmpRobot);
                    Global.gLMMCVisualization.InitTargetPos();
                    Global.controller.LoadedAllRobotEventHandler?.Invoke(lRobot);
                    uc3DView.glControl.Invalidate();
                    break;
                case "STL_FILE":
                    Global.gLMMCRobot.rbtRobotType = RobotType.STL_FILE;
                    tmpRobot.Name = sRobotType;
                    tmpRobot.RbType = (ushort)RobotType.STL_FILE;
                    lRobot.Add(tmpRobot);
                    //Global.gLMMCVisualization.InitTargetPos();
                    Global.controller.LoadedAllRobotEventHandler?.Invoke(lRobot);
                    uc3DView.glControl.Invalidate();
                    break;
                case "STL_FOLDER":
                    Global.gLMMCRobot.rbtRobotType = RobotType.STL_FOLDER;
                    tmpRobot.Name = sRobotType;
                    tmpRobot.RbType = (ushort)RobotType.STL_FOLDER;
                    lRobot.Add(tmpRobot);
                    //Global.gLMMCVisualization.InitTargetPos();
                    Global.controller.LoadedAllRobotEventHandler?.Invoke(lRobot);
                    uc3DView.glControl.Invalidate();
                    break;

                default:
                    break;
            }
        }

        private void btnStartVisualization_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartVisualization.Content.Equals("Start"))
            {
                btnStartVisualization.Content = "Stop";
                uc3DView.bSimulationFlag = true;
            }
            else if (btnStartVisualization.Content.Equals("Stop"))
            {
                btnStartVisualization.Content = "Start";
                uc3DView.bSimulationFlag = false;
                // uc3DView.thSimulate.Resume();
            }
        }

        private void cbxRobotTypes_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Global.gLMMCRobot.Reset_Para();
            Global.gLMMCVisualization.Reset_Para();

            if (textSysParaFile != null)
                textSysParaFile.Text = null;
            if (textLMTrackFile != null)
                textLMTrackFile.Text = null;
            if (textMMTrackFile != null)
                textMMTrackFile.Text = null;
            if (textMMRailFile != null)
                textMMRailFile.Text = null;
            if (textSTLlFile != null)
                textSTLlFile.Text = null;

        }

        private void btnSTLLoad_Click(object sender, RoutedEventArgs e)
        {
            string tmp = cbxRobotTypes.Text;

            if (tmp == "STL_FILE")
            {
                if (OpenSTLFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        if (Global.gLMMCRobot.stSTLPath == null)
                            Global.gLMMCRobot.stSTLPath = new string[1];
                        Global.gLMMCRobot.stSTLPath[0] = OpenSTLFileDialog.FileName;
                        var filename = OpenSTLFileDialog.SafeFileName;
                        textSTLlFile.Text = filename;
                        textSTLlFile.IsReadOnly = true;

                    }
                    catch (SecurityException ex)
                    {
                        MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}");
                    }
                }
            }
            else if (tmp == "STL_FOLDER")
            {
                System.Windows.Forms.DialogResult result = folderBrowserDialog1.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    folderName = folderBrowserDialog1.SelectedPath;
                    //if (!fileOpened)
                    //{
                    //    // No file is opened, bring up openFileDialog in selected path.
                    //    openFileDialog1.InitialDirectory = folderName;
                    //    openFileDialog1.FileName = null;
                    //    openMenuItem.PerformClick();
                    //}
                    string[] filePaths = Directory.GetFiles(folderName, "*.stl");

                    if (filePaths.Length > 0)
                        Global.gLMMCRobot.stSTLPath = filePaths;

                    textSTLlFile.Text = folderName;
                    textSTLlFile.IsReadOnly = true;
                }
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Global.controller.Disconnect();
        }
    }
}

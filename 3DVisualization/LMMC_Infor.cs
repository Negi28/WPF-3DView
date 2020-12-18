using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Presto.PRC.Types;
using OpenTK;
using System.Diagnostics;

namespace PRC_Phatv_3DView
{
    public enum LMMC_TRACK_TYPE
    {
        MM_TRACK = 0,
        MM_SHUTTLE,
        LM_LIFT,
        LM_SHUTTLE
    }

    public enum LMMC_RAIL_TYPE
    {
        STATIC_RAIL = 0,
        DYNAMIC_RAIL,
        LM_RAIL
    }

    public struct ST_CFGAXIS
    {
        public int iIndex;
        public int iRailType;
        public int iDockPos;
        public int iDockTrackType;
    }
    public struct ST_MMTRACKINFO
    {
        public int iMMTrackIndex;
        public int arCfgAxisNum;
        public float fSWNegLimit;
        public float fSWPosLimit;
        public ST_CFGAXIS stAxis;
    }
    public struct ST_LMTRACKINFO
    {
        public int iLMTrackIndex;
        public int iLMTrackType;
        public float fSWNegLimit;
        public float fSWPosLimit;
    }
    public enum VECTOR_TARGET_POS
    {
        VT_TG_TOP_LEFT = 0,
        VT_TG_TOP_RIGHT,
        VT_TG_BOTTOM_RIGHT,
        VT_TG_BOTTOM_LEFT
    }
    public enum LMMC_TYPE03_LINKID
    {
        ST_RAIL_MMTRACK = 0,    // static rail mm track
        DM_RAIL_1_MMTRACK,
        DM_RAIL_2_MMTRACK,
        DM_RAIL_LMSHUTTLE_R,    // dynamic rail lm shuttle right
        DM_RAIL_LMSHUTTLE_L,    // dynamic rail lm shuttle left
        DM_RAIL_LMLIFT_LT,      // dynamic rail lm lift left top
        DM_RAIL_LMLIFT_LB,      // dynamic rail lm lift left bottom

        ST_COIL_MMTRACK,        // static rail coil mm track
        DM_COIL_1_MMTRACK,
        DM_COIL_LMSHUTTLE_R,    // dynamic coil lm shuttle right
        DM_COIL_LMSHUTTLE_L,    // dynamic coil lm shuttle left
        DM_COIL_LMLIFT_LT,      // dynamic coil lm lift left top
        DM_COIL_LMLIFT_LB,      // dynamic coil lm lift left bottom

        ST_LMSHUTTLE_LR,
        ST_LMSHUTTLE_CT,       // static lm shuttle center
        DM_LMSHUTTLE_LT,       // dynamic lm shuttle left top
        DM_LMSHUTTLE_LB,       // dynamic lm shuttle left bottom
        ST_LMLIFT_POLE,
        DM_LMLIFT_CARRIER_LT,
        DM_LMLIFT_CARRIER_LB,
        DM_CARRIER = 20,
    }
    public enum LMMC_TYPE02_LINKID
    {
        ST_RAIL_MMTRACK = 0,    // static rail mm track
        DM_RAIL_LMLIFT_LEFT,
        DM_RAIL_LMLIFT_RIGHT,
        ST_COIL_MMTRACK,        // static rail coil mm track
        DM_COIL_LMLIFT_LEFT,
        DM_COIL_LMLIFT_RIGHT,
        ST_LMLIFT,
        DM_CARRIER = 7,
    }

    public enum LMS_ASSY_LINKID
    {
        ST_LMS_BODY = 0,
        ST_RAIL_MMTRACK,    // static rail mm track
        DM_RAIL_LMLIFT_LEFT,
        DM_RAIL_LMLIFT_RIGHT,
        ST_COIL_MMTRACK,        // static rail coil mm track
        DM_COIL_LMLIFT_LEFT,
        DM_COIL_LMLIFT_RIGHT,
        DM_LMLIFT_LEFT,
        DM_LMLIFT_RIGHT,
        DM_CARRIER = 9,
    }

    public enum LMMC_TYPE01_LINKID
    {
        ST_RAIL_MMTRACK = 0,    // static rail mm track
        ST_COIL_MMTRACK,        // static rail coil mm track
        DM_CARRIER = 2,
    }

    public class LinkIndex
    {
        public int from = 0;
        public int count = 0;
        public float rotAngle = 0.0f;
        public float[] rotVector = new float[3];
        public float[] orgin = new float[3];
        public float[] trans = new float[3];

        public Vector3 transfrom = new Vector3();
        public Vector3 currPosition = new Vector3();
        public Vector3 targetPosition = new Vector3();
        //public Vector3 restrictPosition = new Vector3();
        public Vector3 speed = new Vector3();
        public bool bEnableMove = false;

        public int baseIndex = 0;
        public Vector3 color;
    }
    public class LMShuttleObject
    {
        public LinkedList<VisualObject> shuttleOb = new LinkedList<VisualObject>();
        public bool OnRequest = false;
    }

    public class LMLiftObject
    {
        public LinkIndex lkDynamicRail = null;
        public LinkIndex lkDynamicCoil = null;
        public LinkIndex lkCarrier = null;
        public LinkIndex lkDynamicLmshuttle = null;
        public LinkIndex lkDynamiclmCarrier = null;
        public bool OnRequest = false;
        public bool bFinishProcess = false;
        public int iTimeProcess = 0;
    }

    public class ProcessObject
    {
        public LinkIndex lkDynamicRail_1 = null;
        public LinkIndex lkDynamicRail_2 = null;
        public LinkIndex lkDynamicCoil = null;
        public LinkIndex lkCarrier = null;
        public bool OnProcess = false;
        public int iTimeProcess = 0;
    }

    class LMMC_Infor
    {
        public ST_MMTRACKINFO[] stArrMMTrackInf;
        public ST_LMTRACKINFO[] stArrLMTrackInf;
        public int iMMRails;
        public int iCarrierNum;
        public string stLSysParaPath = null;
        public string stLMTrackPath = null;
        public string stMMTrackPath = null;
        public string stMMRailsPath = null;
        public string[] stSTLPath = null;
        public RobotType rbtRobotType = RobotType.RMODEL_NONE;

        public void Reset_Para()
        {
            stArrMMTrackInf = null;
            stArrLMTrackInf = null;
            iMMRails = 0;
            iCarrierNum = 0;
            stLSysParaPath = null;
            stLMTrackPath = null;
            stMMTrackPath = null;
            stMMRailsPath = null;
            rbtRobotType = RobotType.RMODEL_NONE;
        }
        public int ParsingLSysPara(string stXMLPath, LMMC_Infor lmmInfor)
        {
            if (stXMLPath != null)
            {
                string filename = Path.GetFileName(stXMLPath);
                XmlDataDocument xmldoc = new XmlDataDocument();
                XmlNodeList xmlnode = null;

                xmldoc.Load(stXMLPath);

                xmlnode = xmldoc.GetElementsByTagName("MMTrackNumber");

                if (xmlnode.Count == 1)
                {
                    lmmInfor.stArrMMTrackInf = new ST_MMTRACKINFO[Int16.Parse(xmlnode[0].InnerText)];
                }
                else
                {
                    MessageBox.Show("File: " + filename + " parsing eror element MMTrackNumber.\n Please check again!");
                    return (0);
                }

                xmlnode = xmlnode = xmldoc.GetElementsByTagName("LMTrackNumber");
                if (xmlnode.Count == 1)
                    lmmInfor.stArrLMTrackInf = new ST_LMTRACKINFO[Int16.Parse(xmlnode[0].InnerText)];
                else
                {
                    MessageBox.Show("File: " + filename + " parsing eror element LMTrackNumber.\n Please check again!");
                    return (0);
                }

                // Get MMRailNumber
                xmlnode = xmlnode = xmldoc.GetElementsByTagName("MMRailNumber");
                if (xmlnode.Count == 1)
                    lmmInfor.iMMRails = Int16.Parse(xmlnode[0].InnerText);
                else
                {
                    MessageBox.Show("File: " + filename + " parsing eror element MMRailNumber.\n Please check again!");
                    return (0);
                }

                // Get carrierNumber
                xmlnode = xmlnode = xmldoc.GetElementsByTagName("CarrierNumber");
                if (xmlnode.Count == 1)
                    lmmInfor.iCarrierNum = Int16.Parse(xmlnode[0].InnerText);
                else
                {
                    MessageBox.Show("File: " + filename + " parsing eror element CarrierNumber.\n Please check again!");
                    return (0);
                }

                return (1);
            }

            return (0);
        }

        public int ParsingMMTrack(string stXMLPath, LMMC_Infor lmmInfor)
        {
            if (stXMLPath != null)
            {
                string filename = Path.GetFileName(stXMLPath);
                XmlDataDocument xmldoc = new XmlDataDocument();
                XmlNodeList xmlnode = null;
                xmldoc.Load(stXMLPath);

                if (lmmInfor.stArrMMTrackInf != null)
                {
                    if (lmmInfor.stArrMMTrackInf.Length == Int16.Parse(xmldoc.DocumentElement.ChildNodes[0].InnerText))
                    {
                        for (int i = 0; i < Global.gLMMCRobot.stArrMMTrackInf.Length; i++)
                        {
                            lmmInfor.stArrMMTrackInf[i].fSWNegLimit = float.Parse(xmldoc.DocumentElement.ChildNodes[i + 1].ChildNodes[4].InnerText);
                            lmmInfor.stArrMMTrackInf[i].fSWPosLimit = float.Parse(xmldoc.DocumentElement.ChildNodes[i + 1].ChildNodes[5].InnerText);
                        }
                    }
                    else
                    {
                        MessageBox.Show("File: " + filename + " parsing eror MMTrack number.\n Please check file name or file content!");
                        return (0);
                    }

                }
                else
                {
                    MessageBox.Show("Please chose LSysPara fristly.\n Please select again!");
                    return (0);
                }

                return (1);
            }

            return (0);
        }

        public int ParsingLMTrack(string stXMLPath, LMMC_Infor lmmInfor)
        {
            if (stXMLPath != null)
            {
                string filename = Path.GetFileName(stXMLPath);
                XmlDataDocument xmldoc = new XmlDataDocument();
                XmlNodeList xmlnode = null;
                xmldoc.Load(stXMLPath);

                if (lmmInfor.stArrLMTrackInf != null)
                {
                    if (lmmInfor.stArrLMTrackInf.Length == Int16.Parse(xmldoc.DocumentElement.ChildNodes[0].InnerText))
                    {
                        for (int i = 0; i < Global.gLMMCRobot.stArrLMTrackInf.Length; i++)
                        {
                            Global.gLMMCRobot.stArrLMTrackInf[i].iLMTrackType = Int16.Parse(xmldoc.DocumentElement.ChildNodes[i + 1].ChildNodes[2].InnerText);
                            Global.gLMMCRobot.stArrLMTrackInf[i].fSWNegLimit = float.Parse(xmldoc.DocumentElement.ChildNodes[i + 1].ChildNodes[5].InnerText);
                            Global.gLMMCRobot.stArrLMTrackInf[i].fSWPosLimit = float.Parse(xmldoc.DocumentElement.ChildNodes[i + 1].ChildNodes[6].InnerText);
                        }
                    }
                    else
                    {
                        MessageBox.Show("File: " + filename + " parsing eror LMTrack number.\n Please check file name or file content!");
                        return (0);
                    }

                }
                else
                {
                    MessageBox.Show("Please chose LSysPara fristly.\n Please select again!");
                    return (0);
                }

                return (1);
            }

            return (0);
        }

    }

    class LMMC_SimulateRealSystem
    {
        public List<VisualObject> lstCarrier = new List<VisualObject>();
        public LMShuttleObject lmShuttleRight = new LMShuttleObject();
        public LMShuttleObject lmShuttleLeft = new LMShuttleObject();

        public void LMMC_UpdatePos()
        {
            Vector3 vtOri = new Vector3(-5.000002f, 9.550016f, 1.003348f);
            Vector3 vtMove = new Vector3(0, -957, -785);

            //Vector3 vtCarrier_1 = vtMove + new Vector3(207, 0.0f, 870.93f) + vtOri;
            //Vector3 vtCarrier_2 = vtMove + new Vector3(207, 0.0f, 870.93f + 350) + vtOri;

            if (lstCarrier.Count > 0)
            {
                for (int i = 0; i < lstCarrier.Count; i++)
                {
                    double realCarrPos = Global.gListCarrierInfo[i].Pos.Pos;
                    float realScalePos = (float)((realCarrPos + 11.806f) * 0.6897f + 244.55f - 947.449984f - 50.0);
                    Vector3 currPos = new Vector3();
                    VisualObject vsOb = lstCarrier[i];
                    STLRenderObject rdOb = vsOb.stlListModels[0];

                    if (Global.gListCarrierInfo[i].OnDrv1 == 3)
                    {
                        currPos = new Vector3(vsOb.prePosition.X, 1329.45f, vsOb.prePosition.Z);
                    }
                    else if ((Global.gListCarrierInfo[i].OnDrv1 == 1) || (Global.gListCarrierInfo[i].OnDrv1 == 4))
                    {
                        currPos = new Vector3(vsOb.prePosition.X, 586.05f, vsOb.prePosition.Z);
                    }
                    else if (Global.gListCarrierInfo[i].Pos.TrackNo == 0)
                    {
                        currPos = new Vector3(vsOb.prePosition.X, realScalePos, vsOb.prePosition.Z);
                    }
                    else if (Global.gListCarrierInfo[i].Pos.TrackNo == 1)
                    {
                        currPos = vtMove + vtOri + new Vector3(207, realScalePos, 870.93f);
                    }

                    //currPos = new Vector3(vsOb.prePosition.X, realScalePos, vsOb.prePosition.Z);
                    lstCarrier[i].RealLMMC_Update(currPos);


                    if (Global.gListCarrierInfo[i].OnDrv1 == 9)
                    {
                        if (lmShuttleRight.shuttleOb.ElementAt(0).stlListModels.Contains(rdOb) == false)
                        {
                            lmShuttleRight.shuttleOb.ElementAt(0).stlListModels.Add(rdOb);
                        }
                    }
                    else
                    {
                        if (lmShuttleRight.shuttleOb.ElementAt(0).stlListModels.Contains(rdOb) == true)
                        {
                            lmShuttleRight.shuttleOb.ElementAt(0).stlListModels.Remove(rdOb);
                        }
                    }

                    if (Global.gListCarrierInfo[i].OnDrv1 == 8)
                    {
                        if (lmShuttleLeft.shuttleOb.ElementAt(0).stlListModels.Contains(rdOb) == false)
                        {
                            lmShuttleLeft.shuttleOb.ElementAt(0).stlListModels.Add(rdOb);
                        }
                    }
                    else
                    {
                        if (lmShuttleLeft.shuttleOb.ElementAt(0).stlListModels.Contains(rdOb) == true)
                        {
                            lmShuttleLeft.shuttleOb.ElementAt(0).stlListModels.Remove(rdOb);
                        }
                    }

                    //Debug.WriteLine("Carrier: " + i + " OnDriver1 : " + Global.gListCarrierInfo[i].OnDrv1 + " OnDriver2 : " + Global.gListCarrierInfo[i].OnDrv2);

                }
            }

            if (lmShuttleLeft.shuttleOb.Count > 0)
            {

                double realLShuttlePos = Global.gListShuttleInfo[0].Pos;
                float relLShuttlePosScalse = (float)((realLShuttlePos + 5.506f) * 1.1667f - 250.8667f);
                Vector3 vtLShuttle = new Vector3(196.0f, -892.45f, relLShuttlePosScalse);

                foreach (var visualOb in lmShuttleLeft.shuttleOb)
                    visualOb.RealLMMC_Update(vtLShuttle);

                //Debug.WriteLine("shuttle left: MapSlaNo: " + Global.gListShuttleInfo[0].MapSlaNo + "Carier: " + Global.gListShuttleInfo[0].CarrierNoOn);

            }

            if (lmShuttleRight.shuttleOb.Count > 0)
            {
                double realRShuttlePos = Global.gListShuttleInfo[3].Pos;
                float relRShuttlePosScalse = (float)((realRShuttlePos + 0.306) * 1.1667f - 250.8667f);
                Vector3 vtRShuttle = new Vector3(196.0f, 548.68f, relRShuttlePosScalse);

                foreach (var visualOb in lmShuttleRight.shuttleOb)
                    visualOb.RealLMMC_Update(vtRShuttle);

                //Debug.WriteLine("shuttle Right: MapSlaNo: " + Global.gListShuttleInfo[3].MapSlaNo + "Carier: " + Global.gListShuttleInfo[3].CarrierNoOn);
            }


        }
    }

    class LMMC_Visualization
    {
        // LinkedList for 2 Rails
        public LinkedList<VisualObject> lklBottomRail = new LinkedList<VisualObject>();
        public LinkedList<VisualObject> lklTopRail = new LinkedList<VisualObject>();

        public LMShuttleObject lmShuttleRight = new LMShuttleObject();
        public LMShuttleObject lmShuttleLeft = new LMShuttleObject();

        public LMLiftObject lmLiftTL = new LMLiftObject();
        public LMLiftObject lmLiftBL = new LMLiftObject();
        public ProcessObject obProcess = new ProcessObject();

        public Vector3[] arVTCarrierTargetPos = new Vector3[4];
        public Vector3[] arVTRailTargetPos = new Vector3[4];

        public void Reset_Para()
        {
            lklBottomRail.Clear();
            lklTopRail.Clear();

            lmShuttleRight.shuttleOb.Clear();
            lmShuttleRight.OnRequest = false;

            lmShuttleLeft.shuttleOb.Clear();
            lmShuttleLeft.OnRequest = false;

            lmLiftTL.lkCarrier = null;
            lmLiftTL.lkDynamicCoil = null;
            lmLiftTL.lkDynamiclmCarrier = null;
            lmLiftTL.lkDynamicLmshuttle = null;
            lmLiftTL.lkDynamicRail = null;
            lmLiftTL.OnRequest = false;

            lmLiftBL.lkCarrier = null;
            lmLiftBL.lkDynamicCoil = null;
            lmLiftBL.lkDynamiclmCarrier = null;
            lmLiftBL.lkDynamicLmshuttle = null;
            lmLiftBL.lkDynamicRail = null;
            lmLiftBL.OnRequest = false;

            obProcess.lkDynamicCoil = null;
            obProcess.lkDynamicRail_1 = null;
            obProcess.lkDynamicRail_2 = null;
            obProcess.OnProcess = false;
        }

        public void InitTargetPos()
        {
            if (Global.gLMMCRobot.rbtRobotType == RobotType.LMMC_TYPE_01)
            {
                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_LEFT)] = new Vector3(15, -(Global.gLMMCRobot.iMMRails * (420 / 2)) + 10, 31);
                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_RIGHT)] = new Vector3(15, (Global.gLMMCRobot.iMMRails * (420 / 2)) - 420 + 10, 31);
            }
            else if (Global.gLMMCRobot.rbtRobotType == RobotType.LMMC_TYPE_02)
            {
                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_LEFT)] = new Vector3(15, -((Global.gLMMCRobot.iMMRails - 2) * (420 / 4)) + 10, 31 + 700);
                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT)] = new Vector3(15, -((Global.gLMMCRobot.iMMRails - 2) * (420 / 4)) + 10, 31);
                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_RIGHT)] = new Vector3(15, ((Global.gLMMCRobot.iMMRails - 2) * (420 / 4)) - 420 + 10, 31 + 700);
                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT)] = new Vector3(15, ((Global.gLMMCRobot.iMMRails - 2) * (420 / 4)) - 420 + 10, 31);

                this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_LEFT)] = new Vector3(0, -((Global.gLMMCRobot.iMMRails - 2) * (420 / 4)) - 420, 700);
                this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT)] = new Vector3(0, -((Global.gLMMCRobot.iMMRails - 2) * (420 / 4)) - 420, 0);
                this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_RIGHT)] = new Vector3(0, ((Global.gLMMCRobot.iMMRails - 2) * (420 / 4)), 700);
                this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT)] = new Vector3(0, ((Global.gLMMCRobot.iMMRails - 2) * (420 / 4)), 0);
            }
            else if (Global.gLMMCRobot.rbtRobotType == RobotType.LMMC_TYPE_03)
            {
                float fCoordinateX = (Global.gLMMCRobot.stArrLMTrackInf[0].fSWPosLimit - Global.gLMMCRobot.stArrLMTrackInf[0].fSWNegLimit) / 2;
                float fCoordinateY = ((Global.gLMMCRobot.iMMRails - 3) * (420 / 4));
                float fCoordinateZ = 0;

                this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_LEFT)] = new Vector3(-fCoordinateX - 250 / 2, -fCoordinateY, fCoordinateZ);
                this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT)] = new Vector3(fCoordinateX - 250 / 2, -fCoordinateY, fCoordinateZ);
                this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_RIGHT)] = new Vector3(-fCoordinateX - 250 / 2, fCoordinateY, fCoordinateZ);
                this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT)] = new Vector3(fCoordinateX - 250 / 2, fCoordinateY, fCoordinateZ);


                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_LEFT)] = this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_LEFT)] + new Vector3(15, 10, 31);
                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT)] = this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT)] + new Vector3(15, 10, 31);
                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_RIGHT)] = this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_RIGHT)] + new Vector3(15, 10 - 420, 31);
                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT)] = this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT)] + new Vector3(15, 10 - 420, 31);
            }
            else if (Global.gLMMCRobot.rbtRobotType == RobotType.LMS_ASSY_TYPE)
            {
                Vector3 vtOri = new Vector3(-5.000002f, 9.550016f, 1.003348f);
                Vector3 vtMove = new Vector3(0, -957, -785);

                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_LEFT)] = vtMove + new Vector3(207, 244.55f + 341.5f, 870.93f + 350) + vtOri;
                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT)] = vtMove + new Vector3(207, 244.55f + 341.5f, 870.93f) + vtOri;
                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_RIGHT)] = vtMove + new Vector3(207, 1670.95f - 341.5f, 870.93f + 350) + vtOri;
                this.arVTCarrierTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT)] = vtMove + new Vector3(207, 1670.95f - 341.5f, 870.93f) + vtOri;

                this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_LEFT)] = vtMove + new Vector3(290.0f, 280.0f, 1214.13f) + vtOri;
                this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT)] = vtMove + new Vector3(290.0f, 280.0f, 1214.13f - 350) + vtOri;
                this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_TOP_RIGHT)] = vtMove + new Vector3(290.0f, 1360.0f + 270, 1214.13f) + vtOri;
                this.arVTRailTargetPos[(int)(VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT)] = vtMove + new Vector3(290.0f, 1360.0f + 270, 1214.13f - 350) + vtOri;
            }
        }


        //private void ProcessArea(ProcessObject obPro)
        //{
        //    Vector3 speed = new Vector3();

        //    if (obPro.OnProcess == true)
        //    {
        //        if (obPro.iTimeProcess == 0)
        //        {
        //            obPro.lkDynamicRail_1.targetPosition = new Vector3(obPro.lkDynamicRail_1.orgin[0], obPro.lkDynamicRail_1.orgin[1], obPro.lkDynamicRail_1.orgin[2]) + new Vector3(200, 0, 0);

        //            if (obPro.lkCarrier != null)
        //                obPro.lkCarrier.targetPosition = new Vector3(obPro.lkDynamicRail_1.orgin[0], obPro.lkDynamicRail_1.orgin[1], obPro.lkDynamicRail_1.orgin[2]) + new Vector3(15, 10, 31);

        //            obPro.iTimeProcess++;
        //        }
        //        else if (obPro.iTimeProcess < 300)
        //        {

        //            if (obPro.lkDynamicRail_1.currPosition != obPro.lkDynamicRail_1.targetPosition)
        //            {
        //                speed = Vector3.Normalize(obPro.lkDynamicRail_1.targetPosition - obPro.lkDynamicRail_1.currPosition) * 5;

        //                obPro.lkDynamicRail_1.transfrom += speed;
        //                obPro.lkDynamicRail_1.currPosition += speed;

        //                obPro.lkDynamicRail_2.transfrom += speed;
        //                obPro.lkDynamicRail_2.currPosition += speed;

        //                obPro.lkDynamicCoil.transfrom += speed;
        //                obPro.lkDynamicCoil.currPosition += speed;
        //            }
        //            else
        //            {
        //                if (obPro.lkDynamicRail_1.targetPosition.X == (obPro.lkDynamicRail_1.orgin[0] + 200))
        //                {
        //                    obPro.lkDynamicRail_1.targetPosition = new Vector3(obPro.lkDynamicRail_1.orgin[0], obPro.lkDynamicRail_1.orgin[1], obPro.lkDynamicRail_1.orgin[2]) + new Vector3(-200, 0, 0);
        //                }
        //                else
        //                    obPro.lkDynamicRail_1.targetPosition = new Vector3(obPro.lkDynamicRail_1.orgin[0], obPro.lkDynamicRail_1.orgin[1], obPro.lkDynamicRail_1.orgin[2]) + new Vector3(200, 0, 0);
        //            }

        //            if (obPro.lkCarrier.currPosition != obPro.lkCarrier.targetPosition)
        //            {
        //                Vector3 carrier_speed = Vector3.Normalize(obPro.lkCarrier.targetPosition - obPro.lkCarrier.currPosition) * 5;

        //                obPro.lkCarrier.transfrom += carrier_speed + speed;
        //                obPro.lkCarrier.currPosition += carrier_speed + speed;
        //                obPro.lkCarrier.targetPosition += speed;
        //            }
        //            else
        //            {
        //                if (obPro.lkCarrier.currPosition.Y == (obPro.lkDynamicRail_1.orgin[1] + 10))
        //                {
        //                    obPro.lkCarrier.targetPosition = obPro.lkCarrier.currPosition + new Vector3(0, -420, 0);
        //                }
        //                else
        //                    obPro.lkCarrier.targetPosition = obPro.lkCarrier.currPosition + new Vector3(0, 420, 0);
        //            }

        //            obPro.iTimeProcess++;
        //        }
        //        else if (obPro.iTimeProcess == 300)
        //        {
        //            obPro.lkDynamicRail_1.targetPosition = new Vector3(obPro.lkDynamicRail_1.orgin[0], obPro.lkDynamicRail_1.orgin[1], obPro.lkDynamicRail_1.orgin[2]);
        //            obPro.iTimeProcess++;
        //        }
        //        else
        //        {
        //            if (obPro.lkDynamicRail_1.currPosition != obPro.lkDynamicRail_1.targetPosition)
        //            {
        //                speed = Vector3.Normalize(obPro.lkDynamicRail_1.targetPosition - obPro.lkDynamicRail_1.currPosition) * 5;

        //                obPro.lkDynamicRail_1.transfrom += speed;
        //                obPro.lkDynamicRail_1.currPosition += speed;

        //                obPro.lkDynamicRail_2.transfrom += speed;
        //                obPro.lkDynamicRail_2.currPosition += speed;

        //                obPro.lkDynamicCoil.transfrom += speed;
        //                obPro.lkDynamicCoil.currPosition += speed;
        //            }
        //            else
        //            {
        //                if ((lklTopRail.Contains(lmShuttleLeft.lkCarrier) == false) && (obPro.lkCarrier != null))
        //                {
        //                    if (lklTopRail.First != null)
        //                        obPro.lkCarrier.targetPosition = lklTopRail.First.Value.currPosition + new Vector3(0, -420, 0);
        //                    else
        //                        obPro.lkCarrier.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT];
        //                    obPro.lkCarrier.speed = new Vector3(0.0f, 10.0f, 0.0f);

        //                    lklTopRail.AddFirst(obPro.lkCarrier);
        //                }

        //                obPro.OnProcess = false;
        //                obPro.iTimeProcess = 0;
        //                obPro.lkCarrier = null;
        //            }

        //            if (obPro.lkCarrier != null)
        //            {
        //                if (obPro.lkCarrier.currPosition != obPro.lkCarrier.targetPosition)
        //                {
        //                    Vector3 carrier_speed = Vector3.Normalize(obPro.lkCarrier.targetPosition - obPro.lkCarrier.currPosition) * 5;

        //                    obPro.lkCarrier.transfrom += carrier_speed + speed;
        //                    obPro.lkCarrier.currPosition += carrier_speed + speed;
        //                    obPro.lkCarrier.targetPosition += speed;
        //                }
        //                else
        //                {
        //                    if (obPro.lkCarrier.currPosition.Y == (obPro.lkDynamicRail_1.orgin[1] + 10))
        //                    {
        //                        obPro.lkCarrier.targetPosition = obPro.lkCarrier.currPosition + new Vector3(0, -420, 0);
        //                    }
        //                    else
        //                        obPro.lkCarrier.targetPosition = obPro.lkCarrier.currPosition + new Vector3(0, 420, 0);
        //                }
        //            }
        //        }
        //    }
        //}
        //public void LMMCType03_Visualization()
        //{
        //    // moving the carriers on bottom rails
        //    LinkedListNode<LinkIndex> currBottomRailLinkNode;
        //    LinkedListNode<LinkIndex> currTopRailLinkNode;

        //    // Check BottomRail Linked list
        //    if (lklBottomRail.Count > 0)
        //    {
        //        for (currBottomRailLinkNode = lklBottomRail.First; currBottomRailLinkNode != null; currBottomRailLinkNode = currBottomRailLinkNode.Next)
        //        {
        //            LinkIndex currlink = currBottomRailLinkNode.Value;

        //            if (currBottomRailLinkNode.Previous == null)
        //            {

        //                if (currlink.currPosition.Y > currlink.targetPosition.Y)
        //                {
        //                    if ((currlink.currPosition.Y + currlink.speed.Y) <= currlink.targetPosition.Y)
        //                    {
        //                        currlink.speed = currlink.targetPosition - currlink.currPosition;
        //                    }
        //                    currlink.transfrom += currlink.speed;
        //                    currlink.currPosition += currlink.speed;
        //                    currlink.bEnableMove = true;
        //                }
        //                else
        //                {
        //                    if (currlink.currPosition == arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT])
        //                    {
        //                        if (lmShuttleLeft.OnRequest == false && ((lmLiftTL.OnRequest == false) || (lmLiftBL.OnRequest == false)))
        //                        {
        //                            lmShuttleLeft.OnRequest = true;
        //                            lmShuttleLeft.lkDynamicRail.targetPosition = arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT] + new Vector3(0, -420, 0);

        //                        }
        //                        else if ((lmShuttleLeft.lkDynamicRail.currPosition == arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT] + new Vector3(0, -420, 0)) && (lmShuttleLeft.lkDynamicRail.currPosition == lmShuttleLeft.lkDynamicRail.targetPosition))
        //                        {

        //                            if (currBottomRailLinkNode.Next != null)
        //                            {
        //                                if ((currBottomRailLinkNode.Next.Value.currPosition + new Vector3(0.0f, -420.0f, 0.0f)) == currlink.currPosition)
        //                                {
        //                                    currBottomRailLinkNode.Next.Value.bEnableMove = false;
        //                                }
        //                            }

        //                            currlink.speed = new Vector3(0.0f, -3.0f, 0.0f);
        //                            currlink.transfrom += currlink.speed;
        //                            currlink.currPosition += currlink.speed;
        //                            currlink.targetPosition += new Vector3(0.0f, -420.0f, 0.0f);
        //                        }
        //                    }
        //                    else // carrier already in the lift
        //                    {
        //                        if (currBottomRailLinkNode.Next != null)
        //                        {
        //                            if (currBottomRailLinkNode.Next.Value.bEnableMove == false)
        //                                currBottomRailLinkNode.Next.Value.bEnableMove = true;
        //                        }

        //                        // Add linkIndex to liftRigh

        //                        lmShuttleLeft.lkCarrier = currlink;

        //                        lmShuttleLeft.OnRequest = true;
        //                        if (lmLiftTL.OnRequest == false)
        //                        {
        //                            lmShuttleLeft.lkDynamicRail.targetPosition = lmLiftTL.lkDynamicRail.currPosition + new Vector3(0, 420, 0);
        //                        }
        //                        else
        //                        {
        //                            lmShuttleLeft.lkDynamicRail.targetPosition = lmLiftBL.lkDynamicRail.currPosition + new Vector3(0, 420, 0);
        //                        }

        //                        lklBottomRail.RemoveFirst();
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (currBottomRailLinkNode.Previous.Value.currPosition.Y >= (arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT].Y - 400))
        //                    currlink.targetPosition = currBottomRailLinkNode.Previous.Value.currPosition + new Vector3(0.0f, 420.0f, 0.0f);
        //                else
        //                    currlink.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT];

        //                if (currlink.bEnableMove == false)
        //                {
        //                    if (Math.Abs(currlink.targetPosition.Y - currlink.currPosition.Y) > 100)
        //                        currlink.bEnableMove = true;
        //                }
        //                else
        //                {
        //                    if (currlink.currPosition.Y > currlink.targetPosition.Y)
        //                    {
        //                        if ((currlink.currPosition.Y + currlink.speed.Y) <= currlink.targetPosition.Y)
        //                        {
        //                            currlink.speed = currlink.targetPosition - currlink.currPosition;
        //                        }
        //                        currlink.transfrom += currlink.speed;
        //                        currlink.currPosition += currlink.speed;
        //                    }

        //                }
        //            }
        //        }
        //    }

        //    // move the LM track shuttle right
        //    if (lmShuttleRight.OnRequest == true)
        //    {

        //        if (lmShuttleRight.lkDynamicRail.currPosition != lmShuttleRight.lkDynamicRail.targetPosition)
        //        {
        //            Vector3 speed = Vector3.Normalize(lmShuttleRight.lkDynamicRail.targetPosition - lmShuttleRight.lkDynamicRail.currPosition) * 5;

        //            if (lmShuttleRight.lkCarrier == null)
        //                speed *= 2;

        //            if (speed.X < 0)
        //            {
        //                if ((lmShuttleRight.lkDynamicRail.currPosition.X + speed.X) <= lmShuttleRight.lkDynamicRail.targetPosition.X)
        //                    speed = lmShuttleRight.lkDynamicRail.targetPosition - lmShuttleRight.lkDynamicRail.currPosition;

        //            }
        //            else
        //            {
        //                if ((lmShuttleRight.lkDynamicRail.currPosition.X + speed.X) >= lmShuttleRight.lkDynamicRail.targetPosition.X)
        //                    speed = lmShuttleRight.lkDynamicRail.targetPosition - lmShuttleRight.lkDynamicRail.currPosition;
        //            }

        //            lmShuttleRight.lkDynamicRail.speed = speed;
        //            lmShuttleRight.lkDynamicRail.transfrom += speed;
        //            lmShuttleRight.lkDynamicRail.currPosition += speed;

        //            lmShuttleRight.lkDynamicCoil.speed = speed;
        //            lmShuttleRight.lkDynamicCoil.transfrom += speed;
        //            lmShuttleRight.lkDynamicCoil.currPosition += speed;

        //            if (lmShuttleRight.lkCarrier != null)
        //            {
        //                lmShuttleRight.lkCarrier.speed = speed;
        //                lmShuttleRight.lkCarrier.transfrom += speed;
        //                lmShuttleRight.lkCarrier.currPosition += speed;
        //            }

        //        }
        //        else
        //        {
        //            if (lmShuttleRight.lkCarrier == null)
        //            {
        //                lmShuttleRight.OnRequest = false;
        //            }
        //            else if ((lmShuttleRight.lkDynamicRail.targetPosition.X) == arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT].X)
        //            {
        //                if (lklBottomRail.Contains(lmShuttleRight.lkCarrier) == false)
        //                {
        //                    if (lklBottomRail.Last != null)
        //                        lmShuttleRight.lkCarrier.targetPosition = lklBottomRail.Last.Value.currPosition + new Vector3(0, 420, 0);
        //                    else
        //                        lmShuttleRight.lkCarrier.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT];
        //                    lmShuttleRight.lkCarrier.speed = new Vector3(0.0f, -7.0f, 0.0f);
        //                    lklBottomRail.AddLast(lmShuttleRight.lkCarrier);
        //                }
        //                else
        //                {
        //                    if (lmShuttleRight.lkCarrier.currPosition.Y <= arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT].Y)
        //                    {
        //                        lmShuttleRight.OnRequest = false;
        //                        lmShuttleRight.lkCarrier = null;
        //                    }

        //                }

        //            }
        //        }
        //    }

        //    // Moving the carrier on the Top rails
        //    if (lklTopRail.Count > 0)
        //    {
        //        for (currTopRailLinkNode = lklTopRail.Last; currTopRailLinkNode != null; currTopRailLinkNode = currTopRailLinkNode.Previous)
        //        {
        //            LinkIndex currlink = currTopRailLinkNode.Value;

        //            if (currTopRailLinkNode.Next == null)
        //            {
        //                if (currlink.currPosition.Y < currlink.targetPosition.Y)
        //                {
        //                    if ((currlink.currPosition.Y + currlink.speed.Y) >= currlink.targetPosition.Y)
        //                    {
        //                        currlink.speed = currlink.targetPosition - currlink.currPosition;
        //                    }
        //                    currlink.transfrom += currlink.speed;
        //                    currlink.currPosition += currlink.speed;
        //                    currlink.bEnableMove = true;
        //                }
        //                else
        //                {
        //                    if (currlink.currPosition == arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT])
        //                    {
        //                        if (lmShuttleRight.OnRequest == false)
        //                        {
        //                            lmShuttleRight.OnRequest = true;
        //                            lmShuttleRight.lkDynamicRail.targetPosition = arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT];

        //                        }
        //                        else if (lmShuttleRight.lkDynamicRail.currPosition == arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT])
        //                        {

        //                            if (currTopRailLinkNode.Previous != null)
        //                            {
        //                                if ((currTopRailLinkNode.Previous.Value.currPosition.Y + 420) >= currlink.currPosition.Y)
        //                                {
        //                                    currTopRailLinkNode.Previous.Value.bEnableMove = false;
        //                                }
        //                            }

        //                            currlink.speed = new Vector3(0.0f, 4.0f, 0.0f);
        //                            currlink.transfrom += currlink.speed;
        //                            currlink.currPosition += currlink.speed;
        //                            currlink.targetPosition += new Vector3(0.0f, 420.0f, 0.0f);
        //                        }
        //                    }
        //                    else // carrier already in the lift
        //                    {
        //                        if (currTopRailLinkNode.Previous != null)
        //                        {
        //                            if (currTopRailLinkNode.Previous.Value.bEnableMove == false)
        //                                currTopRailLinkNode.Previous.Value.bEnableMove = true;
        //                        }

        //                        // Add linkIndex to liftRigh
        //                        lmShuttleRight.lkCarrier = currlink;
        //                        lmShuttleRight.OnRequest = true;
        //                        lmShuttleRight.lkDynamicRail.targetPosition = arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT];
        //                        lklTopRail.RemoveLast();
        //                        lmShuttleRight.lkCarrier.bEnableMove = false;

        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (currTopRailLinkNode.Next.Value.currPosition.Y <= (arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT].Y + 400))
        //                    currlink.targetPosition = currTopRailLinkNode.Next.Value.currPosition + new Vector3(0.0f, -410.0f, 0.0f);
        //                else
        //                    currlink.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT];

        //                if (currlink.bEnableMove == false)
        //                {
        //                    if ((currlink.targetPosition.Y - currlink.currPosition.Y) > 200)
        //                        currlink.bEnableMove = true;
        //                }
        //                else
        //                {
        //                    if (currlink.currPosition.Y < currlink.targetPosition.Y)
        //                    {
        //                        if ((currlink.currPosition.Y + currlink.speed.Y) >= currlink.targetPosition.Y)
        //                        {
        //                            currlink.speed = currlink.targetPosition - currlink.currPosition;
        //                        }
        //                        currlink.transfrom += currlink.speed;
        //                        currlink.currPosition += currlink.speed;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    // move the LMTrack shuttle left
        //    if (lmShuttleLeft.OnRequest == true)
        //    {

        //        if (lmShuttleLeft.lkDynamicRail.currPosition != lmShuttleLeft.lkDynamicRail.targetPosition)
        //        {
        //            Vector3 speed = Vector3.Normalize(lmShuttleLeft.lkDynamicRail.targetPosition - lmShuttleLeft.lkDynamicRail.currPosition) * 5;

        //            if (lmShuttleLeft.lkCarrier == null)
        //            {
        //                speed *= 2;
        //            }

        //            if (speed.X < 0)
        //            {
        //                if ((lmShuttleLeft.lkDynamicRail.currPosition.X + speed.X) <= lmShuttleLeft.lkDynamicRail.targetPosition.X)
        //                    speed = lmShuttleLeft.lkDynamicRail.targetPosition - lmShuttleLeft.lkDynamicRail.currPosition;

        //            }
        //            else
        //            {
        //                if ((lmShuttleLeft.lkDynamicRail.currPosition.X + speed.X) >= lmShuttleLeft.lkDynamicRail.targetPosition.X)
        //                    speed = lmShuttleLeft.lkDynamicRail.targetPosition - lmShuttleLeft.lkDynamicRail.currPosition;
        //            }

        //            lmShuttleLeft.lkDynamicRail.speed = speed;
        //            lmShuttleLeft.lkDynamicRail.transfrom += speed;
        //            lmShuttleLeft.lkDynamicRail.currPosition += speed;

        //            lmShuttleLeft.lkDynamicCoil.speed = speed;
        //            lmShuttleLeft.lkDynamicCoil.transfrom += speed;
        //            lmShuttleLeft.lkDynamicCoil.currPosition += speed;

        //            if (lmShuttleLeft.lkCarrier != null)
        //            {
        //                lmShuttleLeft.lkCarrier.speed = speed;
        //                lmShuttleLeft.lkCarrier.transfrom += speed;
        //                lmShuttleLeft.lkCarrier.currPosition += speed;
        //            }
        //        }
        //        else
        //        {

        //            if ((lmShuttleLeft.lkDynamicRail.targetPosition + new Vector3(0, -420, 0)) == lmLiftBL.lkDynamicRail.currPosition)
        //            {
        //                if (lmShuttleLeft.lkCarrier != null)
        //                {
        //                    if (lmShuttleLeft.lkCarrier.currPosition != (lmLiftBL.lkDynamicRail.currPosition + new Vector3(15, 10, 31)))
        //                    {
        //                        lmShuttleLeft.lkCarrier.transfrom += new Vector3(0, -5, 0);
        //                        lmShuttleLeft.lkCarrier.currPosition += new Vector3(0, -5, 0);
        //                    }
        //                    else
        //                    {
        //                        lmLiftBL.lkCarrier = lmShuttleLeft.lkCarrier;
        //                        lmLiftBL.OnRequest = true;
        //                        lmLiftBL.bFinishProcess = false;
        //                        lmLiftBL.lkDynamiclmCarrier.targetPosition = lmLiftBL.lkDynamiclmCarrier.currPosition + new Vector3(0, 0, -700);
        //                        lmShuttleLeft.lkCarrier = null;
        //                        lmShuttleLeft.OnRequest = false;
        //                    }
        //                }
        //            }
        //            else if ((lmShuttleLeft.lkDynamicRail.targetPosition + new Vector3(0, -420, 0)) == lmLiftTL.lkDynamicRail.currPosition)
        //            {
        //                if (lmShuttleLeft.lkCarrier != null)
        //                {
        //                    if (lmShuttleLeft.lkCarrier.currPosition != (lmLiftTL.lkDynamicRail.currPosition + new Vector3(15, 10, 31)))
        //                    {
        //                        lmShuttleLeft.lkCarrier.transfrom += new Vector3(0, -5, 0);
        //                        lmShuttleLeft.lkCarrier.currPosition += new Vector3(0, -5, 0);
        //                    }
        //                    else
        //                    {
        //                        lmLiftTL.lkCarrier = lmShuttleLeft.lkCarrier;
        //                        lmLiftTL.OnRequest = true;
        //                        lmLiftTL.bFinishProcess = false;
        //                        lmLiftTL.lkDynamiclmCarrier.targetPosition = lmLiftTL.lkDynamiclmCarrier.currPosition + new Vector3(0, 0, -700);
        //                        lmShuttleLeft.lkCarrier = null;
        //                        lmShuttleLeft.OnRequest = false;
        //                    }
        //                }
        //            }
        //            else if (((lmShuttleLeft.lkDynamicRail.targetPosition + new Vector3(0, 420, 0)) == arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT]) && (lmShuttleLeft.lkCarrier != null))
        //            {
        //                //if ((lklTopRail.Contains(lmShuttleLeft.lkCarrier) == false) && (lmShuttleLeft.lkCarrier != null))
        //                //{
        //                //    if (lklTopRail.First != null)
        //                //        lmShuttleLeft.lkCarrier.targetPosition = lklTopRail.First.Value.currPosition + new Vector3(0, -420, 0);
        //                //    else
        //                //        lmShuttleLeft.lkCarrier.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT];
        //                //    lmShuttleLeft.lkCarrier.speed = new Vector3(0.0f, 10.0f, 0.0f);

        //                //    lklTopRail.AddFirst(lmShuttleLeft.lkCarrier);
        //                //}
        //                //else
        //                //{
        //                //    if (lmShuttleLeft.lkCarrier.currPosition.Y >= arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT].Y)
        //                //    {
        //                //        lmShuttleLeft.OnRequest = false;
        //                //        lmShuttleLeft.lkCarrier = null;
        //                //    }
        //                //}
        //                if (obProcess.OnProcess == false)
        //                {
        //                    if (lmShuttleLeft.lkCarrier.currPosition.Y < arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT].Y)
        //                    {
        //                        lmShuttleLeft.lkCarrier.transfrom += new Vector3(0, 5, 0);
        //                        lmShuttleLeft.lkCarrier.currPosition += new Vector3(0, 5, 0);
        //                    }
        //                    else
        //                    {
        //                        obProcess.lkCarrier = lmShuttleLeft.lkCarrier;
        //                        obProcess.OnProcess = true;
        //                        obProcess.iTimeProcess = 0;
        //                        lmShuttleLeft.lkCarrier = null;
        //                        lmShuttleLeft.OnRequest = false;
        //                    }
        //                }

        //            }
        //        }
        //    }

        //    // move the LMTrack Lift Top left
        //    if (lmLiftTL.OnRequest == true)
        //    {
        //        if (lmLiftTL.bFinishProcess == false)
        //        {
        //            if (lmLiftTL.lkDynamiclmCarrier.currPosition != lmLiftTL.lkDynamiclmCarrier.targetPosition)
        //            {
        //                Vector3 speed = Vector3.Normalize(lmLiftTL.lkDynamiclmCarrier.targetPosition - lmLiftTL.lkDynamiclmCarrier.currPosition) * 7;
        //                lmLiftTL.lkDynamicRail.transfrom += speed;
        //                lmLiftTL.lkDynamicRail.currPosition += speed;

        //                lmLiftTL.lkDynamicCoil.transfrom += speed;
        //                lmLiftTL.lkDynamicCoil.currPosition += speed;

        //                lmLiftTL.lkDynamicLmshuttle.transfrom += speed;
        //                lmLiftTL.lkDynamicLmshuttle.currPosition += speed;

        //                lmLiftTL.lkDynamiclmCarrier.transfrom += speed;
        //                lmLiftTL.lkDynamiclmCarrier.currPosition += speed;

        //                lmLiftTL.lkCarrier.transfrom += speed;
        //                lmLiftTL.lkCarrier.currPosition += speed;
        //            }
        //            else
        //            {
        //                if (lmLiftTL.lkDynamiclmCarrier.currPosition.Z == (lmLiftTL.lkDynamiclmCarrier.orgin[2] - 700))
        //                {
        //                    if ((lmLiftTL.iTimeProcess == 0) && (lmLiftTL.bFinishProcess == false))
        //                    {
        //                        lmLiftTL.iTimeProcess += 1;
        //                        lmLiftTL.lkCarrier.targetPosition = new Vector3(lmLiftTL.lkDynamiclmCarrier.orgin[0] + 200 - 150, lmLiftTL.lkCarrier.currPosition.Y, lmLiftTL.lkCarrier.currPosition.Z);
        //                    }
        //                    else if (lmLiftTL.iTimeProcess < 200)
        //                    {
        //                        if (lmLiftTL.lkCarrier.currPosition != lmLiftTL.lkCarrier.targetPosition)
        //                        {
        //                            Vector3 speed = Vector3.Normalize(lmLiftTL.lkCarrier.targetPosition - lmLiftTL.lkCarrier.currPosition) * 10;

        //                            lmLiftTL.lkCarrier.transfrom += speed;
        //                            lmLiftTL.lkCarrier.currPosition += speed;

        //                            lmLiftTL.lkDynamicRail.transfrom += speed;
        //                            lmLiftTL.lkDynamicRail.currPosition += speed;

        //                            lmLiftTL.lkDynamicCoil.transfrom += speed;
        //                            lmLiftTL.lkDynamicCoil.currPosition += speed;
        //                        }
        //                        else
        //                        {
        //                            if (lmLiftTL.lkCarrier.currPosition.X == (lmLiftTL.lkDynamiclmCarrier.orgin[0] + 120 - 150))
        //                            {
        //                                lmLiftTL.lkCarrier.targetPosition = new Vector3(lmLiftTL.lkDynamiclmCarrier.orgin[0] + 120 + 150, lmLiftTL.lkCarrier.currPosition.Y, lmLiftTL.lkCarrier.currPosition.Z);
        //                            }
        //                            else
        //                                lmLiftTL.lkCarrier.targetPosition = new Vector3(lmLiftTL.lkDynamiclmCarrier.orgin[0] + 120 - 150, lmLiftTL.lkCarrier.currPosition.Y, lmLiftTL.lkCarrier.currPosition.Z);
        //                        }

        //                        lmLiftTL.iTimeProcess++;
        //                    }
        //                    else
        //                    {
        //                        lmLiftTL.iTimeProcess = 0;
        //                        lmLiftTL.lkDynamiclmCarrier.targetPosition = new Vector3(lmLiftTL.lkDynamiclmCarrier.orgin[0], lmLiftTL.lkDynamiclmCarrier.orgin[1], lmLiftTL.lkDynamiclmCarrier.orgin[2]);
        //                        // lmLiftBL.bFinishProcess = true;
        //                    }
        //                }
        //                else if (lmLiftTL.lkDynamiclmCarrier.currPosition.Z == lmLiftTL.lkDynamiclmCarrier.orgin[2])
        //                {
        //                    lmLiftTL.bFinishProcess = true;
        //                }

        //            }
        //        }
        //        else if (lmLiftTL.bFinishProcess == true)
        //        {
        //            if (lmShuttleLeft.OnRequest == false)
        //            {
        //                lmShuttleLeft.lkDynamicRail.targetPosition = lmLiftTL.lkDynamicRail.currPosition + new Vector3(0, 420, 0);
        //                lmShuttleLeft.OnRequest = true;
        //            }
        //            else if (lmShuttleLeft.lkDynamicRail.currPosition == (lmLiftTL.lkDynamicRail.currPosition + new Vector3(0, 420, 0)))
        //            {
        //                if (lmLiftTL.lkCarrier.currPosition != (lmShuttleLeft.lkDynamicRail.currPosition + new Vector3(15, 10, 31)))
        //                {
        //                    lmLiftTL.lkCarrier.transfrom += new Vector3(0, 5, 0);
        //                    lmLiftTL.lkCarrier.currPosition += new Vector3(0, 5, 0);
        //                }
        //                else
        //                {
        //                    lmShuttleLeft.lkCarrier = lmLiftTL.lkCarrier;
        //                    lmShuttleLeft.lkDynamicRail.targetPosition = arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT] + new Vector3(0, -420, 0);
        //                    lmLiftTL.lkCarrier = null;
        //                    lmLiftTL.OnRequest = false;
        //                    lmLiftTL.bFinishProcess = false;
        //                }
        //            }
        //        }

        //    }

        //    // move the LMTrack Lift bottom left
        //    if (lmLiftBL.OnRequest == true)
        //    {
        //        if (lmLiftBL.bFinishProcess == false)
        //        {
        //            if (lmLiftBL.lkDynamiclmCarrier.currPosition != lmLiftBL.lkDynamiclmCarrier.targetPosition)
        //            {
        //                Vector3 speed = Vector3.Normalize(lmLiftBL.lkDynamiclmCarrier.targetPosition - lmLiftBL.lkDynamiclmCarrier.currPosition) * 7;
        //                lmLiftBL.lkDynamicRail.transfrom += speed;
        //                lmLiftBL.lkDynamicRail.currPosition += speed;

        //                lmLiftBL.lkDynamicCoil.transfrom += speed;
        //                lmLiftBL.lkDynamicCoil.currPosition += speed;

        //                lmLiftBL.lkDynamicLmshuttle.transfrom += speed;
        //                lmLiftBL.lkDynamicLmshuttle.currPosition += speed;

        //                lmLiftBL.lkDynamiclmCarrier.transfrom += speed;
        //                lmLiftBL.lkDynamiclmCarrier.currPosition += speed;

        //                lmLiftBL.lkCarrier.transfrom += speed;
        //                lmLiftBL.lkCarrier.currPosition += speed;
        //            }
        //            else
        //            {
        //                if (lmLiftBL.lkDynamiclmCarrier.currPosition.Z == (lmLiftBL.lkDynamiclmCarrier.orgin[2] - 700))
        //                {
        //                    if ((lmLiftBL.iTimeProcess == 0) && (lmLiftBL.bFinishProcess == false))
        //                    {
        //                        lmLiftBL.iTimeProcess += 1;
        //                        lmLiftBL.lkDynamicRail.targetPosition = new Vector3(lmLiftBL.lkDynamiclmCarrier.orgin[0] + 150 - 120, lmLiftBL.lkDynamicRail.currPosition.Y, lmLiftBL.lkDynamicRail.currPosition.Z);
        //                    }
        //                    else if (lmLiftBL.iTimeProcess < 200)
        //                    {

        //                        //Vector3 speed = new Vector3(5, 0, 0) * random_bl.Next(-2, 2);
        //                        //if (Math.Abs(lmLiftBL.lkCarrier.currPosition.X + speed.X - (lmLiftBL.lkDynamiclmCarrier.orgin[0] + 200)) <= 300)
        //                        //{
        //                        //    lmLiftBL.lkCarrier.transfrom += speed;
        //                        //    lmLiftBL.lkCarrier.currPosition += speed;

        //                        //    lmLiftBL.lkDynamicRail.transfrom += speed;
        //                        //    lmLiftBL.lkDynamicRail.currPosition += speed;

        //                        //    lmLiftBL.lkDynamicCoil.transfrom += speed;
        //                        //    lmLiftBL.lkDynamicCoil.currPosition += speed;
        //                        //}

        //                        if (lmLiftBL.lkDynamicRail.currPosition != lmLiftBL.lkDynamicRail.targetPosition)
        //                        {
        //                            Vector3 speed = Vector3.Normalize(lmLiftBL.lkDynamicRail.targetPosition - lmLiftBL.lkDynamicRail.currPosition) * 5;

        //                            lmLiftBL.lkCarrier.transfrom += speed;
        //                            lmLiftBL.lkCarrier.currPosition += speed;

        //                            lmLiftBL.lkDynamicRail.transfrom += speed;
        //                            lmLiftBL.lkDynamicRail.currPosition += speed;

        //                            lmLiftBL.lkDynamicCoil.transfrom += speed;
        //                            lmLiftBL.lkDynamicCoil.currPosition += speed;
        //                        }
        //                        else
        //                        {
        //                            if (lmLiftBL.lkDynamicRail.currPosition.X == (lmLiftBL.lkDynamiclmCarrier.orgin[0] + 150 - 120))
        //                            {
        //                                lmLiftBL.lkDynamicRail.targetPosition = new Vector3(lmLiftBL.lkDynamiclmCarrier.orgin[0] + 150 + 120, lmLiftBL.lkDynamicRail.currPosition.Y, lmLiftBL.lkDynamicRail.currPosition.Z);
        //                            }
        //                            else
        //                                lmLiftBL.lkDynamicRail.targetPosition = new Vector3(lmLiftBL.lkDynamiclmCarrier.orgin[0] + 150 - 120, lmLiftBL.lkDynamicRail.currPosition.Y, lmLiftBL.lkDynamicRail.currPosition.Z);
        //                        }

        //                        lmLiftBL.iTimeProcess++;
        //                    }
        //                    else
        //                    {
        //                        lmLiftBL.iTimeProcess = 0;
        //                        lmLiftBL.lkDynamiclmCarrier.targetPosition = new Vector3(lmLiftBL.lkDynamiclmCarrier.orgin[0], lmLiftBL.lkDynamiclmCarrier.orgin[1], lmLiftBL.lkDynamiclmCarrier.orgin[2]);
        //                    }
        //                }
        //                else if (lmLiftBL.lkDynamiclmCarrier.currPosition.Z == lmLiftBL.lkDynamiclmCarrier.orgin[2])
        //                {
        //                    lmLiftBL.bFinishProcess = true;
        //                }

        //            }
        //        }
        //        else if (lmLiftBL.bFinishProcess == true)
        //        {
        //            if (lmShuttleLeft.OnRequest == false)
        //            {
        //                lmShuttleLeft.lkDynamicRail.targetPosition = lmLiftBL.lkDynamicRail.currPosition + new Vector3(0, 420, 0);
        //                lmShuttleLeft.OnRequest = true;
        //            }
        //            else if (lmShuttleLeft.lkDynamicRail.currPosition == (lmLiftBL.lkDynamicRail.currPosition + new Vector3(0, 420, 0)))
        //            {
        //                if (lmLiftBL.lkCarrier.currPosition != (lmShuttleLeft.lkDynamicRail.currPosition + new Vector3(15, 10, 31)))
        //                {
        //                    lmLiftBL.lkCarrier.transfrom += new Vector3(0, 5, 0);
        //                    lmLiftBL.lkCarrier.currPosition += new Vector3(0, 5, 0);
        //                }
        //                else
        //                {
        //                    lmShuttleLeft.lkCarrier = lmLiftBL.lkCarrier;
        //                    lmShuttleLeft.lkDynamicRail.targetPosition = arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT] + new Vector3(0, -420, 0);
        //                    lmLiftBL.lkCarrier = null;
        //                    lmLiftBL.OnRequest = false;
        //                    lmLiftBL.bFinishProcess = false;
        //                }
        //            }
        //        }
        //    }

        //    ProcessArea(obProcess);
        //}
        public void LMMCType02_Visualization()
        {
            LinkedListNode<VisualObject> currBottomRailLinkNode;
            LinkedListNode<VisualObject> currTopRailLinkNode;

            int index = 0;

            for (currBottomRailLinkNode = lklBottomRail.Last; currBottomRailLinkNode != null; currBottomRailLinkNode = currBottomRailLinkNode.Previous, index++)
            {
                VisualObject currlink = currBottomRailLinkNode.Value;

                if (currBottomRailLinkNode.Next == null)
                {
                    Vector3 vtCurrPos = currlink.currPosition;
                    Vector3 vtTargetPos = currlink.targetPosition;

                    if (vtCurrPos != vtTargetPos)
                    {
                        Vector3 speed = Vector3.Normalize(vtTargetPos - vtCurrPos) * 15;
                        if ((speed.Y * (speed.Y + vtCurrPos.Y - vtTargetPos.Y)) > 0)
                            speed = vtTargetPos - vtCurrPos;

                        currlink.Update(speed);
                    }
                    else
                    {
                        if (currlink.currPosition == arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT])
                        {
                            if (lmShuttleRight.OnRequest == false)
                            {
                                lmShuttleRight.OnRequest = true;
                                lmShuttleRight.shuttleOb.ElementAt(0).targetPosition = arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT];

                            }
                            else if (lmShuttleRight.shuttleOb.ElementAt(0).currPosition == arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT])
                            {

                                if (currBottomRailLinkNode.Previous != null)
                                {
                                    if ((currBottomRailLinkNode.Previous.Value.currPosition.Y + 420) >= currlink.currPosition.Y)
                                    {
                                        currBottomRailLinkNode.Previous.Value.bEnableMove = false;
                                    }
                                }

                                Vector3 speed = new Vector3(0.0f, 3.0f, 0.0f);

                                currlink.Update(speed);
                                currlink.targetPosition += new Vector3(0.0f, 420.0f, 0.0f);
                            }
                        }
                        else // carrier already in the lift
                        {
                            if (currBottomRailLinkNode.Previous != null)
                            {
                                if (currBottomRailLinkNode.Previous.Value.bEnableMove == false)
                                    currBottomRailLinkNode.Previous.Value.bEnableMove = true;
                            }

                            // Add linkIndex to liftRigh
                            lmShuttleRight.shuttleOb.AddLast(currlink);
                            lmShuttleRight.OnRequest = true;
                            lmShuttleRight.shuttleOb.ElementAt(0).targetPosition = arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT];
                            lklBottomRail.RemoveLast();
                        }
                    }
                }
                else
                {
                    if (currBottomRailLinkNode.Next.Value.currPosition.Y <= (arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT].Y + 400))
                        currlink.targetPosition = currBottomRailLinkNode.Next.Value.currPosition - new Vector3(0.0f, 420.0f, 0.0f);
                    else
                        currlink.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT];

                    if (currlink.bEnableMove == false)
                    {
                        if ((currlink.targetPosition.Y - currlink.currPosition.Y) > 420)
                            currlink.bEnableMove = true;
                    }
                    else
                    {
                        if (currlink.currPosition != currlink.targetPosition)
                        {
                            Vector3 speed = Vector3.Normalize(currlink.targetPosition - currlink.currPosition) * 15;
                            if ((speed.Y * (speed.Y + currlink.currPosition.Y - currlink.targetPosition.Y)) > 0)
                                speed = currlink.targetPosition - currlink.currPosition;

                            currlink.Update(speed);
                        }
                    }
                }
            }

            // move the lift right
            if (lmShuttleRight.OnRequest == true)
            {
                Vector3 vtCurrPos = lmShuttleRight.shuttleOb.ElementAt(0).currPosition;
                Vector3 vtTargetPos = lmShuttleRight.shuttleOb.ElementAt(0).targetPosition;

                if (vtCurrPos != vtTargetPos)
                {
                    Vector3 speed = Vector3.Normalize(vtTargetPos - vtCurrPos) * 5;
                    if ((speed.Z * (speed.Z + vtCurrPos.Z - vtTargetPos.Z)) > 0)
                        speed = vtTargetPos - vtCurrPos;

                    foreach (var visualOb in lmShuttleRight.shuttleOb)
                        visualOb.Update(speed);
                }
                else
                {
                    if ((lmShuttleRight.shuttleOb.Last.Value.currPosition.Y - 420) == arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT].Y)
                    {

                        if (lklTopRail.Last != null)
                            lmShuttleRight.shuttleOb.Last.Value.targetPosition = lklTopRail.Last.Value.currPosition;
                        else
                            lmShuttleRight.shuttleOb.Last.Value.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT];

                        if (lklTopRail.Contains(lmShuttleRight.shuttleOb.Last.Value) == false)
                            lklTopRail.AddLast(lmShuttleRight.shuttleOb.Last.Value);

                    }
                    else if (lmShuttleRight.shuttleOb.Last.Value.currPosition.Y < arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT].Y)
                    {
                        lmShuttleRight.shuttleOb.RemoveLast();
                        lmShuttleRight.OnRequest = false;
                    }

                }

            }

            // Moving the carrier on the Top rails
            for (currTopRailLinkNode = lklTopRail.First; currTopRailLinkNode != null; currTopRailLinkNode = currTopRailLinkNode.Next, index++)
            {
                VisualObject currlink = currTopRailLinkNode.Value;

                if (currTopRailLinkNode.Previous == null)
                {
                    Vector3 vtCurrPos = currlink.currPosition;
                    Vector3 vtTargetPos = currlink.targetPosition;

                    if (vtCurrPos != vtTargetPos)
                    {
                        Vector3 speed = Vector3.Normalize(vtTargetPos - vtCurrPos) * 15;
                        if ((speed.Y * (speed.Y + vtCurrPos.Y - vtTargetPos.Y)) > 0)
                            speed = vtTargetPos - vtCurrPos;

                        currlink.Update(speed);
                    }
                    else
                    {
                        if (currlink.currPosition == arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT])
                        {
                            if (lmShuttleLeft.OnRequest == false)
                            {
                                lmShuttleLeft.OnRequest = true;
                                lmShuttleLeft.shuttleOb.First.Value.targetPosition = arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT];

                            }
                            else if (lmShuttleLeft.shuttleOb.First.Value.currPosition == arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT])
                            {

                                if (currTopRailLinkNode.Next != null)
                                {
                                    if ((currTopRailLinkNode.Next.Value.currPosition.Y - 420) <= currlink.currPosition.Y)
                                    {
                                        currTopRailLinkNode.Next.Value.bEnableMove = false;
                                    }
                                }

                                Vector3 speed = new Vector3(0.0f, -4.0f, 0.0f);
                                currlink.Update(speed);
                                currlink.targetPosition += new Vector3(0.0f, -420.0f, 0.0f);
                            }
                        }
                        else // carrier already in the lift
                        {
                            if (currTopRailLinkNode.Next != null)
                            {
                                if (currTopRailLinkNode.Next.Value.bEnableMove == false)
                                    currTopRailLinkNode.Next.Value.bEnableMove = true;
                            }

                            // Add linkIndex to liftRigh
                            lmShuttleLeft.shuttleOb.AddLast(currlink);
                            lmShuttleLeft.OnRequest = true;
                            lmShuttleLeft.shuttleOb.First.Value.targetPosition = arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT];
                            lklTopRail.RemoveFirst();
                        }
                    }
                }
                else
                {
                    if (currTopRailLinkNode.Previous.Value.currPosition.Y >= (arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT].Y - 400))
                        currlink.targetPosition = currTopRailLinkNode.Previous.Value.currPosition + new Vector3(0.0f, 420.0f, 0.0f);
                    else
                        currlink.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT];

                    if (currlink.bEnableMove == false)
                    {
                        if ((currlink.targetPosition.Y - currlink.currPosition.Y) < -420)
                            currlink.bEnableMove = true;
                    }
                    else
                    {
                        if (currlink.currPosition != currlink.targetPosition)
                        {
                            Vector3 speed = Vector3.Normalize(currlink.targetPosition - currlink.currPosition) * 15;
                            if ((speed.Y * (speed.Y + currlink.currPosition.Y - currlink.targetPosition.Y)) > 0)
                                speed = currlink.targetPosition - currlink.currPosition;

                            currlink.Update(speed);
                        }
                    }
                }
            }

            // move the lift left
            if (lmShuttleLeft.OnRequest == true)
            {
                Vector3 vtCurrPos = lmShuttleLeft.shuttleOb.ElementAt(0).currPosition;
                Vector3 vtTargetPos = lmShuttleLeft.shuttleOb.ElementAt(0).targetPosition;

                if (vtCurrPos != vtTargetPos)
                {
                    Vector3 speed = Vector3.Normalize(vtTargetPos - vtCurrPos) * 5;
                    if ((speed.Z * (speed.Z + vtCurrPos.Z - vtTargetPos.Z)) > 0)
                        speed = vtTargetPos - vtCurrPos;

                    foreach (var visualOb in lmShuttleLeft.shuttleOb)
                        visualOb.Update(speed);
                }
                else
                {
                    if ((lmShuttleLeft.shuttleOb.Last.Value.currPosition.Y + 420) == arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT].Y)
                    {

                        if (lklBottomRail.First != null)
                            lmShuttleLeft.shuttleOb.Last.Value.targetPosition = lklBottomRail.First.Value.currPosition;
                        else
                            lmShuttleLeft.shuttleOb.Last.Value.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT];


                        if (lklBottomRail.Contains(lmShuttleLeft.shuttleOb.Last.Value) == false)
                            lklBottomRail.AddFirst(lmShuttleLeft.shuttleOb.Last.Value);

                    }
                    else if (lmShuttleLeft.shuttleOb.Last.Value.currPosition.Y > arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT].Y)
                    {
                        lmShuttleLeft.shuttleOb.RemoveLast();
                        lmShuttleLeft.OnRequest = false;
                    }
                }
            }
        }

        public void LMMCType01_Visualization()
        {
            LinkedListNode<VisualObject> currTopRailLinkNode;

            for (currTopRailLinkNode = lklTopRail.Last; currTopRailLinkNode != null; currTopRailLinkNode = currTopRailLinkNode.Previous)
            {
                VisualObject currlink = currTopRailLinkNode.Value;

                if (currTopRailLinkNode.Next == null)
                {
                    if (currlink.currPosition != currlink.targetPosition)
                    {
                        Vector3 speed = Vector3.Normalize(currlink.targetPosition - currlink.currPosition) * 10;

                        if ((currlink.currPosition.Y + speed.Y - currlink.targetPosition.Y) * speed.Y >= 0)
                        {
                            speed = currlink.targetPosition - currlink.currPosition;
                        }
                        currlink.Update(speed);
                    }
                    else
                    {
                        //if (currlink.currPosition == arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT])
                        //{
                        //    currlink.targetPosition = currTopRailLinkNode.Previous.Value.currPosition + new Vector3(0.0f, 420.0f, 0.0f) + currlink.speed;
                        //}
                        //else
                        //{
                        //    currlink.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT];
                        //}

                    }
                }
                else
                {
                    currlink.targetPosition = currTopRailLinkNode.Next.Value.currPosition + new Vector3(0, -420, 0);

                    if (currlink.currPosition != currlink.targetPosition)
                    {
                        Vector3 speed = Vector3.Normalize(currlink.targetPosition - currlink.currPosition) * 10;

                        if ((currlink.currPosition.Y + speed.Y - currlink.targetPosition.Y) * speed.Y >= 0)
                        {
                            speed = currlink.targetPosition - currlink.currPosition;
                        }
                        currlink.Update(speed);
                    }
                    else
                    {

                    }

                }

            }
        }

        public void LMS_ASSY_Visualization()
        {
            LinkedListNode<VisualObject> currBottomRailLinkNode;
            LinkedListNode<VisualObject> currTopRailLinkNode;
            double[] arrCarr = new double[4];
            double[] arrLMTrack = new double[6];

            int index = 0;

            for (currBottomRailLinkNode = lklBottomRail.Last; currBottomRailLinkNode != null; currBottomRailLinkNode = currBottomRailLinkNode.Previous, index++)
            {
                VisualObject currlink = currBottomRailLinkNode.Value;

                if (currBottomRailLinkNode.Next == null)
                {
                    Vector3 vtCurrPos = currlink.currPosition;
                    Vector3 vtTargetPos = currlink.targetPosition;

                    if (vtCurrPos != vtTargetPos)
                    {
                        Vector3 speed = Vector3.Normalize(vtTargetPos - vtCurrPos) * 10;
                        if ((speed.Y * (speed.Y + vtCurrPos.Y - vtTargetPos.Y)) > 0)
                            speed = vtTargetPos - vtCurrPos;

                        currlink.Update(speed);
                    }
                    else
                    {
                        if (currlink.currPosition == arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT])
                        {
                            if (lmShuttleRight.OnRequest == false)
                            {
                                lmShuttleRight.OnRequest = true;
                                lmShuttleRight.shuttleOb.ElementAt(0).targetPosition = arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT];

                            }
                            else if (lmShuttleRight.shuttleOb.ElementAt(0).currPosition == arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT])
                            {

                                if (currBottomRailLinkNode.Previous != null)
                                {
                                    if ((currBottomRailLinkNode.Previous.Value.currPosition.Y + 340) >= currlink.currPosition.Y)
                                    {
                                        currBottomRailLinkNode.Previous.Value.bEnableMove = false;
                                    }
                                }

                                Vector3 speed = new Vector3(0.0f, 4.0f, 0.0f);

                                currlink.Update(speed);
                                currlink.targetPosition += new Vector3(0.0f, 341.5f, 0.0f);
                            }
                        }
                        else // carrier already in the lift
                        {
                            if (currBottomRailLinkNode.Previous != null)
                            {
                                if (currBottomRailLinkNode.Previous.Value.bEnableMove == false)
                                    currBottomRailLinkNode.Previous.Value.bEnableMove = true;
                            }

                            // Add linkIndex to liftRigh
                            lmShuttleRight.shuttleOb.AddLast(currlink);
                            lmShuttleRight.OnRequest = true;
                            lmShuttleRight.shuttleOb.ElementAt(0).targetPosition = arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT];
                            lklBottomRail.RemoveLast();
                        }
                    }
                }
                else
                {
                    if (currBottomRailLinkNode.Next.Value.currPosition.Y <= (arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT].Y + 340))
                        currlink.targetPosition = currBottomRailLinkNode.Next.Value.currPosition - new Vector3(0.0f, 341.5f, 0.0f);
                    else
                        currlink.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT];

                    if (currlink.bEnableMove == false)
                    {
                        if ((currlink.targetPosition.Y - currlink.currPosition.Y) > 100)
                            currlink.bEnableMove = true;
                    }
                    else
                    {
                        if (currlink.currPosition != currlink.targetPosition)
                        {
                            Vector3 speed = Vector3.Normalize(currlink.targetPosition - currlink.currPosition) * 10;
                            if ((speed.Y * (speed.Y + currlink.currPosition.Y - currlink.targetPosition.Y)) > 0)
                                speed = currlink.targetPosition - currlink.currPosition;

                            currlink.Update(speed);
                        }
                    }
                }

                int Index = currlink.stlListModels[0].typeID - (int)LMS_ASSY_LINKID.DM_CARRIER;
                if ((Index >= 0) && (Index < 4))
                {
                    arrCarr[Index] = currlink.currPosition.Y;
                }
            }

            // move the lift right
            if (lmShuttleRight.OnRequest == true)
            {
                Vector3 vtCurrPos = lmShuttleRight.shuttleOb.ElementAt(0).currPosition;
                Vector3 vtTargetPos = lmShuttleRight.shuttleOb.ElementAt(0).targetPosition;

                if (vtCurrPos != vtTargetPos)
                {
                    Vector3 speed = Vector3.Normalize(vtTargetPos - vtCurrPos) * 8;
                    if ((speed.Z * (speed.Z + vtCurrPos.Z - vtTargetPos.Z)) > 0)
                        speed = vtTargetPos - vtCurrPos;

                    foreach (var visualOb in lmShuttleRight.shuttleOb)
                        visualOb.Update(speed);
                }
                else
                {
                    if (lmShuttleRight.shuttleOb.Count == 2)
                    {

                        if (lklTopRail.Last != null)
                            lmShuttleRight.shuttleOb.Last.Value.targetPosition = lklTopRail.Last.Value.currPosition;
                        else
                            lmShuttleRight.shuttleOb.Last.Value.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT];

                        if (lklTopRail.Contains(lmShuttleRight.shuttleOb.Last.Value) == false)
                            lklTopRail.AddLast(lmShuttleRight.shuttleOb.Last.Value);

                        if (lmShuttleRight.shuttleOb.Last.Value.currPosition.Y < arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT].Y)
                        {
                            lmShuttleRight.shuttleOb.RemoveLast();
                            lmShuttleRight.OnRequest = false;
                        }

                    }
                }

                if (lmShuttleRight.shuttleOb.Count == 2)
                {
                    VisualObject carr = lmShuttleRight.shuttleOb.Last.Value;
                    int Index = carr.stlListModels[0].typeID - (int)LMS_ASSY_LINKID.DM_CARRIER;
                    if ((Index >= 0) && (Index < 4))
                    {
                        arrCarr[Index] = carr.currPosition.Y;
                    }
                }

                VisualObject lmTrackRight = lmShuttleRight.shuttleOb.First.Value;
                arrLMTrack[3] = lmTrackRight.currPosition.X;
                arrLMTrack[4] = lmTrackRight.currPosition.Y;
                arrLMTrack[5] = lmTrackRight.currPosition.Z;


            }

            // Moving the carrier on the Top rails
            for (currTopRailLinkNode = lklTopRail.First; currTopRailLinkNode != null; currTopRailLinkNode = currTopRailLinkNode.Next, index++)
            {
                VisualObject currlink = currTopRailLinkNode.Value;

                if (currTopRailLinkNode.Previous == null)
                {
                    Vector3 vtCurrPos = currlink.currPosition;
                    Vector3 vtTargetPos = currlink.targetPosition;

                    if (vtCurrPos != vtTargetPos)
                    {
                        Vector3 speed = Vector3.Normalize(vtTargetPos - vtCurrPos) * 10;
                        if ((speed.Y * (speed.Y + vtCurrPos.Y - vtTargetPos.Y)) > 0)
                            speed = vtTargetPos - vtCurrPos;

                        currlink.Update(speed);
                    }
                    else
                    {
                        if (currlink.currPosition == arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT])
                        {
                            if (lmShuttleLeft.OnRequest == false)
                            {
                                lmShuttleLeft.OnRequest = true;
                                lmShuttleLeft.shuttleOb.First.Value.targetPosition = arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT];

                            }
                            else if (lmShuttleLeft.shuttleOb.First.Value.currPosition == arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT])
                            {

                                if (currTopRailLinkNode.Next != null)
                                {
                                    if ((currTopRailLinkNode.Next.Value.currPosition.Y - 340) <= currlink.currPosition.Y)
                                    {
                                        currTopRailLinkNode.Next.Value.bEnableMove = false;
                                    }
                                }

                                Vector3 speed = new Vector3(0.0f, -4.0f, 0.0f);
                                currlink.Update(speed);
                                currlink.targetPosition += new Vector3(0.0f, -341.5f, 0.0f);
                            }
                        }
                        else // carrier already in the lift
                        {
                            if (currTopRailLinkNode.Next != null)
                            {
                                if (currTopRailLinkNode.Next.Value.bEnableMove == false)
                                    currTopRailLinkNode.Next.Value.bEnableMove = true;
                            }

                            // Add linkIndex to liftRigh
                            lmShuttleLeft.shuttleOb.AddLast(currlink);
                            lmShuttleLeft.OnRequest = true;
                            lmShuttleLeft.shuttleOb.First.Value.targetPosition = arVTRailTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT];
                            lklTopRail.RemoveFirst();
                        }
                    }
                }
                else
                {
                    if (currTopRailLinkNode.Previous.Value.currPosition.Y >= (arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT].Y - 340))
                        currlink.targetPosition = currTopRailLinkNode.Previous.Value.currPosition + new Vector3(0.0f, 341.5f, 0.0f);
                    else
                        currlink.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT];

                    if (currlink.bEnableMove == false)
                    {
                        if ((currlink.targetPosition.Y - currlink.currPosition.Y) < -100)
                            currlink.bEnableMove = true;
                    }
                    else
                    {
                        if (currlink.currPosition != currlink.targetPosition)
                        {
                            Vector3 speed = Vector3.Normalize(currlink.targetPosition - currlink.currPosition) * 10;
                            if ((speed.Y * (speed.Y + currlink.currPosition.Y - currlink.targetPosition.Y)) > 0)
                                speed = currlink.targetPosition - currlink.currPosition;

                            currlink.Update(speed);
                        }
                    }
                }

                int Index = currlink.stlListModels[0].typeID - (int)LMS_ASSY_LINKID.DM_CARRIER;
                if ((Index >= 0) && (Index < 4))
                {
                    arrCarr[Index] = currlink.currPosition.Y;
                }
            }

            // move the lift left
            if (lmShuttleLeft.OnRequest == true)
            {
                Vector3 vtCurrPos = lmShuttleLeft.shuttleOb.ElementAt(0).currPosition;
                Vector3 vtTargetPos = lmShuttleLeft.shuttleOb.ElementAt(0).targetPosition;

                if (vtCurrPos != vtTargetPos)
                {
                    Vector3 speed = Vector3.Normalize(vtTargetPos - vtCurrPos) * 8;
                    if ((speed.Z * (speed.Z + vtCurrPos.Z - vtTargetPos.Z)) > 0)
                        speed = vtTargetPos - vtCurrPos;

                    foreach (var visualOb in lmShuttleLeft.shuttleOb)
                        visualOb.Update(speed);
                }
                else
                {
                    if (lmShuttleLeft.shuttleOb.Count == 2)
                    {

                        if (lklBottomRail.First != null)
                            lmShuttleLeft.shuttleOb.Last.Value.targetPosition = lklBottomRail.First.Value.currPosition;
                        else
                            lmShuttleLeft.shuttleOb.Last.Value.targetPosition = arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT];

                        if (lklBottomRail.Contains(lmShuttleLeft.shuttleOb.Last.Value) == false)
                            lklBottomRail.AddFirst(lmShuttleLeft.shuttleOb.Last.Value);

                        if (lmShuttleLeft.shuttleOb.Last.Value.currPosition.Y > arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_LEFT].Y)
                        {
                            lmShuttleLeft.shuttleOb.RemoveLast();
                            lmShuttleLeft.OnRequest = false;
                        }
                    }

                }

                if (lmShuttleLeft.shuttleOb.Count == 2)
                {
                    VisualObject carr = lmShuttleLeft.shuttleOb.Last.Value;
                    int Index = carr.stlListModels[0].typeID - (int)LMS_ASSY_LINKID.DM_CARRIER;
                    if ((Index >= 0) && (Index < 4))
                    {
                        arrCarr[Index] = carr.currPosition.Y;
                    }
                }

                VisualObject lmTrackLeft = lmShuttleLeft.shuttleOb.First.Value;
                arrLMTrack[0] = lmTrackLeft.currPosition.X;
                arrLMTrack[1] = lmTrackLeft.currPosition.Y;
                arrLMTrack[2] = lmTrackLeft.currPosition.Z;
            }

            Global.UpdateCarrInfo?.Invoke(arrCarr);
            Global.UpdateLMTrackInfo?.Invoke(arrLMTrack);
        }
    }
}

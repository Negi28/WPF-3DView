using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Presto.PRC;
using Presto.PRC.Types;

namespace PRC_Phatv_3DView
{
    class Global
    {
        public static Controller controller = null;
        public static byte prgNo = 255;
        public static MainWindow main = null;
        public static List<Robot> lConfigRobot = new List<Robot>();   //Keep separated from working Robot data
        public static SystemPara configSysPara = null;                       //Keep separated from working System Parameter

        public static LoginLevel loginLevel = LoginLevel.NONE;

        //Docking State
        public static bool IsDockTestClosed = false;
        public static bool IsDockJoggingClosed = false;
        public static bool IsDockSystemConfigClosed = false;
        public static bool IsDockMotionParamClosed = false;
        public static bool IsDockScopeClosed = false;
        public static bool IsDockPosMonClosed = false;
        public static bool IsDockHomingClosed = false;
        public static bool IsDockVariablesClosed = false;
        public static bool IsDock3DViewClosed = false;
        public static bool IsDockProgramClosed = false;
        public static bool IsDockSubProgramClosed = false;
        public static bool IsDockIOClosed = false;
        public static bool IsDockLogMsgClosed = false;
        public static bool IsDockTerminalClosed = false;

        // for testing MLMC robot
        public static LMMC_Infor gLMMCRobot = new LMMC_Infor();
        public static LMMC_Visualization gLMMCVisualization = new LMMC_Visualization();
        public static Orbiter orb = new Orbiter();

        public delegate void UpdateInfo(double[] Info);

        public static UpdateInfo UpdateCarrInfo = null;
        public static UpdateInfo UpdateLMTrackInfo = null;

        // for testing with real LMMC
        public static List<SCarrierInfo> gListCarrierInfo = new List<SCarrierInfo>();
        public static List<SShuttleInfo> gListShuttleInfo = new List<SShuttleInfo>();
        public static LMMC_SimulateRealSystem gLMMCSimulateRealSystem = new LMMC_SimulateRealSystem();
    }
}

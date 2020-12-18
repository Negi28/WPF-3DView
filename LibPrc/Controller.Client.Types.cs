using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Runtime.InteropServices;
using System.IO;
using Presto.PRC.Types;

namespace Presto.PRC
{
    partial class Controller
    {
        private enum State
        {
            Disconnected = 0,
            Connected,
            LoggedIn,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct _SYS_STATUS
        {
            public byte RbNum;        // Robot Number.
            public byte EcInstance;   // EtherCAT Instance for extending System.
            public byte TpMode;       // TP Mode.
            public byte UserLevel;    // User Level.
            public UInt32 Status;      // Refer to SYS_STATUS_BIT Enum for Masking Bit.	
            public UInt64 ErrAxis;         // Mappinged drive that error occurs.
            public UInt32 ErrCode;
        }

        private enum _SYS_STATUS_BIT
        {
            SYS_CPU_ERR = 0x00,     // Power & CPU Error (Ex. Over Temperature(or Voltage) of CPU(Intel, ARM))
            SYS_ERR,                // System Error
            SYS_EC_ERR,             // EtherCAT Error
            SYS_RB_ERR,             // When an error occurs in any robot.
            SYS_EMS_SW_FLAG,        // Box Emergency Switch Flag (1: Switch On, 0 : Switch Off)
            SYS_EXT_EMS_SW,         // External Emergency Switch Flag (1: Switch On, 0 : Switch Off),  All Emergency Switch must be connected in parallel  except TP & Box EMS
            SYS_TP_EMS_SW_FLAG,     // TP Emergency Switch Flag (1: Switch On, 0 : Switch Off)	
            SYS_TP_CONNECT_FLAG,
            SYS_SV_ON,                  // when Turn on All Drive
            SYS_TP_DEAD_SW_FLAG,
        }

        private enum PRG_STATE
        {
            PRG_STATE_NONE,
            PRG_STATE_START = 1,
            PRG_STATE_RUN = 2,
            PRG_STATE_STOP = 3,
            PRG_STATE_PAUSE = 4,
            PRG_STATE_END = 5,
            PRG_STATE_COMPILE_OK = 6,
            PRG_STATE_COMPILE_ERR = 7,
            PRG_STATE_RUN_ERR = 8,
            PRG_STATE_MAX_STATE,
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct PRG_EXE
        {
            public byte Index; //1 byte
            public UInt16 LineExe;
            public byte Status;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct MOTOR_STATUS
        {
            public byte MIdx;         // Motor Index.	
            public UInt32 MStatus;     // Refer to MT_STATUS_BIT Enum for Masking Bit	
            public UInt32 ErrCode;
        }

        private enum MT_STATUS_BIT
        {
            MS_SVON = 0x00,
            MS_HOME_FLAG,
            MS_DRIVE_ALARM,
            MS_SW_NEG_LMT,
            MS_SW_POS_LMT,
            MS_NEG_LMT,
            MS_POS_LMT,
            MS_OVER_VEL,        // Over Velocity	
            MS_OVER_CUR,        // Over Current or Torque
            MS_RUN_FLAG,
            MS_NONE,
        }

        private enum RB_STATUS_BIT
        {
            RS_SVON_FLAG = 0x00,    // Servo On Flag (1: When All Axes of Robot turn on, 0 : otherwise)
            RS_HOME_FLAG,           // Homing Flag (1 : When Homing is performed at least once after Servo On, 0 : otherwise)
            RS_DRIVE_ALARM,         // Drive Fault	
            RS_OVER_WORK_SPACE,     // Over Work Space 	
            RS_DETECT_OB,           // When obstacle is detected
            RS_ERR,                     // Robot Error.
            RS_RUN_FLAG
        }

        //Diagnotic
        private StreamWriter logLib = null;
        private StreamWriter logErr = null;

        private const int MAX_NUM_ROBOT = 10;
        private const int MAX_NUM_PROGRAM = 32;
        private const int RSP_HEADER_SIZE = 10;

        private int period = 100; //In Milisec

        // The port number for the remote device. 
        private string ip = "";
        private const int port = 9001;
        private Socket socket = null;
        private State state = State.Disconnected;        

        //Command Process
        private UInt16 cmdAck = 0;
        private UInt32 errCode = 0;
        private UInt32 size = 0;
        private byte[] data = null;
        private byte[] rspHeader = new byte[RSP_HEADER_SIZE];

        //XML file
        private  XmlDocument xmlSysPara = null;
        private List<XmlDocument> lXmlRobot = new List<XmlDocument>();

        //Data
        //System
        private bool bEcMaster = false;
        private SystemPara sysPara = new SystemPara();
        private List<Robot> lRobot = new List<Robot>();
        private List<MotorStatus> lMtStatus = new List<MotorStatus>();

        //IO              
        private List<Program> lProgram = new List<Program>();
        private List<bool> lAuto = new List<bool>();
        private List<ProgramStatus> lProgramStatus = new List<ProgramStatus>();
        private List<Program> lSubProgram = new List<Program>();
        
    
        private List<Folder> lRobotTemplate = new List<Folder>();
        private int searchFolderCounter = 0;


        private T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return result;
        }

        private UInt32 BITSET(int val)
        {
            return (UInt32)(0x00000001 << val);
        }

    }
}

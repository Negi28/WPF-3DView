#define USE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;
using OpenTK;

namespace Presto.PRC.Types
{
    public class Robot : ICloneable, INotifyPropertyChanged
    {
        public UInt16 RbType { get; set; }
        public string Name { get; set; }
        public UInt16 ToolType { get; set; }
        public byte AxisNumber { get; set; }
        public string Vendor { get; set; }
        public string Model { get; set; }
        public byte CoordType { get; set; }
        public double DefVelP { get; set; }
        public double DefJogVelP { get; set; }
        public WorkArea WorkArea { get; set; }
        public List<Axis> Axes { get; set; }

        public UInt16 _iVarNum;
        public UInt16 IVarNum
        {
            get { return _iVarNum; }
            set { _iVarNum = value; NotifyPropertyChanged("IVarNum"); }
        }

        public UInt16 _dVarNum;
        public UInt16 DVarNum
        {
            get { return _dVarNum; }
            set { _dVarNum = value; NotifyPropertyChanged("DVarNum"); }
        }

        public UInt16 _PosVarNum;
        public UInt16 PosVarNum
        {
            get { return _PosVarNum; }
            set { _PosVarNum = value; NotifyPropertyChanged("PosVarNum"); }
        }

        public Robot()
        {
            WorkArea = new WorkArea();
            Axes = new List<Axis>();
        }

        public object Clone()
        {
            return (Robot)MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WorkArea
    {
        public double Xn { get; set; }
        public double Yn { get; set; }
        public double Zn { get; set; }
        public double Xp { get; set; }
        public double Yp { get; set; }
        public double Zp { get; set; }
    }

    public class Axis
    {
        public byte No { get; set; }
        public byte DriveMapSlaNum { get; set; }
        public UInt32 CmdPulsePerCycle { get; set; }
        public double DistPerCycle { get; set; }
        public double GearRate { get; set; }
        public double MaxVelocity { get; set; }
        public double MaxTorque { get; set; }
        public SWLimit SWLimit { get; set; }
        public sbyte MotDirection { get; set; }
        public UInt32 InposPulse { get; set; }
        public UInt32 SettlingTime { get; set; }
        public UInt32 BrakeReleaseTime { get; set; }
        public double MaxPosErr { get; set; }
        public HomePara HomePara { get; set; }
        public ReadyMovePara ReadyMovePara { get; set; }

        /* Jog & Program */

        public MovePara JogMovePara { get; set; }
        public MovePara MovePara { get; set; }

        public Axis()
        {
            SWLimit = new SWLimit();
            HomePara = new HomePara();
            ReadyMovePara = new ReadyMovePara();
            JogMovePara = new MovePara();
            MovePara = new MovePara();
        }
    }

    public class SWLimit
    {
        public double Neg { get; set; }
        public double Pos { get; set; }
    }

    public class HomePara
    {
        public byte Method { get; set; }
        public double Acceleraion { get; set; }
        public double Velocity { get; set; }
        public double ReturnVel { get; set; }
        public double Offset { get; set; }
    }

    public class ReadyMovePara  //Moving Home: MoveR()
    {
        public double Vel { get; set; }
        public double AccTime { get; set; }
        public double DecTime { get; set; }
        public double AJerkP { get; set; }
        public double DJerkP { get; set; }
        public double KillTime { get; set; }
        public double Position { get; set; }
    }

    public class MovePara
    {
        public double JVel { get; set; }
        public double JAccTime { get; set; }
        public double JDecTime { get; set; }
        public double JAJerkP { get; set; }
        public double JDJerkP { get; set; }
        public double JKillTime { get; set; }

        public double WVel { get; set; }
        public double WAccTime { get; set; }
        public double WDecTime { get; set; }
        public double WAJerkP { get; set; }
        public double WDJerkP { get; set; }
        public double WKillTime { get; set; }
    }

    //Application Type
    public class SystemPara : ICloneable, INotifyPropertyChanged
    {
        private byte _RbNumber;
        public byte RbNumber
        {
            get { return _RbNumber; }
            set { _RbNumber = value; NotifyPropertyChanged("RbNumber"); }
        }

        private UInt16 _CycleTime;
        public UInt16 CycleTime
        {
            get { return _CycleTime; }
            set { _CycleTime = value; NotifyPropertyChanged("CycleTime"); }
        }

        private string _ENIFileName;
        public string ENIFileName
        {
            get { return _ENIFileName; }
            set { _ENIFileName = value; NotifyPropertyChanged("ENIFileName"); }
        }

        private byte _UseTeachPendant;
        public byte UseTeachPendant
        {
            get { return _UseTeachPendant; }
            set { _UseTeachPendant = value; NotifyPropertyChanged("UseTeachPendant"); }
        }

        private byte _TotalAxisNumber;
        public byte TotalAxisNumber
        {
            get { return _TotalAxisNumber; }
            set { _TotalAxisNumber = value; NotifyPropertyChanged("TotalAxisNumber"); }
        }

        public string _IPAddress;
        public string IPAddress
        {
            get { return _IPAddress; }
            set { _IPAddress = value; NotifyPropertyChanged("IPAddress"); }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            return (SystemPara)MemberwiseClone();
        }
    }

    public class SystemStatus
    {
        public byte RbNum { get; set; }
        public bool IsEcatErr { get; set; }
        public bool IsSystemError { get; set; }
        public bool IsRobotError { get; set; }
        public bool IsTpEms { get; set; }
        public bool IsRobotOn { get; set; }
        public bool IsTpDeadSw { get; set; }
        public bool IsUseTp { get; set; }
    }

    public class RobotStatus
    {
        public bool IsAtHome { get; set; }
        public bool IsAlarm { get; set; }
        public bool IsDetectObj { get; set; }
        public bool IsOverWorkSpace { get; set; }
        public bool IsOn { get; set; }
        public bool IsError { get; set; }
        public bool IsRunning { get; set; }
    }

    public class Program : ICloneable
    {
        public bool Auto { get; set; }
        public int No { get; set; }
        public string Description { get; set; }
        public int Lines { get; set; }
        public int ExeLine { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
        public object Clone()
        {
            return (Program)MemberwiseClone();
        }
    }

    public class ProgramStatus
    {
        public UInt16 LineExe { get; set; }
        public string Status { get; set; }
    }

    public class Jogging : INotifyPropertyChanged, ICloneable
    {
        public string J { get; set; }
        public string JPos { get; set; }
        MotorStatus _SV;
        public MotorStatus SV { get { return _SV; } set { _SV = value; NotifyPropertyChanged("SV"); } }
        public string W { get; set; }
        public string WPos { get; set; }

        public object Clone()
        {
            return (Jogging)MemberwiseClone();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public enum MotorStatus
    {
        OFF = 0,
        ON,
        ALARM
    }

    public enum LoginLevel
    {
        NONE = 0,
        OPERATOR,
        PROGRAMMER,
        ADMIN,
    }

    public class DigitalIO
    {
        public UInt16 PortNumber { set; get; }
        public List<byte> PortSizes { set; get; }
        public List<List<byte>> PortBits { set; get; }

        public DigitalIO()
        {
            PortNumber = 0;
            PortSizes = new List<byte>();
            PortBits = new List<List<byte>>();
        }

        public void Reset()
        {
            PortNumber = 0;
            PortSizes.Clear();
            PortBits.Clear();
        }
    }

    public class AnalogIO
    {
        public UInt16 ChannelNumber { set; get; }
        public List<Int32> ChannelValues { set; get; }

        public AnalogIO()
        {
            ChannelNumber = 0;
            ChannelValues = new List<Int32>();
        }

        public void Reset()
        {
            ChannelNumber = 0;
            ChannelValues.Clear();
        }
    }


    public enum ProgramState
    {
        NONE = 0,
        START = 1,
        RUN = 2,
        STOP = 3,
        PAUSE = 4,
        END = 5,
        COMPILE_OK = 6,
        COMPILE_ERR = 7,
        RUN_ERR = 8,
        MAX_STATE,
    }

    public enum LoggingType
    {
        ERR = 0,    //PRC Error
        SYS,        // System Log.(Boot ...)
        MAIN,       // Maintenance Log.
        USER,      // User Log.  -->       
        APP, //For PC Appilication
    }

    //Config
    public class Folder
    {
        public string Name { get; set; }
        public List<string> Files { get; set; }

        public Folder()
        {
            Name = "";
            Files = new List<string>();
        }

        public Folder(string _name)
        {
            Name = _name;
            Files = new List<string>();
        }
    }

    public enum MotionType
    {
        Manual = 0,
        Program,
        Ready,
    }

    public class IVar
    {
        public int Index { get; set; }
        public string Description { get; set; }
        public Int32 Value { get; set; }

        public IVar(int _index, string _text, Int32 _val)
        {
            Index = _index;
            Description = _text;
            Value = _val;
        }
    }

    public class DVar
    {
        public int Index { get; set; }
        public string Description { get; set; }
        public double Value { get; set; }
        public DVar(int _index, string _text, double _val)
        {
            Index = _index;
            Description = _text;
            Value = _val;
        }
    }

    public class PosVarLable
    {
        public string J { get; set; }
        public string W { get; set; }

        public PosVarLable()
        {
            J = "J";
            W = "W";
        }
    }

    public class PosVar
    {
        public int Index { get; set; }
        public string WDescription { get; set; }
        public string JDescription { get; set; }
        public double[] WPos { get; set; }
        public double[] JPos { get; set; }
        public PosVarLable Lable { get; set; }

        public PosVar(UInt16 axisNum)
        {
            WPos = new double[axisNum];
            JPos = new double[axisNum];
            Lable = new PosVarLable();
        }
    }
    // Vin for Testing LMMC
    public struct SCarrierInfo
    {
        public SCarrierPos Pos;
        public byte OnDrv1;
        public byte OnDrv2;
        public byte LoadFlag;
        public UInt16 PhyID;
        public UInt32 Status;
    }

    public struct SCarrierPos
    {
        public byte TrackNo;
        public double Pos;
    }

    public enum LMMC_TRACK_TYPE : UInt32
    {
        MM_TRACK = 0,
        MM_SHUTTLE,
        LM_LIFT,
        LM_SHUTTLE
    }
    public enum LMMC_RAIL_STATUS : UInt32
    {
        SV_ON_BIT = 0,
        HOMING_BIT,
        ENC_S_LEFT,
        ENC_S_RIGHT,
        DRV_VAILD,
        CARRIER_ON,
        ERR_BIT
    }
    public struct SShuttleInfo
    {
        //public byte TrackNo;
        //public LMMC_TRACK_TYPE TrackType;
        //public byte MapSlaNo;
        //public byte CarrierNoOn;
        public double Pos;
        //public LMMC_RAIL_STATUS Status;
        //public UInt32 ErrCode;
        //public UInt16 Torque;  // 0.1 ~ %
    }
    // ~Vin


    public enum RobotType
    {
        RMODEL_NONE = 0000			///< 모델명 없음. 유저 정의 로봇.
	, SINGLE_AXIS_1 = 1000		///< 단축 로봇.
	, ORTHOGONAL_XY = 1200		///< 2축 직교 로봇.
	, ORTHOGONAL_XYZ = 1300		///< 3축 직교 로봇.
	, LM_4AX_RB = 1400			///< LM 4축 로봇.
	, LM_5AX_RB = 1500			///< LM 5축 로봇.
	, LM_6AX_RB = 1600			///< LM 6축 로봇.
	, LM_7AX_RB = 1700			///< LM 7축 로봇.
	, LM_8AX_RB = 1800          ///< LM 8축 로봇.

    , PS_DELTA = 3000           ///< PS 델타로봇.

    , LPK_SP_2525_200 = 4000	///< LPK 스카라 로봇.
	, STH030_500 = 4010			///< YAKO 스카라 로봇.
	, PS_WTR = 4100             ///< PS 스카라 로봇.

    , EX_LTR_AATZ = 4200		///< EX LTR 로봇. Arm축 2개. Z축 1개.
	, EX_LTR_AATZZ = 4201		///< EX LTR 로봇. Arm축 2개, Z축 2개.
	, EX_LTR_AATZZX = 4202		///< EX LTR 로봇. Arm축 2개, Z축 2개. 주행축 추가.
	, THK_W3T1 = 4203           ///< THK WTR 300mm 로봇. Arm축 2개. Z축 1개.

    , RS_V4 = 6000				///< 로보스타 다관절 로봇.
	, RS_1601A = 6001			///< 로보스타 다관절 로봇.
	, HD_HA006 = 6002           ///< 현대중공업 다관절 로봇.

                                ///, UR5 = 6100				///< EX UR 로봇.
    , EX_HEXAPOD = 6200         ///< EX 핵사포드 로봇.

    , LMMC_TYPE_01 = 6501	    // for testing
    , LMMC_TYPE_02 = 6502	    // for testing
    , LMMC_TYPE_03 = 6503	    // for testing
    , LMS_ASSY_TYPE = 6504	    // for testing
    , STL_FILE = 6505	    // for testing
    , STL_FOLDER = 6506	    // for testing
    , CUBE = 6501			    // for testing
    }


}


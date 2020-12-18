using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presto.PRC
{
    partial class Controller
    {
        enum Command
        {
            // R: Robot number,	A: Axis number,	P:Program number
            // Frame format:
            // Send: [CMD:2][Length:4][Data:n]  ; unit: bytes
            // Recv: [CMD-ACK:2][Length:4][Data:n]  ; unit: bytes

            //************************************************************************************************************************************************************************
            // General Command
            //************************************************************************************************************************************************************************
            CMD_NONE = 0x0000,

            CMD_GET_LOGIN_STATUS = 0x0001,      // Send Data[0]							// Respose Data : User Level(1Byte)
            CMD_SET_LOGIN = 0x0002,             // Send User Level(1Byte) + Password String Size 	// Response Data: NULL or Err Message
            CMD_SET_LOGIN_PW = 0x0003,          // Change LogIn Password. User Level(1Byte) + Current Password + '0x00' (null) + Change Password

            CMD_GET_STATUS_SYS = 0x0010,        // Send Data[0]							// Respose Data : Number of Robot(1Byte), SystemStatus(2Bytes), ErrAxis(8Bytes) 
            CMD_GET_STATUS_ROBOT = 0x0011,      //[X] Send Data: [RobotNo:1]      				// Response Data: [RobotNum: 1][RobotStatus: sizeof(struct)][EcMaster:1]
            CMD_GET_STATUS_RB_MOTOR = 0x0012,   //[X] Send Data[2]: BYTE(R) + BYTE(A)			// Response Data [RobotNo:1][AxisNo:1][AxisStatus:1] {ON/OFF, ALARM, RUN, INPOS}
            CMD_GET_STATUS_MAP_MOTOR = 0x0013,  //[X] Send Data[1]: BYTE(Axis Number Mapped as Drive)		// Response Data [AxisNo:1][AxisStatus:1] {ON/OFF, ALARM, RUN, INPOS}

            CMD_GET_CUR_JOINT = 0x0015,         // Send Data[1]: BYTE(R)						// Response Data: [RototNo: 1][8xn]: n is number of axis
            CMD_GET_CUR_WORK = 0x0016,          // Send Data[1]: BYTE(R)						// Response Data: [RototNo: 1][8xn]: n is number of axis

            CMD_SET_SVON = 0x0017,              // Send Data[1]: BYTE(R) + BYTE(A): A=0xff: RobotOn: All axis, else Axis#A On	// Response Data: NULL or Err Message
            CMD_SET_SVOFF = 0x0018,             // Send Data[1]: BYTE(R) + BYTE(A): A=0xff: RobotOff, else Axis Off			// Response Data: NULL or Err Message
            CMD_SET_ALRAM_CLEAR = 0x0019,       // Send Data[1]: BYTE(R) + BYTE(A) :A=0xff: RobotClear, else Axis Clear			// Response Data: NULL or Err Message

            CMD_MOVE_STOP = 0x001A,             // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message
            CMD_MOVE_ESTOP = 0x001B,            // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message
            CMD_MOVE_HOME = 0x001C,             // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message

            CMD_GET_ROBOT_BASE_POS = 0x001D,    //[X] Send Data[1]: BYTE(R)					// Response Data:NULL or Err Message
            CMD_SET_ROBOT_BASE_POS = 0x001E,    //[X] Send Data[1+48]: BYTE(R) + DOUBLE[6]		// Response Data: NULL or Err Message

            CMD_GET_STATUS_ROBOT_ALL = 0x0021,      // Send Data: [RobotNo:1]      				// Response Data: [RobotNum: 1][RobotStatus: sizeof(struct)], [RobotNum: 2][RobotStatus: sizeof(struct)], [RobotNum: 3][RobotStatus: sizeof(struct)]
                                                    // Robot Status
                                                    // 0 : RS_SVON_FLAG
                                                    // 1 : RS_HOME_FLAG
                                                    // 2 : RS_DRIVE_ALARM
                                                    // 3 : RS_OVER_WORK_SPACE
                                                    // 4 : RS_DETECT_OB
                                                    // 5 : RS_ERR
                                                    // 6 : RS_RUN_FLAG

            CMD_GET_STATUS_RB_MOTOR_ALL = 0x0022,   // Send Data[2]: BYTE(R) + BYTE(A)			// Response Data [RobotNo:1][AxisNo:1][AxisStatus:1] {ON/OFF, ALARM, RUN, INPOS},[AxisNo:1][AxisStatus:1] {ON/OFF, ALARM, RUN, INPOS}
            CMD_GET_STATUS_MAP_MOTOR_ALL = 0x0023,  // Send Data[1]: BYTE(Axis Number Mapped as Drive)	 // Response Data [AxisNo:1][AxisStatus:1] {ON/OFF, ALARM, RUN, INPOS}, [AxisNo:2][AxisStatus:1] {ON/OFF, ALARM, RUN, INPOS}

            CMD_GET_SYS_FILE = 0x0030,          // Send Data[0]: NULL					// Response Data[]: XML File
            CMD_SET_SYS_FILE = 0x0031,          // Send Data[]: XML File					// Response Data: NULL or Err Message
            CMD_GET_PRG_FILE = 0x0032,          // Send Data[0]: NULL					// Response Data[]: XML File
            CMD_SET_PRG_FILE = 0x0033,          // Send Data[]: XML File					// Response Data: NULL or Err Message
            CMD_GET_LOG_FILE = 0x0034,          //[X] Send Data[0]: NULL					// Response Data[]: XML File
            CMD_SET_LOG_FILE = 0x0035,          // Send Data[]: XML File					// Response Data: NULL or Err Message
            CMD_GET_VAR_FILE = 0x0036,          // Send Data[0]: NULL					// Response Data[]: XML File
            CMD_SET_VAR_FILE = 0x0037,          // Send Data[]: XML File					// Response Data: NULL or Err Message
            CMD_GET_POS_VAR_FILE = 0x0038,      // Send Data[0]: NULL					// Response Data[]: XML File
            CMD_SET_POS_VAR_FILE = 0x0039,      // Send Data[]: XML File					// Response Data: NULL or Err Message

            CMD_GET_RB_PARA_FILE = 0x0040,      // Send Data[]: Robot Number (1Byte)				// Response Data[]: XML File
            CMD_SET_RB_PARA_FILE = 0X0041,

            CMD_GET_CMD_JOINT = 0x0042,
            CMD_GET_CMD_WORK = 0x0043,
            // Add 2018-08-29
            // Coordinate Convert function 
            CMD_JPOS_TO_WPOS = 0x0044,
            CMD_WPOS_TO_JPOS = 0x0045,

            //*************************************************************************************************************************************************************
            // System Configuration
            //*************************************************************************************************************************************************************
            CMD_GET_SYS_RB_NUM = 0x0050,        // Send Data[0]: NULL						// Response Data[]: Robot Number (1Byte)
            CMD_SET_SYS_RB_NUM = 0x0051,        // Send Data[]: Robot Number (1Byte)		// Response Data[]: Null or Error Message	

            CMD_GET_SYS_ENI_FILE_NAME = 0x0052, // Send Data[0]: NULL						// Response Data[]: Strlen(1byte) + Str
            CMD_SET_SYS_ENI_FILE_NAME = 0x0053, // Send Data[]: Strlen(1byte) + Str			// Response Data[]: Null or Error Message

            CMD_GET_SYS_CYCLE_TIME = 0x0054,    // Send Data[0]: NULL						// Response Data[]: Cycle Time(2Bytes msec)
            CMD_SET_SYS_CYCLE_TIME = 0x0055,    // Send Data[]: Cycle Time(2Bytes msec)	// Response Data[]: Null or Error Message

            CMD_GET_TOTAL_AXIS_NUM = 0x0056,    // Send Data[0]: NULL						// Response Data[]: Total Axis Number(1Byte)
            CMD_SET_TOTAL_AXIS_NUM = 0x0057,    // Send Data[]: Total Axis Num(1Byte)		// Response Data[]: Null or Error Message

            CMD_GET_USE_TP = 0x0058,            // Send Data[0] : NULL						// Response Data[]: 1Byte 0 : Not Use  1 : Use
            CMD_SET_USE_TP = 0x0059,            // Send Data[] : 1 Byte (0 or 1)			// Response Data[]: Null or Error Message

            CMD_GET_LAN_IP_ADDR = 0x005A,       // Send Data[0] : NULL						// Response Data[]: strlen(1Bytes), string(ex "192.168.100.32") or Error
            CMD_SET_LAN_IP_ADDR = 0x005B,       // Send Data : strlen(1Bytes), string(ex "192.168.100.32")   // Response Data[]: Null or Error Message
                                                //*************************************************************************************************************************************************************
                                                // Robot Configuration
                                                //*************************************************************************************************************************************************************
            CMD_GET_RPAR_RB_TYPE = 0x0060,      // Send Data[0]: RbNo(1Byte)                        // Response Data[]: RbNo(1Byte) + Rb Type (2Bytes)
            CMD_SET_RPAR_RB_TYPE = 0x0061,      // Send Data[]: RbNo(1Byte) + Rb Type (2Bytes)	    // Response Data[]: Null or Error Message

            CMD_GET_RPAR_RB_NAME = 0x0062,      // Send Data[0]: RbNo(1Byte)						// Response Data[]: RbNo(1Byte) + Strlen(1byte) + Str
            CMD_SET_RPAR_RB_NAME = 0x0063,      // Send Data[]: RbNo(1Byte) + Strlen(1byte) + Str	// Response Data[]: Null or Error Message

            CMD_GET_RPAR_TOOL_TYPE = 0x0064,    // Send Data[0]: RbNo(1Byte)						// Response Data[]: RbNo(1Byte) + Tool Type (2Bytes)
            CMD_SET_RPAR_TOOL_TYPE = 0x0065,    // Send Data[]: RbNo(1Byte) +Tool Type (2Bytes)		// Response Data[]: Null or Error Message

            CMD_GET_RPAR_AXIS_NUM = 0x0066,     // Send Data[0]: RbNo(1Byte)						// Response Data[]: RbNo(1Byte) + Axis Num (1Byte)
            CMD_SET_RPAR_AXIS_NUM = 0x0067,     // Send Data[]: RbNo(1Byte) + Axis Num (1Byte)		// Response Data[]: Null or Error Message

            CMD_GET_RPAR_COORD_TYPE = 0x0068,   // Send Data[0]: RbNo(1Byte)						// Response Data[]: RbNo(1Byte) + Coord Type (1Byte)
            CMD_SET_RPAR_COORD_TYPE = 0x0069,   // Send Data[]: RbNo(1Byte) + Coord Type (1Byte)	// Response Data[]: Null or Error Message

            CMD_GET_RPAR_DEF_VELP = 0x006A,     // Send Data[0]: RbNo(1Byte)						// Response Data[]: RbNo(1Byte) + VELP (Double 8Bytes)
            CMD_SET_RPAR_DEF_VELP = 0x006B,     // Send Data[]: RbNo(1Byte) + VELP (Double 8Bytes)	// Response Data[]: Null or Error Message

            CMD_GET_RPAR_DEF_JOG_VELP = 0x006C, // Send Data[0]: RbNo(1Byte)						// Response Data[]: RbNo(1Byte) + VELP (Double 8Bytes)
            CMD_SET_RPAR_DEF_JOG_VELP = 0x006D, // Send Data[]: RbNo(1Byte) + VELP (Double 8Bytes)	// Response Data[]: Null or Error Message

            CMD_GET_RPAR_WORK_AREA = 0x006E,    // Send Data[0]: RbNo(1Byte)						// Response Data[]: RbNo(1Byte) + RB_WORK_COORD (sizeof)
            CMD_SET_RPAR_WORK_AREA = 0x006F,    // Send Data[]: RbNo(1Byte) + _RB_WORK_COORD (sizeof)	// Response Data[]: Null or Error Message

            //*************************************************************************************************************************************************************
            // Axis Configuration
            //*************************************************************************************************************************************************************
            CMD_GET_RPAR_DRV_MAP_NUM = 0x0070,  // Send Data[0]: RbNo(1Byte) + AxisNo(1Byte)							// Response Data[]: RbNo(1Byte) + AxisNo(1Byte) + MappingNo(1Byte)
            CMD_SET_RPAR_DRV_MAP_NUM = 0x0071,  // Send Data[]: RbNo(1Byte) + AxisNo(1Byte) + MappingNo(1Byte)			// Response Data[]: Null or Error Message

            CMD_GET_RPAR_CMD_PULSE = 0x0072,    // Send Data[0]: RbNo(1Byte) + AxisNo(1Byte)							// Response Data[]: RbNo(1Byte) + AxisNo(1Byte) + CmdPulsePerCycle(4Bytes)
            CMD_SET_RPAR_CMD_PULSE = 0x0073,    // Send Data[]: RbNo(1Byte) + AxisNo(1Byte) + CmdPulsePerCycle(4Bytes)	// Response Data[]: Null or Error Message

            CMD_GET_RPAR_PITCH = 0x0074,        // Send Data[0]: RbNo(1Byte) + AxisNo(1Byte)							// Response Data[]: RbNo(1Byte) + AxisNo(1Byte) + Pitch(Double 8Bytes)
            CMD_SET_RPAR_PITCH = 0x0075,        // Send Data[]: RbNo(1Byte) + AxisNo(1Byte) + Pitch(Double 8Bytes)		// Response Data[]: Null or Error Message

            CMD_GET_RPAR_GEAR_RATE = 0x0076,    // Send Data[0]: RbNo(1Byte) + AxisNo(1Byte)							// Response Data[]: RbNo(1Byte) + AxisNo(1Byte) + GearRate(Double 8Bytes)
            CMD_SET_RPAR_GEAR_RATE = 0x0077,    // Send Data[]: RbNo(1Byte) + AxisNo(1Byte) + GearRate(Double 8Bytes)	// Response Data[]: Null or Error Message

            CMD_GET_RPAR_MAX_VEL = 0x0078,      // Send Data[0]: RbNo(1Byte) + AxisNo(1Byte)							// Response Data[]: RbNo(1Byte) + AxisNo(1Byte) + Vel(Double 8Bytes)
            CMD_SET_RPAR_MAX_VEL = 0x0079,      // Send Data[]: RbNo(1Byte) + AxisNo(1Byte) + Vel(Double 8Bytes)		// Response Data[]: Null or Error Message

            CMD_GET_RPAR_MAX_TORQUE = 0x007A,   // Send Data[0]: RbNo(1Byte) + AxisNo(1Byte)							// Response Data[]: RbNo(1Byte) + AxisNo(1Byte) + Torque(Double 8Bytes)
            CMD_SET_RPAR_MAX_TORQUE = 0x007B,   // Send Data[]: RbNo(1Byte) + AxisNo(1Byte) + Torque(Double 8Bytes)		// Response Data[]: Null or Error Message

            CMD_GET_RPAR_SW_NEG_LMT = 0x007C,   // Send Data[0]: RbNo(1Byte) + AxisNo(1Byte)							// Response Data[]: RbNo(1Byte) + AxisNo(1Byte) + LMT(Double 8Bytes)
            CMD_SET_RPAR_SW_NEG_LMT = 0x007D,   // Send Data[]: RbNo(1Byte) + AxisNo(1Byte) + LMT(Double 8Bytes)		// Response Data[]: Null or Error Message

            CMD_GET_RPAR_SW_POS_LMT = 0x007E,   // Send Data[0]: RbNo(1Byte) + AxisNo(1Byte)							// Response Data[]: RbNo(1Byte) + AxisNo(1Byte) + LMT(Double 8Bytes)
            CMD_SET_RPAR_SW_POS_LMT = 0x007F,   // Send Data[]: RbNo(1Byte) + AxisNo(1Byte) + LMT(Double 8Bytes)		// Response Data[]: Null or Error Message

            CMD_GET_RPAR_DIRECTION = 0x0080,    // Send Data[0]: RbNo(1Byte) + AxisNo(1Byte)							// Response Data[]: RbNo(1Byte) + AxisNo(1Byte) + LMT(Double 8Bytes)
            CMD_SET_RPAR_DIRECTION = 0x0081,    // Send Data[]: RbNo(1Byte) + AxisNo(1Byte) + LMT(Double 8Bytes)		// Response Data[]: Null or Error Message

            CMD_GET_INPOS_PULSE = 0x0082,       // Send Data[0]: RbNo(1Byte) + AxisNo(1Byte)							// Response Data[]: RbNo(1Byte) + AxisNo(1Byte) + Pulse(UINT32)
            CMD_SET_INPOS_PULSE = 0x0083,       // Send Data[]: RbNo(1Byte) + AxisNo(1Byte) + Pulse(UINT32)				// Response Data[]: Null or Error Message

            CMD_GET_SETTLING_TIME = 0x0084,     // Send Data[0]: RbNo(1Byte) + AxisNo(1Byte)									// Response Data[]: RbNo(1Byte) + AxisNo(1Byte) + SettlingTime(Double 8Bytes msec)
            CMD_SET_SETTLING_TIME = 0x0085,     // Send Data[]: RbNo(1Byte) + AxisNo(1Byte) + SettlingTime(Double 8Bytes msec)	// Response Data[]: Null or Error Message

            CMD_GET_BRK_REL_TIME = 0x0086,      // Send Data[0]: RbNo(1Byte) + AxisNo(1Byte)											// Response Data[]: RbNo(1Byte) + AxisNo(1Byte) + Break Release Time(Double 8Bytes msec)
            CMD_SET_BRK_REL_TIME = 0x0087,      // Send Data[]: RbNo(1Byte) + AxisNo(1Byte) + Break Release Time(Double 8Bytes msec)	// Response Data[]: Null or Error Message

            CMD_GET_MAX_POS_ERR = 0x0088,       // Send Data[0]: RbNo(1Byte) + AxisNo(1Byte)											// Response Data[]: RbNo(1Byte) + AxisNo(1Byte) + Break Release Time(Double 8Bytes msec)
            CMD_SET_MAX_POS_ERR = 0x0089,       // Send Data[]: RbNo(1Byte) + AxisNo(1Byte) + Break Release Time(Double 8Bytes msec)	// Response Data[]: Null or Error Message

            //*************************************************************************************************************************************************************
            CMD_SAVE_TO_SYS_FILE = 0x00A1,      // Send Data[0]: NULL				// Response Data[]: Null or Error Message
            CMD_SAVE_TO_RPARA_FILE = 0x00A2,    // Send Data[0]: NULL               // Response Data[]: Null or Error Message
            CMD_SAVE_TO_RPOS_FILE = 0x00A3,     // Send Data[0]: NULL				// Response Data[]: Null or Error Message
            CMD_SAVE_TO_RVAR_FILE = 0x00A4,     // Send Data[0]: NULL				// Response Data[]: Null or Error Message

            CMD_SEARCH_RB_PARA_FOLDER = 0x00B1, // Send Data[0]: NULL	// Response Data[]: Folder Number(1Byte) + 1st strlen(Folder Name) + 1st str(Folder Name) + 2nd strlen(Folder Name) + 2nd str(Folder Name)...
            CMD_SEARCH_RB_PARA_FILE = 0x00B2,   // Send Data: strlen(Folder Name(1Byte)) + str(Folder Name)	// Response Data[]: File Number(2Byte) + 1st strlen(File Name) + 1st str(File Name) + 2nd strlen(File Name) + 2nd str(File Name)...
            CMD_GET_RB_PARA_XML_FILE = 0x00BA,  // Send Data: strlen(file Path(FolderName/fileName(1Byte))) + str(file Path)	// Response Data[]: XML File

            //CMD_CHANGE_APPLIED_ENI = 0x00C0,    // Change ENI File Name applied, if this file use in the system, please restart EcMaster.
            CMD_SEARCH_ENI_FILE = 0x00C1,       // Send Data[0]: NULL	// Response Data[]: File Number(2Byte) + 1st strlen(File Name) + 1st str(File Name) + 2nd strlen(File Name) + 2nd str(File Name)...
            CMD_ADD_ENI_FILE = 0x00CA,          // Send Data[0]: strlen(FileName(1Byte)), str(fileName), xmlfile	// Response Data[]: Null or Error Message
            CMD_DELETE_ENI_FILE = 0x00CF,       // Send Data[0]: strlen(FileName(1Byte)), str(fileName)	// Response Data[]: Null or Error Message

            CMD_CHANGE_EC_LICENSE_STR = 0x00D1, // Send Data[] : strlen(1Byte) + str(26Bytes). 				 // Response Data[]: Null or Error Message
            CMD_GET_KEY_WORD_FILE = 0x00DA,     // Send Data : NULL				// keyword.txt or Error Message

            CMD_SEARCH_TOOL_PARA_FILE = 0x00E1, // Send Data: NULL 				// Response Data[]: File Number(2Byte) + 1st strlen(File Name) + 1st str(File Name) + 2nd strlen(File Name) + 2nd str(File Name)...
            CMD_GET_TOOL_PARA_XML_FILE = 0x00E2,// Send Data: strlen(file Path(fileName(1Byte))) + str(file Name)	// Response Data[]: XML File
            CMD_CHANGE_USER_DEFINE_TOOL_POS = 0x00E3,// Send Data[]: Index(1Byte -> 0~9), sizeof(TOOL_POS) 			// Response Data[]: Null or Error Message

            CMD_ADD_FILE = 0x00EA,
            CMD_CREATE_FOLDER = 0x00EB,
            CMD_DELETE_FILE = 0x00EC,

            CMD_GET_ERR_STRING = 0x00F0,        // Send Data[4] : ErrCode(4Bytes) // Response Data[] : ErrCode(4Bytes) + strlen(1Byte) + strlen(string)
                                                //************************************************************************************************************************************************************************
                                                // Jog & Teaching
                                                //************************************************************************************************************************************************************************
                                                // Profile based on Time ***********************************************************************************************************************************************
                                                // AxisNo : 0 ~ 63 (Based on Joint Coordinate), 64 ~ 127 (Based on Work Coordinate, Ex 64 -> X, 65 -> Y, 66 -> Z...   if Set Command  128(0xF0) : All Joint Axis, 255(0xFF) : All Work Axis)
            CMD_JOG_GET_VELP = 0x0100,      // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)
            CMD_JOG_SET_VELP = 0x0101,      // Send Data[2]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message
            CMD_JOG_GET_XVELP = 0x0102,     // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)
            CMD_JOG_SET_XVELP = 0x0103,     // Send Data[2]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message
            CMD_JOG_GET_ATIME = 0x0104,     // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)
            CMD_JOG_SET_ATIME = 0x0105,     // Send Data[2]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message
            CMD_JOG_GET_DTIME = 0x0106,     // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)
            CMD_JOG_SET_DTIME = 0x0107,     // Send Data[2]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message
            CMD_JOG_GET_AJERKP = 0x0108,    // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)
            CMD_JOG_SET_AJERKP = 0x0109,    // Send Data[2]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message
            CMD_JOG_GET_DJERKP = 0x010A,    // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)
            CMD_JOG_SET_DJERKP = 0x010B,    // Send Data[2]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message
                                            // Axis -> must be input 0xF0, 0xFF as AxisNo
            CMD_JOG_GET_MPARA_BASE_TIME = 0x010C,       // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(VelP, ATIME, DTIME, AJERKP, DJERKP, KillDTime)
            CMD_JOG_SET_MPARA_BASE_TIME = 0x010D,       // Send Data[1]: Robot No(1Byte) + AxisNo(1Byte) + Value(VelP, ATIME, DTIME, AJERKP, DJERKP, KillDTime)		// Response Data: NULL or Err Message
            CMD_JOG_GET_DEF_MPARA_BASE_TIME = 0x010E,   // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(VelP, ATIME, DTIME, AJERKP, DJERKP, KillDTime)
            CMD_JOG_SET_DEF_MPARA_BASE_TIME = 0x010F,   // Send Data[1]: Robot No(1Byte) + AxisNo(1Byte) + Value(VelP, ATIME, DTIME, AJERKP, DJERKP, KillDTime)		// Response Data: NULL or Err Message

            // Profile based on Velocity(Uint) **********************************************************************************************************************************
            CMD_JOG_GET_VEL = 0x0110,       // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)
            CMD_JOG_SET_VEL = 0x0111,       // Send Data[2]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message
            CMD_JOG_GET_XVEL = 0x0112,      // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)
            CMD_JOG_SET_XVEL = 0x0113,      // Send Data[2]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message
            CMD_JOG_GET_ACC = 0x0114,       // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)
            CMD_JOG_SET_ACC = 0x0115,       // Send Data[2]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message
            CMD_JOG_GET_DEC = 0x0116,       // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)
            CMD_JOG_SET_DEC = 0x0117,       // Send Data[2]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message
            CMD_JOG_GET_AJERK = 0x0118,     // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)
            CMD_JOG_SET_AJERK = 0x0119,     // Send Data[2]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message
            CMD_JOG_GET_DJERK = 0x011A,     // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)
            CMD_JOG_SET_DJERK = 0x011B,     // Send Data[2]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message
                                            // Axis -> must be input 0xF0, 0xFF as AxisNo
            CMD_JOG_GET_MPARA_BASE_VEL = 0x011C,        // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)				// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Vel, ACC, DEC, AJERK, DJERK, KillDec)
            CMD_JOG_SET_MPARA_BASE_VEL = 0x011D,        // Send Data[1]: Robot No(1Byte) + AxisNo(1Byte) + Value(Vel, ACC, DEC, AJERK, DJERK, KillDec)		// Response Data: NULL or Err Message
            CMD_JOG_GET_DEF_MPARA_BASE_VEL = 0x011E,    // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Vel, ACC, DEC, AJERK, DJERK, KillDec)
            CMD_JOG_SET_DEF_MPARA_BASE_VEL = 0x011F,    // Send Data[1]: Robot No(1Byte) + AxisNo(1Byte) + Value(Vel, ACC, DEC, AJERK, DJERK, KillDec)		// Response Data: NULL or Err Message

            CMD_JOG_MOVE_NEGATIVE = 0x0120,     // Send Data[2]: BYTE(R) + BYTE(A)				// Response Data: NULL or Err Message
            CMD_JOG_MOVE_POSITIVE = 0x0121,     // Send Data[2]: BYTE(R) + BYTE(A)				// Response Data: NULL or Err Message

            CMD_JOG_MOVE_X_NEGATIVE = 0x0130,   // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message
            CMD_JOG_MOVE_X_POSITIVE = 0x0131,   // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message
            CMD_JOG_MOVE_Y_NEGATIVE = 0x0132,   // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message
            CMD_JOG_MOVE_Y_POSITIVE = 0x0133,   // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message
            CMD_JOG_MOVE_Z_NEGATIVE = 0x0134,   // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message
            CMD_JOG_MOVE_Z_POSITIVE = 0x0135,   // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message

            CMD_JOG_MOVE_TX_NEGATIVE = 0x0140,  // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message
            CMD_JOG_MOVE_TX_POSITIVE = 0x0141,  // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message
            CMD_JOG_MOVE_TY_NEGATIVE = 0x0142,  // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message
            CMD_JOG_MOVE_TY_POSITIVE = 0x0143,  // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message
            CMD_JOG_MOVE_TZ_NEGATIVE = 0x0144,  // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message
            CMD_JOG_MOVE_TZ_POSITIVE = 0x0145,  // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message

            CMD_JOG_MOVE_RX_NEGATIVE = 0x0150,
            CMD_JOG_MOVE_RX_POSITIVE = 0x0151,
            CMD_JOG_MOVE_RY_NEGATIVE = 0x0152,
            CMD_JOG_MOVE_RY_POSITIVE = 0x0153,
            CMD_JOG_MOVE_RZ_NEGATIVE = 0x0154,
            CMD_JOG_MOVE_RZ_POSITIVE = 0x0155,

            CMD_JOG_MOVE_TRX_NEGATIVE = 0x0160,
            CMD_JOG_MOVE_TRX_POSITIVE = 0x0161,
            CMD_JOG_MOVE_TRY_NEGATIVE = 0x0162,
            CMD_JOG_MOVE_TRY_POSITIVE = 0x0163,
            CMD_JOG_MOVE_TRZ_NEGATIVE = 0x0164,
            CMD_JOG_MOVE_TRZ_POSITIVE = 0x0165,

            CMD_JOG_MOVE_STOP = 0x0180,         // Send Data[1]: BYTE(R)						// Response Data: NULL or Err Message
            CMD_JOG_MOVE_STOP_AXIS = 0x0181,    // Send Data[1]: BYTE(R) + BYTE(A)				// Response Data: NULL or Err Message

            CMD_JOG_MOVE_STOP_ALL = CMD_MOVE_ESTOP, // Send Data[1]: BYTE(R)				// Response Data: NULL or Err Message

            // Based on Joint Axis
            CMD_JOG_GOTO = 0x0190,              // Send Data: BYTE(R) + BYTE(A) + DOUBLE		// Response Data: NULL or Err Message  -> Goto Target Position
            CMD_JOG_STEP = 0x0191,              // Send Data: BYTE(R) + BYTE(A) + DOUBLE		// Response Data: NULL or Err Message  -> Goto Increment 
            CMD_JOG_BACK_FORTH = 0x0192,        // Send Data: BYTE(R) + BYTE(A) + DOUBLE(1st) + DOUBLE(2nd) + DOUBLE(WaitM time)		// Response Data: NULL or Err Message

            // Based on World(Tool) Axis
            CMD_JOG_GOTO_WORLD = 0x01A0,        // Send Data: BYTE(R) + BYTE(A) + DOUBLE		// Response Data: NULL or Err Message  -> Goto Target Position
            CMD_JOG_STEP_WORLD = 0x01A1,        // Send Data: BYTE(R) + BYTE(A) + DOUBLE		// Response Data: NULL or Err Message  -> Goto Increment
            CMD_JOG_BACK_FORTH_WORLD = 0x01A2,  // Send Data: BYTE(R) + BYTE(A) + DOUBLE(1st) + DOUBLE(2nd) + DOUBLE(WaitM time)		// Response Data: NULL or Err Message

            //************************************************************************************************************************************************************************
            // Motion Parameters
            //************************************************************************************************************************************************************************
            CMD_GET_VELP = 0x0200,              // Send Data[1]: BYTE(R):1Byte 			// Response Data[10]: Robot No(1Byte) + Value(Double:8Bytes)
            CMD_SET_VELP = 0x0201,              // Send Data[2]: Robot No(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message

            // AxisNo : 0 ~ 63 (Based on Joint Coordinate), 64 ~ 127 (Based on Work Coordinate, Ex 64 -> X, 65 -> Y, 66 -> Z...   if Set Command  128(0xF0) : All Joint Axis, 255(0xFF) : All Work Axis)
            CMD_GET_VEL = 0x0202,               // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[10]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)	            
            CMD_SET_VEL = 0x0203,               // Send Data[2]: Robot No(1Byte) + AxisNo(1Byte) + Value(Double:8Bytes)		// Response Data: NULL or Err Message            
            CMD_GET_XVEL = 0x0204,
            CMD_SET_XVEL = 0x0205,
            // Based on Velocity
            CMD_GET_ACC = 0x0206,
            CMD_SET_ACC = 0x0207,
            CMD_GET_DEC = 0x0208,
            CMD_SET_DEC = 0x0209,
            CMD_GET_AJERK = 0x020A,
            CMD_SET_AJERK = 0x020B,
            CMD_GET_DJERK = 0x020C,
            CMD_SET_DJERK = 0x020D,
            // Based on Time
            CMD_GET_ACCTIME = 0x020E,
            CMD_SET_ACCTIME = 0x020F,
            CMD_GET_DECTIME = 0x0210,
            CMD_SET_DECTIME = 0x0211,
            CMD_GET_AJERKP = 0x0212,
            CMD_SET_AJERKP = 0x0213,
            CMD_GET_DJERKP = 0x0214,
            CMD_SET_DJERKP = 0x0215,

            CMD_SET_PROFILE_MODE_VEL = 0x0216,  //[X] Send Data[1]: BYTE(R)			// Response Data: NULL or Err Message
            CMD_SET_PROFILE_MODE_TIME = 0x0217, //[X] Send Data[1]: BYTE(R)			// Response Data: NULL or Err Message

            //CMD_GET_ROBOT_MOTION_PARA = 0x0230,	// Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte) 		// Response Data[]: Robot No(1Byte) + AxisNo(1Byte) + Value(Ref Vel, ATIME, DTIME, AJERKP, DJERKP)
            //CMD_SET_ROBOT_MOTION_PARA = 0x0231,	// Send Data[1]: Robot No(1Byte) + AxisNo(1Byte) + Value(Ref Vel, ATIME, DTIME, AJERKP, DJERKP)		// Response Data: NULL or Err Message	

            CMD_GET_MPARA_BASE_TIME = 0x0250,   // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte) 		// Response Data[]: Robot No(1Byte) + AxisNo(1Byte) + Value(Ref Vel, ATIME, DTIME, AJERKP, DJERKP, KillDTime)
            CMD_SET_MPARA_BASE_TIME = 0x0251,   // Send Data[1]: Robot No(1Byte) + AxisNo(1Byte) + Value(Ref Vel, ATIME, DTIME, AJERKP, DJERKP, KillDTime)		// Response Data: NULL or Err Message

            CMD_GET_MPARA_BASE_VEL = 0x0252,    // Send Data[1]: BYTE(R):1Byte + AxisNo(1Byte)		// Response Data[]: Robot No(1Byte) + AxisNo(1Byte) + Value(Ref Vel, ACC, DEC, AJERK, DJERK, KillDec)
            CMD_SET_MPARA_BASE_VEL = 0x0253,    // Send Data[1]: Robot No(1Byte) + AxisNo(1Byte) + Value(Vel, ACC, DEC, AJERK, DJERK, KillDec)		// Response Data: NULL or Err Message

            CMD_GET_EMS_DTIME = 0x0280,         // Send Data[1]: Robot No			// Response Data[9]: Robot No(1Byte) + Value(EDTIME(Double))
            CMD_SET_EMS_DTIME = 0x0281,         // Send Data[1]: Robot No Robot No(1Byte) + Value(EDTIME(Double))	// Response Data: NULL or Err Message

            CMD_GET_JOINT_VEL = 0x0291,         // Send Data[1]: Robot No			// Response Data[9]: Robot No(1Byte) + Value(Double) * AxisNum
            CMD_SET_JOINT_VEL = 0x0292,         // Send Data[1]: Robot No(1Byte) + Value(Double) * AxisNum  // Response Data: NULL or Err Message
            CMD_GET_WORK_VEL = 0x0293,
            CMD_SET_WORK_VEL = 0x0294,
            CMD_GET_HOME_VEL = 0x0295,
            CMD_SET_HOME_VEL = 0x0296,

            CMD_GET_JOINT_ATIME = 0x0297,
            CMD_SET_JOINT_ATIME = 0x0298,
            CMD_GET_WORK_ATIME = 0x0299,
            CMD_SET_WORK_ATIME = 0x029A,
            CMD_GET_HOME_ATIME = 0x029B,
            CMD_SET_HOME_ATIME = 0x029C,

            CMD_GET_JOINT_DTIME = 0x02B0,
            CMD_SET_JOINT_DTIME = 0x029E,
            CMD_GET_WORK_DTIME = 0x029F,
            CMD_SET_WORK_DTIME = 0x02A0,
            CMD_GET_HOME_DTIME = 0x02A1,
            CMD_SET_HOME_DTIME = 0x02A2,

            CMD_GET_JOINT_KILL_DTIME = 0x02A3,
            CMD_SET_JOINT_KILL_DTIME = 0x02A4,
            CMD_GET_WORK_KILL_DTIME = 0x02A5,
            CMD_SET_WORK_KILL_DTIME = 0x02A6,
            CMD_GET_HOME_KILL_DTIME = 0x02A7,
            CMD_SET_HOME_KILL_DTIME = 0x02A8,

            CMD_GET_JOINT_JERK_TIMEP = 0x02A9,
            CMD_SET_JOINT_JERK_TIMEP = 0x02AA,
            CMD_GET_WORK_JERK_TIMEP = 0x02AB,
            CMD_SET_WORK_JERK_TIMEP = 0x02AC,
            CMD_GET_HOME_JERK_TIMEP = 0x02AE,
            CMD_SET_HOME_JERK_TIMEP = 0x02AF,

            CMD_GET_READY_MPARA_BASE_TIME = 0x02C0,
            CMD_SET_READY_MPARA_BASE_TIME = 0x02C1,
            CMD_GET_READY_POSITION = 0x02F0,
            CMD_SET_READY_POSITION = 0x02F1,
            CMD_GET_ORG_MPARA = 0x02FA,
            CMD_SET_ORG_MPARA = 0x02FB,

            //************************************************************************************************************************************************************************
            // Program
            //************************************************************************************************************************************************************************
            CMD_PRG_EXEC_SINGLE_STRING = 0x0300,// Send Data[1+n]: BYTE(R) + STRING											// Response Data: NULL or Err Message
            CMD_PRG_OPEN = 0x0301,              // Send Data[n(strlen)]: Program Task Name										// Response Data: STRING
            CMD_PRG_SAVE = 0x0302,              // Send Data[n(strlen+'\0')+String(Program)]: Program Task Name + STRING(Program)		// Response Data: NULL or Err Message
            CMD_PRG_COMPILE = 0x0303,           // Send Data[n(strlen)]: Program Task Name		// Response Data: NULL or Err Message
            CMD_PRG_START = 0x0304,             // Send Data[n(strlen)]: Program Task Name		// Response Data: NULL or Err Message
            CMD_PRG_STOP = 0x0305,              // Send Data[n(strlen)]: Program Task Name		// Response Data: NULL or Err Message
            CMD_PRG_STOP_ALL = 0x0306,          // Send Data[0]: 							// Response Data: NULL or Err Message
            CMD_PRG_PAUSE = 0x0307,             // Send Data[n(strlen)]: Program Task Name		// Response Data: NULL or Err Message
            CMD_PRG_RESUME = 0x0308,            // Send Data[n(strlen)]: Program Task Name		// Response Data: NULL or Err Message

            CMD_PRG_GET_STATUS = 0x0309,        // Send Data[1]: BYTE(R)					// Response Data[]: Undefine
                                                //CMD_PRG_GET_LINE_NO = 0x030A,	    // Send Data: [ProgNameLength:1][ProgName:n]				// Response Data: [ProgNameLength:1][ProgName:n][LineNo:2]: 1+n+2 bytes
                                                //CMD_PRG_SET_LINE_NO = 0x030B,	    // Send Data: [ProgNameLength:1][ProgName:n][LineNo:2]		// Response Data: NULL or Err Message 
                                                //CMD_PRG_SAVE_TO_FLASH = 0x030C,   // Send Data: [ProgNameLength:1][ProgName:n]				// Response Data: NULL or Err Message

            CMD_PRG_GET_AUTORUN = 0x0310,       // Send Data[0]: null			// Response Data[1+n]: Program Count (1Byte) + 1st flag (1Byte, 0 is Off, 1 is On) + 2nd flag (1Byte) + 3rd flag(1Byte) + .... + 255th (1Byte)
            CMD_PRG_SET_AUTORUN = 0x0311,       // Send Data[1+n]: Program Count (1Byte) + 1st flag (1Byte, 0 is Off, 1 is On) + 2nd flag (1Byte) + 3rd flag(1Byte) + .... + 255th (1Byte)	// Response Data: NULL or Err Message

            // Sub Prg has Name, Descrition String
            CMD_SUB_PRG_OPEN = 0x0321,          // Send Data : Sub Prg No(1Byte)				  // Response Data: STRING( 
            CMD_SUB_PRG_SAVE = 0x0322,
            CMD_SUB_PRG_COMPILE = 0x0323,
            CMD_SUB_PRG_GET_STATUS = 0x0324,
            CMD_SUB_PRG_DELETE = 0x00325,

            CMD_RB_DEF_FILE_OPEN = 0x0331,
            CMD_RB_DEF_FILE_SAVE = 0x0332,
            CMD_RB_DEF_FILE_COMPILE = 0x0333,
            CMD_RB_DEF_GET_STATUS = 0x0334,

            CMD_PRG_GET_ALL_STATUS = 0x0340,
            CMD_SUB_PRG_GET_ALL_STATUS = 0x0341,
            // Add 2018-08-30
            // Get error string of sub-program
            CMD_SUB_PRG_GET_ERR_MSG = 0x0342,   //Send: [SubPrgName: 1][Name: N]; Rec: [MsgLength:1][Msg: N]

            //CMD_ADD_SUB_PRG_NO = 0x0351,	// SubPrg(1)
            //CMD_ADD_SUB_PRG_STR = 0x0352,	// SubPrg("SubPrgName")

            //CMD_PRG_OPEN = 0x03A0,	            // CMD_PRG_GET_STRING
            //CMD_PRG_SAVE = 0x03A1,	            // CMD_PRG_SET_STRING
            CMD_PRG_SET_LINE = 0x03A2,          // CMD_PRG_SET_LINE_NO
            CMD_PRG_GET_LINE = 0x03A3,          // CMD_PRG_GET_LINE_NO
            CMD_PRG_GET_LIST = 0x03A4,
            //CMD_PRG_NEW_FILE = 0x03A5,
            //CMD_PRG_TERMINAL_SET_MSG = 0x03A6,
            CMD_PRG_GET_ERR_MSG = 0x03A7,
            //CMD_PRG_TRACE = 0x03A8,  //Python Trace function to log message
            //CMD_PRG_SET_ERR_MSG = 0x03A9,
            CMD_CPU_STAT = 0x03AA,              // Get CPU Usage String     // Send Data : null     // Response Data : strlen(1) + Status String(n bytes)
                                                //************************************************************************************************************************************************************************
                                                // Variables
                                                //************************************************************************************************************************************************************************
                                                //Position Variable
            CMD_GET_POS_VAR_ALL = 0x0400,       // Send Data[1]: BYTE(R)											// Response Data: Robot Number(1Byte) + Axis Number(1Byte) + Variable Number(2Bytes) + Strlen(1Byte) + Strlen(Description) + Work Position(Double) * AxisNum... Strlen(1Byte) + Strlen(Description) + Joint Position(Double) * AxisNum or Err Message
            CMD_SET_POS_VAR_ALL = 0x0401,       // Send Data : RobotNo(1byte) + AxisCount(1byte) + VarCount(2bytes) + (VarCount(n) * (DescLen(1Byte) + Desc (n Byte) + Double(8bytes)*AxisCount(n))		// Response Data: Null or Err Message

            CMD_GET_WPOS_VAR = 0x0402,          // Send Data[3]: BYTE(R)+Index(2Bytes)								// Response Data : strlen(1) + Description + (Double*Axes No) or Err Message
            CMD_SET_WPOS_VAR = 0x0403,          // Send Data : RobotNo(1byte) + Index(2Bytes) + DescLen(1Byte) + Desc (n Byte) + (Double (8 Bytes) * Position Count)	// Response : Null or Err Message

            CMD_GET_JPOS_VAR = 0x0404,          // Send Data[3]: BYTE(R)+Index(2Bytes)								// Response Data : strlen(1) + Description + (Double*Axes No) or Err Message
            CMD_SET_JPOS_VAR = 0x0405,          // Send Data : RobotNo(1byte) + Index(2Bytes) + DescLen(1Byte) + Desc (n Byte) + (Double (8 Bytes) * Position Count)	// Response : Null or Err Message

            //CMD_CHANGE_POS_VAR_NUM = 0x0410,	// Send Data[3]: BYTE(R)+Changed Variable Number(2Bytes)				// Response Data : NULL or Err Message
            CMD_ADD_POS_VAR = 0x0411,           // Send Data : Robot Number(1Byte) + Axis Number(1Byte) + strlen(1) + WPos_Desc + WPos(8 Bytes * Axis Number) + strlen(1) + JPos_Desc + JPos(8 Bytes * Axis Number) 			// Response Data : NULL or Err Message
                                                //CMD_INSERT_POS_VAR = 0x0412,	    // Send Data[3] : BYTE(R)+Index(2Bytes) 							// Response Data : NULL or Err Message
            CMD_DELETE_POS_VAR = 0x0413,        // Send Data[3] : BYTE(R)                							// Response Data : NULL or Err Message

            //I, D Variable
            CMD_GET_I_VAR_ALL = 0x0480,         // Send Data[1]: BYTE(R)											// Response Data: Robot Number(1Byte) + Variable Number(2Bytes) + Strlen(1Byte) + Strlen(Description) + Work Position(Double) * AxisNum... Strlen(1Byte) + Strlen(Description) + Variable .... or Err Message
            CMD_SET_I_VAR_ALL = 0x0481,         // Send Data[3+StrLen(1)+Desc(n)+Value]: Robot Number(1Byte) + Variable Number(2Bytes) + Strlen(1Byte) + Strlen(Description) + Work Position(Double) * AxisNum... Strlen(1Byte) + Strlen(Description) + Variable		// Response Data: Null or Err Message

            CMD_GET_I_VAR = 0x0482,             // Send Data[3]: BYTE(R) + Index(2Bytes)							// Response Data: strlen(Description) + Integer or Err Message
            CMD_SET_I_VAR = 0x0483,             // Send Data[3+strlen(Description)+4bytes]: BYTE(R) + Index(2Bytes) + strlen(Description) + 4Bytes(Value)	// Response Data: Null or Err Message

            CMD_GET_D_VAR = 0x0484,             // Send Data[3]: BYTE(R) + Index(2Bytes)							// Response Data: strlen(Description) + Double or Err Message
            CMD_SET_D_VAR = 0x0485,             // Send Data[3+strlen(Description)+8bytes]: BYTE(R) + Index(2Bytes) + strlen(Description) + 8Bytes(Value)	// Response Data: Null or Err Message

            CMD_GET_D_VAR_ALL = 0x0490,         // Send Data[1]: BYTE(R)											// Response Data: Robot Number(1Byte) + Variable Number(2Bytes) + Strlen(1Byte) + Strlen(Description) + Work Position(Double) * AxisNum... Strlen(1Byte) + Strlen(Description) + Variable .... or Err Message
            CMD_SET_D_VAR_ALL = 0x0491,         // Send Data[1+strlen(RVal.xml)]: Robot Number(1Byte) + Variable Number(2Bytes) + Strlen(1Byte) + Strlen(Description) + Work Position(Double) * AxisNum... Strlen(1Byte) + Strlen(Description) + Variable		// Response Data: Null or Err Message

            //CMD_CHANGE_I_VAR_NUM = 0x04A0,	    // Send Data[3]: BYTE(R)+Changed Variable Number(2Bytes)			// Response Data : NULL or Err Message
            CMD_ADD_I_VAR = 0x04A1,             // Send Data : BYTE(R) + StrLen(1Byte) + Desc + Integer (4Bytes) 		// Response Data : NULL or Err Message
                                                //CMD_INSERT_I_VAR = 0x04A2,	        // Send Data[3] : BYTE(R)+Index(2Bytes) 							// Response Data : NULL or Err Message
            CMD_DELETE_I_VAR = 0x04A3,          // Send Data[3] : BYTE(R)                   							// Response Data : NULL or Err Message

            //CMD_CHANGE_D_VAR_NUM = 0x04B0,	    // Send Data[3]: BYTE(R)+Changed Variable Number(2Bytes)			// Response Data : NULL or Err Message
            CMD_ADD_D_VAR = 0x04B1,             // Send Data : BYTE(R) + StrLen(1Byte) + Desc + Double (8Bytes) 		// Response Data : NULL or Err Message
                                                //CMD_INSERT_D_VAR = 0x04B2,	        // Send Data[3] : BYTE(R)+Index(2Bytes) 							// Response Data : NULL or Err Message
            CMD_DELETE_D_VAR = 0x04B3,          // Send Data[3] : BYTE(R)                    							// Response Data : NULL or Err Message

            //************************************************************************************************************************************************************************
            // IO Variable
            //************************************************************************************************************************************************************************
            CMD_GET_DI_NUM = 0x0500,            // Send Data[0] 												// Response Data: Number(2Bytes) or Err Message
            CMD_GET_DOUT_NUM = 0x0501,          // Send Data[0]													// Response Data: Number(2Bytes) or Err Message
            CMD_GET_AI_NUM = 0x0502,            // Send Data[0] 												// Response Data: Number(2Bytes) or Err Message
            CMD_GET_AOUT_NUM = 0x0503,          // Send Data[0] 												// Response Data: Number(2Bytes) or Err Message

            CMD_GET_DI_PORT = 0x0510,           // Send Data[2] : 2Bytes(Port Index) 							// Response Data: Value(4Bytes) or Err Message
                                                //CMD_GET_DI_PORT_BIT = 0x0511,       // Send Data[3] : 2Bytes(Port Index)+1Byte(bit number)			// Response Data: Value(1Bytes -> 0 or 1) or Err Message
            CMD_GET_DI_PORT_SIZE = 0x0512,      // Send Data[0] : 2Bytes (Port Index)                           //Response Data : Port Size(2Byte) ->0,1,2,3,4   
            CMD_GET_DI_ALL = 0x051A,            // Send Data[0] : -                                             // Response Data: Number(2Bytes) + Value(4Bytes) * Number

            CMD_GET_DOUT_PORT = 0x0521,         // Send Data[2] : 2Bytes(Port Index) 								// Response Data: Value(4Bytes) or Err Message
            CMD_SET_DOUT_PORT = 0x0522,         // Send Data[6] : 2Bytes(Port Index)+Value(Int 4Bytes)					// Response Data: NULL or Err Message
                                                //CMD_GET_DOUT_PORT_BIT = 0x0523,	    // Send Data[2] : 2Bytes(Port Index)+1Byte(bit number)					// Response Data: Value(1Bytes -> 0 or 1) or Err Message
            CMD_SET_DOUT_PORT_BIT = 0x0524,     // Send Data[4] : 2Bytes(Port Index)+1Byte(bit number)+1Byte(Value 0 or 1)	// Response Data: NULL or Err Message
            CMD_GET_DOUT_PORT_SIZE = 0x0525,     //sungmin
            CMD_GET_DOUT_ALL = 0x052A,          // Send Data[4] :                                               // Response Data: Number(2Bytes) + Value(4Bytes) * Number

            CMD_GET_AI = 0x0530,                // Send Data[2] : 2Bytes(Port Index) 								// Response Data: Value(4Bytes) or Err Message
            CMD_GET_AI_ALL = 0x053A,            // Send Data[4] :                                               // Response Data: Number(2Bytes) + Value(4Bytes) * Number

            CMD_GET_AO = 0x0540,                // Send Data[2] : 2Bytes(Port Index) 								// Response Data: Value(4Bytes) or Err Message
            CMD_SET_AO = 0x0541,                // Send Data[6] : 2Bytes(Port Index)+Value(Int 4Bytes)					// Response Data: NULL or Err Message
            CMD_GET_AO_ALL = 0x054A,            // Send Data[4] :                                               // Response Data: Number(2Bytes) + Value(4Bytes) * Number

            //  DIO built in Drive
            CMD_GET_DRV_DI_ALL = 0x0550,    // Send Data[] : 1Byte (Robot No)								// Response Data: Robot No(1Byte) + UINT32 Value * Axis Num.
            CMD_GET_DRV_DI_PORT = 0x0551,   // Send Data[] : 2 Byte (Robot No + Axis No)					// Response Data: Robot No(1Byte) + Axis No(1Byte) + UINT32 Value.
            CMD_GET_DRV_DI_BIT = 0x0552,    // Send Data[] : 3 Byte (Robot No + Axis No + Bit No)			// Response Data: Robot No(1Byte) + Axis No(1Byte) + Bit No(1Byte) + Value(1Byte).

            CMD_GET_DRV_DO_ALL = 0x0560,    // Send Data[] : 1Byte (Robot No)								// Response Data: Robot No(1Byte) + UINT32 Value * Axis Num.
            CMD_GET_DRV_DO_PORT = 0x0561,   // Send Data[] : 2 Byte (Robot No + Axis No)					// Response Data: Robot No(1Byte) + Axis No(1Byte) + UINT32 Value.
            CMD_SET_DRV_DO_PORT = 0x0562,   // Send Data[] : 6 Byte (Robot No + Axis No + Value(4Bytes))	// Response Data: NULL or Err Message

            CMD_GET_DRV_DO_BIT = 0x0563,    // Send Data[] : 3 Byte (Robot No + Axis No + Bit No)					// Response Data: Robot No(1Byte) + Axis No(1Byte) + Bit No(1Byte) + Value(1Byte)
            CMD_SET_DRV_DO_BIT = 0x0564,    // Send Data[] : 4 Byte (Robot No + Axis No + Bit No + Value(1Byte))	// Response Data: NULL or Err Message

            // TP Command
            //CMD_TP_KEY_EVENT = 0x0601,      	// Send Data[3] : Key_Event(2Bytes), Key_Status(Press, Long Press, Released)

            // Firmware
            CMD_FW_GET_VERSION = 0x0701,        // Send Data[0]: NULL					// Response Data: strlen(1Byte) + str
                                                //CMD_FW_UPGRADE = 0x0702,	        // Send Data[]: Undefine				// Response Data[]: Undefine

            // PC Libaray(or Law Data) Protocol
            CMD_LIB_MOVE = 0x0800,              // Send Data : RbNo(1Byte) + Motion Type(2Byte, CMD_ROBOT_MOVEJ, CMD_ROBOT_MOVEA...) 
                                                // + Pos Type(Refer Pos Type Struct(POS_CONSTANT, POS_VARIABLE, D_VARIABLE) + Sz(2Byte) + Data(?)..
                                                // Respone data: NULL or Error Message

            //Robot CMD
            CMD_ROBOT_MOVEJ = 0x0A01,           // Send Data: [RobotNo:1][8xn]: n axis           //Respone data: NULL or Error Message
            CMD_ROBOT_MOVE = 0x0A02,            // Send Data: [RobotNo:1][8xn]: n axis           //Respone data: NULL or Error Message
            CMD_ROBOT_MOVEL = 0x0A03,           // Send Data: [RobotNo:1][8xn]: n axis           //Respone data: NULL or Error Message
            CMD_ROBOT_MOVEC = 0x0A04,           // Send Data: [RobotNo:1][8xn]: n axis           //Respone data: NULL or Error Message

            CMD_ROBOT_CHECK_WAITM = 0x0A06,     // Send Data: NULL						//Respone data: [Status:1]
            CMD_BLEND_START = 0x0A07,           // Send Data: [RobotNo:1]           //Respone data: NULL or Error Message
            CMD_BLEND_END = 0x0A08,             // Send Data: [RobotNo:1]        //Respone data: NULL or Error Message

            CMD_PRG_ROBOT_MAP = 0x0A09,
            CMD_ROBOT_MOVEJ_REL = 0x0A0A,
            CMD_ROBOT_MOVEL_REL = 0x0A0B,
            CMD_ROBOT_MOVE_REL = 0x0A0C,
            CMD_ROBOT_MOVE_X = 0x0A0D,
            CMD_ROBOT_MOVE_Y = 0x0A0E,
            CMD_ROBOT_MOVE_Z = 0x0A0F,
            CMD_ROBOT_MOVE_TX = 0x0A10,
            CMD_ROBOT_MOVE_TY = 0x0A11,
            CMD_ROBOT_MOVE_TZ = 0x0A12,

            CMD_RESTART_ECMASTER = 0x0B00,      // Send Data: NULL           //Respone data: [Status:1]: 0 Off, 1:On
            CMD_END_ECMASTER = 0x0B01,          // Send Data: NULL           //Respone data: NULL or Error Message
            CMD_SYSTEM_REBOOT = 0x0B02,         // Send Data: NULL           //Respone data: NULL or Error Message
            CMD_TRANSFER_ENI = 0x0B03,          // Send Data: XML data file: N bytes           //Respone data: NULL or Error Message

            CMD_SET_TOOL_Z = 0x0C00,
            //************************************************************************************************************************************************************************
            // Homing Commands
            //************************************************************************************************************************************************************************
            CMD_START_HOMING = 0x0D01,          //Send Data: RobotNo:1 + SelAxis(4Bytes)		//Respone data: NULL or Error Message
            CMD_STOP_HOMING = 0x0D02,           //Send Data : NULL								//Respone data: NULL or Error Message
            CMD_GET_HOMING_STATUS = 0x0D10,     //Send Data : NULL								//Respone data: RobotNo(1Byte) + (AxisNum(1Byte) * (OP Mode (1Byte) + HOMING_FLAG(1Byte)) or Error Message

            //************************************************************************************************************************************************************************
            // Log File
            //************************************************************************************************************************************************************************
            CMD_LOAD_LOG_FILE = 0x0E01,         // Load Log File(CMD + Log ID(1Byte))                       // Response Data: Log ID(1Byte) + Log file string...
            CMD_GET_LOG_LINE_NUM = 0x0E02,      // Get Log Line Num (CMD + Log ID)                          // Response Data: Log ID(1Byte) + Number of Line(4Bytes)
            CMD_GET_LOG_STR = 0x0E10,           // Get Log Str (CMD + Log ID + Start Line Num(4Bytes), Stop Line Num(4Bytes) )    // Response Log ID(1Byte) + String....
            CMD_CLEAR_LOG = 0x0E11,             // Clear Log(CMD + Log ID)                                  // Response Data : NULL or Err Message
                                                //************************************************************************************************************************************************************************
                                                // Scope
                                                //************************************************************************************************************************************************************************
            CMD_GET_SCOPE_PARA_ALL = 0x0F01,    // Get Scope Parameter(CMD + Data Sz(0x00))     //Response : CMD_ACK + Data Sz(4Bytes) + TY_SCOPE_PARA * MAX_CH_NUM(8CH)
            CMD_SET_SCOPE_PARA_ALL = 0x0F02,    // Set Scope Parameter(CMD + Data Sz(4Bytes) + TY_SCOPE_PARA * MAX_CH_NUM(8CH)
            CMD_SET_SCOPE_PARA = 0x0F03,        // Set Scope Parameter(CMD + Data Sz(4Bytes) + ChNo(1Bytes) + TY_SCOPE_PARA																
            CMD_RUN_SCOPE = 0x0F10,             // Run Scope(CMD + Data Sz(0x00))
            CMD_STOP_SCOPE = 0x0F11,            // Stop Scope(CMD + Data Sz(0x00))	
            CMD_GET_SCOPE_DATA = 0x0F20,        // Get Scope Data(CMD + Data Sz(0x00) )
                                                // Response : 100 Data  -> CMD_ACK + Data(Ch1 1st(Double : 8Bytes), Ch2 1st, Ch3 1st, Ch4 1st,....... Ch1 100th, Ch2 100th Ch2 100th)
                                                // ERR CODE
            CMD_FACTORY_RESET = 0x0FB0,         // Send Data : 1Byte(Program(0bit), Pos Variable(1bit), Variable(2bit) -> 0 : No, 1 : Yes(Reset)  // Response Data : NULL or Err Message
            CMD_ERR_NOT_DEFINE = 0x0F00,

            CMD_GET_LA_PARA_FILE = 0x0F10,      // Send Data[]: LAxis Number (1Byte)				// Response Data[]: XML File
            CMD_SET_LA_PARA_FILE = 0X0F11,
            CMD_GET_LT_PARA_FILE = 0x0F12,      // Send Data[]: LTrack Number (1Byte)				// Response Data[]: XML File
            CMD_SET_LT_PARA_FILE = 0X0F13,
            CMD_GET_LC_PARA_FILE = 0x0F14,      // Send Data[]: LCarrier Number (1Byte)				// Response Data[]: XML File
            CMD_SET_LC_PARA_FILE = 0X0F15,

            CMD_CONNECT = 0x03B1,
            CMD_GET_CMD_POS = 0x0046,
            CMD_GET_STATUS_ROBOT_FOR_PRG = 0x0024,

            CMD_GET_CARR_INFO_ALL = 0x101B,      // Send Data[0]:NULL                               // Response Data : Total Carrier Num 1Byte + Struct SCarrierInfo * Carrier Num
            CMD_GET_LM_TRACK_POS_ALL = 0x1060       // Send Data[0]:NULL                               // Response Data : Total Shuttle & Lift Num 1Byte + Struct SShuttleInfo * Shuttle Num
        }
    }
}

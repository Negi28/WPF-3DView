using Presto.PRC.EventHandlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Presto.PRC.Types;
using System.Xml;

namespace Presto.PRC
{
    /// <summary> Represent Presto Robot Controller  </summary>
    public partial class Controller
    {
        public DigitalIO DigitalInput { get; set; }
        public DigitalIO DigitalOutput { get; set; }
        public AnalogIO AnalogInput { get; set; }
        public AnalogIO AnalogOutput { get; set; }

        public Controller()
        {
            DigitalInput = new DigitalIO();
            DigitalOutput = new DigitalIO();
            AnalogInput = new AnalogIO();
            AnalogOutput = new AnalogIO();
        }

        #region InternalHelpingMethod
        private byte[] sendData = null;

        private byte[] StringToBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        #endregion

        #region General        

        public bool IsEcatConnected
        {
            get { return bEcMaster; }
            private set { }
        }

        public void GetSystemStatus()
        {
            Send(Command.CMD_GET_STATUS_SYS, null);
        }

        public int Period
        {
            get { return period; }
            set { period = value; }
        }

        public bool IsVelocityPeriod
        {
            get { return true; }
            private set { }
        }

        public SystemPara SystemPara
        {
            get { return sysPara; }
            set { sysPara = value; }
        }

        public List<Robot> Robots
        {
            get { return lRobot; }
            private set { }
        }



        public int RobotCount()
        {
            return lRobot.Count;
        }


        #endregion

        #region ConnectAndLogin
        /// <summary> Connect to PRC and trigger event
        /// <see cref="EventHandler.OnConnect"/> if successful, else will trigger event
        /// <see cref="EventHandler.OnError(Error)"/>
        /// </summary>
        /// <param name="ip"> is string of ip address</param>
        /// <returns>NONE</returns>             
        public void Connect(string _ip)
        {
            new Thread(() =>
            {
                if (state != State.Disconnected) return;
                if (logLib == null && logErr == null)
                {
                    logLib = new StreamWriter(Environment.CurrentDirectory + "\\lib.log");
                    logErr = new StreamWriter(Environment.CurrentDirectory + "\\error.log");
                }
                ip = _ip;
                // Connect to a remote device.  
                try
                {
                    IPAddress ipAddress = IPAddress.Parse(ip);
                    socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(ipAddress, port);
                    //Connected
                    state = State.Connected;

                    //Receive
                    new Thread(() =>
                    {
                        PRINT("Receive thread START");
                        while (state != State.Disconnected)
                        {
                            try
                            {
                                Receive();
                            }
                            catch (Exception ex)
                            {
                                if (logErr != null) logErr.WriteLine(ex.ToString());
                                Disconnect();
                            }
                        }
                        PRINT("Receive thread END");
                    }).Start();
                    Thread.Sleep(100);
                    recCounter = 0;
                    //Clear all list
                    lRobot.Clear();
                    lXmlRobot.Clear();
                    Send(Command.CMD_CONNECT, null);
                }
                catch (Exception ex)
                {
                    if (logErr != null) logErr.WriteLine(ex.ToString());
                    ErrorEventHandler?.Invoke(Error.CONNECT);
                }
            }).Start();
        }

        /// <summary> Disconnect from PRC </summary>
        /// <returns>NONE</returns>               
        /// <see cref="EventHandler.OnDisconnect"/>
        public void Disconnect()
        {
            try
            {

                state = State.Disconnected;
               // logLib.Close(); logLib = null;
               // logErr.Close(); logErr = null;
                if (socket == null) return;
                if (socket.Connected) socket.Shutdown(SocketShutdown.Both);
                socket.Close(); socket = null;

                Thread.Sleep(100);
                lRobot.Clear();

                lMtStatus.Clear();
                lXmlRobot.Clear();
                lAuto.Clear();
                lProgram.Clear();
                lProgramStatus.Clear();
                lSubProgram.Clear();

                DigitalInput.Reset();
                DigitalOutput.Reset();
                AnalogInput.Reset();
                AnalogOutput.Reset();
            }
            catch (Exception ex)
            {
                if (logErr != null) logErr.WriteLine(ex.ToString());
            }

            DisconnectEventHandler?.Invoke();
        }

        public bool IsConnected
        {
            get { return (state == State.Connected); }
        }
        public bool IsDisconnected
        {
            get { return (state == State.Disconnected); }
        }


        public bool IsLoggedIn
        {
            get { return (state == State.LoggedIn); }
        }
        public string IpAddress
        {
            get { return ip; }
        }

        /// <summary> Login to PRC and trigger event
        /// <see cref="EventHandler.OnLogin"/> if successful, else will trigger event
        /// <see cref="EventHandler.OnError(Error)"/>
        /// </summary>
        /// <returns>NONE</returns>               
        public void Login(LoginLevel level, string pw)
        {
            byte[] bytePw = StringToBytes(pw);
            sendData = new byte[1 + bytePw.Length];
            sendData[0] = (byte)level;
            Buffer.BlockCopy(bytePw, 0, sendData, 1, bytePw.Length);
            Send(Command.CMD_SET_LOGIN, sendData);
        }

        /// <summary>Change Password and trigger event 
        /// <see cref="EventHandler.OnLoginChangePassword"/> if successful, else will trigger event
        /// <see cref="EventHandler.OnError(Error)"/>
        /// </summary>
        /// <returns>NONE</returns>                       
        public void LoginChangePassword(LoginLevel level, string oldPw, string newPw)//OnLoginChangePasswordEvent
        {
            //Send Data layout
            //[Level: 1]["OldPass"'\0']["NewPass"'\0']
            byte len1 = (byte)oldPw.Length;
            byte len2 = (byte)newPw.Length;
            int size = 1 + len1 + 1 + len2 + 1;
            sendData = new byte[size];
            sendData[0] = (byte)level;
            Buffer.BlockCopy(StringToBytes(oldPw), 0, sendData, 1, len1);
            Buffer.BlockCopy(StringToBytes(newPw), 0, sendData, 1 + len1 + 1, len2);
            Send(Command.CMD_SET_LOGIN_PW, sendData);
        }

        #endregion

        #region Robot
        /// <summary>Get current joint & work position</summary>
        /// <param name="robotNo"> index of robot</param>
        /// <returns>NONE</returns>               
        /// <see cref="EventHandler.OnRobotGetCurrentPos(byte, double[], double[])"/>
        public void RobotGetCurrentPos(byte robotNo)
        {
            if (!IsEcatConnected)
            {
                ErrorEventHandler(Error.ECAT_NOT_CONNECTED);
                return;
            }
            byte[] sendData = new byte[1];
            sendData[0] = robotNo;
            Send(Command.CMD_GET_CMD_POS, sendData);
        }

        /// <summary>Get current robot status and trigger event
        /// <see cref="EventHandler.OnRobotGetStatus(byte, RobotStatus)(byte, List{MotorStatus})"/>
        /// </summary>
        /// <param name="robotNo"> index of robot</param>
        /// <returns>NONE</returns>                       
        public void RobotGetStatus(byte robotNo)
        {
            if (!IsEcatConnected)
            {
                ErrorEventHandler(Error.ECAT_NOT_CONNECTED);
                return;
            }
            byte[] sendData = new byte[1];
            sendData[0] = robotNo;
            Send(Command.CMD_GET_STATUS_ROBOT_FOR_PRG, sendData);
        }

        /// <summary>Get status of all motors of robot and trigger event
        /// <see cref="EventHandler.OnRobotGetMotorStatus(byte, List{MotorStatus})"/>
        /// </summary>
        /// <param name="robotNo"> index of robot</param>
        /// <returns>NONE</returns>                       
        public void RobotGetMotorsStatus(byte robotNo)
        {

            byte[] sendData = new byte[1];
            sendData[0] = robotNo;
            Send(Command.CMD_GET_STATUS_RB_MOTOR_ALL, sendData);
        }

        /// <summary>Turn on all motors of robot</summary>
        /// <param name="robotNo"> index of robot</param>
        /// <returns>NONE</returns>               
        /// <see cref="EventHandler.OnError(Error)"/>
        public void RobotOn(byte robotNo)
        {
            byte[] sendData = new byte[2];
            sendData[0] = robotNo;
            sendData[1] = 0xff;
            Send(Command.CMD_SET_SVON, sendData);
        }

        /// <summary>Turn off all motors of robot and trigger event
        /// <see cref="EventHandler.OnError(Error)"/>
        /// </summary>
        /// <param name="robotNo"> index of robot</param>
        /// <returns>NONE</returns>

        public void RobotOff(byte robotNo)
        {
            byte[] sendData = new byte[2];
            sendData[0] = robotNo;
            sendData[1] = 0xff;
            Send(Command.CMD_SET_SVOFF, sendData);
        }

        /// <summary>Clear all motors of robot</summary>
        /// <param name="robotNo"> index of robot</param>
        /// <returns>NONE</returns>               
        /// <see cref="EventHandler.OnError(Error)"/>
        public void RobotClearError(byte robotNo)
        {
            byte[] sendData = new byte[2];
            sendData[0] = robotNo;
            sendData[1] = 0xff;
            Send(Command.CMD_SET_ALRAM_CLEAR, sendData);
        }

        /// <summary>Set jog velocity percent of robot and may trigger event        
        /// <see cref="EventHandler.OnError(Error)"/>
        /// </summary>
        /// <param name="robotNo"> index of robot</param>
        /// <param name="val"> value</param>
        /// <returns>NONE</returns>    

        public void RobotSetJogVelP(byte robotNo, double val)
        {
            if (!IsEcatConnected) return;
            byte[] sendData = new byte[9];
            sendData[0] = robotNo;
            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, sendData, 1, 8);
            Send(Command.CMD_JOG_SET_VELP, sendData);
        }

        /// <summary>Get jog velocity percent of robot and trigger event
        /// <see cref="EventHandler.OnRobotGetJogVelP(byte, double))"/>
        /// </summary>
        /// <param name="robotNo"> index of robot</param>        
        /// <returns>NONE</returns>            
        public void RobotGetJogVelP(byte robotNo)
        {
            if (!IsEcatConnected) return;
            byte[] sendData = { robotNo };
            Send(Command.CMD_JOG_GET_VELP, sendData);
        }

        /// <summary>Get how many configured robots</summary>        
        /// <returns>The number of robots</returns>    
        public byte RobotGetNum()
        {
            if (!IsEcatConnected) return 0;
            return 0;
        }

        /// <summary>Get robot name</summary>
        /// <param name="robotNo"> index of robot</param>        
        /// <returns>Name of robot</returns>   
        public string RobotGetName(byte robotNo)
        {
            return "";
        }

        /// <summary>Get robot type</summary>
        /// <param name="robotNo"> index of robot</param>        
        /// <returns>Type of robot</returns> 
        public UInt16 RobotGetType(byte robotNo)
        {
            return 0;
        }

        /// <summary>Get the number of axes of robot</summary>
        /// <param name="robotNo"> index of robot</param>        
        /// <returns>The number of axes</returns> 
        public byte RobotGetAxisNum(byte robotNo)
        {

            return 0;
        }

        /// <summary>Get Robot Information</summary>
        /// <param name="robotNo"> index of robot</param>        
        /// <returns>Robot Information </returns> 
        public Robot RobotGetInfo(byte robotNo)
        {
            return new Robot();
        }

        /// <summary>Get Axis Information Of Robot</summary>
        /// <param name="robotNo"> index of robot</param>        
        /// <returns>Axis Information </returns> 
        public Axis RobotGetAxisInfo(byte robotNo, byte axisNo)
        {
            return new Axis();
        }

        /// <summary>Stop Robot Motion</summary>
        /// <param name="robotNo"> index of robot</param>        
        /// <returns>None</returns> 
        public void RobotStop(byte robotNo)
        {
            byte[] sendData = { robotNo };
            Send(Command.CMD_MOVE_STOP, sendData);
        }

        #endregion

        #region Jog
        public void JogJointNeg(byte robotNo, byte axisNo) {
            byte[] sendData = { robotNo, axisNo };
            Send(Command.CMD_JOG_MOVE_NEGATIVE, sendData);
        }

        public void JogJointPos(byte robotNo, byte axisNo)
        {
            byte[] sendData = { robotNo, axisNo };
            Send(Command.CMD_JOG_MOVE_POSITIVE, sendData);
        }

        public void JogWorkNeg(byte robotNo, byte axisNo)
        {
            byte[] sendData = { robotNo };
            Command cmd = Command.CMD_NONE;
            switch (axisNo)
            {
                case 0: cmd = Command.CMD_JOG_MOVE_X_NEGATIVE; break;
                case 1: cmd = Command.CMD_JOG_MOVE_Y_NEGATIVE; break;
                case 2: cmd = Command.CMD_JOG_MOVE_Z_NEGATIVE; break;
                case 3: cmd = Command.CMD_JOG_MOVE_RX_NEGATIVE; break;
                case 4: cmd = Command.CMD_JOG_MOVE_RY_NEGATIVE; break;
                case 5: cmd = Command.CMD_JOG_MOVE_RZ_NEGATIVE; break;
            }
            Send(cmd, sendData);
        }
        public void JogWorkPos(byte robotNo, byte axisNo)
        {
            byte[] sendData = { robotNo };
            Command cmd = Command.CMD_NONE;
            switch (axisNo)
            {
                case 0: cmd = Command.CMD_JOG_MOVE_X_NEGATIVE; break;
                case 1: cmd = Command.CMD_JOG_MOVE_Y_NEGATIVE; break;
                case 2: cmd = Command.CMD_JOG_MOVE_Z_NEGATIVE; break;
                case 3: cmd = Command.CMD_JOG_MOVE_RX_NEGATIVE; break;
                case 4: cmd = Command.CMD_JOG_MOVE_RY_NEGATIVE; break;
                case 5: cmd = Command.CMD_JOG_MOVE_RZ_NEGATIVE; break;
            }
            UInt16 _cmd = (UInt16)cmd;
            _cmd += 1; //Pos = Neg + 1	
            Send((Command)_cmd, sendData);
        }

        public void JogJointStep(byte robotNo, byte axisNo, double val)
        {
            byte[] sendData = new byte[10];
            sendData[0] = robotNo;
            sendData[1] = axisNo;
            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, sendData, 2, 8);
            Send(Command.CMD_JOG_STEP, sendData);
        }
        public void JogWorkStep(byte robotNo, byte axisNo, double val)
        {
            byte[] sendData = new byte[10];
            sendData[0] = robotNo;
            sendData[1] = axisNo;
            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, sendData, 2, 8);
            Send(Command.CMD_JOG_STEP_WORLD, sendData);
        }
        public void JogJointTarget(byte robotNo, byte axisNo, double val)
        {
            byte[] sendData = new byte[10];
            sendData[0] = robotNo;
            sendData[1] = axisNo;
            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, sendData, 2, 8);
            Send(Command.CMD_JOG_GOTO, sendData);
        }
        public void JogWorkTarget(byte robotNo, byte axisNo, double val)
        {
            byte[] sendData = new byte[10];
            sendData[0] = robotNo;
            sendData[1] = axisNo;
            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, sendData, 2, 8);
            Send(Command.CMD_JOG_GOTO_WORLD, sendData);
        }
        #endregion

        /// <summary>Execute a single command on Robot Controller 
        /// and trigger event 
        /// <see cref="EventHandler.OnExecSingleCmd(string)"/>
        /// </summary>
        /// <param name="cmd"> string of command</param>        
        /// <returns>None</returns>         
        public void ExecSingleCmd(string cmd)
        {
            Send(Command.CMD_PRG_EXEC_SINGLE_STRING, (StringToBytes(cmd)));
        }


        #region Program
        public void ProgramGetList()
        {
            Send(Command.CMD_PRG_GET_LIST, null);
        }

        public void ProgramGetAllStatus()
        {
            Send(Command.CMD_PRG_GET_ALL_STATUS, null);
        }

        public void ProgramGetAutoRun()
        {
            Send(Command.CMD_PRG_GET_AUTORUN, null);
        }

        public void ProgramGetStatus(byte prgNo)
        {
            byte[] sendData = { prgNo };
            Send(Command.CMD_PRG_GET_STATUS, sendData);
        }

        /// <summary>Open Program
        /// and trigger event 
        /// <see cref="EventHandler.OnProgramOpen(byte, string, string)"/>
        /// </summary>
        /// <param name="prgNo"> Index of program</param>        
        /// <returns>None</returns>         
        public void ProgramOpen(byte prgNo)
        {
            byte[] sendData = { prgNo };
            Send(Command.CMD_PRG_OPEN, sendData);
        }

        public void ProgramSave(byte prgNo, string des, string text)
        {
            int s1 = des.Length;
            int s2 = text.Length;
            int size = 6 + des.Length + text.Length;

            byte[] sendData = new byte[size];
            sendData[0] = prgNo;
            sendData[1] = (byte)s1;
            Buffer.BlockCopy(StringToBytes(des), 0, sendData, 2, s1);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt32)s2), 0, sendData, 2 + s1, 4);
            Buffer.BlockCopy(StringToBytes(text), 0, sendData, 6 + s1, s2);
            Send(Command.CMD_PRG_SAVE, sendData);
        }

        public void ProgramCompile(byte prgNo)
        {
            byte[] sendData = { prgNo };
            Send(Command.CMD_PRG_COMPILE, sendData);
        }

        public void ProgramStart(byte prgNo)
        {
            byte[] sendData = { prgNo };
            Send(Command.CMD_PRG_START, sendData);
        }

        public void ProgramStop(byte prgNo)
        {
            byte[] sendData = { prgNo };
            Send(Command.CMD_PRG_STOP, sendData);
        }

        public void ProgramStopAll(byte prgNo)
        {
            byte[] sendData = { prgNo };
            Send(Command.CMD_PRG_STOP_ALL, sendData);
        }

        public void ProgramPause(byte prgNo)
        {
            byte[] sendData = { prgNo };
            Send(Command.CMD_PRG_PAUSE, sendData);
        }

        public void ProgramResume(byte prgNo)
        {
            byte[] sendData = { prgNo };
            Send(Command.CMD_PRG_RESUME, sendData);
        }

        public void ProgramGetErrMsg(byte prgNo)
        {
            byte[] sendData = { prgNo };
            Send(Command.CMD_PRG_GET_ERR_MSG, sendData);
        }

        #endregion


        #region Sub-Program
        public void SubProgramGetList()
        {
            Send(Command.CMD_SUB_PRG_GET_ALL_STATUS, null);
        }

        public void SubProgramOpen(string name)
        {
            int nameSize = name.Length;
            sendData = new byte[1 + nameSize];
            sendData[0] = (byte)nameSize;
            Buffer.BlockCopy(StringToBytes(name), 0, sendData, 1, nameSize);
            Send(Command.CMD_SUB_PRG_OPEN, sendData);
        }

        public void SubProgramDelete(string name) {
            int nameSize = name.Length;
            sendData = new byte[1 + nameSize];
            sendData[0] = (byte)nameSize;
            Buffer.BlockCopy(StringToBytes(name), 0, sendData, 1, nameSize);
            Send(Command.CMD_SUB_PRG_DELETE, sendData);
        }

        public void SubProgramSave(string name, string text) {
            int nameSize = name.Length;
            UInt32 contenSize = (UInt32)text.Length;
            sendData = new byte[1 + nameSize + 4 + contenSize];
            sendData[0] = (byte)nameSize;
            Buffer.BlockCopy(StringToBytes(name), 0, sendData, 1, nameSize);
            Buffer.BlockCopy(BitConverter.GetBytes(contenSize), 0, sendData, 1 + nameSize, 4);
            Buffer.BlockCopy(StringToBytes(text), 0, sendData, 1 + nameSize + 4, (int)contenSize);
            Send(Command.CMD_SUB_PRG_SAVE, sendData);
        }

        public void SubProgramCompile(string name) {
            int nameSize = name.Length;
            sendData = new byte[1 + nameSize];
            sendData[0] = (byte)nameSize;
            Buffer.BlockCopy(StringToBytes(name), 0, sendData, 1, nameSize);
            Send(Command.CMD_SUB_PRG_COMPILE, sendData);
        }

        public void SubProgramGetErrMsg(string name) {
            int nameSize = name.Length;
            sendData = new byte[1 + nameSize];
            sendData[0] = (byte)nameSize;
            Buffer.BlockCopy(StringToBytes(name), 0, sendData, 1, nameSize);
            Send(Command.CMD_SUB_PRG_GET_ERR_MSG, sendData);
        }

        #endregion

        #region IO             

        /// <summary>Get value of all port Digital Input and trigger event
        /// <see cref="EventHandler.OnDInGetAllPortValue(List{uint}))"/>
        /// </summary>
        /// <returns>NONE</returns>            
        public void DInGetAllPortValue() {
            Send(Command.CMD_GET_DI_ALL, null);
        }

        /// <summary>Get value of all port Digital Output and trigger event
        /// <see cref="EventHandler.OnDOutGetAllPortValue(List{uint}))"/>
        /// </summary>
        /// <returns>NONE</returns>            
        public void DOutGetAllPortValue() {
            Send(Command.CMD_GET_DOUT_ALL, null);
        }

        /// <summary>Get value of all port Analog Input and trigger event
        /// <see cref="EventHandler.OnAInGetAllChannelValue(List{int})"/>
        /// </summary>
        /// <returns>NONE</returns> 
        public void AInGetAllChannelValue() {
            Send(Command.CMD_GET_AI_ALL, null);
        }

        /// <summary>Get value of all port Analog Output and trigger event
        /// <see cref="EventHandler.OnAOutGetAllChannelValue(List{int})"/>
        /// </summary>
        /// <returns>NONE</returns> 
        public void AOutGetAllChannelValue() {
            Send(Command.CMD_GET_AO_ALL, null);
        }

        /// <summary>Set Digital Ouput Value and may trigger event
        /// <see cref="EventHandler.OnError(Error)"/>
        /// </summary>
        /// <param name="port">Port index</param>
        /// <param name="bitIndex">Bit index on this port</param>
        /// <param name="bitValue">Value of bit (0 or 1)</param>
        /// <returns>NONE</returns> 
        public void DOutSet(UInt16 port, byte bitIndex, byte bitValue) {
            byte[] sendData = new byte[4];
            Buffer.BlockCopy(BitConverter.GetBytes(port), 0, sendData, 0, 2);
            sendData[2] = bitIndex;
            sendData[3] = bitValue;
            Send(Command.CMD_SET_DOUT_PORT_BIT, sendData);
        }

        /// <summary>Set Analog Ouput Value and may trigger event
        /// <see cref="EventHandler.OnError(Error)"/>
        /// </summary>        
        /// <param name="channel">Channel index</param>
        /// <param name="val">Value</param>        
        /// <returns>NONE</returns>         
        public void AOutSet(UInt16 channel, Int32 val) {
            byte[] sendData = new byte[6];
            Buffer.BlockCopy(BitConverter.GetBytes(channel), 0, sendData, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, sendData, 2, 4);
            Send(Command.CMD_SET_AO, sendData);
        }

        #endregion

        #region Variables     
        public void IVarGetAll(byte robotNo)
        {
            Send(Command.CMD_GET_I_VAR_ALL, new byte[] { robotNo });
        }
       
        public void IVarAdd(byte robotNo, int index)
        {
            string text = "Int " + index;
            int len = text.Length;
            byte[] sendData = new byte[2+len+4];            
            sendData[0] = robotNo;
            sendData[1] = (byte)len; //len        
            Buffer.BlockCopy(StringToBytes(text), 0, sendData, 2, len);
            Buffer.BlockCopy(BitConverter.GetBytes((Int32)0), 0, sendData, 2+len, 4);
            Send(Command.CMD_ADD_I_VAR, sendData);
        }

        public void IVarDelete(byte robotNo)
        {
            Send(Command.CMD_DELETE_I_VAR, new byte[] { robotNo });
        }

        public void IVarSet(byte robotNo, UInt16 index, string text, Int32 val)
        {
            int memIndex = 0;
            int len = text.Length;
            int size = 1 + 2 + 1 + len + 4;
            byte[] sendData = new byte[size];

            sendData[memIndex] = robotNo; memIndex += 1;
            Buffer.BlockCopy(BitConverter.GetBytes(index), 0, sendData, memIndex, 2); memIndex += 2;
            sendData[memIndex] = (byte) len; memIndex += 1;
            Buffer.BlockCopy(StringToBytes(text), 0, sendData, memIndex, len); memIndex += len;
            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, sendData, memIndex, 4); memIndex += 4;
            Send(Command.CMD_SET_I_VAR, sendData);
        }

        public void DVarGetAll(byte robotNo)
        {
            Send(Command.CMD_GET_D_VAR_ALL, new byte[] { robotNo });
        }
        
        public void GetCarrierInfo()
        {
            Send(Command.CMD_GET_CARR_INFO_ALL, null);
        }

        public void GetShutterInfo()
        {
            Send(Command.CMD_GET_LM_TRACK_POS_ALL, null);
        }


        public void DVarAdd(byte robotNo, int index)
        {
            string text = "Double " + index;
            int len = text.Length;
            byte[] sendData = new byte[2 + len + 8];
            sendData[0] = robotNo;
            sendData[1] = (byte)len; //len        
            Buffer.BlockCopy(StringToBytes(text), 0, sendData, 2, len);
            Buffer.BlockCopy(BitConverter.GetBytes((double)0), 0, sendData, 2 + len, 8);
            Send(Command.CMD_ADD_D_VAR, sendData);
        }

        public void DVarDelete(byte robotNo)
        {
            Send(Command.CMD_DELETE_D_VAR, new byte[] { robotNo });
        }

        public void DVarSet(byte robotNo, UInt16 index, string text, double val)
        {
            int memIndex = 0;
            int len = text.Length;
            int size = 1 + 2 + 1 + len + 8;
            byte[] sendData = new byte[size];

            sendData[memIndex] = robotNo; memIndex += 1;
            Buffer.BlockCopy(BitConverter.GetBytes(index), 0, sendData, memIndex, 2); memIndex += 2;
            sendData[memIndex] = (byte)len; memIndex += 1;
            Buffer.BlockCopy(StringToBytes(text), 0, sendData, memIndex, len); memIndex += len;
            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, sendData, memIndex, 8); memIndex += 8;
            Send(Command.CMD_SET_D_VAR, sendData);
        }

        public void PosVarGetAll(byte robotNo)
        {
            Send(Command.CMD_GET_POS_VAR_ALL, new byte[] { robotNo });
        }

        public void PosVarAdd(byte robotNo, int index=0)
        {
            if (robotNo >= lRobot.Count) return;
            int axisNum = lRobot[robotNo].Axes.Count;
            double[] wPos = new double[axisNum];
            double[] jPos = new double[axisNum];
            string des = "Pos " + index;
            int desLen = des.Length;
            int size = 1 + 1 + desLen + 1 + 1 + axisNum * 2 * 8;
            byte[] sendData = new byte[size];
            int memIndex = 0;
            sendData[memIndex] = robotNo;               memIndex += 1;
            sendData[memIndex] = (byte)axisNum;         memIndex += 1;
            sendData[memIndex] = (byte)desLen; /*wDesc Length*/    memIndex += 1;
            Buffer.BlockCopy(StringToBytes(des), 0, sendData, memIndex, desLen); memIndex += desLen;
            Buffer.BlockCopy(wPos, 0, sendData, memIndex, axisNum * 8); memIndex += axisNum * 8;
            sendData[memIndex] = 0; /*jDesc Length*/    memIndex += 1;
            Buffer.BlockCopy(jPos, 0, sendData, memIndex, axisNum * 8); memIndex += axisNum * 8;
            Send(Command.CMD_ADD_POS_VAR, sendData);
        }

        public void PosVarDelete(byte robotNo)
        {
            Send(Command.CMD_DELETE_POS_VAR, new byte[] { robotNo });
        }

        public void PosVarSet(byte type, byte robotNo, UInt16 index, string desc, double[] pos)
        {
            byte len = (byte)desc.Length;
            int axisNum = pos.Length;
            int size = 1 + 2 + 1 + len + axisNum * 8;
            byte[] sendData = new byte[size];
            int memIndex = 0;            
            sendData[memIndex] = robotNo; memIndex += 1;
            Buffer.BlockCopy(BitConverter.GetBytes(index), 0, sendData, memIndex, 2); memIndex += 2;
            sendData[memIndex] = len; memIndex += 1;
            Buffer.BlockCopy(StringToBytes(desc), 0, sendData, memIndex, len); memIndex += len;
            Buffer.BlockCopy(pos, 0, sendData, memIndex, axisNum * 8); memIndex += axisNum * 8;
            if (type % 2 == 1) Send(Command.CMD_SET_WPOS_VAR, sendData);
            else Send(Command.CMD_SET_JPOS_VAR,  sendData);
        }

        #endregion

        #region Scope
        #endregion

        #region Logging     
        public void LogGetLineNum(LoggingType type)
        {
            Send(Command.CMD_GET_LOG_LINE_NUM, new byte[] { (byte)type });
        }

        public void LogGetStr(LoggingType type, UInt32 startLine, UInt32 endLine)
        {
            byte[] sendData = new byte[9];
            sendData[0] = (byte)type;
            Buffer.BlockCopy(BitConverter.GetBytes(startLine), 0, sendData, 1, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(endLine), 0, sendData, 5, 4);
            Send(Command.CMD_GET_LOG_STR, sendData);
        }

        public void LogClear(LoggingType type)
        {
            Send(Command.CMD_CLEAR_LOG, new byte[] { (byte)type });
        }
        #endregion

        #region Homing
        #endregion

        #region SystemConfig
        //public void SystemConfigGetSystemParaFile()
        //{
            //Send(Command.CMD_GET_SYS_FILE, null);
        //}

        //public void SystemConfigSetSystemParaFile(UInt16 len, string text)
        //{
        //    byte[] sendData = StringToBytes(text);
        //    Send(Command.CMD_SET_SYS_FILE, sendData);
        //}

        private XmlNode CreateNode(XmlDocument doc, string key, string value)
        {
            XmlNode node = doc.CreateElement(key);
            node.InnerText = value;                
            return node;
        }

        private string XmlDocToString(XmlDocument doc)
        {
            StringWriter strWriter = new StringWriter();
            doc.Save(strWriter);
            string text = strWriter.ToString();
            return text;
        }       

        public void SystemConfigSetSystemPara(SystemPara sys)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);
            XmlElement sysNode = doc.CreateElement("SysPara");
            doc.AppendChild(sysNode);
            sysNode.AppendChild(CreateNode(doc, "RbNumber", sys.RbNumber.ToString()));
            sysNode.AppendChild(CreateNode(doc, "ENIFileName", sys.ENIFileName.ToString()));
            sysNode.AppendChild(CreateNode(doc, "CycleTime", sys.CycleTime.ToString()));
            sysNode.AppendChild(CreateNode(doc, "TotalAxisNumber", sys.TotalAxisNumber.ToString()));
            sysNode.AppendChild(CreateNode(doc, "UseTeachPendant", sys.UseTeachPendant.ToString()));
            sysNode.AppendChild(CreateNode(doc, "IPAddress", sys.IPAddress.ToString()));
            SystemConfigSaveSysParaFile(XmlDocToString(doc));
        }

        //Save by text
        public void SystemConfigSaveSysParaFile(string text)
        {
            byte[] sendData = StringToBytes(text);
            Send(Command.CMD_SET_SYS_FILE, sendData);
        }

        //Save by internal memory
        public void SystemConfigSaveSysParaFile()
        {            
            Send(Command.CMD_SAVE_TO_SYS_FILE, null);
        }


        public void SystemConfigSaveRobotPara(byte robotNo, Robot robot)
        {
            string text = "";

            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);
            XmlElement robotNode = doc.CreateElement("RbPara");
            doc.AppendChild(robotNode);
            robotNode.AppendChild(CreateNode(doc, "RbType", robot.RbType.ToString()));
            robotNode.AppendChild(CreateNode(doc, "Name", robot.Name.ToString()));
            robotNode.AppendChild(CreateNode(doc, "ToolType", robot.ToolType.ToString()));
            robotNode.AppendChild(CreateNode(doc, "AxisNumber", robot.AxisNumber.ToString()));
            robotNode.AppendChild(CreateNode(doc, "Vendor", robot.Vendor.ToString()));
            robotNode.AppendChild(CreateNode(doc, "Model", robot.Model.ToString()));
            robotNode.AppendChild(CreateNode(doc, "CoordType", robot.CoordType.ToString()));
            robotNode.AppendChild(CreateNode(doc, "DefVelP", robot.DefVelP.ToString()));
            robotNode.AppendChild(CreateNode(doc, "DefJogVelP", robot.DefJogVelP.ToString()));
            
            XmlElement workAreaNode = doc.CreateElement("WorkArea");
            robotNode.AppendChild(workAreaNode);
            workAreaNode.AppendChild(CreateNode(doc, "X_N", robot.WorkArea.Xn.ToString()));
            workAreaNode.AppendChild(CreateNode(doc, "Y_N", robot.WorkArea.Yn.ToString()));
            workAreaNode.AppendChild(CreateNode(doc, "Z_N", robot.WorkArea.Zn.ToString()));
            workAreaNode.AppendChild(CreateNode(doc, "X_P", robot.WorkArea.Xp.ToString()));
            workAreaNode.AppendChild(CreateNode(doc, "Y_P", robot.WorkArea.Yp.ToString()));
            workAreaNode.AppendChild(CreateNode(doc, "Z_P", robot.WorkArea.Zp.ToString()));           
            
            foreach(Axis axis in robot.Axes)
            {
                XmlElement axisNode = doc.CreateElement("AxisPara");
                robotNode.AppendChild(axisNode);
                axisNode.AppendChild(CreateNode(doc, "No", axis.No.ToString()));
                axisNode.AppendChild(CreateNode(doc, "DriveMappSlaNum", axis.DriveMapSlaNum.ToString()));
                axisNode.AppendChild(CreateNode(doc, "CmdPulsePerCycle", axis.CmdPulsePerCycle.ToString()));
                axisNode.AppendChild(CreateNode(doc, "DistPerCycle", axis.DistPerCycle.ToString()));
                axisNode.AppendChild(CreateNode(doc, "GearRate", axis.GearRate.ToString()));
                axisNode.AppendChild(CreateNode(doc, "MaxVelocity", axis.MaxVelocity.ToString()));
                axisNode.AppendChild(CreateNode(doc, "MaxTorque", axis.MaxTorque.ToString()));

                XmlElement swLitmitNode = doc.CreateElement("SWLimit");
                axisNode.AppendChild(swLitmitNode);
                swLitmitNode.AppendChild(CreateNode(doc, "Neg", axis.SWLimit.Neg.ToString()));
                swLitmitNode.AppendChild(CreateNode(doc, "Pos", axis.SWLimit.Pos.ToString()));

                axisNode.AppendChild(CreateNode(doc, "MotDirection", axis.MotDirection.ToString()));
                axisNode.AppendChild(CreateNode(doc, "InposPulse", axis.InposPulse.ToString()));
                axisNode.AppendChild(CreateNode(doc, "SettlingTime", axis.SettlingTime.ToString()));
                axisNode.AppendChild(CreateNode(doc, "BrakeReleaseTime", axis.BrakeReleaseTime.ToString()));
                axisNode.AppendChild(CreateNode(doc, "MaxPosErr", axis.MaxPosErr.ToString()));

                XmlElement homeParaNode = doc.CreateElement("HomePara");
                axisNode.AppendChild(homeParaNode);
                homeParaNode.AppendChild(CreateNode(doc, "Method", axis.HomePara.Method.ToString()));
                homeParaNode.AppendChild(CreateNode(doc, "Acceleraion", axis.HomePara.Acceleraion.ToString()));
                homeParaNode.AppendChild(CreateNode(doc, "Velocity", axis.HomePara.Velocity.ToString()));
                homeParaNode.AppendChild(CreateNode(doc, "ReturnVel", axis.HomePara.ReturnVel.ToString()));
                homeParaNode.AppendChild(CreateNode(doc, "Offset", axis.HomePara.Offset.ToString()));

                XmlElement readyNode = doc.CreateElement("ReadyMovePara");
                axisNode.AppendChild(readyNode);
                readyNode.AppendChild(CreateNode(doc, "Vel", axis.ReadyMovePara.Vel.ToString()));
                readyNode.AppendChild(CreateNode(doc, "AccTime", axis.ReadyMovePara.AccTime.ToString()));
                readyNode.AppendChild(CreateNode(doc, "DecTime", axis.ReadyMovePara.DecTime.ToString()));
                readyNode.AppendChild(CreateNode(doc, "AJerkP", axis.ReadyMovePara.AJerkP.ToString()));
                readyNode.AppendChild(CreateNode(doc, "DJerkP", axis.ReadyMovePara.DJerkP.ToString()));
                readyNode.AppendChild(CreateNode(doc, "KillTime", axis.ReadyMovePara.KillTime.ToString()));
                readyNode.AppendChild(CreateNode(doc, "Position", axis.ReadyMovePara.Position.ToString()));

                XmlElement jogNode = doc.CreateElement("JogMovePara");
                axisNode.AppendChild(jogNode);
                jogNode.AppendChild(CreateNode(doc, "JVel", axis.JogMovePara.JVel.ToString()));
                jogNode.AppendChild(CreateNode(doc, "JAccTime", axis.JogMovePara.JAccTime.ToString()));
                jogNode.AppendChild(CreateNode(doc, "JDecTime", axis.JogMovePara.JDecTime.ToString()));
                jogNode.AppendChild(CreateNode(doc, "JAJerkP", axis.JogMovePara.JAJerkP.ToString()));
                jogNode.AppendChild(CreateNode(doc, "JDJerkP", axis.JogMovePara.JDJerkP.ToString()));
                jogNode.AppendChild(CreateNode(doc, "JKillTime", axis.JogMovePara.JKillTime.ToString()));
                jogNode.AppendChild(CreateNode(doc, "WVel", axis.JogMovePara.WVel.ToString()));
                jogNode.AppendChild(CreateNode(doc, "WAccTime", axis.JogMovePara.WAccTime.ToString()));
                jogNode.AppendChild(CreateNode(doc, "WDecTime", axis.JogMovePara.WDecTime.ToString()));
                jogNode.AppendChild(CreateNode(doc, "WAJerkP", axis.JogMovePara.WAJerkP.ToString()));
                jogNode.AppendChild(CreateNode(doc, "WDJerkP", axis.JogMovePara.WDJerkP.ToString()));
                jogNode.AppendChild(CreateNode(doc, "WKillTime", axis.JogMovePara.WKillTime.ToString()));

                XmlElement prgNode = doc.CreateElement("MovePara");
                axisNode.AppendChild(prgNode);
                prgNode.AppendChild(CreateNode(doc, "JVel", axis.JogMovePara.JVel.ToString()));
                prgNode.AppendChild(CreateNode(doc, "JAccTime", axis.JogMovePara.JAccTime.ToString()));
                prgNode.AppendChild(CreateNode(doc, "JDecTime", axis.JogMovePara.JDecTime.ToString()));
                prgNode.AppendChild(CreateNode(doc, "JAJerkP", axis.JogMovePara.JAJerkP.ToString()));
                prgNode.AppendChild(CreateNode(doc, "JDJerkP", axis.JogMovePara.JDJerkP.ToString()));
                prgNode.AppendChild(CreateNode(doc, "JKillTime", axis.JogMovePara.JKillTime.ToString()));
                prgNode.AppendChild(CreateNode(doc, "WVel", axis.JogMovePara.WVel.ToString()));
                prgNode.AppendChild(CreateNode(doc, "WAccTime", axis.JogMovePara.WAccTime.ToString()));
                prgNode.AppendChild(CreateNode(doc, "WDecTime", axis.JogMovePara.WDecTime.ToString()));
                prgNode.AppendChild(CreateNode(doc, "WAJerkP", axis.JogMovePara.WAJerkP.ToString()));
                prgNode.AppendChild(CreateNode(doc, "WDJerkP", axis.JogMovePara.WDJerkP.ToString()));
                prgNode.AppendChild(CreateNode(doc, "WKillTime", axis.JogMovePara.WKillTime.ToString()));
            }
            text = XmlDocToString(doc);
            SystemConfigSaveRobotParaFile(robotNo, text);
        }


        private void SystemConfigSaveFile(Command cmd, byte robotNo, string text) 
        {
            byte[] sendData = new byte[text.Length + 1];
            sendData[0] = robotNo;
            Buffer.BlockCopy(StringToBytes(text), 0, sendData, 1, text.Length);
            Send(cmd, sendData);
        }

        //Text
        private void SystemConfigSaveRobotParaFile(byte robotNo, string text)
        {            
            SystemConfigSaveFile(Command.CMD_SET_RB_PARA_FILE, robotNo, text);
        }

        //Internel memory
        public void SystemConfigSaveRobotParaFile(byte robotNo)
        {
            Send(Command.CMD_SAVE_TO_RPARA_FILE, new byte[] { robotNo});
        }

        //Text
        public void SystemConfigSavePosFile(byte robotNo, string text) 
        {
            SystemConfigSaveFile(Command.CMD_SET_POS_VAR_FILE, robotNo, text);
        }

        //Internel memory
        public void SystemConfigSavePosFile(byte robotNo)
        {
            Send(Command.CMD_SAVE_TO_RPOS_FILE, new byte[] { robotNo });
        }

        //Text
        public void SystemConfigSaveVarFile(byte robotNo,  string text) 
        {
            SystemConfigSaveFile(Command.CMD_SET_VAR_FILE, robotNo, text);
        }

        //Internel memory
        public void SystemConfigSaveVarFile(byte robotNo)
        {
            Send(Command.CMD_SAVE_TO_RVAR_FILE, new byte[] { robotNo });
        }

        public void SystemConfigGetRobotTemplate() {
            Send(Command.CMD_SEARCH_RB_PARA_FOLDER, null);
        }

        public void SystemConfigGetRobotTemplateFile(string name) {
            byte[] sendData = new byte[1 +name.Length];
            sendData[0] = (byte)name.Length;
            Buffer.BlockCopy(StringToBytes(name), 0, sendData, 1, name.Length);
            Send(Command.CMD_GET_RB_PARA_XML_FILE, sendData);
        }

        public void SystemConfigGetEniFiles() {
            Send(Command.CMD_SEARCH_ENI_FILE, null);
        }
        #endregion

        //Motion Parameters
        public void MotionParaGetJoint(MotionType type, byte robotNo, byte axisNo)
        {
            byte[] sendData = new byte[] { robotNo , axisNo};
            Command cmd = Command.CMD_NONE;
            switch (type)
            {
                case MotionType.Manual:
                    cmd = Command.CMD_JOG_GET_MPARA_BASE_TIME;
                    break;
                case MotionType.Program:
                    cmd = Command.CMD_GET_MPARA_BASE_TIME;
                    break;
                case MotionType.Ready:
                    cmd = Command.CMD_GET_READY_MPARA_BASE_TIME;
                    break;
            }
            Send(cmd, sendData);
        }

        public void MotionParaGetWork(MotionType type, byte robotNo, byte axisNo)
        {
            if (type != MotionType.Ready)
            {
                axisNo += 64;
                MotionParaGetJoint(type, robotNo, axisNo);
            }
        }

        public void MotionParaSetJoint(MotionType type, byte robotNo, byte axisNo, double[] para)
        {

            Command cmd = Command.CMD_NONE;
            int size = 50; // 50 = 2+6*8						
            switch (type)
            {
                case MotionType.Manual:
                    cmd = Command.CMD_JOG_SET_MPARA_BASE_TIME;
                    break;
                case MotionType.Program:
                    cmd = Command.CMD_SET_MPARA_BASE_TIME;
                    break;
                case MotionType.Ready:
                    cmd = Command.CMD_SET_READY_MPARA_BASE_TIME;
                    size = 58; //1 more position
                    break;
            }
            byte[] sendData = new byte[size];
            sendData[0] = robotNo;
            sendData[1] = axisNo;
            Buffer.BlockCopy(para, 0, sendData, 2, size-2);
            Send(cmd, sendData);
        }

        public void MotionParaSetWork(MotionType type, byte robotNo, byte axisNo, double[] para)
        {
            axisNo += 64;
            MotionParaSetJoint(type, robotNo, axisNo, para);
        }
    }

}

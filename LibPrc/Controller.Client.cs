//#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

using Presto.PRC.EventHandlers;
using Presto.PRC.Types;

namespace Presto.PRC
{
    partial class Controller
    {

        private void PRINT(string text)
        {
            //Debug.WriteLine(text);
        }

        private void PrintByteArray(byte[] byteData)
        {
            PRINT(BitConverter.ToString(byteData));
        }

        private void PrintByteArray(byte[] byteData, int size)
        {
            if (byteData == null) return;
            byte[] data = new byte[size];
            if (size <= byteData.Length) Buffer.BlockCopy(byteData, 0, data, 0, size);
            else return;
            PRINT(BitConverter.ToString(data));
        }


        private readonly object sendLock = new object();
        private UInt64 sendCounter = 0;
        private void Send(Command cmd, byte[] byteData)
        {
            lock (sendLock)
            {
                int size = 0;
                if (byteData != null) size = byteData.Length;
                int frameSize = 6 + size;
                byte[] frame = new byte[frameSize];
                byte[] byteCmd = System.BitConverter.GetBytes((UInt16)cmd);
                byte[] byteSize = System.BitConverter.GetBytes((UInt32)size);
                Buffer.BlockCopy(byteCmd, 0, frame, 0, 2);
                Buffer.BlockCopy(byteSize, 0, frame, 2, 4);
                if (byteData != null) Buffer.BlockCopy(byteData, 0, frame, 6, size);
                try
                {
#if DEBUG
                    sendCounter++;
                    string logMsg = "[" + sendCounter + "] CMD: " + cmd.ToString() + ", Size = " + size.ToString();
                    LogLibWrite(logMsg);
                    PRINT(logMsg);
#endif
                    socket.Send(frame, frameSize, 0);
                }
                catch (Exception ex)
                {
                    LogErrWrite(ex.ToString());
                    ErrorEventHandler?.Invoke(Error.CONNECT);

                }
            }
        }

        private void Receive()
        {
            try
            {
                int retHeader = socket.Receive(rspHeader, 0, RSP_HEADER_SIZE, 0);

                while (retHeader < RSP_HEADER_SIZE)
                {
                    retHeader += socket.Receive(rspHeader, retHeader, (int)(RSP_HEADER_SIZE - retHeader), 0); //Recceive rest of data
                }

                if (retHeader == RSP_HEADER_SIZE)
                {
                    cmdAck = BitConverter.ToUInt16(rspHeader, 0);
                    errCode = BitConverter.ToUInt32(rspHeader, 2);
                    size = BitConverter.ToUInt32(rspHeader, 6);

                    if (size == 0)
                    {
                        CommandProcess();
                    }
                    else if (size > 0)
                    {
                        data = new byte[size];
                        int retData = socket.Receive(data, 0, (int)size, 0);
                        while (retData < size)
                        {
                            //PRINT("\tRec Size: " + retData);
                            retData += socket.Receive(data, retData, (int)(size - retData), 0); //Recceive rest of data
                        }
                        if (retData == size) CommandProcess();
                    }
                }
                else if (retHeader == 0)
                {
                    ErrorEventHandler?.Invoke(Error.CONNECT);
                }
                else PRINT("Rec Header: " + retHeader);
            }
            catch (Exception ex)
            {
                LogErrWrite(ex.ToString());
                if (ex is ArgumentOutOfRangeException || ex is IndexOutOfRangeException)
                {
                    ErrorEventHandler?.Invoke(Error.OUT_OF_RANGE);
                }
                else if (ex is NullReferenceException)
                {
                    ErrorEventHandler?.Invoke(Error.NULL_REFERENCE);
                }
                else if (ex is SocketException)
                {
                    ErrorEventHandler?.Invoke(Error.CONNECT);
                }
            }
        }

        private void PeriodThreadFunc()
        {
            PRINT("Period thread START");
            while (state == State.LoggedIn)
            {
                GetSystemStatus();
                if (lProgram.Count == MAX_NUM_PROGRAM) ProgramGetAutoRun();
                if (lProgram.Count == MAX_NUM_PROGRAM) ProgramGetAllStatus();
                ProgramGetStatus(10); //Back and Forth

                if (IsEcatConnected)
                {
                    //Robot
                    for (byte robotNo = 0; robotNo < sysPara.RbNumber; robotNo++)
                    {
                        RobotGetCurrentPos(robotNo);
                        RobotGetStatus(robotNo);
                        RobotGetMotorsStatus(robotNo);
                        RobotGetJogVelP(robotNo);
                    }

                    //IO
                    DInGetAllPortValue();
                    DOutGetAllPortValue();
                    AInGetAllChannelValue();
                    AOutGetAllChannelValue();
                }

                //Logging
                LogGetLineNum(LoggingType.ERR);
                LogGetLineNum(LoggingType.SYS);
                LogGetLineNum(LoggingType.MAIN);
                LogGetLineNum(LoggingType.USER);

                Thread.Sleep(period);
            }
            PRINT("Period thread END");
        }

        private object logLibLock = new object();
        void LogLibWrite(string msg)
        {
            lock (logLibLock)
            {
                //if (logLib != null) logLib.WriteLine(msg);
            }
        }

        private object logErrLock = new object();
        void LogErrWrite(string msg)
        {
            lock (logLibLock)
            {
                if (logErr != null) logErr.WriteLine(msg);
            }
        }

        //CMD-SIZE-DATA
        private UInt64 recCounter = 0;
        private void CommandProcess()
        {
            cmdAck -= 0x8000;
            Command cmd = (Command)cmdAck;
#if DEBUG
            recCounter++;
            string logMsg = "-->[" + recCounter + "]  CMD: " + cmd.ToString() + ", ErrCode = " + errCode.ToString("X") + ", Size = " + size.ToString();
            LogLibWrite(logMsg);
            PRINT(logMsg);
#endif
            try
            {
                switch (cmd)
                {
                    case Command.CMD_CONNECT:
                        {
                            ConnectEventHandler?.Invoke();
                        }
                        break;
                    case Command.CMD_SET_LOGIN:
                        {
                            if (errCode != 0)
                            {
                                LoginEventHandler?.Invoke(Error.LOGIN);
                            }
                            else
                            {
                                state = State.LoggedIn;

                                Send(Command.CMD_GET_SYS_FILE, null); //Get number of configured robot user
                                Send(Command.CMD_GET_DI_NUM, null); //Get number of Digital Input
                                Send(Command.CMD_GET_DOUT_NUM, null); //Get number of Digital Output     
                                Send(Command.CMD_GET_AI_NUM, null); //Get number of Analog Output     
                                Send(Command.CMD_GET_AOUT_NUM, null); //Get number of Analog Output     
                                Send(Command.CMD_PRG_GET_LIST, null);
                                Send(Command.CMD_SUB_PRG_GET_ALL_STATUS, null);
                                LoginEventHandler?.Invoke(Error.NONE);
                            }
                        }
                        break;

                    case Command.CMD_SET_LOGIN_PW:
                        {
                            if (errCode != 0) LoginChangePasswordEventHandler?.Invoke(Error.LOGIN_CHANGE_PW);
                            else LoginChangePasswordEventHandler?.Invoke(Error.NONE);
                        }
                        break;


                    case Command.CMD_GET_STATUS_SYS:
                        {
                            _SYS_STATUS sys = ByteArrayToStructure<_SYS_STATUS>(data);
                            SystemStatus s = new SystemStatus();
                            s.RbNum = sys.RbNum;
                            if (s.RbNum != sysPara.RbNumber)
                            {
                                ErrorEventHandler?.Invoke(Error.CONFIG_ROBOT_NUM);
                            }
                            s.IsSystemError = ((byte)(sys.Status >> (byte)_SYS_STATUS_BIT.SYS_ERR) & 0x01) > 0 ? true : false;
                            s.IsEcatErr = ((byte)(sys.Status >> (byte)_SYS_STATUS_BIT.SYS_EC_ERR) & 0x01) > 0 ? true : false;
                            s.IsRobotError = ((byte)(sys.Status >> (byte)_SYS_STATUS_BIT.SYS_RB_ERR) & 0x01) > 0 ? true : false;
                            s.IsRobotOn = ((byte)(sys.Status >> (byte)_SYS_STATUS_BIT.SYS_SV_ON) & 0x01) > 0 ? true : false;
                            s.IsTpEms = ((byte)(sys.Status >> (byte)_SYS_STATUS_BIT.SYS_TP_EMS_SW_FLAG) & 0x01) > 0 ? true : false;
                            s.IsTpDeadSw = ((byte)(sys.Status >> (byte)_SYS_STATUS_BIT.SYS_TP_DEAD_SW_FLAG) & 0x01) > 0 ? true : false;
                            s.IsUseTp = ((byte)(sys.Status >> (byte)_SYS_STATUS_BIT.SYS_TP_CONNECT_FLAG) & 0x01) > 0 ? true : false;

                            //Check EtherCat error
                            if (sys.ErrCode >= 0xE0000000)
                            {
                                bEcMaster = false;
                            }
                            else bEcMaster = true;

                            s.IsEcatErr = !bEcMaster;

                            GetSystemStatusEventHandler?.Invoke(s);
                        }

                        break;



                    case Command.CMD_GET_DI_NUM:
                        {
                            DigitalInput.PortNumber = BitConverter.ToUInt16(data, 0);
                            //PRINT("\t CMD_GET_DI_NUM = " + DigitalInput.Number);
                            DigitalInput.PortSizes.Clear();
                            if (DigitalInput.PortNumber > 0)
                            {
                                for (UInt16 i = 0; i < DigitalInput.PortNumber; i++)
                                {
                                    Send(Command.CMD_GET_DI_PORT_SIZE, BitConverter.GetBytes(i));
                                }
                            }
                            else
                            {
                                LoadedDigitalInputInfoEventHandler?.Invoke(DigitalInput.PortSizes);
                            }
                        }
                        break;

                    //Digital Input
                    case Command.CMD_GET_DI_PORT_SIZE:
                        {
                            UInt16 portSize = BitConverter.ToUInt16(data, 0);
                            //PRINT("\t CMD_GET_DI_PORT_SIZE = " + portSize*8);
                            DigitalInput.PortSizes.Add((byte)(portSize * 8));
                            if (DigitalInput.PortSizes.Count == DigitalInput.PortNumber)
                            {
                                LoadedDigitalInputInfoEventHandler?.Invoke(DigitalInput.PortSizes);
                            }
                        }
                        break;

                    case Command.CMD_GET_DOUT_NUM:
                        {
                            DigitalOutput.PortNumber = BitConverter.ToUInt16(data, 0);
                            //PRINT("\t CMD_GET_DOUT_NUM = " + DigitalOutput.Quantity);
                            DigitalOutput.PortSizes.Clear();
                            if (DigitalOutput.PortNumber > 0)
                            {
                                for (UInt16 i = 0; i < DigitalOutput.PortNumber; i++)
                                {
                                    Send(Command.CMD_GET_DOUT_PORT_SIZE, BitConverter.GetBytes(i));
                                }
                            }
                            else
                            {
                                LoadedDigitalOutputInfoEventHandler?.Invoke(DigitalOutput.PortSizes);
                                new Thread(() =>
                                {
                                    Thread.Sleep(1000);
                                    PeriodThreadFunc();
                                }).Start();
                            }
                        }
                        break;
                    //Digital Output
                    case Command.CMD_GET_DOUT_PORT_SIZE:
                        {
                            UInt16 portSize = BitConverter.ToUInt16(data, 0);
                            //PRINT("\t CMD_GET_DOUT_PORT_SIZE = " + portSize * 8);
                            DigitalOutput.PortSizes.Add((byte)(portSize * 8));
                            if (DigitalOutput.PortSizes.Count == DigitalOutput.PortNumber)
                            {
                                LoadedDigitalOutputInfoEventHandler?.Invoke(DigitalOutput.PortSizes);
                                new Thread(() =>
                                {
                                    Thread.Sleep(1000);
                                    PeriodThreadFunc();
                                }).Start();
                            }
                        }
                        break;

                    case Command.CMD_SET_DOUT_PORT_BIT:
                        {

                        }
                        break;

                    case Command.CMD_GET_AI_NUM:
                        {
                            AnalogInput.ChannelNumber = BitConverter.ToUInt16(data, 0);
                            //PRINT("\t CMD_GET_AI_NUM = " + AnalogInput.ChannelNumber);            
                            LoadedAnalogInputInfoEventHandler?.Invoke(AnalogInput.ChannelNumber);
                        }
                        break;

                    case Command.CMD_GET_AOUT_NUM:
                        {
                            AnalogOutput.ChannelNumber = BitConverter.ToUInt16(data, 0);
                            //PRINT("\t CMD_GET_AOUT_NUM = " + AnalogOutput.ChannelNumbe);
                            LoadedAnalogOutputInfoEventHandler?.Invoke(AnalogOutput.ChannelNumber);
                        }
                        break;

                    case Command.CMD_GET_DI_ALL:
                    case Command.CMD_GET_DOUT_ALL:
                        {
                            UInt16 portNum = BitConverter.ToUInt16(data, 0);
                            UInt32 val = 0;
                            List<UInt32> lPortValue = new List<UInt32>();
                            for (UInt16 i = 0; i < portNum; i++)
                            {
                                val = BitConverter.ToUInt32(data, 2 + i * 4);
                                lPortValue.Add(val);
                            }
                            if (cmd == Command.CMD_GET_DI_ALL)
                            {
                                DInGetAllPortValueEventHandler?.Invoke(lPortValue);
                            }
                            else
                            {
                                DOutGetAllPortValueEventHandler?.Invoke(lPortValue);
                            }
                        }
                        break;

                    case Command.CMD_GET_AI_ALL:
                    case Command.CMD_GET_AO_ALL:
                        {
                            UInt16 portNum = BitConverter.ToUInt16(data, 0);
                            Int32 val = 0;
                            List<Int32> lPortValue = new List<Int32>();
                            for (UInt16 i = 0; i < portNum; i++)
                            {
                                val = BitConverter.ToInt32(data, 2 + i * 4);
                                lPortValue.Add(val);
                            }
                            if (cmd == Command.CMD_GET_AI_ALL)
                            {
                                AInGetAllChannelValueEventHandler?.Invoke(lPortValue);
                            }
                            else
                            {
                                AOutGetAllChannelValueEventHandler?.Invoke(lPortValue);
                            }
                        }
                        break;

                    case Command.CMD_SET_AO:
                        {
                            AOutSetEventHandler?.Invoke();
                        }
                        break;



                    case Command.CMD_GET_CMD_POS:

                        {
                            byte rbNo = data[0];
                            byte axisNum = lRobot[rbNo].AxisNumber;
                            byte axisNum1 = (byte)((size - 1) / (8 * 2));
                            if (axisNum != axisNum1) break;
                            double[] jPos = new double[axisNum];

                            double[] wPos = new double[axisNum];
                            int memIndex = 1;
                            for (int i = 0; i < axisNum; i++)
                            {
                                jPos[i] = BitConverter.ToDouble(data, memIndex);
                                memIndex += 8;
                            }

                            for (int i = 0; i < axisNum; i++)
                            {
                                wPos[i] = BitConverter.ToDouble(data, memIndex);
                                memIndex += 8;
                            }
                            RobotGetCurrentPosEventHandler?.Invoke(rbNo, jPos, wPos);
                        }
                        break;

                    case Command.CMD_GET_STATUS_ROBOT_FOR_PRG:

                        {
                            byte robotNo = data[0];
                            UInt32 status = BitConverter.ToUInt32(data, 1);
                            RobotStatus rs = new RobotStatus();
                            rs.IsOn = ((status >> ((int)RB_STATUS_BIT.RS_SVON_FLAG)) & 0x01) > 0 ? true : false;
                            rs.IsAtHome = ((status >> ((int)RB_STATUS_BIT.RS_HOME_FLAG)) & 0x01) > 0 ? true : false;
                            rs.IsAlarm = ((status >> ((int)RB_STATUS_BIT.RS_DRIVE_ALARM)) & 0x01) > 0 ? true : false;
                            rs.IsOverWorkSpace = ((status >> ((int)RB_STATUS_BIT.RS_OVER_WORK_SPACE)) & 0x01) > 0 ? true : false;
                            rs.IsDetectObj = ((status >> ((int)RB_STATUS_BIT.RS_DETECT_OB)) & 0x01) > 0 ? true : false;
                            rs.IsError = ((status >> ((int)RB_STATUS_BIT.RS_ERR)) & 0x01) > 0 ? true : false;
                            rs.IsRunning = ((status >> ((int)RB_STATUS_BIT.RS_RUN_FLAG)) & 0x01) > 0 ? true : false;

                            RobotGetStatusEventHandler?.Invoke(robotNo, rs);
                        }
                        break;


                    case Command.CMD_GET_STATUS_RB_MOTOR_ALL:
                        {

                            if (errCode == 0)
                            {
                                byte rbNo = data[0];
                                int memIndex = 1;
                                int sizeOfStruct = Marshal.SizeOf(typeof(MOTOR_STATUS));
                                byte[] buffer = new byte[sizeOfStruct];

                                lMtStatus.Clear();
                                while (memIndex < size)
                                {
                                    Buffer.BlockCopy(data, memIndex, buffer, 0, sizeOfStruct);
                                    MOTOR_STATUS ms = ByteArrayToStructure<MOTOR_STATUS>(buffer);
                                    memIndex += sizeOfStruct;

                                    if (ms.MIdx >= lRobot[rbNo].Axes.Count) break;

                                    MotorStatus status = MotorStatus.OFF;
                                    if ((UInt32)((UInt32)ms.MStatus & BITSET((int)MT_STATUS_BIT.MS_SVON)) > 0)
                                        status = MotorStatus.ON;
                                    else if ((UInt32)((UInt32)ms.MStatus & BITSET((int)MT_STATUS_BIT.MS_DRIVE_ALARM)) > 0)
                                        status = MotorStatus.ALARM;
                                    lMtStatus.Add(status);
                                }//end while


                                RobotGetMotorStatusEventHandler?.Invoke(rbNo, lMtStatus);
                            }
                            else
                            {

                            }
                        }
                        break;

                    case Command.CMD_SET_SVON:
                        {
                            if (errCode != 0)
                            {
                                Send(Command.CMD_GET_ERR_STRING, BitConverter.GetBytes((UInt32)errCode));
                                ErrorEventHandler?.Invoke(Error.ROBOT_ON);
                            }
                        }
                        break;

                    case Command.CMD_SET_SVOFF:
                        {
                            if (errCode != 0) ErrorEventHandler?.Invoke(Error.ROBOT_OFF);
                        }
                        break;

                    case Command.CMD_SET_ALRAM_CLEAR:
                        {
                        }
                        break;

                    case Command.CMD_JOG_MOVE_NEGATIVE:
                    case Command.CMD_JOG_MOVE_POSITIVE:
                    case Command.CMD_JOG_GOTO:
                    case Command.CMD_JOG_GOTO_WORLD:
                    case Command.CMD_JOG_STEP:
                    case Command.CMD_JOG_STEP_WORLD:
                    case Command.CMD_JOG_MOVE_X_NEGATIVE:
                    case Command.CMD_JOG_MOVE_X_POSITIVE:
                    case Command.CMD_JOG_MOVE_Y_NEGATIVE:
                    case Command.CMD_JOG_MOVE_Y_POSITIVE:
                    case Command.CMD_JOG_MOVE_Z_NEGATIVE:
                    case Command.CMD_JOG_MOVE_Z_POSITIVE:
                    case Command.CMD_JOG_MOVE_TX_NEGATIVE:
                    case Command.CMD_JOG_MOVE_TX_POSITIVE:
                    case Command.CMD_JOG_MOVE_TY_NEGATIVE:
                    case Command.CMD_JOG_MOVE_TY_POSITIVE:
                    case Command.CMD_JOG_MOVE_TZ_NEGATIVE:
                    case Command.CMD_JOG_MOVE_TZ_POSITIVE:
                    case Command.CMD_JOG_MOVE_RX_NEGATIVE:
                    case Command.CMD_JOG_MOVE_RX_POSITIVE:
                    case Command.CMD_JOG_MOVE_RY_NEGATIVE:
                    case Command.CMD_JOG_MOVE_RY_POSITIVE:
                    case Command.CMD_JOG_MOVE_RZ_NEGATIVE:
                    case Command.CMD_JOG_MOVE_RZ_POSITIVE:
                    case Command.CMD_JOG_MOVE_TRX_NEGATIVE:
                    case Command.CMD_JOG_MOVE_TRX_POSITIVE:
                    case Command.CMD_JOG_MOVE_TRY_NEGATIVE:
                    case Command.CMD_JOG_MOVE_TRY_POSITIVE:
                    case Command.CMD_JOG_MOVE_TRZ_NEGATIVE:
                    case Command.CMD_JOG_MOVE_TRZ_POSITIVE:
                    case Command.CMD_MOVE_STOP:
                        {
                            if (errCode != 0)
                            {
                                Send(Command.CMD_GET_ERR_STRING, BitConverter.GetBytes((UInt32)errCode));
                                ErrorEventHandler?.Invoke(Error.JOG);
                            }
                        }
                        break;

                    case Command.CMD_JOG_GET_VELP:
                        {
                            byte robotNo = data[0];
                            double val = BitConverter.ToDouble(data, 1);
                            RobotGetJogVelPEventHandler?.Invoke(robotNo, val);
                        }
                        break;

                    case Command.CMD_JOG_SET_VELP:
                        {
                            if (errCode != 0)
                            {
                                Send(Command.CMD_GET_ERR_STRING, BitConverter.GetBytes((UInt32)errCode));
                                ErrorEventHandler?.Invoke(Error.SET_JOG_VELP);
                            }
                        }
                        break;

                    //Program
                    case Command.CMD_PRG_GET_LIST:
                        {
                            byte prgNo = 0;
                            byte nameSize = 0;
                            string name;
                            byte dateTimeSize = 0;
                            string dateTime;
                            UInt16 lines = 0;

                            UInt32 memIndex = 0;
                            lProgram.Clear();
                            while (memIndex < size)
                            {
                                prgNo = data[memIndex]; memIndex += 1;
                                nameSize = data[memIndex]; memIndex += 1;
                                name = BytesToString(data, (int)memIndex, nameSize); memIndex += nameSize;
                                dateTimeSize = data[memIndex]; memIndex += 1;
                                dateTime = BytesToString(data, (int)memIndex, dateTimeSize); memIndex += dateTimeSize;
                                lines = BitConverter.ToUInt16(data, (int)memIndex); memIndex += 2;

                                //PRINT("\tPrgNo = %d, %s, %s, %d", PrgNo, Name, DateTime, Lines);
                                //PRINT(PrgNo + " - " + Name + " - " + Lines + " - " + DateTime);
                                Program program = new Program();
                                program.Auto = false;
                                program.No = prgNo;
                                program.Description = name;
                                program.Lines = lines;
                                program.ExeLine = 0;
                                program.Status = "Compiled";
                                program.Date = dateTime;
                                lProgram.Add(program);
                            }

                            ProgramGetListEventHandler?.Invoke(lProgram);
                        }
                        break;

                    case Command.CMD_PRG_GET_AUTORUN:
                        {
                            byte count = data[0];
                            lAuto.Clear();
                            for (byte i = 0; i < count; i++)
                            {
                                lAuto.Add(data[i + 1] > 0 ? true : false);
                            }
                            ProgramGetAutoRunEventHandler?.Invoke(lAuto);
                        }
                        break;

                    case Command.CMD_PRG_GET_ALL_STATUS:
                        {
                            lProgramStatus.Clear();
                            int memIndex = 0;
                            int sizeOfStruct = Marshal.SizeOf(typeof(PRG_EXE));
                            byte[] buffer = new byte[sizeOfStruct];
                            while (memIndex < size)
                            {
                                Buffer.BlockCopy(data, memIndex, buffer, 0, sizeOfStruct);
                                PRG_EXE pe = ByteArrayToStructure<PRG_EXE>(buffer);
                                memIndex += sizeOfStruct;
                                ProgramStatus ps = new ProgramStatus();

                                if ((pe.Status & 0x04) > 0) ps.Status = "Error";
                                else if ((pe.Status & 0x02) > 0) ps.Status = "Running";
                                else if ((pe.Status & 0x08) > 0) ps.Status = "Pause";
                                else if ((pe.Status & 0x01) > 0) ps.Status = "Compiled";
                                else ps.Status = "None";
                                ps.LineExe = pe.LineExe;

                                PRINT("\t" + pe.Index + " - " + ps.LineExe + " - " + ps.Status);
                                lProgramStatus.Add(ps);
                            }
                            ProgramGetAllStatusEventHandler?.Invoke(lProgramStatus);
                        }
                        break;

                    case Command.CMD_PRG_GET_STATUS:
                        {
                            ProgramGetStatusEventHandler?.Invoke((byte)data[0], (ProgramState)data[1]);
                        }
                        break;

                    case Command.CMD_PRG_OPEN:
                        {
                            if (errCode == 0)
                            {
                                byte prgNo = data[0];
                                byte desSize = data[1];
                                string des = BytesToString(data, 2, (int)desSize);
                                UInt32 contenSize = BitConverter.ToUInt32(data, 2 + desSize);
                                string text = BytesToString(data, 6 + desSize, (int)contenSize);
                                ProgramOpenEventHandler?.Invoke(prgNo, des, text);
                            }
                            else ErrorEventHandler?.Invoke(Error.PROGRAM_OPEN);
                        }
                        break;

                    case Command.CMD_PRG_SAVE:
                        {
                            if (errCode != 0) ProgramSaveEventHandler?.Invoke(Error.PROGRAM_SAVE);
                            else ProgramSaveEventHandler?.Invoke(Error.NONE);
                        }
                        break;
                    case Command.CMD_PRG_COMPILE:
                        {
                            if (errCode != 0) ProgramCompileEventHandler?.Invoke(Error.PROGRAM_COMPILE);
                            else ProgramCompileEventHandler?.Invoke(Error.NONE);
                        }
                        break;
                    case Command.CMD_PRG_START:
                        {
                            if (errCode != 0) ProgramStartEventHandler?.Invoke(Error.PROGRAM_START);
                            else ProgramStartEventHandler?.Invoke(Error.NONE);
                        }
                        break;
                    case Command.CMD_PRG_STOP:
                        {
                            if (errCode != 0) ProgramStopEventHandler?.Invoke(Error.PROGRAM_STOP);
                            else ProgramStopEventHandler?.Invoke(Error.NONE);
                        }
                        break;
                    case Command.CMD_PRG_STOP_ALL:
                        {
                            if (errCode != 0) ProgramStopAllEventHandler?.Invoke(Error.PROGRAM_STOP_ALL);
                            else ProgramStopAllEventHandler?.Invoke(Error.NONE);
                        }
                        break;
                    case Command.CMD_PRG_PAUSE:
                        {
                            if (errCode != 0) ProgramPauseEventHandler?.Invoke(Error.PROGRAM_PAUSE);
                            else ProgramPauseEventHandler?.Invoke(Error.NONE);
                        }
                        break;
                    case Command.CMD_PRG_RESUME:
                        {
                            if (errCode != 0) ProgramResumeEventHandler?.Invoke(Error.PROGRAM_RESUME);
                            else ProgramResumeEventHandler?.Invoke(Error.NONE);
                        }
                        break;

                    case Command.CMD_PRG_GET_ERR_MSG:
                        {
                            byte len = data[0];
                            string msg = BytesToString(data, 1, len);
                            ProgramGetErrMsgEventHandler?.Invoke(msg);
                        }
                        break;

                    case Command.CMD_PRG_EXEC_SINGLE_STRING:
                        {
                            ExecSingleCmdEventHandler?.Invoke(BytesToString(data));
                        }
                        break;

                    case Command.CMD_GET_STATUS_ROBOT_ALL:
                        {
                        }
                        break;

                    //Sub-Program
                    case Command.CMD_SUB_PRG_GET_ALL_STATUS:
                        {
                            lSubProgram.Clear();
                            int numSubPrg = data[0];
                            int memIndex = 1;
                            for (int i = 0; i < numSubPrg; i++)
                            {
                                Program sp = new Program();

                                byte nameSize = data[memIndex]; memIndex += 1;
                                sp.Description = BytesToString(data, (int)memIndex, nameSize); memIndex += nameSize;
                                byte dateTimeSize = data[memIndex]; memIndex += 1;
                                sp.Date = BytesToString(data, (int)memIndex, dateTimeSize); memIndex += dateTimeSize;
                                sp.Lines = BitConverter.ToUInt16(data, (int)memIndex); memIndex += 2;
                                byte spState = data[memIndex]; memIndex += 1;
                                if ((spState & 0x01) > 0) sp.Status = "Compiled";
                                else sp.Status = "Error";
                                UInt16 lineNo = BitConverter.ToUInt16(data, (int)memIndex); memIndex += 2;

                                //PRINT(sp.Description + " - " + sp.Lines + " - " + sp.Status);
                                lSubProgram.Add(sp);
                            }

                            SubProgramGetListStatusEventHandler?.Invoke(lSubProgram);
                        }
                        break;


                    case Command.CMD_SUB_PRG_OPEN:
                        {
                            UInt32 size = BitConverter.ToUInt32(data, 0);
                            string text = BytesToString(data, 4, (int)size);
                            SubProgramOpenEventHandler?.Invoke(text);
                        }
                        break;

                    case Command.CMD_SUB_PRG_SAVE:
                        {
                            if (errCode != 0) SubProgramSaveEventHandler?.Invoke(Error.SUB_PROGRAM_SAVE);
                            else SubProgramSaveEventHandler?.Invoke(Error.NONE);
                        }
                        break;

                    case Command.CMD_SUB_PRG_DELETE:
                        {
                            if (errCode != 0) SubProgramDeleteEventHandler?.Invoke(Error.SUB_PROGRAM_DELETE);
                            else SubProgramDeleteEventHandler?.Invoke(Error.NONE);
                        }
                        break;


                    case Command.CMD_SUB_PRG_COMPILE:
                        {
                            if (errCode != 0) SubProgramCompileEventHandler?.Invoke(Error.SUB_PROGRAM_COMPILE);
                            else SubProgramCompileEventHandler?.Invoke(Error.NONE);
                        }
                        break;


                    case Command.CMD_SUB_PRG_GET_ERR_MSG:
                        {
                            byte len = data[0];
                            string msg = BytesToString(data, 1, len);
                            SubProgramGetErrMsgEventHandler?.Invoke(msg);
                        }
                        break;

                    //Log
                    case Command.CMD_GET_LOG_LINE_NUM:
                        {
                            LoggingType type = (LoggingType)data[0];
                            UInt32 lineNo = BitConverter.ToUInt32(data, 1);
                            LogGetLineNumEventHandler?.Invoke(type, lineNo);
                        }
                        break;

                    case Command.CMD_GET_LOG_STR:
                        {
                            LoggingType type = (LoggingType)data[0];
                            string text = BytesToString(data, 1, (int)size - 1);
                            LogGetStrEventHandler?.Invoke(type, text);
                        }
                        break;

                    case Command.CMD_CLEAR_LOG:
                        {
                            LogClearEventHandler?.Invoke((LoggingType)data[0]);
                        }
                        break;

                    //System Config                    

                    case Command.CMD_GET_SYS_FILE:
                        {
                            try
                            {
                                string text = BytesToString(data);
                                xmlSysPara = new XmlDocument();
                                xmlSysPara.LoadXml(text);
                                XmlNode sysNode = xmlSysPara.DocumentElement.SelectSingleNode("//SysPara");
                                sysPara.RbNumber = byte.Parse(sysNode.SelectSingleNode("//RbNumber").InnerText);
                                sysPara.ENIFileName = sysNode.SelectSingleNode("//ENIFileName").InnerText;
                                sysPara.CycleTime = UInt16.Parse(sysNode.SelectSingleNode("//CycleTime").InnerText);
                                sysPara.TotalAxisNumber = byte.Parse(sysNode.SelectSingleNode("//TotalAxisNumber").InnerText);
                                sysPara.UseTeachPendant = byte.Parse(sysNode.SelectSingleNode("//UseTeachPendant").InnerText);
                                sysPara.IPAddress = sysNode.SelectSingleNode("//IPAddress").InnerText;

                                Send(Command.CMD_GET_STATUS_SYS, null); //Get number of real Robot & EtherCat Status
                                for (byte rbNo = 0; rbNo < sysPara.RbNumber; rbNo++)
                                {
                                    byte[] sendData = new byte[1];
                                    sendData[0] = rbNo;
                                    Send(Command.CMD_GET_RB_PARA_FILE, sendData);
                                }

                            }
                            catch (Exception ex)
                            {
                                LogErrWrite(ex.ToString());
                            }
                        }
                        break;

                    case Command.CMD_SET_SYS_FILE:
                        {
                        }
                        break;

                    case Command.CMD_GET_RB_PARA_FILE:
                        {
                            try
                            {
                                string text = BytesToString(data);
                                Robot robot = Text2Robot(text);
                                lRobot.Add(robot);
                                //Loaded All Robot ?
                                if (lRobot.Count == sysPara.RbNumber)
                                {
                                    LoadedAllRobotEventHandler?.Invoke(lRobot);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogErrWrite(ex.ToString());
                            }
                        }
                        break;


                    case Command.CMD_SET_RB_PARA_FILE:
                        {
                        }
                        break;

                    case Command.CMD_SET_POS_VAR_FILE:
                        {
                        }
                        break;

                    case Command.CMD_SET_VAR_FILE:
                        {
                        }
                        break;


                    case Command.CMD_SEARCH_RB_PARA_FOLDER:
                        {
                            UInt16 memIndex = 1;
                            lRobotTemplate.Clear();
                            while (memIndex < size)
                            {
                                byte len = data[memIndex]; memIndex++;
                                lRobotTemplate.Add(new Folder(BytesToString(data, memIndex, len))); memIndex += len;
                            }
                            //lRobotTemplate.Sort();

                            searchFolderCounter = 0;
                            foreach (Folder folder in lRobotTemplate)
                            {
                                int len = folder.Name.Length;
                                byte[] sendData = new byte[len + 1];
                                sendData[0] = (byte)len;
                                Buffer.BlockCopy(StringToBytes(folder.Name), 0, sendData, 1, len);
                                Send(Command.CMD_SEARCH_RB_PARA_FILE, sendData);
                            }
                        }
                        break;

                    case Command.CMD_SEARCH_RB_PARA_FILE:
                        {
                            //UInt16 numFiles = BitConverter.ToUInt16(data, 0);
                            UInt16 memIndex = 2;
                            while (memIndex < size)
                            {
                                byte len = data[memIndex]; memIndex++;
                                string name = BytesToString(data, memIndex, len); memIndex += len;
                                if (searchFolderCounter < lRobotTemplate.Count)
                                {
                                    lRobotTemplate[searchFolderCounter].Files.Add(name);
                                }
                            }
                            searchFolderCounter++;

                            if (searchFolderCounter == lRobotTemplate.Count)
                            {
                                SystemConfigGetRobotTemplateEventHandler?.Invoke(lRobotTemplate);
                            }
                        }
                        break;



                    case Command.CMD_GET_RB_PARA_XML_FILE:
                        {
                            string text = BytesToString(data, 0, (int)size);
                            Robot robot = Text2Robot(text);
                            if (errCode == 0)
                                SystemConfigGetRobotTemplateFileEventHandler?.Invoke(Error.NONE, robot);
                            else
                                SystemConfigGetRobotTemplateFileEventHandler?.Invoke(Error.CONFIG_GET_ROBOT_TEMPLATE_FILE, robot);

                        }
                        break;



                    case Command.CMD_SEARCH_ENI_FILE:
                        {
                            int numXMl = 0;
                            int memIndex = 0;
                            numXMl = BitConverter.ToUInt16(data, memIndex); memIndex += 2;
                            List<string> lst = new List<string>();
                            for (int i = 0; i < numXMl; i++)
                            {
                                byte len = data[memIndex]; memIndex += 1;
                                string fileName = BytesToString(data, memIndex, len); memIndex += len;
                                lst.Add(fileName);
                            }
                            SystemConfigGetEniFilesEventHandler?.Invoke(lst);
                        }
                        break;


                    //Motion Parameters
                    case Command.CMD_JOG_GET_MPARA_BASE_TIME: //Manual
                    case Command.CMD_GET_MPARA_BASE_TIME: //Program	
                    case Command.CMD_GET_READY_MPARA_BASE_TIME: //Ready, return home MoveR
                        {
                            MotionType type = MotionType.Manual;
                            if (cmd == Command.CMD_GET_MPARA_BASE_TIME) type = MotionType.Program;
                            else if (cmd == Command.CMD_GET_READY_MPARA_BASE_TIME) type = MotionType.Ready;

                            byte robotNo = data[0];
                            byte axisNo = data[1];
                            double[] dData = null;
                            if (cmd == Command.CMD_GET_READY_MPARA_BASE_TIME)
                            {
                                dData = new double[7];
                                Buffer.BlockCopy(data, 2, dData, 0, 48 + 8);
                            }
                            else
                            {
                                dData = new double[6];
                                Buffer.BlockCopy(data, 2, dData, 0, 48);
                            }

                            if (axisNo < 64)
                                MotionParaGetJointEventHandler?.Invoke(type, robotNo, axisNo, dData);
                            else
                            {
                                axisNo -= 64;
                                MotionParaGetWorkEventHandler?.Invoke(type, robotNo, axisNo, dData);
                            }
                        }
                        break;
                    case Command.CMD_JOG_SET_MPARA_BASE_TIME: //manual
                    case Command.CMD_SET_MPARA_BASE_TIME: //program
                    case Command.CMD_SET_READY_MPARA_BASE_TIME: //ready
                        break;

                    //Save from internal memory to file
                    case Command.CMD_SAVE_TO_SYS_FILE: break;
                    case Command.CMD_SAVE_TO_RPARA_FILE: break;
                    case Command.CMD_SAVE_TO_RPOS_FILE: break;
                    case Command.CMD_SAVE_TO_RVAR_FILE: break;

                    //Variable
                    case Command.CMD_GET_I_VAR_ALL:
                        {
                            int memIndex = 0;
                            byte robotNo = 0;
                            UInt16 varNum = 0;
                            byte len = 0;
                            Int32 val = 0;
                            string text;
                            robotNo = data[memIndex]; memIndex += 1;
                            varNum = BitConverter.ToUInt16(data, memIndex); memIndex += 2;

                            List<IVar> lst = new List<IVar>();
                            for (UInt16 i = 0; i < varNum; i++)
                            {
                                len = data[memIndex]; memIndex += 1;
                                text = BytesToString(data, memIndex, len); memIndex += len;
                                val = BitConverter.ToInt32(data, memIndex); memIndex += 4;
                                lst.Add(new IVar(i, text, val));
                            }
                            IVarGetAllEventHandler?.Invoke(robotNo, lst);
                        }
                        break;

                    case Command.CMD_ADD_I_VAR: break;
                    case Command.CMD_DELETE_I_VAR: break;
                    case Command.CMD_SET_I_VAR: break;

                    case Command.CMD_GET_D_VAR_ALL:
                        {
                            int memIndex = 0;
                            byte robotNo = 0;
                            UInt16 varNum = 0;
                            byte len = 0;
                            double val = 0;
                            string text;
                            robotNo = data[memIndex]; memIndex += 1;
                            varNum = BitConverter.ToUInt16(data, memIndex); memIndex += 2;

                            List<DVar> lst = new List<DVar>();
                            for (UInt16 i = 0; i < varNum; i++)
                            {
                                len = data[memIndex]; memIndex += 1;
                                text = BytesToString(data, memIndex, len); memIndex += len;
                                val = BitConverter.ToDouble(data, memIndex); memIndex += 8;
                                lst.Add(new DVar(i, text, val));
                            }
                            DVarGetAllEventHandler?.Invoke(robotNo, lst);
                        }
                        break;

                    case Command.CMD_ADD_D_VAR: break;
                    case Command.CMD_DELETE_D_VAR: break;
                    case Command.CMD_SET_D_VAR: break;

                    case Command.CMD_GET_POS_VAR_ALL:
                        {
                            int memIndex = 0;
                            byte robotNo = data[memIndex++];
                            byte axisNum = data[memIndex++];
                            if (robotNo < lRobot.Count && axisNum != lRobot[robotNo].Axes.Count)
                            {
                                //Error
                                PosVarGetAllEventHandler?.Invoke(robotNo, null);
                                break;
                            }
                            UInt16 varNum = BitConverter.ToUInt16(data, memIndex); memIndex += 2;
                            byte len = 0;
                            List<PosVar> lst = new List<PosVar>();
                            //Work
                            for (UInt16 i = 0; i < varNum; i++)
                            {
                                PosVar pos = new PosVar(axisNum);
                                pos.Index = i;
                                len = data[memIndex++];
                                pos.WDescription = BytesToString(data, memIndex, len); memIndex += len;
                                Buffer.BlockCopy(data, memIndex, pos.WPos, 0, axisNum * 8);
                                memIndex += axisNum * 8;
                                lst.Add(pos);
                            }

                            //Joint
                            for (UInt16 i = 0; i < varNum; i++)
                            {
                                len = data[memIndex++];
                                lst[i].JDescription = BytesToString(data, memIndex, len); memIndex += len;
                                Buffer.BlockCopy(data, memIndex, lst[i].JPos, 0, axisNum * 8);
                                memIndex += axisNum * 8;
                            }
                            PosVarGetAllEventHandler?.Invoke(robotNo, lst);
                        }
                        break;

                    case Command.CMD_ADD_POS_VAR:
                    case Command.CMD_DELETE_POS_VAR:
                    case Command.CMD_SET_WPOS_VAR:
                    case Command.CMD_SET_JPOS_VAR:
                        //case Command.CMD_GET_JPOS_VAR:
                        //case Command.CMD_GET_WPOS_VAR:
                        {

                        }
                        break;

                    case Command.CMD_GET_ERR_STRING:
                        {
                            UInt32 code = BitConverter.ToUInt32(data, 0);
                            byte msgLength = data[4];
                            string msg = BytesToString(data, 5, msgLength);
                            PrcErrorEventHandler?.Invoke(code, msg);
                        }
                        break;

                    // Vin
                    case Command.CMD_GET_CARR_INFO_ALL:
                        {
                            int memIndex = 0;
                            byte iCarrCount = data[memIndex];
                            memIndex++;

                            List<SCarrierInfo> lstCarr = new List<SCarrierInfo>();
                            for (int i = 0; i < iCarrCount; i++)
                            {
                                SCarrierInfo carrInfo = new SCarrierInfo();
                                carrInfo.OnDrv1 = data[memIndex]; memIndex += 1;
                                carrInfo.OnDrv2 = data[memIndex]; memIndex += 1;
                                carrInfo.LoadFlag = data[memIndex]; memIndex += 1;
                                carrInfo.PhyID = BitConverter.ToUInt16(data, memIndex); memIndex += 2;
                                carrInfo.Status = BitConverter.ToUInt32(data, memIndex); memIndex += 4;
                                carrInfo.Pos.TrackNo = data[memIndex]; memIndex += 1;
                                carrInfo.Pos.Pos = BitConverter.ToDouble(data, memIndex); memIndex += 8;
                                lstCarr.Add(carrInfo);
                            }
                            LmmcGetCarrierInfoEventHandler(lstCarr);
                        }
                        break;

                    case Command.CMD_GET_LM_TRACK_POS_ALL:
                        {
                            int memIndex = 0;
                            byte shuttleCount = data[memIndex];

                            List<SShuttleInfo> lstShuttInfo = new List<SShuttleInfo>(); memIndex += 1;

                            for (int i = 0; i < shuttleCount; i++)
                            {
                                SShuttleInfo shuttInfo = new SShuttleInfo();
                                //shuttInfo.TrackNo = data[memIndex]; memIndex += 1;
                                //shuttInfo.TrackType = (LMMC_TRACK_TYPE)BitConverter.ToUInt32(data, memIndex); memIndex += 4;
                                //shuttInfo.MapSlaNo = data[memIndex]; memIndex += 1;
                                //shuttInfo.CarrierNoOn = data[memIndex]; memIndex += 1;
                                shuttInfo.Pos = BitConverter.ToDouble(data, memIndex); memIndex += 8;
                                //shuttInfo.Status = (LMMC_RAIL_STATUS)BitConverter.ToUInt32(data, memIndex); memIndex += 4;
                                //shuttInfo.ErrCode = BitConverter.ToUInt32(data, memIndex); memIndex += 4;
                                //shuttInfo.Torque = BitConverter.ToUInt16(data, memIndex); memIndex += 2;
                                lstShuttInfo.Add(shuttInfo);
                            }

                            LmmcGetShuttleInfoEventHandler(lstShuttInfo);
                        }
                        break;

                    // ~Vin


                    default: break;
                } //end switch
            }
            catch (Exception ex)
            {
                if (logErr != null) LogErrWrite(ex.ToString());
                throw ex;
            }
        }

        private string BytesToString(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }

        private string BytesToString(byte[] bytes, int offset, int size)
        {
            return Encoding.ASCII.GetString(bytes, offset, size);
        }


        private Robot Text2Robot(string text)
        {
            Robot robot = new Robot();
            try
            {
                XmlDocument xmlRbPara = new XmlDocument();
                xmlRbPara.LoadXml(text);
                XmlNode rbParaNode = xmlRbPara.DocumentElement.SelectSingleNode("//RbPara");
                robot.RbType = UInt16.Parse(rbParaNode.SelectSingleNode("//RbType").InnerText);
                robot.Name = rbParaNode.SelectSingleNode("//Name").InnerText;
                robot.ToolType = UInt16.Parse(rbParaNode.SelectSingleNode("//ToolType").InnerText);
                robot.AxisNumber = byte.Parse(rbParaNode.SelectSingleNode("//AxisNumber").InnerText);
                robot.Vendor = rbParaNode.SelectSingleNode("//Vendor").InnerText;
                robot.Model = rbParaNode.SelectSingleNode("//Model").InnerText;
                robot.CoordType = byte.Parse(rbParaNode.SelectSingleNode("//CoordType").InnerText);
                robot.DefVelP = double.Parse(rbParaNode.SelectSingleNode("//DefVelP").InnerText);
                robot.DefJogVelP = double.Parse(rbParaNode.SelectSingleNode("//DefJogVelP").InnerText);

                robot.WorkArea.Xn = double.Parse(rbParaNode.SelectSingleNode("//WorkArea/X_N").InnerText);
                robot.WorkArea.Yn = double.Parse(rbParaNode.SelectSingleNode("//WorkArea/Y_N").InnerText);
                robot.WorkArea.Zn = double.Parse(rbParaNode.SelectSingleNode("//WorkArea/Z_N").InnerText);
                robot.WorkArea.Xp = double.Parse(rbParaNode.SelectSingleNode("//WorkArea/X_P").InnerText);
                robot.WorkArea.Yp = double.Parse(rbParaNode.SelectSingleNode("//WorkArea/Y_P").InnerText);
                robot.WorkArea.Zp = double.Parse(rbParaNode.SelectSingleNode("//WorkArea/Z_P").InnerText);

                foreach (XmlNode node in rbParaNode.SelectNodes("//AxisPara")) //Get all axis parameters
                {
                    Axis axis = new Axis();
                    axis.No = byte.Parse(node.SelectSingleNode("//No").InnerText);
                    axis.DriveMapSlaNum = byte.Parse(node.SelectSingleNode("//DriveMappSlaNum").InnerText);
                    axis.CmdPulsePerCycle = UInt32.Parse(node.SelectSingleNode("//CmdPulsePerCycle").InnerText);
                    axis.DistPerCycle = double.Parse(node.SelectSingleNode("//DistPerCycle").InnerText);
                    axis.GearRate = double.Parse(node.SelectSingleNode("//GearRate").InnerText);
                    axis.MaxVelocity = double.Parse(node.SelectSingleNode("//MaxVelocity").InnerText);
                    axis.MaxTorque = double.Parse(node.SelectSingleNode("//MaxTorque").InnerText);

                    axis.SWLimit.Neg = double.Parse(node.SelectSingleNode("//SWLimit/Neg").InnerText);
                    axis.SWLimit.Pos = double.Parse(node.SelectSingleNode("//SWLimit/Pos").InnerText);

                    axis.MotDirection = sbyte.Parse(node.SelectSingleNode("//MotDirection").InnerText);
                    axis.InposPulse = UInt32.Parse(node.SelectSingleNode("//InposPulse").InnerText);
                    axis.SettlingTime = UInt32.Parse(node.SelectSingleNode("//SettlingTime").InnerText);
                    axis.BrakeReleaseTime = UInt32.Parse(node.SelectSingleNode("//SettlingTime").InnerText);
                    axis.MaxPosErr = double.Parse(node.SelectSingleNode("//MaxPosErr").InnerText);

                    axis.HomePara.Method = byte.Parse(node.SelectSingleNode("//HomePara/Method").InnerText);
                    axis.HomePara.Acceleraion = double.Parse(node.SelectSingleNode("//HomePara/Acceleraion").InnerText);
                    axis.HomePara.Velocity = double.Parse(node.SelectSingleNode("//HomePara/Velocity").InnerText);
                    axis.HomePara.ReturnVel = double.Parse(node.SelectSingleNode("//HomePara/ReturnVel").InnerText);
                    axis.HomePara.Offset = double.Parse(node.SelectSingleNode("//HomePara/Offset").InnerText);

                    axis.ReadyMovePara.Vel = double.Parse(node.SelectSingleNode("//ReadyMovePara/Vel").InnerText);
                    axis.ReadyMovePara.AccTime = double.Parse(node.SelectSingleNode("//ReadyMovePara/AccTime").InnerText);
                    axis.ReadyMovePara.DecTime = double.Parse(node.SelectSingleNode("//ReadyMovePara/DecTime").InnerText);
                    axis.ReadyMovePara.AJerkP = double.Parse(node.SelectSingleNode("//ReadyMovePara/AJerkP").InnerText);
                    axis.ReadyMovePara.DJerkP = double.Parse(node.SelectSingleNode("//ReadyMovePara/DJerkP").InnerText);
                    axis.ReadyMovePara.KillTime = double.Parse(node.SelectSingleNode("//ReadyMovePara/KillTime").InnerText);
                    axis.ReadyMovePara.Position = double.Parse(node.SelectSingleNode("//ReadyMovePara/Position").InnerText);

                    axis.JogMovePara.JVel = double.Parse(node.SelectSingleNode("//JogMovePara/JVel").InnerText);
                    axis.JogMovePara.JAccTime = double.Parse(node.SelectSingleNode("//JogMovePara/JAccTime").InnerText);
                    axis.JogMovePara.JDecTime = double.Parse(node.SelectSingleNode("//JogMovePara/JDecTime").InnerText);
                    axis.JogMovePara.JAJerkP = double.Parse(node.SelectSingleNode("//JogMovePara/JAJerkP").InnerText);
                    axis.JogMovePara.JDJerkP = double.Parse(node.SelectSingleNode("//JogMovePara/JDJerkP").InnerText);
                    axis.JogMovePara.JKillTime = double.Parse(node.SelectSingleNode("//JogMovePara/JKillTime").InnerText);
                    axis.JogMovePara.WVel = double.Parse(node.SelectSingleNode("//JogMovePara/WVel").InnerText);
                    axis.JogMovePara.WAccTime = double.Parse(node.SelectSingleNode("//JogMovePara/WAccTime").InnerText);
                    axis.JogMovePara.WDecTime = double.Parse(node.SelectSingleNode("//JogMovePara/WDecTime").InnerText);
                    axis.JogMovePara.WAJerkP = double.Parse(node.SelectSingleNode("//JogMovePara/WAJerkP").InnerText);
                    axis.JogMovePara.WDJerkP = double.Parse(node.SelectSingleNode("//JogMovePara/WDJerkP").InnerText);
                    axis.JogMovePara.WKillTime = double.Parse(node.SelectSingleNode("//JogMovePara/WKillTime").InnerText);

                    axis.MovePara.JVel = double.Parse(node.SelectSingleNode("//MovePara/JVel").InnerText);
                    axis.MovePara.JAccTime = double.Parse(node.SelectSingleNode("//MovePara/JAccTime").InnerText);
                    axis.MovePara.JDecTime = double.Parse(node.SelectSingleNode("//MovePara/JDecTime").InnerText);
                    axis.MovePara.JAJerkP = double.Parse(node.SelectSingleNode("//MovePara/JAJerkP").InnerText);
                    axis.MovePara.JDJerkP = double.Parse(node.SelectSingleNode("//MovePara/JDJerkP").InnerText);
                    axis.MovePara.JKillTime = double.Parse(node.SelectSingleNode("//MovePara/JKillTime").InnerText);
                    axis.MovePara.WVel = double.Parse(node.SelectSingleNode("//MovePara/WVel").InnerText);
                    axis.MovePara.WAccTime = double.Parse(node.SelectSingleNode("//MovePara/WAccTime").InnerText);
                    axis.MovePara.WDecTime = double.Parse(node.SelectSingleNode("//MovePara/WDecTime").InnerText);
                    axis.MovePara.WAJerkP = double.Parse(node.SelectSingleNode("//MovePara/WAJerkP").InnerText);
                    axis.MovePara.WDJerkP = double.Parse(node.SelectSingleNode("//MovePara/WDJerkP").InnerText);
                    axis.MovePara.WKillTime = double.Parse(node.SelectSingleNode("//MovePara/WKillTime").InnerText);
                    robot.Axes.Add(axis);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return robot;
        }

    }

}

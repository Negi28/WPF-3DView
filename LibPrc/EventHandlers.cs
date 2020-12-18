using Presto.PRC.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presto.PRC.EventHandlers
{
    public delegate void ErrorEventHandler(Error error);
    public delegate void PrcErrorEventHandler(UInt32 code, string error);
    public delegate void ConnectEventHandler();
    public delegate void DisconnectEventHandler();
    public delegate void LoginEventHandler(Error error);
    public delegate void LoginChangePasswordEventHandler(Error error);

    public delegate void GetSystemStatusEventHandler(SystemStatus s);

    //Trigger when all robot information are loaded
    public delegate void LoadedAllRobotEventHandler(List<Robot> lRobot);    

    public delegate void RobotGetCurrentPosEventHandler(byte robotNo, double[] jPos, double[] wPos);
    public delegate void RobotGetStatusEventHandler(byte robotNo, RobotStatus rs);
    public delegate void RobotGetMotorStatusEventHandler(byte robotNo, List<MotorStatus> lMs);
    public delegate void RobotGetJogVelPEventHandler(byte robotNo, double val);

    //Program
    public delegate void ProgramGetListEventHandler(List<Program> lProgram);
    public delegate void ProgramGetAutoRunEventHandler(List<bool> lAuto);
    public delegate void ProgramGetAllStatusEventHandler(List<ProgramStatus> lProgramStatus);
    public delegate void ProgramGetStatusEventHandler(byte prgNo, ProgramState state);
    public delegate void ProgramOpenEventHandler(byte prgNo, string des, string text);
    public delegate void ProgramSaveEventHandler(Error error);
    public delegate void ProgramCompileEventHandler(Error error);
    public delegate void ProgramStartEventHandler(Error error);
    public delegate void ProgramStopEventHandler(Error error);
    public delegate void ProgramStopAllEventHandler(Error error);
    public delegate void ProgramPauseEventHandler(Error error);
    public delegate void ProgramResumeEventHandler(Error error);
    public delegate void ProgramGetErrMsgEventHandler(string err);
    public delegate void ExecSingleCmdEventHandler(string msg);


    //Sub program
    public delegate void SubProgramGetListStatusEventHandler(List<Program> lSubProgram);
    public delegate void SubProgramOpenEventHandler(string text);
    public delegate void SubProgramSaveEventHandler(Error error);
    public delegate void SubProgramDeleteEventHandler(Error error);
    public delegate void SubProgramCompileEventHandler(Error error);
    public delegate void SubProgramGetErrMsgEventHandler(string err);


    //IO
    public delegate void LoadedDigitalInputInfoEventHandler(List<byte> lPortSizes);
    public delegate void LoadedDigitalOutputInfoEventHandler(List<byte> lPortSizes);
    public delegate void LoadedAnalogInputInfoEventHandler(int channelNumber);
    public delegate void LoadedAnalogOutputInfoEventHandler(int channelNumber);

    public delegate void DInGetAllPortValueEventHandler(List<UInt32> lPortValue);
    public delegate void DOutGetAllPortValueEventHandler(List<UInt32> lPortValue);
    public delegate void AInGetAllChannelValueEventHandler(List<Int32> lChannelValue);
    public delegate void AOutGetAllChannelValueEventHandler(List<Int32> lChannelValue);
    public delegate void AOutSetEventHandler();

    //Scope

    //Variables

    //Log
    public delegate void LogGetLineNumEventHandler(LoggingType type, UInt32 lineNo);
    public delegate void LogGetStrEventHandler(LoggingType type, string text);
    public delegate void LogClearEventHandler(LoggingType type);

    //Config
    public delegate void SystemConfigGetRobotTemplateEventHandler(List<Folder> lFolder);
    public delegate void SystemConfigGetRobotTemplateFileEventHandler(Error error, Robot robot);
    public delegate void SystemConfigGetEniFilesEventHandler(List<string> lst);

    //Motion Parameters
    public delegate void MotionParaGetJointEventHandler(MotionType type, byte robotNo, byte axisNo, double[] para);
    public delegate void MotionParaGetWorkEventHandler(MotionType type, byte robotNo, byte axisNo, double[] para);

    //Variables
    public delegate void IVarGetAllEventHandler(byte robotNo, List<IVar> lst);
    public delegate void DVarGetAllEventHandler(byte robotNo, List<DVar> lst);
    public delegate void PosVarGetAllEventHandler(byte robotNo, List<PosVar> lst);

    // For testing LMMC
    public delegate void LGetCarrierInfoEventHandler(List<SCarrierInfo> lstCarr);
    public delegate void LGetShuttleInfoEventHandler(List<SShuttleInfo> lstShuttle);

}

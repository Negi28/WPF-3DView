using Presto.PRC.EventHandlers;

namespace Presto.PRC
{
    partial class Controller
    {
        public ErrorEventHandler ErrorEventHandler { get; set; }
        public PrcErrorEventHandler PrcErrorEventHandler { get; set; }
        public ConnectEventHandler ConnectEventHandler { get; set; }
        public DisconnectEventHandler DisconnectEventHandler { get; set; }
        public LoginEventHandler LoginEventHandler { get; set; }
        public LoginChangePasswordEventHandler LoginChangePasswordEventHandler { get; set; }

        public GetSystemStatusEventHandler GetSystemStatusEventHandler { get; set; }
        public LoadedAllRobotEventHandler LoadedAllRobotEventHandler { get; set; }

        //Robot
        public RobotGetCurrentPosEventHandler RobotGetCurrentPosEventHandler { get; set; }
        public RobotGetStatusEventHandler RobotGetStatusEventHandler { get; set; }
        public RobotGetMotorStatusEventHandler RobotGetMotorStatusEventHandler { get; set; }
        public RobotGetJogVelPEventHandler RobotGetJogVelPEventHandler { get; set; }

        //Program
        public ProgramGetListEventHandler ProgramGetListEventHandler { get; set; }
        public ProgramGetAutoRunEventHandler ProgramGetAutoRunEventHandler { get; set; }
        public ProgramGetAllStatusEventHandler ProgramGetAllStatusEventHandler { get; set; }
        public ProgramGetStatusEventHandler ProgramGetStatusEventHandler { get; set; }
        public ProgramOpenEventHandler ProgramOpenEventHandler { get; set; }
        public ProgramSaveEventHandler ProgramSaveEventHandler { get; set; }
        public ProgramCompileEventHandler ProgramCompileEventHandler { get; set; }
        public ProgramStartEventHandler ProgramStartEventHandler { get; set; }
        public ProgramStopEventHandler ProgramStopEventHandler { get; set; }
        public ProgramStopAllEventHandler ProgramStopAllEventHandler { get; set; }
        public ProgramPauseEventHandler ProgramPauseEventHandler { get; set; }
        public ProgramResumeEventHandler ProgramResumeEventHandler { get; set; }
        public ProgramGetErrMsgEventHandler ProgramGetErrMsgEventHandler { get; set; }
        public ExecSingleCmdEventHandler ExecSingleCmdEventHandler { get; set; }

        //Sub Program
        public SubProgramGetListStatusEventHandler SubProgramGetListStatusEventHandler { get; set; }
        public SubProgramOpenEventHandler SubProgramOpenEventHandler { get; set; }
        public SubProgramSaveEventHandler SubProgramSaveEventHandler { get; set; }
        public SubProgramDeleteEventHandler SubProgramDeleteEventHandler { get; set; }
        public SubProgramCompileEventHandler SubProgramCompileEventHandler { get; set; }
        public SubProgramGetErrMsgEventHandler SubProgramGetErrMsgEventHandler { get; set; }


        //IO
        public LoadedDigitalInputInfoEventHandler LoadedDigitalInputInfoEventHandler { get; set; }
        public LoadedDigitalOutputInfoEventHandler LoadedDigitalOutputInfoEventHandler { get; set; }
        public LoadedAnalogInputInfoEventHandler LoadedAnalogInputInfoEventHandler { get; set; }
        public LoadedAnalogOutputInfoEventHandler LoadedAnalogOutputInfoEventHandler { get; set; }

        public DInGetAllPortValueEventHandler DInGetAllPortValueEventHandler { get; set; }
        public DOutGetAllPortValueEventHandler DOutGetAllPortValueEventHandler { get; set; }
        public AInGetAllChannelValueEventHandler AInGetAllChannelValueEventHandler { get; set; }
        public AOutGetAllChannelValueEventHandler AOutGetAllChannelValueEventHandler { get; set; }
        public AOutSetEventHandler AOutSetEventHandler { get; set; }

        //Scope

        //Variables

        //Log
        public LogGetLineNumEventHandler LogGetLineNumEventHandler { get; set; }
        public LogGetStrEventHandler LogGetStrEventHandler { get; set; }
        public LogClearEventHandler LogClearEventHandler { get; set; }

        //Config
        public SystemConfigGetRobotTemplateEventHandler SystemConfigGetRobotTemplateEventHandler { get; set; }
        public SystemConfigGetRobotTemplateFileEventHandler SystemConfigGetRobotTemplateFileEventHandler { get; set; }
        public SystemConfigGetEniFilesEventHandler SystemConfigGetEniFilesEventHandler { get; set; }

        //Motion Parameters
        public MotionParaGetJointEventHandler MotionParaGetJointEventHandler { get; set; }
        public MotionParaGetWorkEventHandler MotionParaGetWorkEventHandler { get; set; }

        //Variables
        public IVarGetAllEventHandler IVarGetAllEventHandler { get; set; }
        public DVarGetAllEventHandler DVarGetAllEventHandler { get; set; }
        public PosVarGetAllEventHandler PosVarGetAllEventHandler { get; set; }

        // For LMMC Testing
        public LGetCarrierInfoEventHandler LmmcGetCarrierInfoEventHandler { get; set; }
        public LGetShuttleInfoEventHandler LmmcGetShuttleInfoEventHandler { get; set; }

    }
}

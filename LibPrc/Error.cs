using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presto.PRC
{
    public enum Error
    {
        //Connect & Loging
        NONE = 0,
        CONNECT,
        DISCONNECT,
        LOGIN,
        LOGIN_CHANGE_PW,
        LOGIN_REQUIRED,

        //System
        ECAT_NOT_CONNECTED,

        ROBOT_ON,
        ROBOT_OFF,
        ROBOT_CLEAR_ERR,

        JOG,
        SET_JOG_VELP,

        //Program
        PROGRAM_OPEN,
        PROGRAM_SAVE,
        PROGRAM_COMPILE,
        PROGRAM_START,
        PROGRAM_STOP,
        PROGRAM_STOP_ALL,
        PROGRAM_PAUSE,
        PROGRAM_RESUME,

        //Sub-Program
        SUB_PROGRAM_OPEN,
        SUB_PROGRAM_SAVE,
        SUB_PROGRAM_DELETE,
        SUB_PROGRAM_COMPILE,       

        CONFIG_ROBOT_NUM,
        CONFIG_GET_ROBOT_TEMPLATE_FILE,

        XML_PARSING,

        //System
        OUT_OF_RANGE,
        NULL_REFERENCE,
    }
}

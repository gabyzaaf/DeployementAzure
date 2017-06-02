using System;
using System.IO;
using ConsoleApplication;
using core.configuration;
using CORE.LogManager;
using Serilog;

namespace exception.sql
{

    public class SqlCustomException:System.Exception{

        LogManager log = new LogManager(JsonConfiguration.conf.getLogPath());

        public SqlCustomException(string code,string message):base(message){            
                 
         log.Write("[Error]",this.GetType().Name+" - "+code+" - "+message);       
        }

    }

}
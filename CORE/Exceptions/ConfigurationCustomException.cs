using System.IO;
using core.configuration;
using CORE.LogManager;

namespace exception.configuration{

    public class ConfigurationCustomException : System.Exception{
        LogManager log = new LogManager(Directory.GetCurrentDirectory()+"/configuration.log");
        public ConfigurationCustomException():base(){
            log.Write("[Error]",$"{this.GetType().Name} - Le fichier appsettings.json n'est pas présent dans votre system ou est non conforme");   
        }

        public ConfigurationCustomException(string code,string message){
            LogManager log = new LogManager(JsonConfiguration.conf.getLogPath());
            log.Write("[Error]",$" {this.GetType().Name} - {code} - {message}");
        }

    }

}
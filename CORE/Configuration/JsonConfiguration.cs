using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using exception.configuration;
using Microsoft.Extensions.Configuration;

namespace core.configuration{
    /* 
    Tous les champs du fichier appsettings ne doivent pas etre vide 
    */
    public class JsonConfiguration{

        public static JsonConfiguration conf = null;
        private static string configurationLog = "logPath";
        private static string configurationPluginFolder="pluginPath";

        private static string configurationCode = "ErrorCode";

        private static string configurationEmail="email";

        private static string configurationSql="SQL";

        private static IConfigurationRoot configuration = null ;

        

        public static JsonConfiguration getInstance(){
             if(configuration==null || conf == null){
                return new JsonConfiguration();
             }
             return conf;
        }
        public JsonConfiguration(){
                try{
                     var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");
                    configuration = builder.Build();
                    conf = this;
                }catch(Exception){
                    throw new ConfigurationCustomException();
                }  
            }
            
        

        public string getLogPath(){
            settingsExist();
            if(String.IsNullOrEmpty(configuration[configurationLog])){
                throw new Exception("Ne contiens aucun fichier de configuration");
            }
            return configuration[configurationLog];
        }

        public string getPluginFolder(){
            settingsExist();
            if(String.IsNullOrEmpty(configuration[configurationPluginFolder])){
                throw new Exception("Ne contiens aucun dossier de plugin");
            }
            if(!Directory.Exists(configuration[configurationPluginFolder])){
                 throw new Exception("Le dossier n'existe pas veuillez creer le dossier de plugin");
            }
            return configuration[configurationPluginFolder];
        }
        
        public Dictionary<string,string> getErrors(string country){
            settingsExist();
            bool found = false;
            Dictionary<string,string> errorDico = new Dictionary<string,string>();
            if(String.IsNullOrEmpty(country)){
                throw new ConfigurationCustomException(this.GetType().Name,"Votre reference country est null");
            }
            if(String.IsNullOrEmpty(configuration.GetSection(configurationCode).Key)){
                throw new ConfigurationCustomException(this.GetType().Name,$"votre reference {configurationCode} n'existe pas dans votre fichier de appsettings");
            }
            IEnumerable<IConfigurationSection> liste =  configuration.GetSection(configurationCode).GetChildren();
            if(liste==null){
                throw new ConfigurationCustomException(this.GetType().Name,$"Votre appsettings ne contient aucun element");
            }
            foreach(IConfigurationSection element in liste){
                if(element.Key.Equals(country)){
                    found = true;
                    IEnumerable<IConfigurationSection> errors =  element.GetChildren();
                    if(errors==null){
                        throw new ConfigurationCustomException(this.GetType().Name,$"Vous n'avez pas les données d'erreur inseré dans votre fichier appsettings");
                    }
                    foreach(IConfigurationSection conf in errors){
                        if(!errorDico.ContainsKey(conf.Key)){
                            errorDico.Add(conf.Key,conf.Value);
                        }
                    }
                }
            }
            if(found==false){
                throw new ConfigurationCustomException(this.GetType().Name,$"Aucun pays n'a ete trouve selon vos criteres");
            }
            return errorDico;
        }
         
         public string getEmail(){
             try{
                settingsExist();
                if(String.IsNullOrEmpty(configuration[configurationEmail])){
                    throw new Exception("Vous ne possedez pas d'email dans votre fichier de configuration");
                }
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(configuration[configurationEmail]);
                if (!match.Success){
                    throw new Exception("Votre adresse email n'est pas conforme");
                }
                return configuration[configurationEmail];
             }catch(Exception exception){
                throw new ConfigurationCustomException(this.GetType().Name,$"{exception.Message}");
             }
         }
         
         public string getConnectionSql(){
            string connectionString = null;
            try{
                settingsExist();
                if(String.IsNullOrEmpty(configuration.GetSection(configurationSql).Key)){
                    throw new Exception("Votre fichier appsettings n'est pas conforme avec la partie SQL");
                }
                IEnumerable<IConfigurationSection> connectionsSql =  configuration.GetSection(configurationSql).GetChildren();
                if(connectionsSql==null){
                    throw new Exception("Aucune donnee de configuration est présente");
                }
                foreach(IConfigurationSection section in connectionsSql){
                    if(String.IsNullOrEmpty(section.Value)){
                        throw new Exception("Votre champs de connection est vide");
                    }
                    connectionString =  section.Value;
                }
                if(String.IsNullOrEmpty(connectionString)){
                    throw new Exception("Votre champs de connection est vide");
                }
                return connectionString;
            }catch(Exception exception){
                throw new ConfigurationCustomException(this.GetType().Name,$"{exception.Message}");
            }
         }
        private void settingsExist(){
            if(configuration==null){
                throw new ConfigurationCustomException();
            }
        }
        

    }


}
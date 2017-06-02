using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using core.configuration;
using core.webservice;

namespace core.plugin.engine{
    
    public class StartPluginController{
        private static Dictionary<String,dynamic> dico = new Dictionary<String,dynamic>();


        public void getJsonDll(){
            string[] files = getDll();
        }

        public string[] getDll(){ 
            try{
                return this.getConfAndLibrary("*.dll");
            }catch(Exception exc){
                throw exc;
            }
            
        }

        private string[] getConfAndLibrary(string extension){
             try{
                // a variabiliser
                string[] fileEntries = Directory.GetFiles(JsonConfiguration.getInstance().getPluginFolder(),extension);
                if(fileEntries ==null ||  fileEntries.Length==0){
                    throw new Exception("vous n'avez aucun fichier");
                }
                return fileEntries;
            }catch(Exception exc){
                throw exc;
            }
        }

        public string[] getJsonConf(){
            try{
                return this.getConfAndLibrary("*.json");
            }catch(Exception exc){
                throw exc;
            }
        }

        

        public void loadPlugin(){
            Console.WriteLine("In load plugin");
            if(!dico.ContainsKey("ContractController")){
                 var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath("/Users/zaafranigabriel/Documents/Training/v4/v4.dll");
                 var types = myAssembly.GetType("core.contract.ContractController");
                    try{
                        if(types.GetTypeInfo().GetInterface("Iwebservice")!=null){
                                    Console.WriteLine("OK");
                                    dico.Add("ContractController",Activator.CreateInstance(types));
                                    Console.WriteLine("load");
                        }else{
                                        Console.WriteLine("KO");
                                        throw new Exception("Is an error");
                        }
                    }catch(Exception exc){
                        Console.WriteLine(exc.Message);
                        throw exc;
                    }   
            }
            dico["ContractController"].GetById("2");
           
           
            
        }


    }

}
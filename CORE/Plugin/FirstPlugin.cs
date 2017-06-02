using System;
using CORE.plugin;

namespace CORE.PLUGIN{

    public class FirstPlugin : Iplugin
    {
        public string Name()
        {
            
            
                return "FirstPlugin";
            
        }

        public void Do()
        {
            Console.WriteLine("DO all ");
        }
    }


}
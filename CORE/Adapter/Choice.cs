using Core.Adapter.Inteface;

namespace Core.Adapter{

    
    public class ChoiceTypeDb{

        public IsqlMethod isql{get;set;}
        // variabiliser cette partie.
        public ChoiceTypeDb(string type){
            if("mysql".Equals(type)){
                isql = new MysqlDb();
            }
        }    

    }


}
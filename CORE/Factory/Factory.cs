using Core.Adapter;
using Core.Adapter.Inteface;

namespace Factory{

    public class Factory{

        public static IsqlMethod GetSQLInstance(string type){
            return new ChoiceTypeDb(type).isql;
        }

    }

}
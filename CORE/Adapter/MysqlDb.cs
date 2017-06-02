using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using core.candidat;
using core.configuration;
using core.report;
using core.success;
using core.user;
using Core.Adapter.Inteface;
using exception.sql;
using MySql.Data.MySqlClient;

namespace Core.Adapter{

    public class MysqlDb : IsqlMethod
    {
        
        private MySqlConnection connection = null;
        private void connect(){
            try{
            if(connection==null){
                
                connection  = new MySqlConnection
                {
                   ConnectionString = JsonConfiguration.getInstance().getConnectionSql()
                };
                connection.Open();
                
            }
            }catch(Exception exception){
                throw new SqlCustomException(this.GetType().Name,exception.Message);
            }
        } 

      
        private ArrayList queryExecute(string query,Dictionary<String,Object> dico,LinkedList<String> results){
            ArrayList listeHash= null;
            try{
            if(String.IsNullOrEmpty(query)){
                throw new Exception("La requete n'est pas conforme");
            }
            if(dico==null || dico.Count==0){
                throw new Exception("Le dictionnaire n'est pas conforme");
            }
            
            Dictionary<String,String> outPuts = null;
           
                connect();
                MySqlCommand command = new MySqlCommand(query,connection);
                foreach(var item in dico){
                    command.Parameters.AddWithValue(item.Key,item.Value);
                }
                if(results == null ){
                    command.ExecuteNonQuery();
                }else{
                    listeHash  = new ArrayList();
                    outPuts = new Dictionary<String,String>();
                    using (MySqlDataReader reader =  command.ExecuteReader()){
                       while(reader.Read()){  
                        foreach(string result in results){
                            outPuts.Add(result,reader[result].ToString());
                        }
                        listeHash.Add(outPuts);
                        outPuts = new Dictionary<String,String>();
                       }
                    }
                    
                }
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }finally{
                disconnect();
            }
            return listeHash;
        }

        public void Authentification(string email, string password)
        {
            
            try{
                if(String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password)){
                    throw new Exception("Email ou mot de passe incorrect");
                 }
                 Dictionary<String,Object> param = new Dictionary<String,Object>();
                 param.Add("@email",email);
                 param.Add("@pass",password);
                 LinkedList<String> element = new LinkedList<String>();
                 element.AddLast("nb");
                 ArrayList liste = queryExecute("SELECT count(*) as nb FROM user where email=@email and mdp=md5(@pass)",param,element);
                 Dictionary<String,String> dico = (Dictionary<String,String>)liste[0];
                 if(dico==null || dico.Count==0){
                     throw new Exception("Vos identifiants sont incorrect");
                 }
                  if(int.Parse(dico["nb"])==0){
                      throw new Exception("Email ou mot de passe incorrect");
                  }
            }catch(Exception exception){
                throw new SqlCustomException(this.GetType().Name,exception.Message);
            }
            
        }

        public void TokenExist(string token)
        {
           try{
                if(String.IsNullOrEmpty(token)){
                    throw new Exception("Le token est vide, veuillez vous reconnecter");
                }
                string sql = "select count(*) as nb from user where session_id=@session_id";
                Dictionary<String,Object> param = new Dictionary<String,Object>();
                param.Add("@session_id",token);
                LinkedList<string> listes = new LinkedList<string>();
                listes.AddLast("nb");
                ArrayList liste =queryExecute(sql,param,listes);
                if(liste.Count==0){
                    throw new Exception("Aucun elements n'a ete retourné");
                }
                Dictionary<String,String> dico = (Dictionary<String,String>)liste[0];
                if(dico==null || dico.Count==0){
                        throw new Exception("Aucun element n'a ete retourné veuillez vous connecter");
                }
                if(int.Parse(dico["nb"])>=1){
                        throw new Exception("Veuillez vous reconnecter");
                }
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }
        }

        

        public bool UserCanRead(string token)
        {
            try{
                if(String.IsNullOrEmpty(token)){
                    throw new Exception("Le token n'existe pas ");
                }
                Dictionary<String,Object> input = new Dictionary<String,Object>();
                input.Add("@token",token);
                LinkedList<String> result = new LinkedList<String>();
                result.AddLast("regle_lecture");
                ArrayList outputs = queryExecute("SELECT regle_lecture from user where session_id=@token",input,result);
                if(outputs.Count==0){
                    throw new Exception("Aucun token ayant ce numero "+token+" existe veuillez vous identifier");
                }
                Dictionary<String,String> element = (Dictionary<String,String>) outputs[0];
               
                if(!element.ContainsKey("regle_lecture")){
                    throw new Exception(" Votre token ne vous permet pas de lire");
                }
                if(!bool.Parse(element["regle_lecture"])){
                    throw new Exception(" Vous n'avez pas les droits necessaire pour effectuer un ajout ou une modification sur un candidat");
                }
                return Boolean.Parse(element["regle_lecture"]);
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }
           
            
            
        }

        public bool UserCanUpdate(string token)
        {
            Dictionary<String,String> element;
            try{
                if(String.IsNullOrEmpty(token)){
                    throw new Exception("Le token n'existe pas ");
                }
                Dictionary<String,Object> input = new Dictionary<String,Object>();
                input.Add("@token",token);
                LinkedList<String> results = new LinkedList<String>();
                results.AddLast("regle_modification");
                ArrayList dicos =  queryExecute("SELECT regle_modification from user where session_id=@token",input,results);
                if(dicos.Count==0){
                        throw new Exception("Aucun token ayant ce numero "+token+" existe veuillez vous identifier");
                }
                element = (Dictionary<String,String>) dicos[0];
                if(!element.ContainsKey("regle_modification")){
                    throw new Exception("Votre token ne vous permet pas de modifier");
                }
                if(!bool.Parse(element["regle_modification"])){
                    throw new Exception(" Vous n'avez pas les droits necessaire pour effectuer un ajout ou une modification sur un candidat");
                }
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }
            return bool.Parse(element["regle_modification"]);
        }

       
        public User GenerateToken()
        {
            Random rnd = new Random();
            int nb = rnd.Next(1,100);
            return new User(){sessionId=nb.ToString()};
        }

        public void addTokenToUser(string token,string email)
        {
              try{
                if(String.IsNullOrEmpty(token)){
                    throw new Exception("Le token est vide");
                }
                if(String.IsNullOrEmpty(email)){
                    throw new Exception("L'email est vide");
                 }     
                Dictionary<String,Object> input = new Dictionary<String,Object>();
                input.Add("@session",token);
                input.Add("@email",email);
                queryExecute("Update user set session_id=@session where email=@email",input,null);
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }
            
            
        }

        private void disconnect(){
                if(connection!=null){
                    connection.Close();
                    connection = null;  
                }
                
        }

        public ArrayList searchCandidate(string nom, string token)
        {
            ArrayList output=null;
            try{
                if(String.IsNullOrEmpty(nom)){
                    throw new Exception("Le nom du candidat est vide");
                }
                Dictionary<String,Object> dico = new Dictionary<String,Object>();
                dico.Add("@nom",nom);
                LinkedList<String> results = new LinkedList<String>(); 
                results.AddLast("nom");
                results.AddLast("prenom");
                results.AddLast("sexe");
                results.AddLast("phone");
                results.AddLast("actions");
                results.AddLast("annee");
                results.AddLast("lien");
                results.AddLast("crCall");
                results.AddLast("NS");
                results.AddLast("approche_email");
                results.AddLast("note");
                results.AddLast("link");
                results.AddLast("xpNote");
                results.AddLast("nsNote");
                results.AddLast("jobIdealNote");
                results.AddLast("pisteNote");
                results.AddLast("pieCouteNote");
                results.AddLast("locationNote");
                results.AddLast("EnglishNote");
                results.AddLast("nationalityNote");
                results.AddLast("competences");
                output = queryExecute("SELECT * from candidate,meeting where candidate.id = meeting.fid_candidate_meeting and nom=@nom",dico,results);
                if(output == null || output.Count==0){
                    throw new Exception("Aucun candidat ne possede vos criteres de recherche");
                }
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }
            return output;
        }

        public bool verifyTheTokenExist(string token)
        {
            try{
            if(String.IsNullOrEmpty(token)){
                    throw new Exception("Le token n'existe pas");
                }
                Dictionary<String,Object> dico = new Dictionary<String,Object>();
                dico.Add("@session_id",token);
                LinkedList<String> returns = new LinkedList<String>();
                returns.AddLast("nb");
                ArrayList output= queryExecute("SELECT count(*) as nb from user where session_id=@session_id",dico,returns);
                Dictionary<String,String> element = (Dictionary<String,String>) output[0];
                if(!element.ContainsKey("nb")){
                    throw new Exception("Aucun utilisateur ne possede ce token");
                }
                if(int.Parse(element["nb"])==0){
                    return false;
                }
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }
            return true;
        }

        public void addCandidate(Candidat candidat,int id)
        {
            if(candidat==null){
                throw new Exception(" Vous devez renseigner les informations du candidat");
            }
            if(id == 0){
                throw new Exception("Vous devez avoir un id  valide");
            }
            Dictionary<String,Object> dico = new Dictionary<String,Object>();
            try{
                dico.Add("@nom",candidat.Name);
                dico.Add("@prenom",candidat.Firstname);
                dico.Add("@num",candidat.phone);
                dico.Add("@emailAdress",candidat.emailAdress);
                dico.Add("@sexe",candidat.sexe);
                dico.Add("@etat",candidat.action);
                dico.Add("@annee",candidat.year);
                dico.Add("@lien",candidat.link);
                dico.Add("@crcall",candidat.crCall);
                dico.Add("@ns",candidat.ns);
                dico.Add("@email",candidat.email);
                dico.Add("@userId",id);
                queryExecute("insert into candidate (nom,prenom,phone,email,sexe,actions,annee,lien,crCall,NS,approche_email,fid_user_candidate) values (@nom,@prenom,@num,@emailAdress,@sexe,@etat,@annee,@lien,@crcall,@ns,@email,@userId)",dico,null);
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }
          
        }

       
        public int getIdFromToken(string token)
        {
            Dictionary<String,String> element;
            try{
                if(String.IsNullOrEmpty(token)){
                    throw new Exception(" Votre token n'est pas valide");
                }
                Dictionary<String,Object> param = new Dictionary<String,Object>();
                param.Add("@session_id",token);
                LinkedList<String> returnValue = new LinkedList<String>();
                returnValue.AddLast("id");
                ArrayList output = queryExecute("SELECT id from user where session_id=@session_id",param,returnValue);
                if(output == null || output.Count==0){
                    throw new Exception("Votre token de session n'est pas conforme");
                }
                if(output.Count>1){
                    throw new Exception(" Votre token a ete usurpé, veuillez vous reconnecter ou contacter l'administrateur ");
                }
                element = (Dictionary<String,String>)output[0];
                if(!element.ContainsKey("id")){
                    throw new Exception(" - Votre session id est incorrect");
                }
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }
            return int.Parse(element["id"]);
            
        }


    
        public bool CandidatAlreadyExist(Candidat candidat)
        {
            try{
            if(candidat == null){
                throw new Exception("Vos Parametres ne sont pas conforme");
            }
            if(String.IsNullOrEmpty(candidat.emailAdress)){
                throw new Exception("L'adresse email saisit du candidat n'est pas conforme");
            }
            
            Dictionary<String,Object> param = new Dictionary<String,Object>();
            param.Add("@email",candidat.emailAdress);
            LinkedList<String> result = new LinkedList<String>();
            result.AddLast("nb");
            ArrayList output =queryExecute("SELECT count(*) as nb from candidate where email=@email",param,result);
            if(output.Count==0){
                return false;
            }
            Dictionary<String,String> element = (Dictionary<String,String>) output[0];
            if(!element.ContainsKey("nb")){
                return false;
            }
            if(int.Parse(element["nb"])==0){
                return false;
            }
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }
            return true;
        }

        private void checkPrice(int prix){
            if(prix <= 0){
                throw new Exception("Le prix saisit n'est pas conforme");
            }
        }
        public void addFreeLance(int prix,int id)
        {
           
            checkPrice(prix);
            checkId(id);
            try{
                Dictionary<String,Object> param = new Dictionary<String,Object>();
                param.Add("@value",prix);
                param.Add("@fid",id);
                queryExecute("insert into internNumeric (contentType,fid_candidate_internNumeric) values (@value,@fid)",param,null);
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }
            
        }




        public int getIdFromCandidateEmail(string email)
        {
            try{
                if(String.IsNullOrEmpty(email)){
                    throw new Exception("L'email n'est pas conforme");
                }
                Dictionary<String,Object> dico = new Dictionary<String,Object>();
                dico.Add("@email",email);
                LinkedList<String> liste = new LinkedList<String>();
                liste.AddLast("id");
                ArrayList output = queryExecute("SELECT id  from candidate where email=@email",dico,liste);
                if(output == null || output.Count==0){
                    throw new Exception("Aucun candidat ne possède cet email");
                }
                if(output.Count>1){
                    throw new Exception("Votre candidat est présent plusieurs fois dans la base de donnée ");
                }
                Dictionary<String,String> element = (Dictionary<String,String>)output[0];
                if(!element.ContainsKey("id")){
                     throw new Exception("Le candidat n'est pas présent dans votre application");
                }
                return int.Parse(element["id"]); 
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);;
            }
        }

        private void checkId(int id){
            if(id<=0){
                throw new Exception("L'id n'est pas conforme");
            }
        }

        private void checkDate(DateTime date){
            if(date == null){
                throw new Exception("La date mis en paramtre n'est pas conforme");
            }
            if(date.Date == DateTime.Now.Date){
                 throw new Exception(this.GetType().Name+" La date ne peut pas etre identique a la date d'aujourd'hui");
            } 
        }
        public void remindType(int id,DateTime date)
        {

            try{
                 checkId(id);
                 checkDate(date);
                 Dictionary<String,Object> param = new Dictionary<String,Object>();
                 param.Add("@date",date.Date);
                 param.Add("@fid",id);
                 param.Add("@finish",false);
                 queryExecute("insert into remind (dates,fid_candidate_remind,finish) values (@date,@fid,@finish)",param,null);
            }catch(Exception exception){
                throw new SqlCustomException(this.GetType().Name,exception.Message);
            }finally{
                disconnect();
            }
        
        }

        public void updateRemindType(int id,DateTime date){
            try{
                checkId(id);
                checkDate(date);
                
                Dictionary<String,Object> param = new Dictionary<String,Object>();
                param.Add("@date",date.Date);
                param.Add("@fid",id);
                queryExecute("update remind set dates=@date where fid_candidate_remind=@fid",param,null);
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }finally{
                disconnect();
            }
        }
        public void updateFreeLance(int prix,int id){
            try{
                checkId(id);
                checkPrice(prix);
                
                Dictionary<String,Object> param = new Dictionary<String,Object>();
                param.Add("@price",prix);
                param.Add("@id",id);
                queryExecute("update internNumeric set contentType=@price where fid_candidate_internNumeric=@id",param,null);
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }finally{
                disconnect();
            }
        }


        public void typeAction(string actionType,int prix,DateTime date,int id,string type){
          
            switch (actionType)
            {
                case "freelance":
                // Ajoute dans la table internNumeric
                    if("ADD".Equals(type)){
                        addFreeLance(prix,id);
                    }else if("UPDATE".Equals(type)){
                        updateFreeLance(prix,id);
                    }      
                    break;
                case "enCours":
                // remind toutes les 2 semaines
                    if("ADD".Equals(type)){
                        remindType(id,date.AddDays(2*7));
                    }else if("UPDATE".Equals(type)){
                        updateRemindType(id,date.AddYears(1));
                    }
                    
                    break;
                case "appellerRemind":
                 // remind tous les jous à 18h
                    if("ADD".Equals(type)){
                        remindType(id,date.AddDays(1));
                    }else if("UPDATE".Equals(type)){
                            updateRemindType(id,date.AddYears(1));
                    }
                   
                    break;
                case "aRelancerLKD":
                    // remind tous les 2 jours
                    if("ADD".Equals(type)){
                        remindType(id,date.AddDays(2));
                    }else if("UPDATE".Equals(type)){
                            updateRemindType(id,date.AddYears(1));
                    }
                    break;
                case "aRelancerMail":
                    // remind tous les 2 jours
                     if("ADD".Equals(type)){
                        remindType(id,date.AddDays(2));
                    }else if("UPDATE".Equals(type)){
                        updateRemindType(id,date.AddYears(1));
                    }
                    break;
                case "PAERemind":
                    // remind tous les 6 mois
                     if("ADD".Equals(type)){
                        remindType(id,date.AddMonths(6));
                    }else if("UPDATE".Equals(type)){
                        remindType(id,date.AddMonths(6));
                    }
                    
                    break;
                case "HcLangue":
                    // remind tous les 6 mois
                     if("ADD".Equals(type)){
                        remindType(id,date.AddMonths(6));
                    }else if("UPDATE".Equals(type)){
                        updateRemindType(id,date.AddMonths(6));
                    }
                    break;
                case "HCGeo":
                    // remind tous les 6 mois
                    if("ADD".Equals(type)){
                        remindType(id,date.AddMonths(6));
                    }else if("UPDATE".Equals(type)){
                        updateRemindType(id,date.AddMonths(6));
                    }
                    break;
            }
        }

            private void checkCandidateExist(Candidat candidat){
                if(candidat==null){
                    throw new Exception("Veuillez renseigner tous les champs de votre candidat");
                }
                if(String.IsNullOrEmpty(candidat.session_id)){
                    // loguer l'information
                    throw new Exception("Vous n'avez pas les droits associe a la modification du candidat");
                }
            }
            /**
            1) Verifier si le candidat est pas null et que la session_id est renseigne.
            2) Verifier si l'id de l'utilisateur existe (int id)
            3) Creer la connexion (methode connect())
            4) Faire la modification.
            5) Sauvegarder qui a fait la modification (sauvegarder l'id dans une table modification avec l'id du candidat)
            6) Fermer la connexion
             */
            public void updateCandidate(Candidat candidat,int id){
                try{
                    
                    checkCandidateExist(candidat);
                    checkId(id);
                    Dictionary<String,Object> param = new Dictionary<String,Object>();
                    param.Add("@nom",candidat.Name);
                    param.Add("@prenom",candidat.Firstname);
                    param.Add("@phone",candidat.phone);
                    param.Add("@sexe",candidat.sexe);
                    param.Add("@actions",candidat.action);
                    param.Add("@annee",candidat.year);
                    param.Add("@lien",candidat.link);
                    param.Add("@crCall",candidat.crCall);
                    param.Add("@ns",candidat.ns);
                    param.Add("@approcheemail",candidat.email);
                    param.Add("@email",candidat.emailAdress);
                    queryExecute("update candidate set nom=@nom,prenom=@prenom,phone=@phone,sexe=@sexe,actions=@actions,annee=@annee,lien=@lien,crCall=@crCall,NS=@ns,approche_email=@approcheemail where email=@email",param,null);
                
                }catch(Exception exc){
                    throw new SqlCustomException(this.GetType().Name,exc.Message);
                }finally{
                    disconnect();
                }
            }

        public ArrayList searchCandidateMobile(string nom, string token)
        {
            //
            ArrayList output=null;
            try{
                if(String.IsNullOrEmpty(nom)){
                    throw new Exception("Le nom du candidat est vide");
                }
                Dictionary<String,Object> dico = new Dictionary<String,Object>();
                dico.Add("@nom",nom);
                LinkedList<String> results = new LinkedList<String>();
                results.AddLast("id");
                results.AddLast("nom");
                results.AddLast("prenom");
                output = queryExecute("SELECT candidate.id,nom,prenom from candidate,meeting where candidate.id = meeting.fid_candidate_meeting and nom=@nom",dico,results);
                if(output == null || output.Count==0){
                    throw new Exception("Aucun candidat ne possede vos criteres de recherche");
                }
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }
            return output;
        }

        public bool reportAlreadyExist(int idCandidat)
        {
            try{
                if(idCandidat <= 0){
                    throw new Exception("L'identifiant du candidat n'est pas valable");
                }
                string sql = "select count(*) as nb from meeting where fid_candidate_meeting=@fid";
                LinkedList<String> results = new LinkedList<String>();
                results.AddLast("nb");
                Dictionary<String,Object> dico = new Dictionary<String,Object>();
                dico.Add("@fid",idCandidat);
                ArrayList listeReturn = queryExecute(sql,dico,results);
                if(listeReturn == null || listeReturn.Count==0){
                    throw new Exception("Aucun element n'est retourné est lié au report");
                }
                Dictionary<String,String> returnData = (Dictionary<String,String>)listeReturn[0];
                return Convert.ToBoolean(int.Parse(returnData["nb"]));
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }
        }

        private void checkReport(Report report){
            if(report==null){
                throw new Exception("Veuillez tous les champs de votre report");
            }
            if(String.IsNullOrEmpty(report.sessionId)){
                throw new Exception("Vous n'avez pas les droits necessaire pour ajouter ou modifier un report");
            }
        }

        public void addReport(Report report,int idCandidat)
        {
         try{
            
            checkReport(report);
            string sql = "insert into meeting (note,link,xpNote,nsNote,jobIdealNote,pisteNote,pieCouteNote,locationNote,EnglishNote,nationalityNote,competences,fid_candidate_meeting) values (@note,@link,@xpNote,@nsNote,@jobIdealNote,@pisteNote,@pieCouteNote,@locationNote,@EnglishNote,@nationalityNote,@competences,@fid_candidate_meeting)";
            Dictionary<String,Object> dico = new Dictionary<String,Object>();
            dico.Add("@note",report.note);
            dico.Add("@link",report.link);
            dico.Add("@xpNote",report.xpNote);
             dico.Add("@nsNote",report.nsNote);
             dico.Add("@jobIdealNote",report.jobIdealNote);
            dico.Add("@pisteNote",report.pisteNote);
            dico.Add("@pieCouteNote",report.pieCouteNote);
            dico.Add("@locationNote",report.locationNote);
            dico.Add("@EnglishNote",report.EnglishNote);
            dico.Add("@nationalityNote",report.nationalityNote);
            dico.Add("@competences",report.competences);
            dico.Add("@fid_candidate_meeting",idCandidat);
            queryExecute(sql,dico,null);
         }catch(Exception exc){
            throw new SqlCustomException(this.GetType().Name,exc.Message);
         }
        }

        public void updateReport(Report report, int idCandidat)
        {
           try{ 
           checkReport(report);
           string sqlUpdate = "update  meeting set note =@note,link=@link,xpNote=@xpNote,nsNote=@nsNote,jobIdealNote=@jobIdealNote,pisteNote=@pisteNote,pieCouteNote=@pieCouteNote,locationNote=@locationNote,EnglishNote=@EnglishNote,nationalityNote=@nationalityNote,competences=@competences where fid_candidate_meeting=@fid_candidate_meeting";
           Dictionary<String,Object> dico = new Dictionary<String,Object>();
           dico.Add("@note",report.note);
           dico.Add("@link",report.link);
           dico.Add("@xpNote",report.xpNote);
           dico.Add("@nsNote",report.nsNote);
           dico.Add("@jobIdealNote",report.jobIdealNote);
           dico.Add("@pisteNote",report.pisteNote);
           dico.Add("@pieCouteNote",report.pieCouteNote);
           dico.Add("@locationNote",report.locationNote);
           dico.Add("@EnglishNote",report.EnglishNote);
           dico.Add("@nationalityNote",report.nationalityNote);
           dico.Add("@competences",report.competences);
           dico.Add("@fid_candidate_meeting",idCandidat);
           queryExecute(sqlUpdate,dico,null);
           }catch(Exception exc){
            throw new SqlCustomException(this.GetType().Name,exc.Message);
           }
        }

        private void checkEmail(string email){
            try{
                if(String.IsNullOrEmpty(email)){
                    throw new Exception("Votre champ email est vide ");
                }
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(email);
                if(!match.Success){
                    throw new Exception("L'email n'est pas conforme");
                }
            }catch(Exception exc){
                throw exc;
            }
            

        }

         public ArrayList searchCandidateFromEmail(string email, string token)
        {
            ArrayList output;
            try{
                checkEmail(email);
                if(String.IsNullOrEmpty(token)){
                    throw new Exception("Le token du candidat est vide");
                }
                string sql = "SELECT * from candidate,meeting where candidate.id = meeting.fid_candidate_meeting and candidate.email=@email";
                Dictionary<String,Object> dico = new Dictionary<String,Object>();
                dico.Add("@email",email);
                LinkedList<String> results = new LinkedList<String>(); 
                results.AddLast("nom");
                results.AddLast("prenom");
                results.AddLast("sexe");
                results.AddLast("phone");
                results.AddLast("email");
                results.AddLast("actions");
                results.AddLast("annee");
                results.AddLast("lien");
                results.AddLast("crCall");
                results.AddLast("NS");
                results.AddLast("approche_email");
                results.AddLast("note");
                results.AddLast("link");
                results.AddLast("xpNote");
                results.AddLast("nsNote");
                results.AddLast("jobIdealNote");
                results.AddLast("pisteNote");
                results.AddLast("pieCouteNote");
                results.AddLast("locationNote");
                results.AddLast("EnglishNote");
                results.AddLast("nationalityNote");
                results.AddLast("competences");
                output = queryExecute(sql,dico,results);
                if(output == null || output.Count==0){
                    throw new Exception("Aucun candidat ne possede vos criteres de recherche");
                }
            }catch(Exception exc){
                throw new SqlCustomException(this.GetType().Name,exc.Message);
            }
            return output;
        }
    }

}
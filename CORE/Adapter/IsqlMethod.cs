using System;
using System.Collections;
using System.Collections.Generic;
using core.candidat;
using core.report;
using core.success;
using core.user;

namespace Core.Adapter.Inteface
{

    public interface IsqlMethod{
        void TokenExist(string token);
         bool UserCanRead(string token);
         
         bool UserCanUpdate(string token);

         void Authentification(string email,string password);

    
         ArrayList searchCandidate(string nom,string token);

         User GenerateToken();

         void addTokenToUser(string token,string email);

         bool verifyTheTokenExist(string token);

         void addCandidate(Candidat candidat,int id);

         int getIdFromToken(string token);

         bool CandidatAlreadyExist(Candidat candidat);

         void addFreeLance(int prix,int id);

         int getIdFromCandidateEmail(string email);

         

         void remindType(int id,DateTime date);

         void typeAction(string actionType,int prix,DateTime date,int id,string type);

         void updateCandidate(Candidat candidat,int id);

          ArrayList searchCandidateMobile(string nom,string token);

          bool reportAlreadyExist(int idCandidat);

          void addReport(Report report,int idCandidat);

          void updateReport(Report report,int idCandidat);

          ArrayList searchCandidateFromEmail(string email,string token);
    }

} 
//using System;
//using System.Collections.Generic;
//using Verse;

//namespace RimVore2
//{
//    public static class DescriptionUtility
//    {
//        public static string ExpandPlaceholders(this string originalString, Pawn pawn)
//        {
//            PawnGrammaticalCases cases = new PawnGrammaticalCases(pawn);
//            return ReplacePlaceholders(originalString, cases);
//        }

//        public static string ExpandPlaceholders(this string originalString, string name, Gender gender)
//        {
//            PawnGrammaticalCases cases = new PawnGrammaticalCases(name, gender);
//            return ReplacePlaceholders(originalString, cases);
//        }

//        private static string ReplacePlaceholders(string originalString, PawnGrammaticalCases cases)
//        {
//            List<Func<string, string>> replacements = new List<Func<string, string>>()
//                {
//                    s => s.Replace("$pName", cases.pName),
//                    s => s.Replace("$pNom", cases.pNom),
//                    s => s.Replace("$pAcc", cases.pAcc),
//                    s => s.Replace("$pPos", cases.pPos),
//                    s => s.Replace("$pGen", cases.pGen),
//                    s => s.Replace("$pRef", cases.pRef),
//                    s => s.Replace("$PName", cases.pName.CapitalizeFirst()),
//                    s => s.Replace("$PNom", cases.pNom.CapitalizeFirst()),
//                    s => s.Replace("$PAcc", cases.pAcc.CapitalizeFirst()),
//                    s => s.Replace("$PPos", cases.pPos.CapitalizeFirst()),
//                    s => s.Replace("$PGen", cases.pGen.CapitalizeFirst()),
//                    s => s.Replace("$PRef", cases.pRef.CapitalizeFirst())
//                };
//            foreach (Func<string, string> replacement in replacements)
//            {
//                originalString = replacement(originalString);
//            }
//            return originalString;
//        }
//    }

//    public class PawnGrammaticalCases
//    {
//        public string pName;
//        public string pNom;
//        public string pAcc;
//        public string pPos;
//        public string pGen;
//        public string pRef;

//        public PawnGrammaticalCases(string name, Gender gender)
//        {
//            pName = name;
//            LoadPronouns(gender);
//        }

//        public PawnGrammaticalCases(Pawn pawn)
//        {
//            //pName = pawn.Name.ToStringShort;
//            pName = pawn.LabelShort;
//            if(pawn.RaceProps?.Animal == true)
//            {
//                // animal has no nickname yet, we consider wild animals to be referred to in the third person
//                if(pawn.Name == null)
//                {
//                    pName = "RV2_Pronoun_Unnamed".Translate(pName);
//                    LoadPronouns(Gender.None);
//                    return;
//                }
//                // but if the animal has a nickname, refer to them by their gender
//            }
//            LoadPronouns(pawn.gender);
//        }

//        private void LoadPronouns(Gender gender)
//        {
//            switch (gender)
//            {
//                case Gender.Male:
//                    pNom = "RV2_Pronoun_nomMale".Translate();
//                    pAcc = "RV2_Pronoun_accMale".Translate();
//                    pPos = "RV2_Pronoun_posMale".Translate();
//                    pGen = "RV2_Pronoun_genMale".Translate();
//                    pRef = "RV2_Pronoun_refMale".Translate();
//                    break;
//                case Gender.Female:
//                    pNom = "RV2_Pronoun_nomFemale".Translate();
//                    pAcc = "RV2_Pronoun_accFemale".Translate();
//                    pPos = "RV2_Pronoun_posFemale".Translate();
//                    pGen = "RV2_Pronoun_genFemale".Translate();
//                    pRef = "RV2_Pronoun_refFemale".Translate();
//                    break;
//                default:
//                    pNom = "RV2_Pronoun_nomObject".Translate();
//                    pAcc = "RV2_Pronoun_accObject".Translate();
//                    pPos = "RV2_Pronoun_posObject".Translate();
//                    pGen = "RV2_Pronoun_genObject".Translate();
//                    pRef = "RV2_Pronoun_refObject".Translate();
//                    /*pNom = "RV2_Pronoun_nomNeutral".Translate();
//                    pAcc = "RV2_Pronoun_accNeutral".Translate();
//                    pPos = "RV2_Pronoun_posNeutral".Translate();
//                    pGen = "RV2_Pronoun_genNeutral".Translate();
//                    pRef = "RV2_Pronoun_refNeutral".Translate();*/
//                    break;
//            }
//        }
//    }
//}

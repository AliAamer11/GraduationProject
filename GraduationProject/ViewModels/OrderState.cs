using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels
{
    public class OrderState
    {
        public static string RequestingParty = "0";//Requesting Party
        public static string VicePrisdent = "1";//VicePrisdent
        public static string NeedOutPutDocmnet = "2";//Storekeeper
        public static string BeingReview = "3";//this for Aya i dont know why
        public static string Finishid = "4";// the order is done so it was created for it OutputDocument
        public static string QuantitiesDistributed = "5";
    }
}

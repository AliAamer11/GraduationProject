using GraduationProject.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.OutPutDocument
{
    public class OutPutDocumentViewModel
    {
        public int OutPutDocumentID { get; set; }
        public int OrderId { get; set; }
        public bool OrderType { get; set; }
        public string RequestingParty { get; set; }
        public string CommissaryName { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}

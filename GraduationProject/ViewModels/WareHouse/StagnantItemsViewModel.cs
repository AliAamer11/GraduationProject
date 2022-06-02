using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.WareHouse
{
    public class StagnantItemsViewModel
    {
        public int ItemID { get; set; }
        public string Name { get; set; }
        public DateTime InputDocumentDate { get; set; }
        public DateTime OutPutDocumentDate { get; set; }
    }
}

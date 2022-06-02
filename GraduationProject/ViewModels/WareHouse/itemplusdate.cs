using System;
using System.Collections.Generic;
using System.Linq;
using GraduationProject.Data.Models;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.WareHouse
{
    public class itemplusdate
    {
        public Data.Models.Items Item { get; set; }

        public string Name { get; set; }
        public DateTime OutPutDocumentDate { get; set; }
    }
}

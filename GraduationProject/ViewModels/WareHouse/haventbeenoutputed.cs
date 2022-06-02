using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.WareHouse
{
    public class haventbeenoutputed
    {
        public Data.Models.Order Order { get; set; }
        public Data.Models.Items Item { get; set; }

        List <Data.Models.Items> items { set; get; }
        public string Name { get; set; }
        public DateTime OutPutDocumentDate { get; set; }
        public TimeSpan Span { get; set; }
    }
}

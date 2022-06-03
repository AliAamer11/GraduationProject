using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.WareHouse
{
    public class materialreportsViewModel
    {
        public int ItemID { get; set; }

        public string Name { get; set; }
        public string Barcode { get; set; }
        public string FlowRate { get; set; }
        public int InputQuantity { get; set; }
        public int OutputQuantity { get; set; }
        public int InStock { get; set; }


    }
}

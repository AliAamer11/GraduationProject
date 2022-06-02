using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels
{
    public class ItemsDatagridViewModel
    {
        public int ItemID { get; set; }
        public string BarCode { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public int MinimumRange { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public string Category { get; set; }
        public string Measurement { get; set; }
    }
}

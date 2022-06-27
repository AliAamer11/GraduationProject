using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Data.Models;

namespace GraduationProject.ViewModels.WareHouse
{
    public class RPitemsReportViewModel
    {
        public Data.Models.Items Item { get; set; }
        public string itemname { get; set; }
        public int requested_quantity { get; set; }
        public int taken_quantity { get; set; }

    }
}

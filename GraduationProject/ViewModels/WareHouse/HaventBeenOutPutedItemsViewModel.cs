using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.Warehouse
{
    public class HaventBeenOutPutedItemsViewModel
    {
        public int outputdocumentid { get; set; }
        public Data.Models.Items item { get; set; }
        public int recentQuantity { get; set; }
        public int Quantity { get; set; }
        public DateTime Createdat { get; set; }
    }
}

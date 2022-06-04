using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.Warehouse
{
    public class HaventBeenOutPutedItemsViewModel
    {
        public Data.Models.Items item { get; set; }
        public int recentQuantity { get; set; }
        public DateTime Createdat { get; set; }
    }
}

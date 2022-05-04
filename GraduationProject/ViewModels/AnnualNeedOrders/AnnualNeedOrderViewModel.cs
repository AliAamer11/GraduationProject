using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.AnnualNeedOrders
{
    public class AnnualNeedOrderViewModel
    {

        public Data.Models.Items Item { get; set; }
        public int ItemId { get; set; }
        public int AnnualOrderID { get; set; }
        public int FirstSemQuantity { get; set; }
        public int SecondSemQuantity { get; set; }
        public int ThirdSemQuantity { get; set; }
        public int FlowRate { get; set; }
        public int ApproxRate { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public int OrderId { get; set; }
        public bool IsSelected { get; set; }

    }
}

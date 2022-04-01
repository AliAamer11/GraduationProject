using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.AnnualNeedOrders
{
    public class CreateAnnualNeedOrderViewModel
    {
        public Data.Models.Items Item { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int ItemId { get; set; }

        public int AnnualOrderID { get; set; }
        public int FirstSemQuantity { get; set; }
        public int SecondSemQuantity { get; set; }
        public int ThirdSemQuantity { get; set; }
        public int FlowRate { get; set; }
        public int ApproxRate { get; set; }
        public string Comment { get; set; }
        public int OrderId { get; set; }
    }
}


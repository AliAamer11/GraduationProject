using GraduationProject.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.AnnualNeedOrders
{
    public class EditAnnualNeedOrderViewModel
    {
        public Data.Models.Items Item { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int ItemId { get; set; }
        public int AnnualOrderID { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "القيمة ليست ضمن المجال المحدد")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int FirstSemQuantity { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "القيمة ليست ضمن المجال المحدد")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int SecondSemQuantity { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "القيمة ليست ضمن المجال المحدد")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int ThirdSemQuantity { get; set; }
        public int FlowRate { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public int ApproxRate { get; set; }
        public Order Order { get; set; }

        public int OrderId { get; set; }
    }
}

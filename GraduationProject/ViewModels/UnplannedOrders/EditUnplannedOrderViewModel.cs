using GraduationProject.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.UnplannedOrders
{
    public class EditUnplannedOrderViewModel
    {
        public int UnPlannedOrderID { get; set; }

        public Data.Models.Items Item { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int ItemId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "القيمة ليست ضمن المجال المحدد")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int Quantity { set; get; }

        public string Description { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public string Reason { get; set; }
        public string Comment { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

    }
}

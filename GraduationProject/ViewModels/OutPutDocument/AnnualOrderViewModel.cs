using GraduationProject.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.OutPutDocument
{
    public class AnnualOrderViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [StringLength(20, ErrorMessage = "اسم المندوب طويل للغاية")]
        public string CommisaryName { get; set; }

        public List<AnnualOrder> AnnualOrders { get; set; }

        public List<OutPutDocumnetForAnnualViewModel> QuantityModel { get; set; }
        public AnnualOrderViewModel()
        {
            AnnualOrders = new List<AnnualOrder>();
            QuantityModel = new List<OutPutDocumnetForAnnualViewModel>();
        }


    }
}

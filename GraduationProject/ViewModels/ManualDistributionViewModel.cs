using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels
{
    public class ManualDistributionViewModel
    {
        public string ItemName { get; set; }
        public int AnnualOrderID { get; set; }
        public int FirstSemQuantity { get; set; }
        public int SecondSemQuantity { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "القيمة ليست ضمن المجال المحدد")]
        public int ThirdSemQuantity { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "القيمة ليست ضمن المجال المحدد")]
        public int TotalQuantity { get; set; }
    }
}

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
        [Range(1, int.MaxValue, ErrorMessage = "فضلًا أدخل قيمة موجبة")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int FirstSemQuantity { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "فضلًا أدخل قيمة موجبة")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int SecondSemQuantity { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "فضلًا أدخل قيمة موجبة")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int ThirdSemQuantity { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "فضلًا أدخل قيمة موجبة")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public int TotalQuantity { get; set; }
        public string comment { get; set; }
    }
}

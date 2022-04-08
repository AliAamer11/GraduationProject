using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.Items
{
    public class CreateItemViewModel
    {
        [Required(ErrorMessage ="هذا الحقل مطلوب")]
        [Display(Name = "اسم المادة")]
        [StringLength(30)]
        public string Name { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [Display(Name = "حالة المادة")]
        public string Status { get; set; }

        //[Required(ErrorMessage = "هذا الحقل مطلوب")]
        //[Display(Name = "كمية المادة")]
        //public int Quantity { get; set; }

        [StringLength(20)]
        [Display(Name = "العلامة")]
        public string Brand { get; set; }

        [Required(ErrorMessage ="هذا الحقل مطلوب")]
        [Display(Name = "الحد الأدنى")]
        [Range(1, 1000, ErrorMessage = "أدخل العدد بشكله الصحيح")]
        public int MinimumRange { get; set; }

        [Display(Name = "ملاحظات")]
        public string Note { get; set; }

        [Display(Name = "الصنف")]
        public int CategoryId { get; set; }

        [Display(Name = "وحدة القياس")]
        public int MeasurementId { get; set; }


    }
}

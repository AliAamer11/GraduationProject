using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.Administration
{
    public class CreateUserViewModel
    {
        public string UserID { get; set; }

        [Required(ErrorMessage ="هذا الحقل مطلوب ")]
        public string RequestingParty { get; set; }

        [Required(ErrorMessage = "البريد الالكتروني مطلوب")]
        [Display(Name = "البريد الإلتكروني")]
        [StringLength(74)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني ليس بالصيغة المناسبة")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة السر مطلوبة")]
        [Display(Name = "كلمة السر")]
        public string Password { get; set; }

        [Required]
        public string Type { get; set; }

    }
}

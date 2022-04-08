using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.Account
{
    public class LoginViewModel
    {

        [Required(ErrorMessage = "البريد الالكتروني مطلوب")]
        [Display(Name = "البريد الإلتكروني")]
        [DataType(DataType.EmailAddress)]
        [StringLength(73,ErrorMessage ="الحد الأقصى للبريد 73 محرف")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني ليس بالصيغة المناسبة")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة السر مطلوبة ")]
        [Display(Name = "كلمة السر")]
        [DataType(DataType.Password)]
        public string Password { get; set; }


    }
}


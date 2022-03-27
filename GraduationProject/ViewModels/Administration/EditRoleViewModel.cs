using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.Administration
{
    public class EditRoleViewModel
    {
        public EditRoleViewModel()
        {
            Users = new List<string>();
        }
        public string Id { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "على الأقل 5 حروف")]
        [Display(Name = "عنوان الدور")]
        public string RoleName { get; set; }

        public List<string> Users { get; set; }
    }
}

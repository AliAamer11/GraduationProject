using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.Category
{
    public class EditCategoryViewModel
    {
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [StringLength(30)]
        public string Name { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [StringLength(3, MinimumLength = 2, ErrorMessage = "طول الترميز إما 2 أو 3 ")]
        public string ShortCutName { get; set; }

        public int? MainCategoryId { get; set; }
    }
}

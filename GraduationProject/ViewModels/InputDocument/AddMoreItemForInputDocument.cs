using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.InputDocument
{
    public class AddMoreItemForInputDocument
    {
        [Required]
        public int Quantity { get; set; }
        [StringLength(20)]
        public string source { get; set; }
        public string Brand { get; set; }

        public string Supplier { get; set; }
        public int ItemId { get; set; }

        public List<AddMoreItemForInputDocument> AddMoreList { get; set; }
    }
}

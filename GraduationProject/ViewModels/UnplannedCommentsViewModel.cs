using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels
{
    public class UnplannedCommentsViewModel
    {
        public int UnplannedOrderID { get; set; }
        public string ItemName { get; set; }
        public string Reason { get; set; }

        public int Quantity { get; set; }

        [StringLength(100)]
        public string Comment { get; set; }

        [StringLength(100)]
        public string Description { get; set; }
    }
}

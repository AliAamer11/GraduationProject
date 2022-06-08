using GraduationProject.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels
{
    public class AnnualCommentsViewModel
    {
        public string ItemName { get; set; }
        public int AnnualOrderID { get; set; }
        public int FirstSemQuantity { get; set; }
        public int SecondSemQuantity { get; set; }
        public int ThirdSemQuantity { get; set; }
        public int TotalQuantity { get; set; }
        public int FlowRate { get; set; }
        public int ApproxRate { get; set; }

        [StringLength(100)]
        public string Comment { get; set; }

        [StringLength(100)]
        public string Description { get; set; }
    }
}

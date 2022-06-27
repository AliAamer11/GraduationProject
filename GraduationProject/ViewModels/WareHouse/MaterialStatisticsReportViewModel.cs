using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Data.Models;


namespace GraduationProject.ViewModels.WareHouse
{
    public class MaterialStatisticsReportViewModel
    {
        
        public Data.Models.Items item { get; set; }
        public int FirstQuantity { get; set; }
        public int FirstYear { get; set; }
        public float SecondYear { get; set; }
        public float SecondQuantity { get; set; }
        public float Percentage { get; set; }



    }
}

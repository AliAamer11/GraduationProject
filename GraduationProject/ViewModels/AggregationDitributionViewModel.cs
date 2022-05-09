using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels
{
    public class AggregationDitributionViewModel
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public int FirstSemQuantity  { get; set; }
        public int SecondSemQuantity  { get; set; }
        public int ThirdSemQuantity  { get; set; }
        public int TotalQuantity  { get; set; }
    }
}

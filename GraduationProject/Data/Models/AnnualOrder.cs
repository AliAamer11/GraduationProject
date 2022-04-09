using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Data.Models
{
    public class AnnualOrder
    {
        [Key]
        public int AnnualOrderID { get; set; }
        public int FirstSemQuantity { get; set; }
        public int SecondSemQuantity { get; set; }
        public int ThirdSemQuantity { get; set; }
        public int FlowRate { get; set; }
        public int ApproxRate { get; set; }

        [StringLength(100)]
        public string Comment { get; set; }
        [StringLength(100)]
        public string Description { get; set; }

        //Navigation Prop
        //this for Item
        public int ItemId { get; set; }
        public Items Item { get; set; }

        //this for Order
        public int OrderId { get; set; }
        public Order Order { get; set; }

    }
}

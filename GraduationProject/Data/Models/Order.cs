using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Data.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }

        [StringLength(1)]
        public string State { get; set; }

        public bool Type { get; set; }
        //to differentiate between the completed ones on the RP side and the ones to be altered(Set not not complete)

        public bool Complete { get; set; }

        //Navigation Prop
        //this for User
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        //this for UnPlannedOrder
        public ICollection<UnPlannedOrder> UnPlannedOrder { get; set; }

        //this for AnnualOrder
        public ICollection<AnnualOrder> AnnualOrder { get; set; }

        //this for OutputDocument
        public ICollection<OutPutDocument> OutPutDocument { get; set; }

    }
}

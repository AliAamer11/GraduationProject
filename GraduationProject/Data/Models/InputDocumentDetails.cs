using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Data.Models
{
    public class InputDocumentDetails
    {
        [Key]
        public int InputDocumentDetailsID { get; set; }

        public int Quantity { get; set; }

        [StringLength(20)]
        public string Source { get; set; }

        [StringLength(45)]
        public string Supplier { get; set; }

        [StringLength(20)]
        public string Brand { get; set; }

        //Navigation Prop
        //this for InputDocument
        public int InputDocumentId { get; set; }
        public InputDocument InputDocument { get; set; }

        //this for Items
        public int ItemId { get; set; }
        public Items Item { get; set; }

    }
}

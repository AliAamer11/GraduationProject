﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Data.Models
{
    public class Items
    {
        [Key]
        public int ItemID { get; set; }

        [StringLength(40)]
        public string BarCode { get; set; }

        [StringLength(30)]
        public string Name { get; set; }

        public int Quantity { get; set; }

        [StringLength(20)]
        public string Brand { get; set; }

        public int MinimumRange { get; set; }

        public string Note { get; set; }

        //Navigation prop
        //this for Category
        public int CategoryId { get; set; }
        public Categoreis Category { get; set; }

        //this for Measurements
        public int MeasurementId { get; set; }
        public Measurements Measurement { get; set; }

        //this for InputDocumentDetails 
        public ICollection<InputDocumentDetails> InputDocumentDetails { get; set; }

        //this for OutPutDocumentDetails 
        public ICollection<OutPutDocumentDetails> OutPutDocumentDetails { get; set; }

        //this for UnPlannedOrder
        public ICollection<UnPlannedOrder> UnPlannedOrder { get; set; }

        //this for AnnualOrder
        public ICollection<AnnualOrder> AnnualOrder { get; set; }

    }
}

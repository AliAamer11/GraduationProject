using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.Items
{
    public class editItemViewModel
    {
        public int ItemID { get; set; }
        public string Name  { get; set; }

        public string Barcode { get; set; }

        public int MinimumRange { get; set; }

        public string Note { get; set; }

    }
}

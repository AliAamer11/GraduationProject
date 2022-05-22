using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.Items
{
    public class ExcededMinimumRangeViewModel
    {
        public List<string> Itemsname { get; set; }
        public List<int> ItemsCountofExcededMinimumRange { get; set; }

        public ExcededMinimumRangeViewModel()
        {
            Itemsname = new List<string>();
            ItemsCountofExcededMinimumRange = new List<int>();
        }
    }
}

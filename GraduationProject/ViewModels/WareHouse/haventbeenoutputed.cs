using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels.Warehouse;


namespace GraduationProject.ViewModels.WareHouse
{
    public class haventbeenoutputed
    {
        public Order Order { get; set; }

        public List<HaventBeenOutPutedItemsViewModel> items { set; get; }

        public haventbeenoutputed()
        {
            items = new List<HaventBeenOutPutedItemsViewModel>();

        }

    }
}

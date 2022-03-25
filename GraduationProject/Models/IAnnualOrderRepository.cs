using GraduationProject.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Models
{
    interface IAnnualOrderRepository
    {
        public AnnualOrder Create();
        public IEnumerable<AnnualOrder> GetAnnualOrders();
        public AnnualOrder GetAnnualOrder();
        public AnnualOrder Update();



    }
}

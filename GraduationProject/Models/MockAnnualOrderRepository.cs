using GraduationProject.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Models
{
    public class MockAnnualOrderRepository : IAnnualOrderRepository
    {
        public AnnualOrder Create()
        {
            throw new NotImplementedException();
        }

        public AnnualOrder GetAnnualOrder()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AnnualOrder> GetAnnualOrders()
        {
            throw new NotImplementedException();
        }

        public AnnualOrder Update()
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.OutPutDocument
{
    public class OrdersViewModels
    {
        public int countAnnualOrder { get; set; }

        public int countUnPlannedOrder { get; set; }

        public List<OutPutDocumentViewModel> OutPutDocumentViewModelsUnPlanned { get; set; }
        public List<OutPutDocumentViewModel> OutPutDocumentViewModelsAnnual { get; set; }
        public OrdersViewModels()
        {
            OutPutDocumentViewModelsUnPlanned = new List<OutPutDocumentViewModel>();
            OutPutDocumentViewModelsAnnual = new List<OutPutDocumentViewModel>();
        }
    }
}

using GraduationProject.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.ViewModels.InputDocument
{
    public class InputDocumnetDetailsViewModel
    {
        public List<InputDocumentDetails> InputDocumentDetails { get; set; }
        public InputDocumnetDetailsViewModel()
        {
            InputDocumentDetails = new List<InputDocumentDetails>();
        }
        public DateTime CreatedAt { get; set; }
    }
}

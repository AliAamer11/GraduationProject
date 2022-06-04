using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels.InputDocument;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
    [Authorize(Roles = "StoreKeep")]
    public class InputDocumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InputDocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var inputDocuments = _context.InputDocument.ToList().OrderByDescending(i => i.InputDocumentID);
            return View(inputDocuments);
        }

        [HttpGet]
        public IActionResult AddMorePartialView()
        {
            AddMoreItemForInputDocument model = new AddMoreItemForInputDocument();
            ViewData["Item"] = new SelectList(bindListforItems(), "Value", "Text");
            ViewData["source"] = new SelectList(bindListforsource(), "Value", "Text");
            return PartialView("_AddMorePartialView", model);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddMoreItemForInputDocument viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int count = 0;
                    if (viewModel.AddMoreList == null)
                    {
                        ViewBag.errorMessage = "اضغط على + من أجل ادخال المادة وتفاصيلها";
                        return View(viewModel);
                    }
                    foreach (var item in viewModel.AddMoreList)
                    {
                        if (item != null)
                        {
                            if (item.Quantity < 1)
                            {
                                ViewBag.errorMessage = "لا يمكن لكمية المادة أن تكون سالبة";
                                return View(viewModel);
                            }
                        }
                        else
                        {
                            count++;
                        }
                    }
                    if (count == viewModel.AddMoreList.Count)
                    {
                        ViewBag.errorMessage = "اضغط على + من أجل ادخال المادة وتفاصيلها";
                        return View(viewModel);
                    }
                    var inputDocument = new InputDocument()
                    {
                        CreatedAt = DateTime.Now.Date,
                    };
                    _context.Add(inputDocument);
                    await _context.SaveChangesAsync();

                    foreach (var item in viewModel.AddMoreList)
                    {
                        if (item != null)
                        {
                            var inputDocumentDetail = new InputDocumentDetails()
                            {
                                InputDocumentId = inputDocument.InputDocumentID,
                                ItemId = item.ItemId,
                                Quantity = item.Quantity,
                                Source = item.source,
                                Brand = item.Brand,
                                Supplier = item.Supplier,
                            };
                            _context.Add(inputDocumentDetail);
                            await _context.SaveChangesAsync();
                            var updateditem = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == item.ItemId);
                            updateditem.Quantity += item.Quantity;
                            _context.Update(updateditem);
                            await _context.SaveChangesAsync();
                        }

                    }
                    return RedirectToAction("Index", "InputDocument");
                }
                catch
                {
                    throw;
                }
            }
            return RedirectToAction("Index", "InputDocument");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? inputDocumentId)
        {
            try
            {
                if (inputDocumentId == null)
                {
                    return NotFound();
                }
                var inputDoucment = await _context.InputDocument.FirstOrDefaultAsync(i => i.InputDocumentID == inputDocumentId);
                var inputDoumentDetails = await _context.InputDocumentDetails.Include(d => d.Item).Where(d => d.InputDocumentId == inputDocumentId).ToListAsync();

                var inputDocumnetDetailsViewModel = new InputDocumnetDetailsViewModel()
                {
                    InputDocumentDetails = inputDoumentDetails,
                    CreatedAt = inputDoucment.CreatedAt,
                };
                return View(inputDocumnetDetailsViewModel);
            }
            catch
            {
                throw;
            }

        }

        private List<SelectListItem> bindListforItems()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var items = _context.Items.ToList();
            foreach (var item in items)
            {
                list.Add(new SelectListItem { Text = item.BarCode + "  " + item.Name, Value = item.ItemID.ToString() });
            }
            return list;
        }

        private List<SelectListItem> bindListforsource()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Text = "هبة", Value = "هبة" });
            list.Add(new SelectListItem { Text = "شراء", Value = "شراء" });
            list.Add(new SelectListItem { Text = "تبرع", Value = "تبرع" });
            return list;
        }
    }
}

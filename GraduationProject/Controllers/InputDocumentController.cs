using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels.InputDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
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
            var inputDocuments = _context.InputDocument.ToList().OrderByDescending(i=>i.InputDocumentID);
            return View(inputDocuments);
        }

        [HttpGet]
        public IActionResult AddMorePartialView()
        {
            AddMoreItemForInputDocument model = new AddMoreItemForInputDocument();
            ViewData["Item"] = new SelectList(_context.Items, "ItemID", "BarCode");
            return PartialView("_AddMorePartialView", model);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddMoreItemForInputDocument viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (viewModel.AddMoreList==null)
                    {
                        ViewBag.errorMessage = "طيب عبيلك شي شغلة";
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
                        var inputDocumentDetail = new InputDocumentDetails()
                        {
                            InputDocumentId = inputDocument.InputDocumentID,
                            ItemId = item.ItemId,
                            Quantity = item.Quantity,
                            Source = item.source,
                        };
                        _context.Add(inputDocumentDetail);
                        await _context.SaveChangesAsync();
                        var updateditem = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == item.ItemId);
                        updateditem.Quantity += item.Quantity;
                        _context.Update(updateditem);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction("Index", "InputDocument");
                }
                catch
                {
                    throw;
                }
            }
            return RedirectToAction("Index","InputDocument");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? inputDocumentId)
        {
            try
            {
                if (inputDocumentId==null)
                {
                    return NotFound();
                }
                var inputDoucment = await _context.InputDocument.FirstOrDefaultAsync(i => i.InputDocumentID == inputDocumentId);
                var inputDoumentDetails =await _context.InputDocumentDetails.Include(d=>d.Item).Where(d => d.InputDocumentId == inputDocumentId).ToListAsync();

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

    }
}

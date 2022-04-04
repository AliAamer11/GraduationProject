using GraduationProject.Data;
using GraduationProject.ViewModels.UnplannedOrders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers.RP
{
    public class UnplannedOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UnplannedOrderController(ApplicationDbContext context)
        {
            _context = context;
        }
       
        public IActionResult Index()
        {
            var annualneeds = _context.Orders
                .Where(x => x.Type == true).ToList();
            return View(annualneeds);
        }

        public IActionResult GetAllUnplanned(int id)
        {
            var unplannedorders = _context.UnPlannedOrder
                .Where(x => x.OrderId == id)
                .ToList();


            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Item"] = new SelectList(BindListforItem(), "Value", "Text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUnplannedOrderViewModel viewModel)
        {
            
            ViewData["Item"] = new SelectList(BindListforItem(), "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {

                    var unplannedorder = new CreateUnplannedOrderViewModel()
                    {
                       ItemId = viewModel.ItemId,
                        Quantity = viewModel.Quantity,
                        Description = viewModel.Description,
                        Reason = viewModel.Reason,
                        OrderId =viewModel.OrderId,

                    };
                    _context.Add(unplannedorder);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("GetAllUnplanned");
                }
                catch
                { throw; }
            }
            ModelState.AddModelError("", "الحقول هذه مطلوبة");
            return View(viewModel);
        }


       
        private List<SelectListItem> BindListforItem()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var itemlist = _context.Items.ToList();
            foreach (var item in itemlist)
            {
                list.Add(new SelectListItem { Text = item.Name, Value = item.ItemID.ToString() });
            }
            return list;
        }

        private bool UnplannedOrderExists(int id) => _context.UnPlannedOrder.Any(c => c.UnPlannedOrderID == id);

    }
}

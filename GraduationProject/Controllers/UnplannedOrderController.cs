using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels.UnplannedOrders;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> userManager;

        public UnplannedOrderController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
            _context = context;
        }

        public int GetUnplannedOrderid()
        {
            var userid = userManager.GetUserId(User);
            var model = _context.Orders.Last();
            if (model == null) // no unplanned order 
            {
                var order = new Order
                {
                    CreatedAt = DateTime.Today,
                    State = "0",
                    Type = false,
                    UserId = "1" //userid 
                };
                _ = _context.Orders.Add(order);
                _context.SaveChanges();
                return order.OrderID;
            }
           ///// order either complete|| or not complete or state is in VP side
                return model.OrderID;


        }
        public string CheckOrderState(int id)
        {
            var model = _context.Orders.Find(id);

            //still in editing 
            if (model.Complete == false && model.State == "0")
                return "incomplete_rp_side";

            //being reviewed
            else if (model.Complete == true && model.State == "1")
                return "complete_vp_side";

            //being re-altered according to comments
            else if (model.Complete == true && model.State == "0") // then i set it to incomplete manually on user side
                return "complete_rp_side";

            //archive
            else if (model.Complete == true && model.State == "2")
                return "complete_sk_side";
            return "error";
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
        public async Task<IActionResult> Create(int id, CreateUnplannedOrderViewModel viewModel)
        {
            ViewData["OrderId"] = id;
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
                        OrderId = viewModel.OrderId,

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
            List<SelectListItem> list = new();
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

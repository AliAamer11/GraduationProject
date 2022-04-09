using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels.UnplannedOrders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
             
            
                var model = _context.Orders
                .OrderBy(o => o.CreatedAt)
                .Where(x=> x.Type == true && x.UserId == userid
               ).LastOrDefault();
           
            if (model == default) // no unplanned order 
            {
                var order = new Order
                {
                    CreatedAt = DateTime.Today,
                    State = "0",
                    Type = true,
                    UserId = userid
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

        public IActionResult GetAllUnplanned()
        {
            int id = GetUnplannedOrderid();
            var unplannedorders = _context.UnPlannedOrder
                .Include(i => i.Item)
                .Where(x => x.OrderId == id)
                .ToList();
            return View();
        }

        //public IActionResult GetAllUnplanned(int id)
        //{
        //    var unplannedorders = _context.UnPlannedOrder
        //        .Where(x => x.OrderId == id)
        //        .ToList();
        //    return View();
        //}

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
            ViewData["OrderId"] = GetUnplannedOrderid();
            ViewData["Item"] = new SelectList(BindListforItem(), "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    var unplannedorder = new CreateUnplannedOrderViewModel()
                    {
                        UnPlannedOrderID = viewModel.UnPlannedOrderID,
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

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var unplannedorder = await _context.UnPlannedOrder.FindAsync(id);
            if (unplannedorder == null)
            {
                return NotFound();
            }
            var editunplannedorderViewModel = new EditUnplannedOrderViewModel()
            {
                ItemId = unplannedorder.ItemId,
                UnPlannedOrderID = unplannedorder.UnPlannedOrderID,
                Quantity = unplannedorder.Quantity,
                Description = unplannedorder.Description,
                Reason = unplannedorder.Reason,
                OrderId = unplannedorder.OrderId,
            };
            ViewData["Item"] = new SelectList(BindListforItem(), "Value", "Text");
            return View(editunplannedorderViewModel);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUnplannedOrderViewModel viewModel, int id)
        {
            ViewData["Item"] = new SelectList(BindListforItem(), "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    var unplannedorder = new UnPlannedOrder()
                    {
                        ItemId = viewModel.ItemId,
                        UnPlannedOrderID = viewModel.UnPlannedOrderID,
                        Quantity = viewModel.Quantity,
                        Reason = viewModel.Reason,
                        Description = viewModel.Description,
                        OrderId = viewModel.OrderId,
                    };
                    _context.Update(unplannedorder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UnplannedOrderExists(viewModel.UnPlannedOrderID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                int idd = id;
                return RedirectToAction();
            }
            ModelState.AddModelError("", "تأكد من صحة الحقول");
            return View(viewModel);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var unplannedorder = await _context.UnPlannedOrder.FindAsync(id);
            if (unplannedorder == null)
            {
                return RedirectToAction(nameof(Index));
            }
            _context.UnPlannedOrder.Remove(unplannedorder);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(GetAllUnplanned));
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

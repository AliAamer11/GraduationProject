using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels;
using GraduationProject.ViewModels.UnplannedOrders;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Requester")]
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
            .Where(x => x.Type == true && x.UserId == userid
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

            if (model.State == OrderState.RequestingParty)
                return "RequestingParty";

            else if (model.State == OrderState.VicePrisdent)
                return "VicePrisdent";

            else if (model.State == OrderState.NeedOutPutDocmnet)
                return "NeedOutPutDocmnet";

            else if (model.State == OrderState.BeingReview)
                return "BeingReview";

            else if (model.State == OrderState.Finishid)
                return "Finishid";

            return "error";
        }


        public async Task<IActionResult> Index()
        {
            var userid = userManager.GetUserId(User);

            var unplannedorders = await _context.Orders
                .Where(x => x.Type == true)   //// unplanned order type
                .Where(o => o.UserId == userid)    /// getting orders to this current user
                .Where(o => o.State == OrderState.NeedOutPutDocmnet || o.State == OrderState.Finishid)  ///getting orders that are complete on StoreKeeper side
                .ToListAsync();
            return View(unplannedorders);
        }

        //Get All Unplanned to Order where it is still not complete
        [HttpGet]
        public async Task<IActionResult> GetAllUnplanned()
        {
            var userid = userManager.GetUserId(User);
            int id = GetUnplannedOrderid();
            var unplannedorders =await _context.UnPlannedOrder
                .Include(i => i.Item)
                .Where(x => x.OrderId == id)
                .Where(o => o.Order.Type == true && o.Order.State == OrderState.RequestingParty)
                .Where(o => o.Order.UserId == userid)
                .ToListAsync();
            ViewData["id"] = id;
            return View(unplannedorders);
        }

        //Get All Unplanned to Order where it is complete &needs alteration
        [HttpGet]
        public async Task<IActionResult> GetAllUnplannedAltered()
        {
            var userid = userManager.GetUserId(User);
            int id = GetUnplannedOrderid();
            var unplannedorders = await _context.UnPlannedOrder
                .Include(i => i.Item)
                .Where(x => x.OrderId == id)
                .Where(x => x.Comment != null)
                .Where(o => o.Order.Type == true && o.Order.State == OrderState.BeingReview)
                .Where(o => o.Order.UserId == userid)
                .ToListAsync();
            ViewData["id"] = id;
            return View(unplannedorders);
        }

        public async Task<IActionResult> GetUnplannedNeedsDisplay()
        {
            var userid = userManager.GetUserId(User);
            int id = GetUnplannedOrderid();

            var unplannedorders = await _context.UnPlannedOrder.Include(o => o.Order)
                    .Include(i => i.Item)
                    .Where(x => x.OrderId == id)
                    .Where(o => o.Order.Type == true && o.Order.State == OrderState.VicePrisdent || o.Order.State == OrderState.NeedOutPutDocmnet || o.Order.State == OrderState.Finishid)
                    .Where(o => o.Order.UserId == userid)
                    .ToListAsync();
            ViewData["id"] = id;
            return View(unplannedorders);
        }


        [HttpGet]
        public IActionResult Create()
        {
            ViewData["OrderId"] = GetUnplannedOrderid();
            ViewData["Item"] = new SelectList(BindListforItem(), "Value", "Text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUnplannedOrderViewModel viewModel)
        {
            ViewData["OrderId"] = GetUnplannedOrderid();
            ViewData["Item"] = new SelectList(BindListforItem(), "Value", "Text");
            var unplannedorderss = _context.UnPlannedOrder.Where(i => i.OrderId == GetUnplannedOrderid());

            if (ModelState.IsValid)
            {
                foreach (var ano in unplannedorderss)
                {
                    if (viewModel.ItemId == ano.ItemId)
                    {
                        ViewBag.errorMassage = ".هذه المادة موجودة في الطلب";
                        return View(viewModel);
                    }
                }
                try
                {
                    var unplannedorder = new UnPlannedOrder
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
            //var unplannedorder = await _context.UnPlannedOrder.FindAsync(id);
            var unplannedorder = await _context.UnPlannedOrder.Include(o => o.Order).Where(i => i.UnPlannedOrderID == id).FirstOrDefaultAsync();

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
                Comment = unplannedorder.Comment,
                OrderId = unplannedorder.OrderId,
                Order = unplannedorder.Order,
            };
            ViewData["OrderId"] = GetUnplannedOrderid();
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
                    var unplannedorder = await _context.UnPlannedOrder.Include(o => o.Order)
                        .FirstOrDefaultAsync(i => i.UnPlannedOrderID == viewModel.UnPlannedOrderID);

                    unplannedorder.ItemId = viewModel.ItemId;
                    unplannedorder.UnPlannedOrderID = viewModel.UnPlannedOrderID;
                    unplannedorder.Quantity = viewModel.Quantity;
                    unplannedorder.Description = viewModel.Description;
                    unplannedorder.Reason = viewModel.Reason;
                    unplannedorder.OrderId = viewModel.OrderId;
                    unplannedorder.Comment = viewModel.Comment;


                    //var unplannedorder = new UnPlannedOrder()
                    //{
                    //    ItemId = viewModel.ItemId,
                    //    UnPlannedOrderID = viewModel.UnPlannedOrderID,
                    //    Quantity = viewModel.Quantity,
                    //    Reason = viewModel.Reason,
                    //    Description = viewModel.Description,
                    //    Comment = viewModel.Comment,
                    //    OrderId = viewModel.OrderId,
                    //};
                    _context.Update(unplannedorder);
                    await _context.SaveChangesAsync();

                    if (unplannedorder.Order.State == OrderState.BeingReview)
                    { return RedirectToAction("GetAllUnplannedAltered"); }

                    return RedirectToAction(nameof(GetAllUnplanned));
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

using GraduationProject.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Authorization;
using GraduationProject.ViewModels;

namespace GraduationProject.Controllers
{
    [Authorize(Roles = "Requester")]
    public class RPOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;

        public RPOrderController(ApplicationDbContext context,
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
        public int GetAnnualNeedOrderid()
        {
            var userid = userManager.GetUserId(User);
            DateTime CurrentDate = DateTime.Now;
            int CurrentYear = CurrentDate.Year;

            var model = _context.Orders
                .OrderBy(o => o.CreatedAt)
                .Where(x => x.CreatedAt.Year == CurrentYear
                                            && x.Type == false
                                            && x.UserId == userid
                                            ).LastOrDefault();


            if (model == default) //to check if there is an annual need initialized or not
            {
                var order = new Order
                {
                    CreatedAt = DateTime.Today,
                    State = "0",
                    Type = false,
                    UserId = userid
                };
                var model1 = _context.Orders.Add(order);
                _context.SaveChanges();
                return order.OrderID;
            }
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

        [HttpGet]
        public IActionResult Home()
        {
            ViewData["Current_Year"] = DateTime.Now.Year;
            ViewData["Annual_State"] = CheckOrderState(GetAnnualNeedOrderid());
            ViewData["Unplanned_State"] = CheckOrderState(GetUnplannedOrderid());
            return View();
        }
        public IActionResult CheckCompleteOrder(int id)
        {
            //var order = _context.Orders.Find(id);
            var order = _context.Orders.Where(o => o.OrderID == id).FirstOrDefault();
            if (order != null)
            {
                try
                {
                    order.State = OrderState.VicePrisdent;        //the order is now on the VP side

                    _context.SaveChanges();
                    if (order.Type == false)
                        return RedirectToAction("GetAnnualNeedsDisplay", "AnnualOrder");
                    return RedirectToAction("GetUnplannedNeedsDisplay", "UnplannedOrder");
                }
                catch
                { return NotFound(); }
            }
            ModelState.AddModelError("", "الطلب غير موجود");
            return RedirectToAction("Home", "RPOrder");
        }
    }
}

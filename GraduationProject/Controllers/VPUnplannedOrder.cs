using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
    [Authorize(Roles = "VicePris")]
    public class VPUnplannedOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VPUnplannedOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? OrderId)
        {
            ViewBag.OrderId = OrderId;
            var UnPlannedOrders = await _context.UnPlannedOrder.Include(i => i.Item).Where(o => o.OrderId == OrderId).ToListAsync();
            var RequestingParty = await _context.Orders.Include(u => u.User).Where(o => o.OrderID == OrderId).FirstOrDefaultAsync();
            ViewBag.RequestingParty = RequestingParty.User.RequstingParty;
            List<UnplannedCommentsViewModel> UnplannedComments = new List<UnplannedCommentsViewModel>();
            foreach (var item in UnPlannedOrders)
            {
                UnplannedCommentsViewModel model = new UnplannedCommentsViewModel();
                model.UnplannedOrderID = item.UnPlannedOrderID;
                model.ItemName = item.Item.Name;
                model.Quantity = item.Quantity;
                model.Comment = item.Comment;
                model.Reason = item.Reason;
                model.Description = item.Description;
                UnplannedComments.Add(model);
            }
            return View(UnplannedComments);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveOrder(int? id)
        {
            var order = await _context.Orders.FindAsync(id);
            order.State = OrderState.NeedOutPutDocmnet;
            _context.Update(order);
            _context.SaveChanges();
            return RedirectToAction("Unplanned", "VPOrder");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Index(List<UnplannedCommentsViewModel> models, int? OrderId)
        {

            foreach (var item in models)
            {
                UnPlannedOrder model = await _context.UnPlannedOrder.FirstOrDefaultAsync(a => a.UnPlannedOrderID == item.UnplannedOrderID);
                model.Comment = item.Comment;
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == OrderId);
            order.State =OrderState.BeingReview;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Unplanned", "VPOrder");
        }

        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //public async Task<IActionResult> Index(int? Orderid, string[] comments)
        //{
        //    var unplannedOrders = await _context.UnPlannedOrder.Where(o => o.OrderId == Orderid).ToListAsync();
        //    var order = await _context.Orders.FindAsync(Orderid);

        //    bool check = false; 
        //    foreach(var c in comments)
        //    {
        //        if (c==null)
        //        {
        //            check = false;
        //        }
        //        else
        //        {
        //            check = true;
        //            break;
        //        }
        //    }
        //        if (ModelState.IsValid && check)
        //        {

        //            try
        //            {
        //                using (var e1 = unplannedOrders.GetEnumerator())
        //                {

        //                    while (e1.MoveNext())
        //                    {
        //                        foreach (var c in comments)
        //                        {
        //                            var obj = e1.Current;
        //                            obj.Comment = c;
        //                            _context.UnPlannedOrder.Update(obj);
        //                            _context.SaveChanges();
        //                            e1.MoveNext();

        //                        }
        //                    }
        //                }
        //            order.State = "0";
        //            _context.Orders.Update(order);
        //            _context.SaveChanges();

        //            return RedirectToAction("Unplanned", "VPOrder");

        //        }

        //        catch
        //            {
        //                return StatusCode(500, "Internal server error");
        //            }
        //        }
        //    ViewBag.comment = "يجب ادخال تعليق واحد على الاقل ";

        //    return View(unplannedOrders);
        //    }


    }
}


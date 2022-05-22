using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
    [Authorize(Roles = "VicePris")]
    public class VPAnnualOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public VPAnnualOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? OrderId)
        {
            ViewBag.OrderId = OrderId;
            var annualOrders = await _context.AnnualOrder.Include(i => i.Item).Where(o => o.OrderId == OrderId).ToListAsync();
            List<AnnualCommentsViewModel> annualComments = new List<AnnualCommentsViewModel>();
            foreach (var item in annualOrders)
            {
                AnnualCommentsViewModel model = new AnnualCommentsViewModel();
                model.AnnualOrderID = item.AnnualOrderID;
                model.ItemName = item.Item.Name;
                model.FirstSemQuantity = item.FirstSemQuantity;
                model.SecondSemQuantity = item.SecondSemQuantity;
                model.ThirdSemQuantity = item.ThirdSemQuantity;
                model.Comment = item.Comment;
                model.ApproxRate = item.ApproxRate;
                model.FlowRate = item.FlowRate;
                model.Description = item.Description;
                model.TotalQuantity = item.FirstSemQuantity + item.SecondSemQuantity + item.ThirdSemQuantity;
                annualComments.Add(model);
            }

            return View(annualComments);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveOrder(int? id)
        {
            var order = await _context.Orders.FindAsync(id);
            order.State = OrderState.NeedOutPutDocmnet;
            _context.Update(order);
            _context.SaveChanges();

            return RedirectToAction("Annual", "VPOrder");
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Index(List<AnnualCommentsViewModel> models, int? OrderId)
        {

            foreach (var item in models)
            {
                AnnualOrder model = await _context.AnnualOrder.FirstOrDefaultAsync(a => a.AnnualOrderID == item.AnnualOrderID);
                model.Comment = item.Comment;
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == OrderId);
            order.State = OrderState.RequestingParty;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Annual", "VPOrder");
        }
    }
}


//[ValidateAntiForgeryToken]
//[HttpPost]
//public async Task<IActionResult> Index(int? Orderid, string[] comments)
//{
//    var AnnualOrders = await _context.AnnualOrder.Where(o => o.OrderId == Orderid).ToListAsync();
//    var order = await _context.Orders.FindAsync(Orderid);
//    bool check = false;
//    foreach (var c in comments)
//    {
//        if (c == null)
//        {
//            check = false;
//        }
//        else
//        {
//            check = true;
//            break;
//        }
//    }
//    ViewBag.check = check;
//    if (ModelState.IsValid && check)
//    {

//        try
//        {

//            using (var e1 = AnnualOrders.GetEnumerator())
//            {

//                while (e1.MoveNext())
//                {
//                    foreach (var c in comments)
//                    {
//                        var obj = e1.Current;
//                        obj.Comment = c;
//                        _context.AnnualOrder.Update(obj);
//                        _context.SaveChanges();
//                        e1.MoveNext();
//                    }
//                }
//            }
//            order.State = "0";
//            _context.Orders.Update(order);
//            _context.SaveChanges();
//            return RedirectToAction("Annual","VPOrder");
//        }

//        catch
//        {
//            return StatusCode(500, "Internal server error");
//        }
//    }
//    ViewBag.comment = "يجب ادخال تعليق واحد على الاقل ";
//    return View(AnnualOrders);
//}
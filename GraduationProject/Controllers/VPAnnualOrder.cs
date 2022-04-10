using GraduationProject.Data;
using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
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
            var annualOrders = await _context.AnnualOrder.Where(o => o.OrderId == OrderId).ToListAsync();

            ViewBag.Orderid = _context.AnnualOrder.Find(OrderId);
          
            return View(annualOrders);
        }
        [HttpPost]
        public async Task<IActionResult> ApproveOrder(int? id)
        {

            var order = await _context.Orders.FindAsync(id);
            order.State = "2";
            _context.Update(order);
            _context.SaveChanges();

            return RedirectToAction("Annual" ,"VPOrder");
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Index(int? Orderid, string[] comments)
        {

            var AnnualOrders = await _context.AnnualOrder.Where(o => o.OrderId == Orderid).ToListAsync();
            var order = await _context.Orders.FindAsync(Orderid);
            bool check = false;
            foreach (var c in comments)
            {
                if (c == null)
                {
                    check = false;
                }
                else
                {
                    check = true;
                    break;
                }
            }
            ViewBag.check = check;
            if (ModelState.IsValid && check)
            {

                try
                {
                   
                    using (var e1 = AnnualOrders.GetEnumerator())
                    {

                        while (e1.MoveNext())
                        {
                            foreach (var c in comments)
                            {
                                var obj = e1.Current;
                                obj.Comment = c;
                                _context.AnnualOrder.Update(obj);
                                _context.SaveChanges();
                                e1.MoveNext();
                            }
                        }
                    }
                    order.Complete = false;
                    order.State = "0";
                    _context.Orders.Update(order);
                    _context.SaveChanges();
                    return RedirectToAction("Annual","VPOrder");
                }

                catch
                {
                    return StatusCode(500, "Internal server error");
                }
            }
            ViewBag.comment = "يجب ادخال تعليق واحد على الاقل ";
            return View(AnnualOrders);
        }
    }
}

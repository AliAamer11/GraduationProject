using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
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
            var unplannedOrders = await _context.UnPlannedOrder.Where(o => o.OrderId == OrderId).ToListAsync();

            ViewBag.Orderid = _context.UnPlannedOrder.Find(OrderId);


            return View(unplannedOrders);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveOrder(int? id)
        {
            var order = await _context.Orders.FindAsync(id);
            order.State = "1";
            _context.Update(order);
            _context.SaveChanges();
            return RedirectToAction("Unplanned", "VPOrder");
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddComment(int? Orderid, string[] comments)
        {
            bool check = false; 
            foreach(var c in comments)
            {
                if (c==null)
                {
                    check = false;
                }
                else
                {
                    check = true;
                    break;
                }
            }
                if (ModelState.IsValid && check)
                {

                    try
                    {
                        var unplannedOrders = await _context.UnPlannedOrder.Where(o => o.OrderId == Orderid).ToListAsync();
                        using (var e1 = unplannedOrders.GetEnumerator())
                        {

                            while (e1.MoveNext())
                            {
                                foreach (var c in comments)
                                {
                                    var obj = e1.Current;
                                    obj.Comment = c;
                                    _context.UnPlannedOrder.Update(obj);
                                    _context.SaveChanges();
                                    e1.MoveNext();
                                }
                            }
                        }

                    return RedirectToAction("Index", new { Orderid });
                    }

                    catch
                    {
                        return StatusCode(500, "Internal server error");
                    }
                }
            return Ok("يجب إدخال تعليق واحد على الأقل حتى يتم إعادةإرسال الطلب!! ");
            }

            
        }
    }


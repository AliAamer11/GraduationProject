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
            order.State = "1";
            _context.Update(order);
            _context.SaveChanges();

            return RedirectToAction("Annual" ,"VPOrder");
        }

    }
}

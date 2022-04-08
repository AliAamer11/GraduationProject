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


    }
}

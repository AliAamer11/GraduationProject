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
    public class AnnualOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AnnualOrderController(ApplicationDbContext context)
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

        public IActionResult ApproveOrder(int? OrderId)
        {
            var order = _context.Orders.Find(OrderId);
            order.State = "1";
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}

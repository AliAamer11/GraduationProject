using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers.VPControllers
{
    public class VPOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public VPOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var annualorders = await _context.Orders.Where(o => o.State == "1" & o.Type == false).ToListAsync();
            var unplannedorders = await _context.Orders.Where(o => o.State == "1" & o.Type == true).ToListAsync();
            ViewBag.AnnualCount = annualorders.Count;
            ViewBag.UnplannedCount = unplannedorders.Count;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Annual()
        {
            var AnnualOrders = await _context.Orders.Include(u => u.User).Where(o => o.Type == false & o.State == "1" & o.Complete == true).ToListAsync();
            return View(AnnualOrders);
        }

        [HttpGet]
        public async Task<IActionResult> Unplanned()
        {
            var UnplannedOrders = await _context.Orders.Include(u => u.User).Where(o => o.State == "1" & o.Type == true).ToListAsync();
            return View(UnplannedOrders);
        }

        public async Task<IActionResult> ManualDistributionIndex()
        {
            var annualOrders = await _context.Orders.Include(u => u.User).Where(o => o.Type == false & o.State == "1").ToListAsync();
            return View(annualOrders);
        }

        [HttpGet]
        public async Task<IActionResult> ManualDistribution(int? OrderId)
        {
            ViewBag.OrderId = OrderId;
            var annualOrders = await _context.AnnualOrder.Include(i => i.Item).Where(o => o.OrderId == OrderId).ToListAsync();
            List<ManualDistributionViewModel> manualDistributions = new List<ManualDistributionViewModel>();
            foreach (var item in annualOrders)
            {
                ManualDistributionViewModel model = new ManualDistributionViewModel();
                model.AnnualOrderID = item.AnnualOrderID;
                model.ItemName = item.Item.Name;
                model.FirstSemQuantity = item.FirstSemQuantity;
                model.SecondSemQuantity = item.SecondSemQuantity;
                model.ThirdSemQuantity = item.ThirdSemQuantity;
                model.TotalQuantity = item.FirstSemQuantity + item.SecondSemQuantity + item.ThirdSemQuantity;

                manualDistributions.Add(model);
            }
            return View(manualDistributions);
        }

        [HttpPost]
        public async Task<IActionResult> ManualDistribution(List<ManualDistributionViewModel> models, int? OrderId)
        {
            foreach (var item in models)
            {
                AnnualOrder model = await _context.AnnualOrder.FirstOrDefaultAsync(a => a.AnnualOrderID == item.AnnualOrderID);
                model.FirstSemQuantity = item.FirstSemQuantity;
                model.SecondSemQuantity = item.SecondSemQuantity;
                model.ThirdSemQuantity = item.ThirdSemQuantity;
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == OrderId);
            order.State = "1";
            _context.Update(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManualDistributionIndex");
        }
    }
}

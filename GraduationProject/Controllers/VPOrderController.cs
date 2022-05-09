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


        private int distribution(int sem , int oldtotal, int newtotal)
        {
            float semester = (float)sem;
            float oldtot = (float)oldtotal;
            float division = semester / oldtot;
            double result = Math.Floor(division * newtotal);
            return (int)result;
        }
        [HttpPost]
        public async Task<IActionResult> ManualDistribution(List<ManualDistributionViewModel> models, int? OrderId)
        {
            foreach (var item in models)
            {
                AnnualOrder model = await _context.AnnualOrder.FirstOrDefaultAsync(a => a.AnnualOrderID == item.AnnualOrderID);
                int oldtotal = model.FirstSemQuantity + model.SecondSemQuantity + model.ThirdSemQuantity;
                //if only a new semester quantity is updated (total quantity not updated)
                model.FirstSemQuantity = item.FirstSemQuantity;
                model.SecondSemQuantity = item.SecondSemQuantity;
                model.ThirdSemQuantity = item.ThirdSemQuantity;
                _context.Update(model);
                await _context.SaveChangesAsync();
                //if a new semester quantity  and the total is updated too 
                //then the above code will be executed anyways then the new total quantity will be distributed on the three semesters
                int newtotal = item.TotalQuantity;
                if (newtotal != oldtotal)
                {

                    model.FirstSemQuantity = distribution(model.FirstSemQuantity, oldtotal, newtotal); 
                    model.SecondSemQuantity = distribution(model.SecondSemQuantity, oldtotal, newtotal);
                    model.ThirdSemQuantity = distribution(model.ThirdSemQuantity, oldtotal, newtotal);
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
            }
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == OrderId);
            order.State = "1";
            _context.Update(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManualDistributionIndex");
        }
    }
}

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
            var AnnualOrders = await _context.Orders.Include(u => u.User).Where(o => o.Type == false & o.State == "1" ).ToListAsync();
            return View(AnnualOrders);
        }

        [HttpGet]
        public async Task<IActionResult> Unplanned()
        {
            var UnplannedOrders = await _context.Orders.Include(u => u.User).Where(o => o.State == "1" & o.Type == true).ToListAsync();
            return View(UnplannedOrders);
        }
        /// <summary>
        /// distribution function
        /// </summary>

        [HttpGet]
        public async Task<IActionResult> DistributionIndex()
        {
            var annualorders = await _context.Orders.Where(o => o.State == "1" & o.Type == false).ToListAsync();

            ViewBag.AnnualCount = annualorders.Count;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AggregationDistribution()
        {
            var items = await _context.AnnualOrder.Where(AO=>AO.Order.State == "1").GroupBy(m => m.Item.Name).Select(m => new { name = m.Key, itemcount = m.Count() , sum1 = m.Sum(m =>m.FirstSemQuantity), sum2= m.Sum(m => m.SecondSemQuantity), sum3 = m.Sum(m => m.ThirdSemQuantity) }).ToListAsync();
            List<AggregationDitributionViewModel> AggregationDistribution = new List<AggregationDitributionViewModel>();
            foreach (var item in items)
            {
                AggregationDitributionViewModel model = new AggregationDitributionViewModel();
                model.ItemName = item.name;
                model.FirstSemQuantity = item.sum1;
                model.SecondSemQuantity = item.sum2;
                model.ThirdSemQuantity = item.sum3;
                model.TotalQuantity = item.sum1 + item.sum2 + item.sum3;

                AggregationDistribution.Add(model);
            }

            return View(AggregationDistribution);
        }

        [HttpPost]
        public async Task<IActionResult> AggregationDistribution(List<AggregationDitributionViewModel> models)
        {
            var items = await _context.AnnualOrder.Where(AO => AO.Order.State == "1").GroupBy(m => m.Item.Name).Select(m => new { name = m.Key, itemcount = m.Count(), sum1 = m.Sum(m => m.FirstSemQuantity), sum2 = m.Sum(m => m.SecondSemQuantity), sum3 = m.Sum(m => m.ThirdSemQuantity) }).ToListAsync();

            //القيم الجديدة المجمعه 
            foreach (var m in models)
            {
                IEnumerable<AnnualOrder>annualorders = await _context.AnnualOrder.Where(AO => AO.Order.State == "1").Include(i=>i.Item).ToListAsync();
                //القيم منغير تجميع 
                foreach(var obj in annualorders)
                {
                    if (obj.Item.Name == m.ItemName)
                    {
                        var Aggitem = items.Find(m => m.name == obj.Item.Name);
                        int FirstSem = Aggitem.sum1;
                        int SecondSem = Aggitem.sum2;
                        int ThirdSem = Aggitem.sum3;
                        int Total = FirstSem + SecondSem + ThirdSem;
                        if (FirstSem != m.FirstSemQuantity)
                        {
                            //float percentage =((float)m.FirstSemQuantity / FirstSem);
                            //float NewSemQuantity = (percentage * (float)obj.FirstSemQuantity);
                            //obj.FirstSemQuantity =(int) NewSemQuantity;

                            obj.FirstSemQuantity = (int)SemDistrebution(m.FirstSemQuantity, obj.FirstSemQuantity, FirstSem);
                            _context.Update(obj);
                        }
                        else if (SecondSem != m.SecondSemQuantity)
                        {
                            //float percentage = ((float)m.SecondSemQuantity / SecondSem);
                            //float NewSemQuantity = percentage * (float)obj.SecondSemQuantity;
                            //obj.SecondSemQuantity =(int)NewSemQuantity;

                            obj.SecondSemQuantity = (int)SemDistrebution(m.SecondSemQuantity, obj.SecondSemQuantity, SecondSem);
                            _context.Update(obj);
                        }
                        else if (ThirdSem != m.ThirdSemQuantity)
                        {
                            //float percentage = ((float)m.ThirdSemQuantity / ThirdSem);
                            //float NewSemQuantity = percentage * (float)obj.ThirdSemQuantity;
                            //obj.ThirdSemQuantity =(int)NewSemQuantity;
                            obj.ThirdSemQuantity = (int)SemDistrebution(m.ThirdSemQuantity, obj.ThirdSemQuantity, ThirdSem);
                            _context.Update(obj);
                        }

                        else if (Total != m.TotalQuantity)
                        {
                            float TotalObjQuantity = obj.FirstSemQuantity + obj.SecondSemQuantity + obj.ThirdSemQuantity;
                            float percentage = ((float)m.TotalQuantity / Total);
                            float NewTotalQuantity = percentage * TotalObjQuantity;
                            float FirstSemDistribution = ((float)obj.FirstSemQuantity / TotalObjQuantity) * NewTotalQuantity;
                            obj.FirstSemQuantity = (int)FirstSemDistribution;
                            float SecondSemDistribution = ((float)obj.SecondSemQuantity / TotalObjQuantity) * NewTotalQuantity;
                            obj.SecondSemQuantity = (int)SecondSemDistribution;
                            float ThirdSemsterDistribution = ((float)obj.SecondSemQuantity / TotalObjQuantity) * NewTotalQuantity;
                            obj.ThirdSemQuantity = (int)ThirdSemsterDistribution;
                            _context.Update(obj);
                        }

                    }
                }
                await _context.SaveChangesAsync();
            }
           
            return RedirectToAction("AggregationDistribution");
        }

        //Semesters distribution function
        public float SemDistrebution(float modelSem ,float ObjSem , int AggSem)
        {
            float percentage = modelSem / AggSem;
            float NewSemQuantity = percentage * ObjSem;
            return NewSemQuantity;
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

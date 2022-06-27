using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers.VPControllers
{
    [Authorize(Roles = "VicePris")]
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
            var annualorders = await _context.Orders.Where(o => o.State == OrderState.VicePrisdent & o.Type == false).ToListAsync();
            var unplannedorders = await _context.Orders.Where(o => o.State == "1" & o.Type == true).ToListAsync();
            ViewBag.AnnualCount = annualorders.Count;
            ViewBag.UnplannedCount = unplannedorders.Count;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Annual()
        {
            var AnnualOrders = await _context.Orders.Include(u => u.User).Where(o => o.Type == false & o.State == OrderState.VicePrisdent).ToListAsync();
            return View(AnnualOrders);
        }

        [HttpGet]
        public async Task<IActionResult> Unplanned()
        {
            var UnplannedOrders = await _context.Orders.Include(u => u.User).Where(o => o.State == OrderState.VicePrisdent & o.Type == true).ToListAsync();
            return View(UnplannedOrders);
        }

        [HttpGet]
        public async Task<IActionResult> DistributionIndex()
        {
            var annualorders = await _context.Orders.Where(o => o.State == OrderState.VicePrisdent & o.Type == false).ToListAsync();

            ViewBag.AnnualCount = annualorders.Count;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AggregationDistribution()
        {
            var items = await _context.AnnualOrder.Where(AO=>AO.Order.State == OrderState.VicePrisdent).GroupBy(m => m.Item.Name).Select(m => new { name = m.Key, itemcount = m.Count() , sum1 = m.Sum(m =>m.FirstSemQuantity), sum2= m.Sum(m => m.SecondSemQuantity), sum3 = m.Sum(m => m.ThirdSemQuantity) }).ToListAsync();
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
            try
            {
                var items = await _context.AnnualOrder.Where(AO => AO.Order.State == OrderState.VicePrisdent).GroupBy(m => m.Item.Name).Select(m => new { name = m.Key, itemcount = m.Count(), sum1 = m.Sum(m => m.FirstSemQuantity), sum2 = m.Sum(m => m.SecondSemQuantity), sum3 = m.Sum(m => m.ThirdSemQuantity) }).ToListAsync();

                //القيم الجديدة المجعة 
                foreach (var m in models)
                {
                    IEnumerable<AnnualOrder> annualorders = await _context.AnnualOrder.Where(AO => AO.Order.State == OrderState.VicePrisdent).Include(i => i.Item).ToListAsync();
                    //القيم منغير تجميع 
                    foreach (var obj in annualorders)
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
                                obj.FirstSemQuantity = (int)SemDistrebution(m.FirstSemQuantity, obj.FirstSemQuantity, FirstSem);
                                _context.Update(obj);
                            }
                            else if (SecondSem != m.SecondSemQuantity)
                            {
                                obj.SecondSemQuantity = (int)SemDistrebution(m.SecondSemQuantity, obj.SecondSemQuantity, SecondSem);
                                _context.Update(obj);
                            }
                            else if (ThirdSem != m.ThirdSemQuantity)
                            {
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
            catch
            {
                throw;
            }
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
            var annualOrders = await _context.Orders.Include(u => u.User).Where(o => o.Type == false & o.State == OrderState.VicePrisdent).ToListAsync();
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
                model.comment = item.Comment;
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
            try
            {
                foreach (var item in models)
                {
                    AnnualOrder model = await _context.AnnualOrder.FirstOrDefaultAsync(a => a.AnnualOrderID == item.AnnualOrderID);
                    int oldtotal = model.FirstSemQuantity + model.SecondSemQuantity + model.ThirdSemQuantity;
                    //if only a new semester quantity is updated (total quantity not updated)
                    model.FirstSemQuantity = item.FirstSemQuantity;
                    model.SecondSemQuantity = item.SecondSemQuantity;
                    model.ThirdSemQuantity = item.ThirdSemQuantity;
                    model.Comment = item.comment;
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    //if a new semester quantity  and the total is updated too 
                    //then the above code will be executed anyways then
                    //the new total quantity will be distributed on the three semesters equally
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
                order.State = OrderState.VicePrisdent;
                _context.Update(order);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManualDistributionIndex");
            }
            catch
            {
                throw;
            }
        }
    }
}

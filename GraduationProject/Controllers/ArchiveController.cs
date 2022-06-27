using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels.AnnualNeedOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using GraduationProject.Controllers;
using GraduationProject.Service;
using GraduationProject.ViewModels;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
    [Authorize(Roles = "Requester")]
    public class ArchiveController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IUserService userService;


        private readonly ApplicationDbContext _context;
        public ArchiveController(ApplicationDbContext context,
                                UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 IUserService userService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.userService = userService;

            _context = context;
        }

        //decide what type of document to review
        public IActionResult Index()
        {
            var userid = userManager.GetUserId(User);

            ViewData["AnnualCount"] = "0" + (_context.Orders.Where(o => o.Type == false)
                                                            .Where(o=>o.State == OrderState.NeedOutPutDocmnet || o.State == OrderState.Finishid)//// annual order type
                                                             .Where(o => o.UserId == userid)
                                                            .Count()).ToString();
            ViewData["UnplannedCount"] = "0" + (_context.Orders.Where(o => o.Type == true)
                                                               .Where(o => o.State == OrderState.NeedOutPutDocmnet || o.State == OrderState.Finishid)//// annual order type
                                                                .Where(o => o.UserId == userid)
                                                               .Count().ToString());
            return View();
        }

        [HttpGet]
        public IActionResult AlLAnnualOrders()
        {

            return RedirectToAction("Index", "AnnualOrder");
        }

        [HttpGet]
        public IActionResult AllUnplannedOrders()
        {

            return RedirectToAction("Index", "UnplannedOrder");
        }


        //Get All AnnualNeed To Specific Order///// from archive
        [HttpGet]
        public async Task<IActionResult> GetAnnualNeedOrders(int id)
        {
            //var user = await _userManager.GetUserAsync(User);
            ViewBag.orderid = id;
            var errormessage = TempData["errormessage"] as string;
            ViewBag.errormessage = errormessage;
            var model = new List<AnnualNeedOrderViewModel>();  ///model list of ano for seecting thwm ot use as template

            var userid = userManager.GetUserId(User);
            var annualneedorders = await _context.AnnualOrder.Include(o => o.Order)
                .Include(i => i.Item)
                .Where(x => x.OrderId == id)
                .Where(o => o.Order.State == OrderState.NeedOutPutDocmnet || o.Order.State == OrderState.Finishid)
                .Where(o => o.Order.UserId == userid)
                .ToListAsync();

            foreach (var ano in annualneedorders)
            {
                var anoviewmodel = new AnnualNeedOrderViewModel
                {
                    ItemId = ano.ItemId,
                    AnnualOrderID = ano.AnnualOrderID,
                    FirstSemQuantity = ano.FirstSemQuantity,
                    SecondSemQuantity = ano.SecondSemQuantity,
                    ThirdSemQuantity = ano.ThirdSemQuantity,
                    Description = ano.Description,
                    Comment = ano.Comment,
                    FlowRate = ano.FlowRate,
                    ApproxRate = ano.ApproxRate,
                    OrderId = ano.OrderId,
                    Item = ano.Item,
                    IsSelected = false,
                };
                model.Add(anoviewmodel);

            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> GetAnnualNeedOrders(List<AnnualNeedOrderViewModel> model, int id)
        {

            AnnualOrderController x = new AnnualOrderController(_context, userManager, userService);

            int orderid = x.GetAnnualNeedOrderid();
            var order =  _context.Orders.Where(i=> i.OrderID == orderid).FirstOrDefault();

            var userid = userManager.GetUserId(User);
            //List<AnnualNeedOrderViewModel> AnnualOrderstoAdd = new();
            if (orderid != id &&  order.State=="0")
            {
                for (int i = 0; i < model.Count(); i++)
                {
                    if (model[i].IsSelected == true)
                    {
                        var annualorderx = new AnnualOrder
                        {
                            //AnnualOrderID = model[i].AnnualOrderID,
                            ItemId = model[i].ItemId,
                            FirstSemQuantity = model[i].FirstSemQuantity,
                            SecondSemQuantity = model[i].SecondSemQuantity,
                            ThirdSemQuantity = model[i].ThirdSemQuantity,
                            Description = model[i].Description,
                            FlowRate = model[i].FlowRate,
                            ApproxRate = model[i].ApproxRate,
                            OrderId = orderid,

                        };
                        var item = _context.AnnualOrder.Where(i => i.OrderId == orderid && i.ItemId == annualorderx.ItemId).FirstOrDefault();
                        if (item == null)
                        {
                        await _context.AddAsync(annualorderx);
                        await _context.SaveChangesAsync();
                        }



                        //AnnualOrderstoAdd.Add(model[i]);
                    }
                }
            }
            else
            {
                //ViewBag.errorMassage = ".لا يمكن إضافة مواد والتعديل على الطلب الحالي";
                TempData["errormessage"] = ".لا يمكن إضافة مواد والتعديل على الطلب الحالي";
                return RedirectToAction("GetAnnualNeedOrders", "Archive");
            }
            return RedirectToAction("GetAnnualNeedOrders", "AnnualOrder");

        }

        //Get All Unplanned Orders To Specific Order///// from archive
        [HttpGet]
        public async Task<IActionResult> GetUnplannedOrders(int id)
        {
            var userid = userManager.GetUserId(User);
            var unplannedorders =await _context.UnPlannedOrder.Include(o => o.Order)
                .Include(i => i.Item)
                .Where(x => x.OrderId == id)
                .Where(o => o.Order.UserId == userid)
                .Where(o => o.Order.State == OrderState.NeedOutPutDocmnet || o.Order.State == OrderState.Finishid)
                .ToListAsync();
            return View(unplannedorders);
        }
    }
}

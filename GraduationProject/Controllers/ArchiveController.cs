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
            ViewData["AnnualCount"] = "0" + (_context.Orders.Where(o => o.Type == false && o.State == "2").Count()).ToString();
            ViewData["UnplannedCount"] = "0" + (_context.Orders.Where(o => o.Type == true && o.State == "2").Count().ToString());
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
        public IActionResult GetAnnualNeedOrders(int id)
                {
            //var user = await _userManager.GetUserAsync(User);

            var model = new List<AnnualNeedOrderViewModel>();  ///model list of ano for seecting thwm ot use as template

            var userid = userManager.GetUserId(User);
            var annualneedorders = _context.AnnualOrder.Include(o => o.Order)
                .Include(i => i.Item)
                .Where(x => x.OrderId == id)
                .Where(o => o.Order.Complete == true && o.Order.State == "2")
                .Where(o => o.Order.UserId == userid)
                .ToList();

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
        public IActionResult GetAnnualNeedOrders(List<AnnualNeedOrderViewModel> model, int id)
        {
            AnnualOrderController x = new AnnualOrderController(_context, userManager, userService);

            int orderid = x.GetAnnualNeedOrderid();


            var userid = userManager.GetUserId(User);
            //List<AnnualNeedOrderViewModel> AnnualOrderstoAdd = new();
            for (int i = 0; i < model.Count(); i++)
            {
                if(model[i].IsSelected ==true)
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
                     _context.Add(annualorderx);
                     _context.SaveChanges();

                   //AnnualOrderstoAdd.Add(model[i]);
                }
            }
            
            return RedirectToAction("GetAnnualNeedOrders", "AnnualOrder");

        }

        //Get All Unplanned Orders To Specific Order///// from archive
        [HttpGet]
        public IActionResult GetUnplannedOrders(int id)
        {
            var userid = userManager.GetUserId(User);
            var unplannedorders = _context.UnPlannedOrder.Include(o => o.Order)
                .Include(i => i.Item)
                .Where(x => x.OrderId == id)
                .Where(o => o.Order.UserId == userid)
                .Where(o => o.Order.Complete == true && o.Order.State == "2")
                .ToList();
            return View(unplannedorders);
        }
    }
}

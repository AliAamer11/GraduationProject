using GraduationProject.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Data.Models;

namespace GraduationProject.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }


        //f to check wether there is an annual need for this current year. in no create one
        public int AnnualNeedCheck()
        {
            DateTime CurrentDate = DateTime.Now;
            int CurrentYear = CurrentDate.Year;
            int ID;
            var model = _context.Orders.Where(x => x.CreatedAt.Year == CurrentYear).FirstOrDefault();
            ID = model.OrderID;
            if (model == null)
            {
                ////need to know how to get current user :|
                var order = new Order { CreatedAt = DateTime.Today, State = "0", Type = false,/*UserId??? User = currentuser,*/   };
                var model1 = _context.Orders.Add(order);
                ID = order.OrderID;
                _context.SaveChanges();

            }
            return ID;

        }

        ///chooose what t
        public IActionResult Index()
        {
            IEnumerable<Order> objList= _context.Orders;
            return View(objList);
        }
        
        //get All Annual Needs
        public IActionResult AllAnualNeeds()
        {
            //var model = _context.AnnualOrder.ToList();
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }

    }
}

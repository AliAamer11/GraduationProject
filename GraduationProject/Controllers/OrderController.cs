using GraduationProject.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using GraduationProject.ViewModels.AnnualNeedOrders;

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
        

        //get All Orders
        [HttpGet]
        public IActionResult Index()
        {

            IEnumerable<Order> objList= _context.Orders;
            return View(objList);
        }

        public IActionResult Create()
        {
            return View();
        }


    }
}

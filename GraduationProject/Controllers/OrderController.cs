using GraduationProject.Data;
using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers.VPControllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public  IActionResult Index()
        { 
            return  View();
        }

        public async Task<IActionResult> Unplanned()
        {
            var objlist = await _context.Orders.Where(o => o.Type == true).ToListAsync();
            return View(objlist);
        }

        public async Task<IActionResult> Annual ()
        {
            var objlist = await _context.Orders.Where(o => o.Type == false).ToListAsync();
            return View(objlist);
        }

    }
}

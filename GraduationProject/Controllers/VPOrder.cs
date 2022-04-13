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
    public class VPOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public VPOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Unplanned()
        {
            var objlist = await _context.Orders.Where(o => o.Complete == true & o.State == "1" & o.Type == true ).ToListAsync();
          
            return View(objlist);
        }

        public async Task<IActionResult> Annual()
        {
            var objlist = await _context.Orders.Where(o => o.Type == false & o.State == "1" & o.Complete==true).ToListAsync();
            return View(objlist);
        }

    }
}

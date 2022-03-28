using GraduationProject.Data;
using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
    public class UnplannedOrdersController : Controller
    {        
        private readonly ApplicationDbContext _context;

        public UnplannedOrdersController(ApplicationDbContext context)
        {
                _context = context;
        }

        public async Task<IActionResult> Index()
        {
           var objlist =await _context.UnPlannedOrder.ToListAsync();
            return View(objlist);
        }
    }
}

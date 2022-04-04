using GraduationProject.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers.RP
{
    public class UnplannedOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UnplannedOrderController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var models = _context.UnPlannedOrder.ToList();

            return View();
        }
    }
}

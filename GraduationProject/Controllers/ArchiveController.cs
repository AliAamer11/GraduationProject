using GraduationProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
    public class ArchiveController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ArchiveController(ApplicationDbContext context)
        {
            _context = context;
        }

        //decide what type of document to review
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AlLAnnualOrders()
        {

            return View();
        }

        ///gotta fix the user id for certain 
        //Get All AnnualNeed To Specific Order///// from archive
        [HttpGet]
        public IActionResult getAnnualNeedOrders(int id)
        {
            var annualneedorders = _context.AnnualOrder.Include(o => o.Order)
                .Include(i => i.Item)
                .Where(x => x.OrderId == id)
                .Where(o => o.Order.UserId == "1")
                .ToList();
            return View(annualneedorders);
        }

        //Get All Unplanned Orders To Specific Order///// from archive
        [HttpGet]
        public IActionResult getUnplannedOrders(int id)
        {
            var unplannedorders = _context.UnPlannedOrder.Include(o => o.Order)
                .Include(i => i.Item)
                .Where(x => x.OrderId == id)
                .ToList();
            return View(unplannedorders);
        }
    }
}

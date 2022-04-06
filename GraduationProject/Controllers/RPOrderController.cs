using GraduationProject.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
    public class RPOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RPOrderController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Home()
        {
            //AnnualNeedOrderCheck();
            return View();
        }
        public IActionResult CheckCompleteOrder(int id)
        {
            try
            {
                var order = _context.Orders.Find(id);
                order.Complete = true;    //the order has been complte from RP side
                order.State = "1";        //the order is now on the VP side
                return RedirectToAction("Index", "Archive");
            }
            catch
            {
                return NotFound();
            }
        }
    }
}

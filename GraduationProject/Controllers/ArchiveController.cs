using GraduationProject.Data;
using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;



namespace GraduationProject.Controllers
{
    [Authorize(Roles = "Requester")]
    public class ArchiveController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        private readonly ApplicationDbContext _context;
        public ArchiveController(ApplicationDbContext context,
                                UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _context = context;
        }

        //decide what type of document to review
        public IActionResult Index()
        {
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
            var userid = userManager.GetUserId(User);
            var annualneedorders = _context.AnnualOrder.Include(o => o.Order)
                .Include(i => i.Item)
                .Where(x => x.OrderId == id)
                .Where(o=>o.Order.Complete == true && o.Order.State =="2")
                .Where(o => o.Order.UserId == userid)
                .ToList();
            return View(annualneedorders);
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

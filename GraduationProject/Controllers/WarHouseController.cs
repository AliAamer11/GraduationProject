using Microsoft.AspNetCore.Mvc;
using GraduationProject.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using GraduationProject.Data.Models;
using GraduationProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GraduationProject.Controllers
{
    [Authorize(Roles = "VicePris")]
    public class WarHouseController : Controller
    {
        private readonly INotyfService _notyf;
        private readonly ApplicationDbContext _context;

        public WarHouseController(ApplicationDbContext context,
                       INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Items()
        {
            var items = await _context.Items.Include(i => i.Category).Include(i => i.Measurement).ToListAsync();
            foreach (var item in items)
            {
                if (item.MinimumRange > item.Quantity)
                {
                    _notyf.Error("إن كمية المادة" + " " + item.Name + " " + "ذات الرمز" + " " + item.BarCode + " " + "تخطت الحد الأدنى");
                }
            }
            return View(items);

        }
        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var items = await _context.Items.Include(i => i.Category).Include(i => i.Measurement).ToListAsync();
            foreach (var item in items)
            {
                if (item.MinimumRange > item.Quantity)
                {
                    _notyf.Error("إن كمية المادة" + " " + item.Name + " " + "ذات الرمز" + " " + item.BarCode + " " + "تخطت الحد الأدنى");
                }
            }
            return View(items);

        }
    }
}

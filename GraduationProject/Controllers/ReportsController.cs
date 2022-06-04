using GraduationProject.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels;

namespace GraduationProject.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReportsController(ApplicationDbContext context)
        {


            _context = context;
        }
        public IActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public IActionResult ItemReport()
        {
            var allitems = _context.Items.Include(c => c.Category).Include(m => m.Measurement).ToList();
            List<ItemsDatagridViewModel> itemsDatagrid = new List<ItemsDatagridViewModel>();
            foreach (var item in allitems)
            {
                ItemsDatagridViewModel model = new ItemsDatagridViewModel();
                model.ItemID = item.ItemID;
                model.ItemName = item.Name;
                model.BarCode = item.BarCode;
                model.Quantity = item.Quantity;
                model.MinimumRange = item.MinimumRange;
                model.Status = item.Status;
                model.Note = item.Note;
                model.Category = item.Category.Name;
                model.Measurement = item.Measurement.Name;
                itemsDatagrid.Add(model);
            }
            string jsonString = System.Text.Json.JsonSerializer.Serialize(itemsDatagrid);
            ViewBag.items = jsonString;
            return View();
        }
    }
}

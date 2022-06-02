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
    public class WarHouseController : Controller
    {
        private readonly ApplicationDbContext _context;
        public WarHouseController(ApplicationDbContext context)
        {


            _context = context;
        }
        public IActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public IActionResult test()
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

            //List<Items> model = new List<Items>();
            //model = allitems;
            //for (int i = 0; i < allitems.Count; i++)
            //{
            //    model[i].ItemID = allitems[i].ItemID;
            //    model[i].Name = allitems[i].Name;
            //    model[i].Quantity = allitems[i].Quantity;
            //    model[i].Status = allitems[i].Status;
            //    model[i].Note = allitems[i].Note;
            //    model[i].Measurement.Name = allitems[i].Measurement.Name;
            //    model[i].Category.Name = allitems[i].Category.Name;
            //    model.Add(model[i]);
            //}
            //string jsonString = System.Text.Json.JsonSerializer.Serialize(model);
            //ViewBag.items = jsonString;

            //var itemss = _context.Items.ToList();
            //string jsonString = System.Text.Json.JsonSerializer.Serialize(itemss);
            //ViewBag.items = jsonString;
            //Console.WriteLine(jsonString);
        }
    }
}

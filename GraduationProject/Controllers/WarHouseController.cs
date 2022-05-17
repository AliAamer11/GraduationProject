using GraduationProject.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json;



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

        public IActionResult test()
        {
            var itemss = _context.Items.ToList();
            string jsonString = System.Text.Json.JsonSerializer.Serialize(itemss);
            ViewBag.items = jsonString;
            //Console.WriteLine(jsonString);
            return View();
        }
    }
}

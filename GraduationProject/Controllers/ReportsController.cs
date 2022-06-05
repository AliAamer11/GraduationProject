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
using Microsoft.AspNetCore.Authorization;

namespace GraduationProject.Controllers
{
    [Authorize(Roles = "StoreKeep,Admin")]
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
    }
}

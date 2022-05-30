using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using GraduationProject.Data;
using GraduationProject.ViewModels.Items;
using Microsoft.EntityFrameworkCore;
using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Authorization;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace GraduationProject.Controllers
{
    [Authorize(Roles = "StoreKeep")]
    public class ItemsController : Controller
    {
        private readonly INotyfService _notyf;
        private readonly ApplicationDbContext _context;
        public ItemsController(ApplicationDbContext context,
                               INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        //get a list Items
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _context.Items.Include(i => i.Category).Include(i => i.Measurement).ToListAsync();
            foreach (var item in items)
            {
                if (item.MinimumRange > item.Quantity && item.ExceededMinimumRange > 0)
                {
                    _notyf.Error("إن كمية المادة" + " " + item.Name + " " + "ذات الرمز" + " " + item.BarCode + " " + "تخطت الحد الأدنى");
                }
            }
            return View(items);
        }
        //show how many time items was ExcededMinimumRange
        [HttpGet]
        public IActionResult ExcededMinimumRange()
        {
            ExcededMinimumRangeViewModel viewModel = new ExcededMinimumRangeViewModel();
            var items = _context.Items.Where(i => i.ExceededMinimumRange > 0).Select(i => new
            {
                Name = i.Name,
                ExceededMinimumRange = i.ExceededMinimumRange,
            }).OrderBy(i => i.ExceededMinimumRange).ToList();

            if (items == null)
            {
                return View(viewModel);
            }
            foreach (var item in items)
            {
                viewModel.Itemsname.Add(item.Name);
                viewModel.ItemsCountofExcededMinimumRange.Add(item.ExceededMinimumRange);
            }
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, "CategoryID", "Name");
            ViewData["MeasurementId"] = new SelectList(_context.Measurements, "MeasurmentID", "Name");
            ViewData["Status"] = new SelectList(bindListforStatus(), "Value", "Text");
            return View();
        }


        //Post create item
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(CreateItemViewModel viewModel)
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, "CategoryID", "Name");
            ViewData["MeasurementId"] = new SelectList(_context.Measurements, "MeasurmentID", "Name");
            ViewData["Status"] = new SelectList(bindListforStatus(), "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    if (existedItem(viewModel.Name))
                    {
                        ViewBag.errorMassage = "إن المادة" + " " + viewModel.Name + " " + "موجودة مسبقاً";
                        return View(viewModel);
                    }
                    string barCode = genereteBarCode(viewModel.Status, viewModel.CategoryId);
                    string serialNumber = generetSerialNumber(barCode);
                    barCode = barCode + "" + serialNumber;


                    var item = new Items()
                    {
                        Name = viewModel.Name,
                        BarCode = barCode,
                        Status = viewModel.Status,
                        Quantity = 0,
                        MinimumRange = viewModel.MinimumRange,
                        ExceededMinimumRange = 0,
                        Note = viewModel.Note,
                        CategoryId = viewModel.CategoryId,
                        MeasurementId = viewModel.MeasurementId,
                    };
                    //add item to data base 
                    _context.Add(item);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Items");
                }
                catch
                {
                    return StatusCode(500, "Internal server error");
                }
            }
            ModelState.AddModelError("", "الحقول هذه مطلوبة");
            return View(viewModel);
        }

        // GET: item/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? itemId)
        {
            if (itemId == null)
            {
                return NotFound();
            }
            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
            {
                return NotFound();
            }

            var editItemViewModel = new editItemViewModel
            {
                ItemID = item.ItemID,
                Name = item.Name,
                Barcode = item.BarCode,
                MinimumRange = item.MinimumRange,
                Note = item.Note,
            };
            return View(editItemViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(editItemViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existedItem = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == viewModel.ItemID);
                    existedItem.Note = viewModel.Note;
                    existedItem.MinimumRange = viewModel.MinimumRange;
                    _context.Update(existedItem);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Items");
                }
                catch (DbUpdateConcurrencyException)
                {

                }
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "زبط حقولك");
            return View(viewModel);
        }

        /// <summary>
        /// this bind list for select status in dropdown menue box 
        /// </summary>
        /// <returns>dropdownlist with key and value</returns>
        private List<SelectListItem> bindListforStatus()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Text = "مستهلكة", Value = "1" });
            list.Add(new SelectListItem { Text = "ثابثة", Value = "2" });
            list.Add(new SelectListItem { Text = "تالفة", Value = "3" });
            return list;
        }

        /// <summary>
        /// this method is for generete BarCode 
        /// we need to use alot of Parameter 
        /// </summary>
        /// <returns></returns>
        private string genereteBarCode(string status, int categoryId)
        {
            // this logic to get MainCategory and subCategory shortcutName
            var category = _context.Category.Include(c => c.MainCategory).FirstOrDefault(c => c.CategoryID == categoryId);
            string maincategorybarcode = "";
            string subcategorybarcode = "";
            if (category.MainCategoryId == null)
            {
                maincategorybarcode = category.ShortCutName;
                subcategorybarcode = "000";
            }
            else
            {
                maincategorybarcode = category.MainCategory.ShortCutName;
                subcategorybarcode = category.ShortCutName;
            }
            string barcode = status + "" + maincategorybarcode + "" + subcategorybarcode;
            return barcode;
        }

        private string generetSerialNumber(string barCode)
        {
            var serialNumber = _context.Items.Where(i => i.BarCode.StartsWith(barCode)).Count();
            serialNumber = serialNumber + 1;
            //make serial number as 4 digit 
            string fullserialnumber = makeserialnumber4digit(serialNumber);
            return fullserialnumber;
        }



        private string makeserialnumber4digit(int serialnumber)
        {
            return serialnumber.ToString().PadLeft(4, '0');
        }

        /// <summary>
        /// to lock if item was already existed
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns>true if item duplicate otherwise flase</returns>
        private bool existedItem(string itemName)
        {
            return _context.Items.Any(i => i.Name == itemName);
        }
    }
}

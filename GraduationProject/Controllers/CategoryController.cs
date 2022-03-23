using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using GraduationProject.Data;
using Microsoft.EntityFrameworkCore;
using GraduationProject.ViewModels.Category;
using GraduationProject.Data.Models;

namespace GraduationProject.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var Categories = _context.Category.Include(c => c.Category).ToList();
            return View(Categories);
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["MainCategory"] = new SelectList(bindListforCategory(), "Value", "Text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryViewModel viewmodel)
        {
            ViewData["MainCategory"] = new SelectList(bindListforCategory(), "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    //check for Duplicate The CategoryName
                    if (checkCategoryName(viewmodel.Name))
                    {
                        ViewBag.errorMassage = "هذا الصنف موجود بالفعل";
                        return View(viewmodel);
                    }

                    //check for Duplicate The ShortCutName
                    if (checkShortCutName(viewmodel.ShortcutName))
                    {
                        ViewBag.errorMassage = "هذا الترميز موجود بالفعل لصنف آخر";
                        return View(viewmodel);
                    }
                    //check for Category ShortcuntName length
                    if (checkforCategoryShortcutnameLength(viewmodel.MainCategoryId, viewmodel.ShortcutName))
                    {
                        ViewBag.errorMassage = "ترميز الصنف الرئيسي 2 بينما ترميز الصنف الفرعي 3";
                        return View(viewmodel);
                    }
                    // so the Category dosen't Has MainCategory
                    if (viewmodel.MainCategoryId == -1)
                    {
                        viewmodel.MainCategoryId = null;
                    }

                    //Initial the Category 
                    var category = new Categoreis
                    {
                        Name = viewmodel.Name,
                        ShortCutName = viewmodel.ShortcutName,
                        MainCategoryId = viewmodel.MainCategoryId,
                    };
                    //add category to Database
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Category");
                }
                catch
                {
                    throw;
                }
            }


            ModelState.AddModelError("", "الحقول هذه مطلوبة");
            return View();

        }
        // GET: Category/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var editCategoryViewModel = new EditCategoryViewModel()
            {
                CategoryID = category.CategoryID,
                Name = category.Name,
                ShortCutName = category.ShortCutName,
                MainCategoryId = category.MainCategoryId,
            };
            ViewData["MainCategory"] = new SelectList(bindListforCategory(category.CategoryID), "Value", "Text");
            return View(editCategoryViewModel);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCategoryViewModel viewModel, int id)
        {
            ViewData["MainCategory"] = new SelectList(bindListforCategory(id), "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    //check for Duplicate The CategoryName
                    if (checkCategoryName(viewModel.Name, id))
                    {
                        ViewBag.errorMassage = "هذا الصنف موجود بالفعل";
                        return View(viewModel);
                    }

                    //check for Duplicate The ShortCutName
                    if (checkShortCutName(viewModel.ShortCutName, id))
                    {
                        ViewBag.errorMassage = "هذا الترميز موجود بالفعل لصنف آخر";
                        return View(viewModel);
                    }
                    // so the Category dosen't Has MainCategory
                    if (viewModel.MainCategoryId == -1)
                    {
                        viewModel.MainCategoryId = null;
                    }
                    var category = new Categoreis()
                    {
                        CategoryID = viewModel.CategoryID,
                        Name = viewModel.Name,
                        ShortCutName = viewModel.ShortCutName,
                        MainCategoryId = viewModel.MainCategoryId,
                    };
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!categoryExists(viewModel.CategoryID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "زبط حقولك");
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                //so the category isn't found
                // TODO make this logic better
                return RedirectToAction(nameof(Index));
            }
            _context.Category.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// this method to check if CategoryName Duplicate or not
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true if Duplicate otherwise false</returns>
        private bool checkCategoryName(string name)
        {
            bool check = false;
            var categoriesName = _context.Category.Select(x => new
            {
                categoryName = x.Name
            });

            foreach (var item in categoriesName)
            {
                if (item.categoryName == name)
                {
                    check = true;
                    break;
                }
            }
            return check;
        }

        /// <summary>
        /// this method to check if CategoryName Duplicate or not for Update 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true if Duplicate otherwise false</returns>
        private bool checkCategoryName(string name, int id)
        {
            bool check = false;
            var categoriesName = _context.Category.Where(c => c.CategoryID != id).Select(x => new
            {
                categoryName = x.Name
            });

            foreach (var item in categoriesName)
            {
                if (item.categoryName == name)
                {
                    check = true;
                    break;
                }
            }
            return check;
        }

        /// <summary>
        /// this method to check if ShortCutName Duplicate or not
        /// </summary>
        /// <param name="shortCut"></param>
        /// <returns>true if Duplicate otherwise false</returns>
        private bool checkShortCutName(string shortCut)
        {
            bool check = false;
            var categoriesShortCutName = _context.Category.Select(x => new
            {
                categoryshortcutName = x.ShortCutName
            });

            foreach (var item in categoriesShortCutName)
            {
                if (item.categoryshortcutName == shortCut)
                {
                    check = true;
                    break;
                }
            }
            return check;
        }

        /// <summary>
        /// this method to check if ShortCutName Duplicate or not for Update
        /// </summary>
        /// <param name="shortCut"></param>
        /// <returns>true if Duplicate otherwise false</returns>
        private bool checkShortCutName(string shortCut, int id)
        {
            bool check = false;
            var categoriesShortCutName = _context.Category.Where(c => c.CategoryID != id).Select(x => new
            {
                categoryshortcutName = x.ShortCutName
            });

            foreach (var item in categoriesShortCutName)
            {
                if (item.categoryshortcutName == shortCut)
                {
                    check = true;
                    break;
                }
            }
            return check;
        }


        private bool checkforCategoryShortcutnameLength(int? categoryId, string shortCutName)
        {
            bool check = false;
            if (categoryId == -1 && shortCutName.Length != 2)
            {
                // so this category is main category and shortcutname should be 2
                check = true;
            }
            else if (categoryId != -1 && shortCutName.Length != 3)
            {
                // so this category is subcategory and shortcutname should be 3
                check = true;

            }
            return check;
        }

        private bool categoryExists(int id) => _context.Category.Any(c => c.CategoryID == id);

        /// <summary>
        /// this method for return A selectList of Categories
        /// </summary>
        /// <returns>SelectList Item</returns>
        private List<SelectListItem> bindListforCategory()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Text = " ", Value = "-1" });

            var category = _context.Category.Where(c=>c.MainCategoryId==null).ToList();
            foreach (var item in category)
            {
                list.Add(new SelectListItem { Text = item.Name, Value = item.CategoryID.ToString() });
            }
            return list;
        }
        /// <summary>
        /// this method for return A selectList of Categories for Update
        /// </summary>
        /// <returns>SelectList Item</returns>
        private List<SelectListItem> bindListforCategory(int id)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Text = " ", Value = "-1" });

            var category = _context.Category.Where(c => c.CategoryID != id && c.MainCategoryId==null).ToList();
            foreach (var item in category)
            {
                list.Add(new SelectListItem { Text = item.Name, Value = item.CategoryID.ToString() });
            }
            return list;
        }
    }
}

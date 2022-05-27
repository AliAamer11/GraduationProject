using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using GraduationProject.Data;
using Microsoft.EntityFrameworkCore;
using GraduationProject.ViewModels.Measurements;
using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace GraduationProject.Controllers
{
    [Authorize(Roles = "StoreKeep")]
    public class MeasurementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MeasurementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(bool isDeleted = false, bool isnotDeleted = false)
        {
            ViewBag.isDeleted = isDeleted;
            ViewBag.isnotDeleted = isnotDeleted;
            var measurements = _context.Measurements.ToList();
            return View(measurements);
        }

        [HttpGet]
        public IActionResult Create()
        {
            CreateMeasurementViewModel model = new CreateMeasurementViewModel();
            return PartialView("_MeasurementModelPartial", model);
        }

        [HttpPost]
        public IActionResult Create(CreateMeasurementViewModel model)
        {
            if (checkMeasurementyName(model.Name))
            {
                ViewBag.errorMassage = "هذا القياس موجود بالفعل";
                return View(model);
            }
            var measurements = new Measurements
            {
                Name = model.Name
            };
            _context.Measurements.Add(measurements);
            _context.SaveChanges();
            return PartialView("_MeasurementModelPartial", model);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(CreateMeasurementViewModel viewModel)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            //check for Duplicate The CategoryName
        //            if (checkMeasurementyName(viewModel.Name))
        //            {
        //                ViewBag.errorMassage = "هذا القياس موجود بالفعل";
        //                return View(viewModel);
        //            }
        //            //Initial the Measurement 
        //            var measurements = new Measurements
        //            {
        //                Name = viewModel.Name
        //            };
        //            //add Measurement to Database
        //            _context.Add(measurements);
        //            await _context.SaveChangesAsync();
        //            return RedirectToAction("Index", "Measurements");
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //    }
        //    ModelState.AddModelError("", "الحقول هذه مطلوبة");
        //    return View();
        //}

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var measurement = _context.Measurements.Find(id);
            if (measurement == null)
            {
                return NotFound();
            }
            var editmeasurmentviewmodel = new EditMeasurementViewModel()
            {
                MeasurementID = measurement.MeasurmentID,
                Name = measurement.Name
            };
            return PartialView("_EditMeasurementModelPartial", editmeasurmentviewmodel);
            //return View(editmeasurmentviewmodel);
        }

        [HttpPost]
        public IActionResult Edit(EditMeasurementViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (checkMeasurementyName(viewModel.Name, viewModel.MeasurementID))
                    {
                        ViewBag.errorMassage = "هذا القياس موجود بالفعل";
                        return PartialView("_EditMeasurementModelPartial", viewModel);
                    }
                    var measurement = new Measurements()
                    {
                        MeasurmentID = viewModel.MeasurementID,
                        Name = viewModel.Name,
                    };
                    _context.Update(measurement);
                    _context.SaveChanges();
                    return PartialView("_EditMeasurementModelPartial", viewModel);

                }
                catch
                {
                    throw;
                }
            }
            return View();
        }

        //Post
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(EditMeasurementViewModel viewModel, int id)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            //check for Duplicate The CategoryName
        //            if (checkMeasurementyName(viewModel.Name, id))
        //            {
        //                ViewBag.errorMassage = "هذا القياس موجود بالفعل";
        //                return View(viewModel);
        //            }

        //            var measurement = new Measurements()
        //            {
        //               MeasurmentID = viewModel.MeasurementID,
        //               Name = viewModel.Name,
        //            };
        //            _context.Update(measurement);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!MeasurementExists(viewModel.MeasurementID))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ModelState.AddModelError("", "زبط حقولك");
        //    return View(viewModel);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var measurement = await _context.Measurements.FindAsync(id);
            if (measurement == null)
            {
                //so the category isn't found
                // TODO make this logic better

                return RedirectToAction(nameof(Index));
            }
            if (checkifMeasurmentHasItems(measurement.MeasurmentID))
            {
                return RedirectToAction(nameof(Index), new { isnotDeleted = true, isDeleted = false });
            }
            _context.Measurements.Remove(measurement);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { isnotDeleted = false, isDeleted = true });
        }

        //get the items that below to measurment
        [HttpGet]
        public IActionResult ExistedItems(int? measurementId)
        {
            if (measurementId == null)
            {
                return NotFound();
            }
            var measument = _context.Measurements.FirstOrDefault(m => m.MeasurmentID == measurementId);
            var items = _context.Items.Include(i => i.Measurement).Include(i => i.Category).Where(i => i.MeasurementId == measurementId).ToList();
            ViewBag.measurmentName = measument.Name;
            return View(items);
        }

        /// <summary>
        /// when delete the category we will check if this category has items or not 
        /// </summary>
        /// <param name="measurementId"></param>
        /// <returns>true if category has items otherwise false</returns>
        private bool checkifMeasurmentHasItems(int measurmentId)
        {
            return _context.Items.Any(i => i.MeasurementId == measurmentId);
        }


        /// <summary>
        /// this method to check if measurementName Duplicate or not
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true if Duplicate otherwise false</returns>
        private bool checkMeasurementyName(string name)
        {
            bool check = false;
            var measurementsName = _context.Measurements.Select(x => new
            {
                measurementName = x.Name
            });

            foreach (var item in measurementsName)
            {
                if (item.measurementName == name)
                {
                    check = true;
                    break;
                }
            }
            return check;
        }

        /// <summary>
        /// this method to check if measurementName Duplicate or not
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true if Duplicate otherwise false</returns>
        private bool checkMeasurementyName(string name, int id)
        {
            bool check = false;
            var measurementsName = _context.Measurements.Where(m => m.MeasurmentID != id).Select(x => new
            {
                measurementName = x.Name
            });

            foreach (var item in measurementsName)
            {
                if (item.measurementName == name)
                {
                    check = true;
                    break;
                }
            }
            return check;
        }

        private bool MeasurementExists(int id) => _context.Measurements.Any(m => m.MeasurmentID == id);

    }
}

using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels.AnnualNeedOrders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
    public class AnnualOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnnualOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public int AnnualNeedOrderCheck()
        {
            DateTime CurrentDate = DateTime.Now;
            int CurrentYear = CurrentDate.Year;
            int ID;
            var model = _context.Orders.Where(x => x.CreatedAt.Year == CurrentYear && x.Type == false).FirstOrDefault();
            if (model == null)
            {
                var order = new Order { CreatedAt = DateTime.Today, State = "0", Type = false, UserId = "1" };
                var model1 = _context.Orders.Add(order);
                ID = order.OrderID;
                _context.SaveChanges();
                return ID;
            }
            else if (model.Complete == true)  ////if the annaul order exists but its complete
            { return 0; }
            ID = model.OrderID;

            return ID;

        }
        public IActionResult Index()
        {
            var annualneeds = _context.Orders
                .Where(x => x.Type == false).ToList();
            return View(annualneeds);
        }

        //Get All AnnualNeed
        [HttpGet]
        public IActionResult getAnnualNeedOrders()
        {

            int id = AnnualNeedOrderCheck();
            if (id > 0)
            {
                var annualneedorders = _context.AnnualOrder.Include(o => o.Order)
                    .Include(i => i.Item)
                    .Where(x => x.OrderId == id)
                    .ToList();
                return View(annualneedorders);
            }
            else
                ///either return the user to home page or let him view the annual need withoput option of alteration
                return RedirectToAction("Home", "Order");
        }

        [HttpGet]
        public IActionResult CreateAnnualNeedOrder()
        {
            ViewData["Item"] = new SelectList(BindListforItem(), "Value", "Text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnualNeedOrder(CreateAnnualNeedOrderViewModel viewModel)
        {
            ViewData["OrderId"] = AnnualNeedOrderCheck();
            ViewData["Item"] = new SelectList(BindListforItem(), "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {

                    var annualorder = new AnnualOrder
                    {
                        AnnualOrderID = viewModel.AnnualOrderID,
                        ItemId = viewModel.ItemId,
                        FirstSemQuantity = viewModel.FirstSemQuantity,
                        SecondSemQuantity = viewModel.SecondSemQuantity,
                        ThirdSemQuantity = viewModel.ThirdSemQuantity,
                        FlowRate = viewModel.FlowRate,
                        ApproxRate = viewModel.ApproxRate,
                        OrderId = viewModel.OrderId,

                    };
                    _context.Add(annualorder);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("getAnnualNeedOrders");
                }
                catch
                { throw; }
            }
            ModelState.AddModelError("", "الحقول هذه مطلوبة");
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditAnnualNeedOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var annualorder = await _context.AnnualOrder.FindAsync(id);
            if (annualorder == null)
            {
                return NotFound();
            }
            var editannualorderViewModel = new EditAnnualNeedOrderViewModel()
            {
                ItemId = annualorder.ItemId,
                AnnualOrderID = annualorder.AnnualOrderID,
                FirstSemQuantity = annualorder.FirstSemQuantity,
                SecondSemQuantity = annualorder.SecondSemQuantity,
                ThirdSemQuantity = annualorder.ThirdSemQuantity,
                FlowRate = annualorder.FlowRate,
                ApproxRate = annualorder.ApproxRate,
                OrderId = annualorder.OrderId,
            };
            ViewData["Item"] = new SelectList(BindListforItem(), "Value", "Text");
            return View(editannualorderViewModel);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnnualNeedOrder(EditAnnualNeedOrderViewModel viewModel, int id)
        {
            ViewData["Item"] = new SelectList(BindListforItem(), "Value", "Text");
            if (ModelState.IsValid)
            {
                try
                {
                    var annualorder = new AnnualOrder()
                    {
                        ItemId = viewModel.ItemId,
                        AnnualOrderID = viewModel.AnnualOrderID,
                        FirstSemQuantity = viewModel.FirstSemQuantity,
                        SecondSemQuantity = viewModel.SecondSemQuantity,
                        ThirdSemQuantity = viewModel.ThirdSemQuantity,
                        FlowRate = viewModel.FlowRate,
                        ApproxRate = viewModel.ApproxRate,
                        OrderId = viewModel.OrderId,
                    };
                    _context.Update(annualorder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!annualOrderExists(viewModel.AnnualOrderID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(getAnnualNeedOrders));
            }
            ModelState.AddModelError("", "تأكد من صحة الحقول");
            return View(viewModel);
        }
        public async Task<IActionResult> DeleteAnnualNeedOrder(int id)
        {
            var annualorder = await _context.AnnualOrder.FindAsync(id);
            if (annualorder == null)
            {
                return RedirectToAction(nameof(Index));
            }
            _context.AnnualOrder.Remove(annualorder);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(getAnnualNeedOrders));
        }

        /// <summary>
        /// this is just fr binding items in dropdownlist for create
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> BindListforItem()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var itemlist = _context.Items.ToList();
            foreach (var item in itemlist)
            {
                list.Add(new SelectListItem { Text = item.Name, Value = item.ItemID.ToString() });
            }
            return list;
        }

        private bool annualOrderExists(int id) => _context.AnnualOrder.Any(c => c.AnnualOrderID == id);

    }
}

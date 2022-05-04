using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels.AnnualNeedOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace GraduationProject.Controllers
{
    [Authorize(Roles = "Requester")]

    public class AnnualOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;

        public AnnualOrderController(ApplicationDbContext context,
                    UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
            _context = context;
        }

        internal string CheckOrderState(int id)
        {
            var model = _context.Orders.Find(id);

            //still in editing 
            if (model.Complete == false && model.State == "0")
                return "incomplete_rp_side";

            //being reviewed
            else if (model.Complete == true && model.State == "1")
                return "complete_vp_side";

            //being re-altered according to comments
            else if (model.Complete == true && model.State == "0") // then i set it to incomplete manually on user side
                return "complete_rp_side";

            //archive
            else if (model.Complete == true && model.State == "2")
                return "complete_sk_side";
            return "error";
        }
        public int GetAnnualNeedOrderid()
        {
            var userid = userManager.GetUserId(User);
            DateTime CurrentDate = DateTime.Now;
            int CurrentYear = CurrentDate.Year;

            var model = _context.Orders
                .OrderBy(o => o.CreatedAt)
                .Where(x => x.CreatedAt.Year == CurrentYear
                                            && x.Type == false
                                            && x.UserId == userid
                                            ).LastOrDefault();


            if (model == default) //to check if there is an annual need initialized or not
            {
                var order = new Order
                {
                    CreatedAt = DateTime.Today,
                    State = "0",
                    Type = false,
                    UserId = userid
                };
                var model1 = _context.Orders.Add(order);
                _context.SaveChanges();
                return order.OrderID;
            }
            return model.OrderID;

        }
        public IActionResult Index()
        {
            var userid = userManager.GetUserId(User);

            var annualneeds = _context.Orders
                .Where(x => x.Type == false)   //// annual order type
                .Where(o => o.UserId == userid)    /// getting orders to this current user
                .Where(o=> o.Complete==true && o.State =="2")  ///getting orders that are complete on StoreKeeper side
                .ToList();
            return View(annualneeds);
        }

        //Get All AnnualNeed to Order where it is still not complete
        [HttpGet]
        public IActionResult getAnnualNeedOrders()
        {
            var userid = userManager.GetUserId(User);

            int id = GetAnnualNeedOrderid();
            //if (id > 0)
            //{
                var annualneedorders = _context.AnnualOrder.Include(o => o.Order)
                    .Include(i => i.Item)
                    .Where(x => x.OrderId == id)
                    .Where(o => o.Order.Type == false && o.Order.Complete == false)
                    .Where(o => o.Order.UserId == userid)
                    .ToList();
            ViewData["State"] = CheckOrderState(id);
            ViewData["id"] = id;

            //ViewData["id"] = annualneedorders.FirstOrDefault().AnnualOrderID;
            return View(annualneedorders);
            //else
            //    ///either return the user to home page or let him view the annual need withoput option of alteration
            //    return RedirectToAction("Home", "Order");
        }



        //Get All AnnualNeed to Order where it is still  complete & needs alteration
        public IActionResult GetAnnualNeedAltered()
        {
            var userid = userManager.GetUserId(User);
            int id = GetAnnualNeedOrderid();

            var annualneedorders = _context.AnnualOrder.Include(o => o.Order)
                    .Include(i => i.Item)
                    .Where(x => x.OrderId == id)
                    .Where(x=> x.Comment!=null)
                    .Where(o => o.Order.Type == false && o.Order.Complete == true)
                    .Where(o => o.Order.UserId == userid)
                    .ToList();
            ViewData["id"] = id;
            return View(annualneedorders);
        }

        public IActionResult GetAnnualNeedsDisplay()
        {
            var userid = userManager.GetUserId(User);
            int id = GetAnnualNeedOrderid();

            var annualneedorders = _context.AnnualOrder.Include(o => o.Order)
                    .Include(i => i.Item)
                    .Where(x => x.OrderId == id)
                    .Where(o => o.Order.Type == false && o.Order.Complete == true && o.Order.State != "0")
                    .Where(o => o.Order.UserId == userid)
                    .ToList();
            ViewData["id"] = id;
            return View(annualneedorders);
        }

        [HttpGet]
        public IActionResult CreateAnnualNeedOrder()
        {
            ViewData["OrderId"] = GetAnnualNeedOrderid();
            ViewData["Item"] = new SelectList(BindListforItem(), "Value", "Text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnualNeedOrder(CreateAnnualNeedOrderViewModel viewModel)
        {
            ViewData["OrderId"] = GetAnnualNeedOrderid();
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
                        Description = viewModel.Description,
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
            var annualorder = await _context.AnnualOrder.Include(o=>o.Order).Where(i=>i.AnnualOrderID==id).FirstOrDefaultAsync();
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
                Description = annualorder.Description,
                Comment = annualorder.Comment,
                FlowRate = annualorder.FlowRate,
                ApproxRate = annualorder.ApproxRate,
                OrderId = annualorder.OrderId,
                Order = annualorder.Order,

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
                    var annualorder = await _context.AnnualOrder.Include(o => o.Order).FirstOrDefaultAsync(i=>i.AnnualOrderID==viewModel.AnnualOrderID);

                    annualorder.ItemId = viewModel.ItemId;
                    annualorder.AnnualOrderID = viewModel.AnnualOrderID;
                    annualorder.FirstSemQuantity = viewModel.FirstSemQuantity;
                    annualorder.SecondSemQuantity = viewModel.SecondSemQuantity;
                    annualorder.ThirdSemQuantity = viewModel.ThirdSemQuantity;
                    annualorder.Description = viewModel.Description;
                    annualorder.FlowRate = viewModel.FlowRate;
                    annualorder.Comment = viewModel.Comment;
                    annualorder.ApproxRate = viewModel.ApproxRate;
                    annualorder.OrderId = viewModel.OrderId;
                       

                    _context.Update(annualorder);
                    await _context.SaveChangesAsync();
                    if (annualorder.Order.Complete == true)
                    { return RedirectToAction("GetAnnualNeedAltered"); }
                   
                    return RedirectToAction(nameof(getAnnualNeedOrders));

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnnualOrderExists(viewModel.AnnualOrderID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
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
            List<SelectListItem> list = new();
            var itemlist = _context.Items.ToList();
            foreach (var item in itemlist)
            {
                list.Add(new SelectListItem { Text = item.Name, Value = item.ItemID.ToString() });
            }
            return list;
        }

        private bool AnnualOrderExists(int id) => _context.AnnualOrder.Any(c => c.AnnualOrderID == id);

    }
}

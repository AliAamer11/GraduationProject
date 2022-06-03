using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels;
using GraduationProject.ViewModels.OutPutDocument;
using GraduationProject.ViewModels.Warehouse;
using GraduationProject.ViewModels.WareHouse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
    public class WarehouseController : Controller
    {
        private readonly ApplicationDbContext _context;
        public WarehouseController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Stagnant()
        {
            //test
            //first   groupby itemid and get the max date of each item 
            var ItemOutputDocumentMaxDate = await _context.OutPutDocumentDetails.GroupBy(i => i.ItemId).Select(o => new { ItemId = o.Key, MaxDate = o.Max(o => o.CreatedAt) }).ToListAsync();
            List<StagnantItemsViewModel> StagnantItems = new List<StagnantItemsViewModel>();

            //2 loop threw grouped items 
            foreach (var obj in ItemOutputDocumentMaxDate)
            {
                //Max date for inputdocument item only the items that have an outputdocument 
                //returns date time
                var IteminputdocumentdetailMaxDate = _context.InputDocumentDetails.Include(i => i.InputDocument).Where(i => i.ItemId == obj.ItemId).Max(i => i.InputDocument.CreatedAt);
                // returns a var of type InputDocumentDetail
                var Iteminputdocumentdetail = await _context.InputDocumentDetails.Include(i => i.InputDocument).Include(i => i.Item).Where(i => i.ItemId == obj.ItemId & i.InputDocument.CreatedAt == IteminputdocumentdetailMaxDate).FirstOrDefaultAsync();
                //Add to view list 
                StagnantItemsViewModel model = new StagnantItemsViewModel();
                //model.ItemID = Iteminputdocumentdetail.Item.ItemID;
                //model.Name = Iteminputdocumentdetail.Item.Name;
                model.InputDocumentDate = IteminputdocumentdetailMaxDate;
                model.OutPutDocumentDate = obj.MaxDate;


                StagnantItems.Add(model);

            }







            return View(StagnantItems);
        }

        [HttpPost]
        public async Task<IActionResult> Stagnant(int period)
        {
            //test
            //first   groupby itemid and get the max date of each item 
            var ItemOutputDocumentMaxDate = await _context.OutPutDocumentDetails.GroupBy(i => i.ItemId).Select(o => new { ItemId = o.Key, MaxDate = o.Max(o => o.CreatedAt) }).ToListAsync();
            List<StagnantItemsViewModel> StagnantItems = new List<StagnantItemsViewModel>();

            //2 loop threw grouped items 
            foreach (var obj in ItemOutputDocumentMaxDate)
            {
                //Max date for inputdocument item only the items that have an outputdocument 
                //returns date time
                var IteminputdocumentdetailMaxDate = await _context.InputDocumentDetails.Include(i => i.InputDocument).Where(i => i.ItemId == obj.ItemId).MaxAsync(i => i.InputDocument.CreatedAt);
                // returns a var of type InputDocumentDetail
                var Iteminputdocumentdetail = await _context.InputDocumentDetails.Include(i => i.InputDocument).Include(i => i.Item).Where(i => i.ItemId == obj.ItemId & i.InputDocument.CreatedAt == IteminputdocumentdetailMaxDate).FirstOrDefaultAsync();
                //Add to view list 
                StagnantItemsViewModel model = new StagnantItemsViewModel();
                //model.ItemID = Iteminputdocumentdetail.Item.ItemID;
                //model.Name = Iteminputdocumentdetail.Item.Name;
                model.InputDocumentDate = IteminputdocumentdetailMaxDate;
                model.OutPutDocumentDate = obj.MaxDate;


                StagnantItems.Add(model);

            }

            int TempPeriod;
            foreach (var m in StagnantItems.ToList())
            {
                StagnantItemsViewModel model = new StagnantItemsViewModel();
                if (m.InputDocumentDate > m.OutPutDocumentDate)
                {
                    TempPeriod = m.InputDocumentDate.Year - m.OutPutDocumentDate.Year;
                    if (TempPeriod >= period)
                    {
                        var items = await _context.OutPutDocumentDetails.Include(i => i.Item).Where(o => o.CreatedAt.Year == TempPeriod).ToListAsync();
                        foreach (var item in items)
                        {
                            model.Name = item.Item.Name;
                        }
                    }
                }
                else
                {
                    TempPeriod = m.OutPutDocumentDate.Year - m.InputDocumentDate.Year;
                    if (TempPeriod >= period)
                    {
                        var items = await _context.OutPutDocumentDetails
                            .Include(i => i.Item)
                            .Where(o => o.CreatedAt.Year >= m.InputDocumentDate.Year && o.CreatedAt.Year <= m.OutPutDocumentDate.Year).ToListAsync();
                        foreach (var item in items)
                        {
                            model.Name = item.Item.Name;
                        }
                    }

                }
                StagnantItems.Add(model);
            }
            return View(StagnantItems);

        }

        [HttpGet]
        public async Task<IActionResult> HaventBeenOutputed()
        {
            List<haventbeenoutputed> orderitems  = new List<haventbeenoutputed>();
            //get all the items that have inputdocument 
            var allitems = await _context.InputDocumentDetails.GroupBy(i => i.ItemId).Select(o=> o.Key ).ToListAsync();
            //get all the orders 
            var recent = 0;
            var orders = await _context.Orders.Include(u=>u.User).Where(o => o.State == OrderState.NeedOutPutDocmnet).ToListAsync();
            //1- loop threw orders 
            for (int o = 0; o < orders.Count; o++)
            {
                //for each order intialize a list to count the taken quantity
                List<OutPutDocumnetForAnnualViewModel> TakenQuantity = new List<OutPutDocumnetForAnnualViewModel>();
                List<HaventBeenOutPutedItemsViewModel> itemrecentquantity = new List<HaventBeenOutPutedItemsViewModel>();
                //all the annual orders of the current order we are looping through
                var anuualOrderDetails = _context.AnnualOrder.Include(o => o.Item).Where(or => or.OrderId == orders[o].OrderID).ToList();

                //intialize the taken quantity
                foreach (var item in anuualOrderDetails)
                {
                    OutPutDocumnetForAnnualViewModel model = new OutPutDocumnetForAnnualViewModel();
                    model.TakenQuantity = 0;
                    model.InputQuantity = 0;
                    model.ItemId = item.ItemId;
                    TakenQuantity.Add(model);
                }
                //count the taken quantity for the current order we are looping through
                var outPutDoument = _context.OutPutDocument.Where(or => or.OrderId == orders[o].OrderID).ToList();
                //if there is an output document then it will be modified relative to the previous taken quantities
                foreach (var element in outPutDoument)
                {
                    var OutPutDocumnetDetails = _context.OutPutDocumentDetails.Where(od => od.OutPutDocumentId == element.OutPutDocumentID).ToList();
                    for (int j = 0; j < OutPutDocumnetDetails.Count; j++)
                    {
                        TakenQuantity[j].TakenQuantity += OutPutDocumnetDetails[j].Quantity;
                    }
                }
                for (int ao = 0; ao < anuualOrderDetails.Count; ao++)
                {
                    HaventBeenOutPutedItemsViewModel model = new HaventBeenOutPutedItemsViewModel();
                       //calculate the total quantity for the current annual order
                        var TotalQuantity = anuualOrderDetails[ao].FirstSemQuantity + anuualOrderDetails[ao].SecondSemQuantity + anuualOrderDetails[ao].ThirdSemQuantity;
                        //count the recent value for the current annual order
                        recent = TotalQuantity - TakenQuantity[ao].TakenQuantity;

                         model.recentQuantity = recent;                    
                }


                //get all the outputdocuments for the specified order 
                var outputdocuments = await _context.OutPutDocument.Where(d => d.OrderId == orders[o].OrderID).ToListAsync();
                //loop threw th outputdocument then  all the items 
                haventbeenoutputed modelitem = new haventbeenoutputed();
                modelitem.Order = orders[o];
                foreach (var outputdocument in outputdocuments)
                {
                    
                    var outputdocumentdetails = await _context.OutPutDocumentDetails.Include(i=>i.Item).Where(i => i.OutPutDocumentId == outputdocument.OutPutDocumentID && allitems.Contains(i.ItemId)).ToListAsync();
                    foreach(var outputdocumentdetail in outputdocumentdetails)
                    {
                        HaventBeenOutPutedItemsViewModel model = new HaventBeenOutPutedItemsViewModel();
                        model.item = outputdocumentdetail.Item;
                        model.Createdat = outputdocumentdetail.CreatedAt;
                        itemrecentquantity.Add(model);

                    }

                }
                modelitem.items = itemrecentquantity;
                orderitems.Add(modelitem);
            }
   
            ViewBag.Empty = "لا يوجد مواد لم يتم تخريجها";
            return View(orderitems);

        }
        //[HttpPost]
        //public async Task<IActionResult> HaventBeenOutputed(DateTime period)
        //{

        //    var itemsmaxdate = await _context.OutPutDocumentDetails
        //        .Include(i=>i.Item)
        //        .GroupBy(i=>i.ItemId)
        //        .Select(o => new { ItemId = o.Key, MaxDate = o.Max(o => o.CreatedAt)})
        //        .ToListAsync();

        //    TimeSpan TempPeriod = System.DateTime.Now - period;
        //    List<haventbeenoutputed> outputdocs = new List<haventbeenoutputed>();
        //    foreach (var item in itemsmaxdate)
        //    {
        //        var name = _context.OutPutDocumentDetails
        //            .Include(i => i.Item)
        //            .Where(i => i.ItemId == item.ItemId).FirstOrDefault();

        //        haventbeenoutputed model = new haventbeenoutputed();
        //        TimeSpan TempSpan = System.DateTime.Now - item.MaxDate;
        //        if (TempSpan <= TempPeriod)
        //        {
        //            model.Name = name.Item.Name;
        //            model.OutPutDocumentDate = name.CreatedAt;
        //            model.Span = TempSpan;
        //            outputdocs.Add(model);
        //        }
        //    }

        //    ViewBag.Empty = "لا يوجد مواد لم يتم تخريجها";

        //    return View(outputdocs);
        //}
    }
}

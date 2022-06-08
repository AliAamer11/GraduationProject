﻿using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels;
using GraduationProject.ViewModels.OutPutDocument;
using GraduationProject.ViewModels.Warehouse;
using GraduationProject.ViewModels.WareHouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
    [Authorize(Roles = "StoreKeep,VicePris")]
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
        public IActionResult HaventBeenOutputed()
        { 
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> HaventBeenOutputed(DateTime period)
        {
            List<haventbeenoutputed> orderitems = new List<haventbeenoutputed>();
            //get all the items that have inputdocument 
            var allitems = await _context.InputDocumentDetails.GroupBy(i => i.ItemId).Select(o => o.Key).ToListAsync();
            //get all the orders
            var orders = await _context.Orders.Include(u => u.User).Where(o => o.State == OrderState.NeedOutPutDocmnet).ToListAsync();
            var recent = 0;

            //1- loop threw orders 
            for (int o = 0; o < orders.Count; o++)
            {

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                ///this block of code to calculate the recent quantity 

                IDictionary<int, int> RecentItemQuantity = new Dictionary<int, int>();

                //for each order intialize a list to count the taken quantity
                List<OutPutDocumnetForAnnualViewModel> TakenQuantity = new List<OutPutDocumnetForAnnualViewModel>();
                List<HaventBeenOutPutedItemsViewModel> itemrecentquantity = new List<HaventBeenOutPutedItemsViewModel>();
                List<HaventBeenOutPutedItemsViewModel> nooutputdocumenitems = new List<HaventBeenOutPutedItemsViewModel>();

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
                    //calculate the total quantity for the current annual order
                    var TotalQuantity = anuualOrderDetails[ao].FirstSemQuantity + anuualOrderDetails[ao].SecondSemQuantity + anuualOrderDetails[ao].ThirdSemQuantity;
                    //count the recent value for the current annual order
                    recent = TotalQuantity - TakenQuantity[ao].TakenQuantity;
                    RecentItemQuantity.Add(new KeyValuePair<int, int>(anuualOrderDetails[ao].ItemId, recent));


                }
                /////////////////////////////////////////////////////////////////////////////////////////

                //this block of code for the havent been outputed items 

                //get all the outputdocuments for the specified order 
                var outputdocuments = await _context.OutPutDocument.Where(d => d.OrderId == orders[o].OrderID).ToListAsync();
                haventbeenoutputed modelitem = new haventbeenoutputed();
                modelitem.Order = orders[o];


                foreach (var item in anuualOrderDetails)
                {
                    var totalquantity = item.FirstSemQuantity + item.SecondSemQuantity + item.ThirdSemQuantity;
                    //check if the order doesnt have any output document 
                    if (RecentItemQuantity[item.ItemId] == totalquantity)
                    {

                        HaventBeenOutPutedItemsViewModel model = new HaventBeenOutPutedItemsViewModel(); ;
                        model.Quantity = totalquantity;
                        model.recentQuantity = RecentItemQuantity[item.ItemId];
                        model.item = item.Item;
                        nooutputdocumenitems.Add(model);
                        modelitem.items = nooutputdocumenitems;

                    }
                    //check if the Annualorder  have an output document detail
                    else if (RecentItemQuantity[item.ItemId] != totalquantity || RecentItemQuantity[item.ItemId] != 0)
                    {
                        var outputdocumentdetail = await _context.OutPutDocumentDetails.Where(i => i.ItemId == item.ItemId && i.OutPutDocument.OrderId == item.OrderId).FirstOrDefaultAsync();
                        HaventBeenOutPutedItemsViewModel model = new HaventBeenOutPutedItemsViewModel();
                        model.recentQuantity = RecentItemQuantity[outputdocumentdetail.ItemId];
                        model.Quantity = totalquantity;
                        model.item = outputdocumentdetail.Item;
                        model.Createdat = outputdocumentdetail.CreatedAt;
                        itemrecentquantity.Add(model);
                    }

                }
                //add the list of items to the order 
                if (itemrecentquantity.Count() != 0)
                {
                    var itemrecentquantityMaxDate = itemrecentquantity.GroupBy(o => o.item.ItemID).Select(o => o.Max(i => i.Createdat)).ToList();
                    modelitem.items = itemrecentquantity.Where(d => itemrecentquantityMaxDate.Contains(d.Createdat)).ToList();
                }
                orderitems.Add(modelitem);
            }
            //now check the item created date to the specified date 
            TimeSpan TempPeriod = System.DateTime.Now - period;
            List<haventbeenoutputed> orderitems2 = new List<haventbeenoutputed>();
            foreach (var model in orderitems)
            {
                haventbeenoutputed items2 = new haventbeenoutputed();
                List<HaventBeenOutPutedItemsViewModel> items = new List<HaventBeenOutPutedItemsViewModel>();

                foreach (var item in model.items)
                {
                    if (item.Quantity != item.recentQuantity)
                    {
                        TimeSpan TempSpan = System.DateTime.Now - item.Createdat;
                        if (TempSpan <= TempPeriod)
                        {
                            // model.items.Add(item);
                            items.Add(item);
                        }
                    }
                    else if (item.Quantity == item.recentQuantity)
                    {
                        items.Add(item);
                    }
                }
                if (items.Count() != 0)
                {
                    items2 = model;
                    items2.items = items;
                    orderitems2.Add(items2);
                }
            }
            return View(orderitems2);
        }



        [HttpGet]
        public IActionResult MaterialInventoryReports()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MaterialInventoryReports(DateTime stardate, DateTime finishdate)
        {
            List<materialreportsViewModel> models = new List<materialreportsViewModel>(); //model to save info of jrd
            var allitems = await _context.Items.Include(i => i.Measurement).ToListAsync(); //allitems from invetory 



            var inputdocsQauntitySum = await _context.InputDocumentDetails
                .Include(i => i.InputDocument)
                .Where(i => i.InputDocument.CreatedAt >= stardate && i.InputDocument.CreatedAt <= finishdate)
                .GroupBy(i => i.ItemId)
                .Select(o => new { ItemId = o.Key, SumInputQuantity = o.Sum(o => o.Quantity) })
                .ToListAsync();

            foreach (var item in allitems)  //go on all items to save name and quantity
            {
                materialreportsViewModel model = new materialreportsViewModel();
                model.ItemID = item.ItemID;
                model.Barcode = item.BarCode;
                model.Name = item.Name;
                model.FlowRate = item.Measurement.Name;
                model.InputQuantity = 0;
                model.OutputQuantity = 0;
                model.InStock = item.Quantity;

                models.Add(model);
            }


            ///////Sum of Output Quantity to All Items 

            //query to get all outputdocuments within the same range of time and group them by itemid and get the sum of the quantity
            var outputdocsQauntitySum = await _context.OutPutDocumentDetails
                .Where(i => i.CreatedAt >= stardate && i.CreatedAt <= finishdate)
                .GroupBy(i => i.ItemId)
                .Select(o => new { ItemId = o.Key, SumOutputQuantity = o.Sum(o => o.Quantity) })
                .ToListAsync();
            //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

            ///////Sum of Input Quantity to All Items 

            //query to get all inputdocuments within the same range of time and group them by itemid and get the sum of the quantity



            foreach (var model in models)
            {
                foreach (var outputdoc in outputdocsQauntitySum)
                {
                    if (outputdoc.ItemId == model.ItemID)
                    {
                        model.OutputQuantity = outputdoc.SumOutputQuantity;
                    }

                }
                foreach (var inputdoc in inputdocsQauntitySum)
                {
                    if (inputdoc.ItemId == model.ItemID)
                    {
                        model.InputQuantity = inputdoc.SumInputQuantity;
                    }

                }
            }
            ViewBag.Empty = "لا يوجد مواد ";
            string jsonString = System.Text.Json.JsonSerializer.Serialize(models);
            ViewBag.items = jsonString;
            return View(models);

        }
    }
}

using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels;
using GraduationProject.ViewModels.OutPutDocument;
using GraduationProject.ViewModels.Warehouse;
using GraduationProject.ViewModels.WareHouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public IActionResult Stagnant()
        {
            List<StagnantItemsViewModel> StagnantItems = new List<StagnantItemsViewModel>();
            return View(StagnantItems);
        }

        [HttpPost]
        public async Task<IActionResult> Stagnant(int period)
        {
            //first   groupby itemid and get the max date of each item 
            var ItemOutputDocumentMaxDate = await _context.OutPutDocumentDetails.GroupBy(i => i.ItemId).Select(o => new { ItemId = o.Key, Maxid = o.Max(o => o.OutPutDocumentDetailsID) }).ToListAsync();
            List<StagnantItemsViewModel> MAxOutputedItems = new List<StagnantItemsViewModel>();
            List<StagnantItemsViewModel> StagnantItems = new List<StagnantItemsViewModel>();

            //2 loop threw grouped items 
            foreach (var obj in ItemOutputDocumentMaxDate)
            {
                //Max date for inputdocument item only the items that have an outputdocument 
                //returns date time
                var IteminputdocumentdetailMaxDate = _context.InputDocumentDetails.Include(i => i.InputDocument).Where(i => i.ItemId == obj.ItemId).Max(i => i.InputDocumentDetailsID);
                // returns a var of type InputDocumentDetail
                var Iteminputdocumentdetail = await _context.InputDocumentDetails.Include(i => i.InputDocument).Include(i => i.Item).Where(i => i.ItemId == obj.ItemId & i.InputDocumentDetailsID == IteminputdocumentdetailMaxDate).FirstOrDefaultAsync();
                var OutPutDocumentitem = await _context.OutPutDocumentDetails.Where(i => i.ItemId == obj.ItemId & obj.Maxid == i.OutPutDocumentDetailsID).FirstOrDefaultAsync();
                //Add to view list 
                StagnantItemsViewModel model = new StagnantItemsViewModel();

                model.Name = Iteminputdocumentdetail.Item.Name;
                model.InputDocumentDate = Iteminputdocumentdetail.InputDocument.CreatedAt;
                model.OutPutDocumentDate = OutPutDocumentitem.CreatedAt;


                MAxOutputedItems.Add(model);

            }

            int TempPeriod;
            foreach (var m in MAxOutputedItems.ToList())
            {
                StagnantItemsViewModel model2 = new StagnantItemsViewModel();
                if (m.InputDocumentDate < m.OutPutDocumentDate)
                {
                    TempPeriod = System.DateTime.Now.Year - m.OutPutDocumentDate.Year;

                    if (TempPeriod >= period)
                    {
                        model2.Name = m.Name;
                        model2.InputDocumentDate = m.InputDocumentDate;
                        model2.OutPutDocumentDate = m.OutPutDocumentDate;
                        StagnantItems.Add(model2);
                    }
                }
                if (m.InputDocumentDate >= m.OutPutDocumentDate)
                {
                    TempPeriod = System.DateTime.Now.Year - m.InputDocumentDate.Year;

                    if (TempPeriod >= period)
                    {
                        model2.Name = m.Name;
                        model2.InputDocumentDate = m.InputDocumentDate;
                        model2.OutPutDocumentDate = m.OutPutDocumentDate;
                        StagnantItems.Add(model2);
                    }
                }

            }
            string jsonString = System.Text.Json.JsonSerializer.Serialize(StagnantItems);
            ViewBag.items = jsonString;
            return View();

        }

        [HttpGet]
        public IActionResult HaventBeenOutputed()
        {
            List<haventbeenoutputed> orderitems = new List<haventbeenoutputed>();
            return View(orderitems);
        }

        [ValidateAntiForgeryToken]
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
                //used this list to get the items maxdate 
                List<HaventBeenOutPutedItemsViewModel> itemrecentquantity = new List<HaventBeenOutPutedItemsViewModel>();
                //this list for the items that doesn't have any outputdocument
                List<HaventBeenOutPutedItemsViewModel> nooutputdocumenitems = new List<HaventBeenOutPutedItemsViewModel>();
                //this list is for adding the filtered items "items that have maxoutputdocumentid"
                List<HaventBeenOutPutedItemsViewModel> MaxDateoutputdocumentItem = new List<HaventBeenOutPutedItemsViewModel>();


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
                        //  modelitem.items = nooutputdocumenitems;
                    }
                    //check if the Annualorder  have an output document detail
                    else if (RecentItemQuantity[item.ItemId] != totalquantity && RecentItemQuantity[item.ItemId] != 0)
                    {
                        //var outpudocuments = await _context.OutPutDocument.Where(i => i.OrderId == item.OrderId).ToListAsync();
                        foreach (var outputitem in outputdocuments)
                        {
                            var outputdocumentdetail = await _context.OutPutDocumentDetails.Where(i => i.ItemId == item.ItemId && outputitem.OutPutDocumentID == i.OutPutDocumentId).FirstOrDefaultAsync();
                            HaventBeenOutPutedItemsViewModel model = new HaventBeenOutPutedItemsViewModel();
                            model.outputdocumentid = outputdocumentdetail.OutPutDocumentId;
                            model.recentQuantity = RecentItemQuantity[outputdocumentdetail.ItemId];
                            model.Quantity = totalquantity;
                            model.item = outputdocumentdetail.Item;
                            model.Createdat = outputdocumentdetail.CreatedAt;
                            if (outputdocumentdetail.Quantity != 0)
                            {
                                itemrecentquantity.Add(model);
                            }

                        }

                    }

                }
                //add the list of items to the order 
                if (itemrecentquantity.Count() != 0)
                {
                    var itemrecentquantityMaxDate = itemrecentquantity.GroupBy(o => o.item.ItemID).Select(o => new { MaxOPid = o.Max(i => i.outputdocumentid), itemid = o.Key }).ToList();
                    //modelitem.items = itemrecentquantity.Where(d => itemrecentquantityMaxDate.Contains(d.Createdat)).ToList();

                    foreach (var item in itemrecentquantityMaxDate)
                    {

                        MaxDateoutputdocumentItem.Add(itemrecentquantity.Where(o => o.outputdocumentid == item.MaxOPid && o.item.ItemID == item.itemid).FirstOrDefault());
                    }
                }
                //append the nooutputdocumenitems list to itemrecentquantity
                MaxDateoutputdocumentItem.AddRange(nooutputdocumenitems);
                modelitem.items = MaxDateoutputdocumentItem;
                orderitems.Add(modelitem);
            }
            ViewBag.errormsg = "لايوجد مواد راكدة";
            // check the item created date to the specified date 
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

        [HttpGet]
        public  IActionResult MaterialStatisticsReport()
        {
            List<MaterialStatisticsReportViewModel> Models = new List<MaterialStatisticsReportViewModel>();
            ViewData["RP"] = new SelectList(BindListforRP(), "Value", "Text");

            return View(Models);
        }

        [HttpPost]
        public async Task<IActionResult> MaterialStatisticsReport(string rp)
        {
            //viewmodel list to store the data
            List<MaterialStatisticsReportViewModel> Models = new List<MaterialStatisticsReportViewModel>();

            //get annual orders for rp where its finished. check if its there or not // finisheed or not 
            var anorder_currentyear = await _context.Orders
           .Where(o => o.Type == false)                                                             //annual orders
           .Where(o => o.State == OrderState.NeedOutPutDocmnet || o.State == OrderState.Finishid)   //finished and approved
           .Where(u => u.User.RequstingParty == rp)                                                //for a specific party
           .Where(o=>o.CreatedAt.Year == DateTime.Now.Year)                                        //for this year
           .OrderBy(o => o.CreatedAt)
           .LastOrDefaultAsync();

            if (anorder_currentyear != null) //if there is an order for the current year
            {
                //we get the order for the year before the current one to compare
                int year_before_this = DateTime.Now.Year - 1;
                var anorder_before = await _context.Orders
                   .Where(o => o.Type == false)
                   .Where(o => o.State == OrderState.NeedOutPutDocmnet || o.State == OrderState.Finishid)
                   .Where(u => u.User.RequstingParty == rp)
                   .Where(o => o.CreatedAt.Year == year_before_this)
                   .OrderBy(o => o.CreatedAt)
                   .FirstOrDefaultAsync();


                if(anorder_before!=null)
                {
                    var itemsforordercurrentyear = await _context.AnnualOrder.Include(u=>u.Item).Where(o => o.OrderId == anorder_currentyear.OrderID).ToListAsync();
                    var itemsfororderbefore = await _context.AnnualOrder.Where(o => o.OrderId == anorder_before.OrderID).ToListAsync();

                    //go thru all items added in current order to add to viewmodel
                    foreach(var item in itemsforordercurrentyear)
                    {
                        MaterialStatisticsReportViewModel model = new MaterialStatisticsReportViewModel();
                        model.item = item.Item;
                        //model.item.Name = item.Item.Name;
                        model.FirstYear = item.Order.CreatedAt.Year; //first for current year
                        model.SecondYear = item.Order.CreatedAt.Year -1;
                        model.FirstQuantity = item.FirstSemQuantity + item.SecondSemQuantity + item.ThirdSemQuantity;  ///for current year

                        var itembefore = await _context.AnnualOrder
                            .Include(u => u.Item)
                            .Where(o => o.OrderId == anorder_before.OrderID)
                            .Where(i=>i.ItemId == item.ItemId)
                            .FirstOrDefaultAsync();
                        if(itembefore!=null)
                        {
                            model.SecondQuantity = itembefore.FirstSemQuantity + itembefore.SecondSemQuantity + itembefore.ThirdSemQuantity;
                        }
                        else
                        {
                            model.SecondQuantity = 0;
                        }
                        model.Percentage = ((model.FirstQuantity - model.SecondQuantity) / Math.Max(model.FirstQuantity,model.SecondQuantity) * 100);
                        Models.Add(model);


                    }

                    //List<MaterialStatisticsReportViewModel> Models = new List<MaterialStatisticsReportViewModel>();

                    //go thru items in year before where they havent been ordered the current year
                    foreach (var item in itemsfororderbefore)
                    {
                        bool test = false;
                       foreach(var testi in itemsforordercurrentyear)
                        {
                            if (testi.Item.Name == item.Item.Name)
                                test = true;
                        }
                        if(!test)
                        {
                            MaterialStatisticsReportViewModel model = new MaterialStatisticsReportViewModel();
                            model.item = item.Item;
                            model.FirstYear = item.Order.CreatedAt.Year;
                            model.SecondYear = item.Order.CreatedAt.Year - 1;
                            model.SecondQuantity = item.FirstSemQuantity + item.SecondSemQuantity + item.ThirdSemQuantity;
                            model.FirstQuantity = 0;
                            model.Percentage = ((model.FirstQuantity - model.SecondQuantity) / Math.Max(model.FirstQuantity, model.SecondQuantity) * 100);
                            Models.Add(model);

                        }
                    }

                }
                else
                {
                    ViewBag.errormessage = "لا يوجد طلب احتياج سنوي للسنة السابقة";
                }


            }
            else
            {
                ViewBag.errormessage = "لا يوجد طلب احتياج للسنة الحالية.";
            }


            ViewData["RP"] = new SelectList(BindListforRP(), "Value", "Text");
            return View(Models);
        }

        private List<SelectListItem> BindListforRP()
        {
            List<SelectListItem> list = new();
            var users = _context.Users.ToList();
            foreach (var user in users)
            {
                if (!(user.RequstingParty == "SuperHeroAdmin" || user.RequstingParty == "سائد الناظر" || user.RequstingParty == "أسامة السجاد"))
                {
                    list.Add(new SelectListItem { Text = user.RequstingParty, Value = user.RequstingParty.ToString() });
                }
            }
            return list;
        }


    
        [HttpGet]
        public  IActionResult RPItems()
        {
            List<RPitemsReportViewModel> RPitems = new List<RPitemsReportViewModel>();
            ViewData["RP"] = new SelectList(BindListforRP(), "Value", "Text");

            return View(RPitems);
        }

        [HttpPost]
        public async Task<IActionResult> RPItems(string rp, DateTime starttime, DateTime finishtime)
        {
            List<RPitemsReportViewModel> RPitems = new List<RPitemsReportViewModel>();
            ViewData["RP"] = new SelectList(BindListforRP(), "Value", "Text");
            ///////get annualorders for rp and in certain time 
            var Aorders = await _context.Orders.Where(i => i.Type == false && i.User.RequstingParty == rp && i.CreatedAt >= starttime && i.CreatedAt <= finishtime).ToListAsync();

            ///////get unplanned orders for rp and in certain time 
            var Uorders = await _context.Orders.Where(i => i.Type == true && i.User.RequstingParty == rp && i.CreatedAt >= starttime && i.CreatedAt <= finishtime).ToListAsync();

            ////this list is to calculate taken quantity foreach item in oRDERS (ANNUAL)
            //List<OutPutDocumnetForAnnualViewModel> AOTakenQuantity = new List<OutPutDocumnetForAnnualViewModel>();

            ////this list is to calculate taken quantity foreach item in oRDERS (UNPLANNED)
            //List<OutPutDocumnetForAnnualViewModel> UOTakenQuantity = new List<OutPutDocumnetForAnnualViewModel>();

            //VIEWMODEL For AnnualoRDER
            if (starttime < finishtime)
            {
                foreach (var Aorder in Aorders)
                {
                    var annualorders = await _context.AnnualOrder.Where(i => i.OrderId == Aorder.OrderID).GroupBy(m => m.Item.ItemID).Select(m => new { itemid = m.Key, total = m.Sum(m => m.FirstSemQuantity) + m.Sum(m => m.SecondSemQuantity) + m.Sum(m => m.ThirdSemQuantity) }).ToListAsync();

                    foreach (var item in annualorders)
                    {
                        RPitemsReportViewModel model = new RPitemsReportViewModel();

                        model.Item = _context.Items.Where(i => i.ItemID == item.itemid).FirstOrDefault();
                        model.requested_quantity = item.total;

                        //taken quantity code
                        var outPutDoument = await _context.OutPutDocument.Where(o => o.OrderId == Aorder.OrderID).ToListAsync();
                        if (outPutDoument == null)
                        {
                            model.taken_quantity = 0;
                        }
                        else
                        {
                            foreach (var element in outPutDoument)
                            {
                                var OutPutDocumnetDetails = await _context.OutPutDocumentDetails.Where(o => o.OutPutDocumentId == element.OutPutDocumentID && o.ItemId == item.itemid).ToListAsync();
                                for (int i = 0; i < OutPutDocumnetDetails.Count; i++)
                                {
                                    model.taken_quantity = model.taken_quantity + OutPutDocumnetDetails[i].Quantity;
                                }
                            }
                        }

                        //test wether the item exists
                        bool test = false;
                        foreach (var itemss in RPitems)
                        {
                            if (item.itemid == itemss.Item.ItemID)
                                test = true;
                        }
                        if (!test)
                        {
                            //add model to the main ITEMS report after calculating ALLLLL of the quantities;
                            RPitems.Add(model);
                        }
                        else
                        {
                            // var updateitem =RPitems.Find(o => o.Item.ItemID == item.itemid);
                            // updateitem.taken_quantity += model.taken_quantity;
                            RPitems.Find(o => o.Item.ItemID == item.itemid).taken_quantity += model.taken_quantity;
                            RPitems.Find(o => o.Item.ItemID == item.itemid).requested_quantity += model.requested_quantity;
                        }
                    }
                }

            }

            else
            {
                ViewBag.errormessage = "الرجاء إدخال تواريخ صحيحة. ";
            }
            ///////get unplanned for rp in certain time

            return View(RPitems);
        }
    }
}

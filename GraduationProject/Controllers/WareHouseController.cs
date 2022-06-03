using GraduationProject.Data;
using GraduationProject.Data.Models;
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
            List<OutPutDocumentDetails> outputdocs = new List<OutPutDocumentDetails>();
            List<haventbeenoutputed> itemstobedisplayed = new List<haventbeenoutputed>();
            var allitems = await _context.InputDocumentDetails.GroupBy(i => i.ItemId).Select(o => new { itemid = o.Key }).ToListAsync();
            var allorders = await _context.Orders.ToListAsync();

            foreach (var order in allorders)
            {
                var outputdocuments = await _context.OutPutDocument.Where(o => o.OrderId == order.OrderID).ToListAsync();
                foreach (var outputdocument in outputdocuments)
                {
                    foreach (var item in allitems)
                    {
                        haventbeenoutputed model = new haventbeenoutputed();
                        var outputdocumentdetails = await _context.OutPutDocumentDetails.Where(i => i.ItemId == item.itemid && outputdocument.OutPutDocumentID == i.OutPutDocumentId).FirstOrDefaultAsync();
                        var itemname = await _context.Items.Where(i => i.ItemID == outputdocumentdetails.ItemId).FirstOrDefaultAsync();
                        model.Name = itemname.Name;
                        itemstobedisplayed.Add(model);
                    }
                }
            }


            //foreach (var item in allitems)
            //{
            //    var itemnames = _context.Items.Where(i => i.ItemID == item.itemid).FirstOrDefault();
            //    foreach (var output in outputdocument)
            //    {
            //        var outputdoc = await _context.OutPutDocumentDetails.Include(o => o.OutPutDocument).Where(i => i.ItemId == item.itemid && i.OutPutDocumentId == output.OutPutDocumentID).ToListAsync();

            //    }

            //    foreach (var x in outputdoc)
            //    {
            //        if (x == null)
            //        {
            //            haventbeenoutputed itemm = new haventbeenoutputed();
            //            itemm.Name = itemnames.Name;
            //            itemstobedisplayed.Add(itemm);
            //        }
            //        outputdocs.Add(x);
            //    }


            //}



            ViewBag.Empty = "لا يوجد مواد لم يتم تخريجها";
            return View(itemstobedisplayed);

        }
        [HttpPost]
        public async Task<IActionResult> HaventBeenOutputed(DateTime period)
        {

            var itemsmaxdate = await _context.OutPutDocumentDetails
                .Include(i => i.Item)
                .GroupBy(i => i.ItemId)
                .Select(o => new { ItemId = o.Key, MaxDate = o.Max(o => o.CreatedAt) })
                .ToListAsync();

            TimeSpan TempPeriod = System.DateTime.Now - period;
            List<haventbeenoutputed> outputdocs = new List<haventbeenoutputed>();
            foreach (var item in itemsmaxdate)
            {
                var name = _context.OutPutDocumentDetails
                    .Include(i => i.Item)
                    .Where(i => i.ItemId == item.ItemId).FirstOrDefault();

                haventbeenoutputed model = new haventbeenoutputed();
                TimeSpan TempSpan = System.DateTime.Now - item.MaxDate;
                if (TempSpan <= TempPeriod)
                {
                    model.Name = name.Item.Name;
                    model.OutPutDocumentDate = name.CreatedAt;
                    model.Span = TempSpan;
                    outputdocs.Add(model);
                }
            }

            ViewBag.Empty = "لا يوجد مواد لم يتم تخريجها";

            return View(outputdocs);
        }

        [HttpGet]
        public async Task<IActionResult> MaterialInventoryReports()
        {
            return View();

        }
        
            [HttpPost]
        public async Task<IActionResult> MaterialInventoryReports(DateTime stardate, DateTime finishdate)
        {
            List<materialreportsViewModel> models = new List<materialreportsViewModel>(); //model to save info of jrd
            var allitems = await _context.Items.Include(i=>i.Measurement).ToListAsync(); //allitems from invetory 



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

            return View(models);

        }
    }
}


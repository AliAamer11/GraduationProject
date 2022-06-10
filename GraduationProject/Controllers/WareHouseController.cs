using GraduationProject.Data;
using GraduationProject.Data.Models;
using GraduationProject.ViewModels;
using GraduationProject.ViewModels.OutPutDocument;
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

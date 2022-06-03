using GraduationProject.Data;
using GraduationProject.ViewModels.OutPutDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Authorization;
using GraduationProject.ViewModels;

namespace GraduationProject.Controllers
{
    [Authorize(Roles = "StoreKeep")]
    public class OutPutDocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OutPutDocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            OrdersViewModels viewModel = new OrdersViewModels();
            //this for get number of AnnualOrder
            viewModel.countAnnualOrder = _context.Orders.Where(o => o.Type == OrderType.Annual && o.State == OrderState.NeedOutPutDocmnet).Count();

            //this for get number of UnPlanned Order
            viewModel.countUnPlannedOrder = _context.Orders.Where(o => o.Type == OrderType.UnPlanned && o.State == OrderState.NeedOutPutDocmnet).Count();

            //get All OutPutDocument
            //this for OutPut Documnet for Annaul
            var outPutDocumentAnnual = await _context.OutPutDocument.Include(o => o.OutPutDocumentDetails).Include(o => o.Order).Include(o => o.OutPutDocumentDetails).Where(o => o.Order.Type == OrderType.Annual).ToListAsync();

            //this for OutPut Documnet for UnPlanned
            var outPutDocumentUnPlanned = await _context.OutPutDocument.Include(o => o.OutPutDocumentDetails).Include(o => o.Order).Include(o => o.OutPutDocumentDetails).Where(o => o.Order.Type == OrderType.UnPlanned).ToListAsync();

            //make List for specific Data for OutPutDocumnet to show in View
            List<OutPutDocumentViewModel> outPutDocumentViewModelsUnPlanned = new List<OutPutDocumentViewModel>();
            List<OutPutDocumentViewModel> outPutDocumentViewModelsAnnaul = new List<OutPutDocumentViewModel>();


            // this to deal with OutPut Document For  AnnualOrders
            foreach (var item in outPutDocumentAnnual)
            {
                OutPutDocumentViewModel tempmModel = new OutPutDocumentViewModel();
                //this loop for Get CreatedAt and CommissaryName but we Just need First one 
                foreach (var subitem in item.OutPutDocumentDetails)
                {
                    // get CreatedAt for this OutPutDocumnet we Just need one
                    tempmModel.CreatedAt = subitem.CreatedAt;
                    // get CommissaryName for this OutPutDocumnet
                    tempmModel.CommissaryName = subitem.CommissaryName;
                    break;
                }
                // get user For knowing the RequestingParty for this OutPutDocument
                var user = await _context.Users.FirstOrDefaultAsync(i => i.Id == item.Order.UserId);
                tempmModel.RequestingParty = user.RequstingParty;
                tempmModel.OrderType = item.Order.Type;
                tempmModel.OrderId = item.OrderId;
                tempmModel.OutPutDocumentID = item.OutPutDocumentID;
                // add this specific OutPutDocument for list
                outPutDocumentViewModelsAnnaul.Add(tempmModel);
            }
            viewModel.OutPutDocumentViewModelsAnnual = outPutDocumentViewModelsAnnaul;

            // this to deal with OutPut Document For UnplannedOrders 
            foreach (var item in outPutDocumentUnPlanned)
            {
                OutPutDocumentViewModel tempmModel = new OutPutDocumentViewModel();
                //this loop for Get CreatedAt and CommissaryName but we Just need First one 
                foreach (var subitem in item.OutPutDocumentDetails)
                {
                    // get CreatedAt for this OutPutDocumnet we Just need one
                    tempmModel.CreatedAt = subitem.CreatedAt;
                    // get CommissaryName for this OutPutDocumnet
                    tempmModel.CommissaryName = subitem.CommissaryName;
                    break;
                }
                // get user For knowing the RequestingParty for this OutPutDocument
                var user = await _context.Users.FirstOrDefaultAsync(i => i.Id == item.Order.UserId);
                tempmModel.RequestingParty = user.RequstingParty;
                tempmModel.OrderType = item.Order.Type;
                tempmModel.OrderId = item.OrderId;
                tempmModel.OutPutDocumentID = item.OutPutDocumentID;
                // add this specific OutPutDocument for list
                outPutDocumentViewModelsUnPlanned.Add(tempmModel);
            }
            viewModel.OutPutDocumentViewModelsUnPlanned = outPutDocumentViewModelsUnPlanned;

            return View(viewModel);
        }

        //get AnnualOrder
        [HttpGet]
        public async Task<IActionResult> AnnualOrder()
        {
            var annualOrders = await _context.Orders.Include(o => o.User).Where(o => o.Type == OrderType.Annual && o.State == OrderState.NeedOutPutDocmnet).ToListAsync();
            return View(annualOrders);
        }


        //******************* AGGREGATION  ******************//
        private IDictionary<string, int> AggregatedTakenQuantity()
        {
            IDictionary<string, int> RecentItemQuantity = new Dictionary<string, int>();

            //all the aggregated items with quantities
            var AnnualOrderItems = _context.AnnualOrder.Where(AO => AO.Order.State == OrderState.NeedOutPutDocmnet).GroupBy(m => m.Item.Name).Select(m => new { name = m.Key, itemcount = m.Count(), sum1 = m.Sum(m => m.FirstSemQuantity), sum2 = m.Sum(m => m.SecondSemQuantity), sum3 = m.Sum(m => m.ThirdSemQuantity) }).ToList();
            //A dictionary for the aggregated item name and its recent value
            var recent = 0;
            //get all the orders of type annual 
            var orders = _context.Orders.Where(o => o.Type == OrderType.Annual && o.State == OrderState.NeedOutPutDocmnet).ToList();
            //now we are going to loop through each order
            for (int o = 0; o < orders.Count; o++)
            {   //for each order intialize a list to count the taken quantity
                List<OutPutDocumnetForAnnualViewModel> TakenQuantity = new List<OutPutDocumnetForAnnualViewModel>();
                //all the annual orders of the current order we are looping through
                var anuualOrderDetails = _context.AnnualOrder.Include(o => o.Item).Where(or => or.OrderId == orders[o].OrderID).ToList();
                //a view model for output document of the current order we are looping through
                AnnualOrderViewModel viewModel = new AnnualOrderViewModel();
                viewModel.CommisaryName = "";
                viewModel.OrderId = orders[o].OrderID;
                viewModel.AnnualOrders = anuualOrderDetails;
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
                //if i has no output document then its just zero
                if (outPutDoument == null)
                {
                    viewModel.QuantityModel = TakenQuantity;
                }
                //if there is an output document then it will be modified relative to the previous taken quantities
                foreach (var element in outPutDoument)
                {
                    var OutPutDocumnetDetails = _context.OutPutDocumentDetails.Where(od => od.OutPutDocumentId == element.OutPutDocumentID).ToList();
                    for (int j = 0; j < OutPutDocumnetDetails.Count; j++)
                    {
                        TakenQuantity[j].TakenQuantity += OutPutDocumnetDetails[j].Quantity;
                    }
                }
                viewModel.QuantityModel = TakenQuantity;

                //Now here the aggregation starting
                //loop through all aggregated items
                for (int i = 0; i < AnnualOrderItems.Count; i++)
                {
                    //loop through each annual order of the current order we are looping through
                    for (int ao = 0; ao < anuualOrderDetails.Count; ao++)
                    {
                        //to check if the aggregated item name is equal to the current annual order item
                        if (AnnualOrderItems[i].name == anuualOrderDetails[ao].Item.Name)
                        {   //calculate the total quantity for the current annual order
                            var TotalQuantity = anuualOrderDetails[ao].FirstSemQuantity + anuualOrderDetails[ao].SecondSemQuantity + anuualOrderDetails[ao].ThirdSemQuantity;
                            //count the recent value for the current annual order
                            recent = TotalQuantity - TakenQuantity[ao].TakenQuantity;
                            //add the recent aggregated value and the item name for the list
                            //but first we need to check if the item doesnt exist in the list
                            if (!RecentItemQuantity.Keys.Contains(AnnualOrderItems[i].name))
                            {
                                RecentItemQuantity.Add(new KeyValuePair<string, int>(AnnualOrderItems[i].name, recent));
                            }
                            //or else the item exist in the list previously so we will just update the recent quantity
                            else
                            {
                                RecentItemQuantity[AnnualOrderItems[i].name] += recent;
                            }
                        }
                    }
                }
            }
            ViewData["recentvalue"] = RecentItemQuantity;
            return (RecentItemQuantity);
        }

        //this code need changes
        [HttpGet]
        public async Task<IActionResult> AnnualOrderDetails(int orderId, bool isnotSuccess = false, string itemName = "")
        {
            // this to know if OutPutDocument was Created Successfully or Not 
            ViewBag.isnotSuccess = isnotSuccess;
            // this to know what the item that dosen't has enough quantity 
            ViewBag.itemName = itemName;

            //get the Unplanned orders 
            var anuualOrderDetails = await _context.AnnualOrder.Include(o => o.Item).Where(o => o.OrderId == orderId).ToListAsync();

            AnnualOrderViewModel viewModel = new AnnualOrderViewModel();
            viewModel.CommisaryName = "";
            viewModel.OrderId = orderId;
            viewModel.AnnualOrders = anuualOrderDetails;

            //this for Taken Quantity
            List<OutPutDocumnetForAnnualViewModel> TakenQuantity = new List<OutPutDocumnetForAnnualViewModel>();
            foreach (var item in anuualOrderDetails)
            {
                OutPutDocumnetForAnnualViewModel model = new OutPutDocumnetForAnnualViewModel();
                model.TakenQuantity = 0;
                model.InputQuantity = 0;
                model.ItemId = item.ItemId;
                TakenQuantity.Add(model);
            }

            var outPutDoument = await _context.OutPutDocument.Where(o => o.OrderId == orderId).ToListAsync();
            if (outPutDoument == null)
            {
                viewModel.QuantityModel = TakenQuantity;
                return View(viewModel);
            }

            foreach (var element in outPutDoument)
            {
                var OutPutDocumnetDetails = await _context.OutPutDocumentDetails.Where(o => o.OutPutDocumentId == element.OutPutDocumentID).ToListAsync();
                for (int i = 0; i < OutPutDocumnetDetails.Count; i++)
                {
                    TakenQuantity[i].TakenQuantity += OutPutDocumnetDetails[i].Quantity;
                }
            }
            viewModel.QuantityModel = TakenQuantity;

            ////*********** DISTRIBUTION***********//
            ////distribute the quantity for each item relative to the aggregated recent value;
            //all the items in the warehouse
            var WHitems = await _context.Items.ToListAsync();
            List<float> suggestedquantity = new List<float>();
            var RecentItemQuantity = AggregatedTakenQuantity();
            float recent = 0;
            //loop through all these item
            for (int ao = 0; ao < anuualOrderDetails.Count; ao++)
            {
                for (int i = 0; i < WHitems.Count; i++)
                {
                    //to check if the item in WH equals to the item in the recentitems list
                    if (RecentItemQuantity.Keys.Contains(WHitems[i].Name))
                    {
                        //check if the item we are looping throgh is the same item in the annual order
                        if (WHitems[i].Name == anuualOrderDetails[ao].Item.Name)
                        {
                            //now only if the quantity of the item in the WH is less than the
                            //aggregated ordered quantity of that item =>
                            //a distribution relative to the quantity of that item in the WH will be implemented
                            if (WHitems[i].Quantity < RecentItemQuantity[WHitems[i].Name])
                            {
                                //now we calculate the percantage of each item
                                float percentage = (float)WHitems[i].Quantity / (float)RecentItemQuantity[WHitems[i].Name];
                                recent = (anuualOrderDetails[ao].FirstSemQuantity + anuualOrderDetails[ao].SecondSemQuantity + anuualOrderDetails[ao].ThirdSemQuantity) - TakenQuantity[ao].TakenQuantity;
                                float newquantity = percentage * recent;
                                suggestedquantity.Add((int)newquantity);
                            }
                            //else if the quantity of the item in the WH is bigger than the aggregated item quantity
                            //the suggested quantity will remain the same
                            else
                            {
                                recent = (anuualOrderDetails[ao].FirstSemQuantity + anuualOrderDetails[ao].SecondSemQuantity + anuualOrderDetails[ao].ThirdSemQuantity) - TakenQuantity[ao].TakenQuantity;
                                suggestedquantity.Add((int)recent);
                            }
                        }
                    }
                }
            }
            ViewData["suggestedquantity"] = suggestedquantity;
            ViewBag.suggestedquantity = suggestedquantity;

            return View(viewModel);
        }
        /// <summary>
        /// this Action has a few steps 
        /// 1_ check the item quantity 
        ///     if the quantity dosent enough we return to AnnualOrder View with message explain what happen to user
        ///     
        /// 2_ if every thing go right we will create outPutDocumnet and insert it to outPutDocumnet Table 
        ///     for Creating outputDocumentDetails table
        ///     
        /// 3_loop    
        ///   -Update Item Quantity 
        ///   -Create OutputDocumnetDetails
        /// 
        /// 4_ Check if all the Order is was Finishid or not 
        /// 
        /// 5_ Finally update the State of order to 3 to make the order Finish
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateOutPutDoucmentForAnnualOrder(AnnualOrderViewModel viewModel)
        {
            try
            {
                var annualOrders = await _context.AnnualOrder.Include(o => o.Item).Where(o => o.OrderId == viewModel.OrderId).ToListAsync();
                //_1_
                foreach (var element in viewModel.QuantityModel)
                {
                    var item = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == element.ItemId);
                    if (item.Quantity < element.InputQuantity)
                    {
                        ViewBag.errorMessage = "إن كمية مادة" + item.Name + "أقل من المطلوب";
                        return RedirectToAction(nameof(AnnualOrderDetails), new { orderId = viewModel.OrderId, isnotSuccess = true, itemName = item.Name });
                    }
                }

                //_2_
                //here we will create outpPutDocument for AnnualOrder 
                //lets go
                //first create an outputdocumnet with order in OutPutDocumnetTable

                GraduationProject.Data.Models.OutPutDocument outPutDocument = new Data.Models.OutPutDocument();
                outPutDocument.OrderId = viewModel.OrderId;
                _context.Add(outPutDocument);
                await _context.SaveChangesAsync();

                // then we need to create the details for OutPutDocument
                foreach (var element in viewModel.QuantityModel)
                {
                    OutPutDocumentDetails outPutDocumentDetails = new OutPutDocumentDetails()
                    {
                        CommissaryName = viewModel.CommisaryName,
                        CreatedAt = DateTime.Now.Date,
                        Quantity = element.InputQuantity,
                        OutPutDocumentId = outPutDocument.OutPutDocumentID,
                        ItemId = element.ItemId,
                    };

                    //_3_
                    // we need to Update Item Quantity
                    Items item = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == element.ItemId);
                    item.Quantity = item.Quantity - element.InputQuantity;
                    //check if item Exceded the MiniumumRange
                    if (item.MinimumRange > item.Quantity)
                    {
                        item.ExceededMinimumRange = item.ExceededMinimumRange + 1;
                    }
                    _context.Update(item);
                    await _context.SaveChangesAsync();

                    // add outPutDocumentDetails to database
                    _context.Add(outPutDocumentDetails);
                    await _context.SaveChangesAsync();
                }
                //_4_
                // we need to Check Order if Done or Not 
                if (checkifAnnualOrderDone(viewModel.OrderId))
                {
                    // if check was true we need to Update The OrderStatus
                    var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == viewModel.OrderId);
                    order.State = OrderState.Finishid;
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Orders", "OutPutDocument");
                }
                return RedirectToAction("Orders", "OutPutDocument");

            }
            catch
            {
                throw;
            }

        }
        //get UnPlannedOrder
        [HttpGet]
        public async Task<IActionResult> UnPlannedOrder()
        {
            var unPlannedOrder = await _context.Orders.Include(o => o.User).Where(o => o.Type == OrderType.UnPlanned && o.State == OrderState.NeedOutPutDocmnet).ToListAsync();
            return View(unPlannedOrder);
        }

        //this code need changes
        [HttpGet]
        public async Task<IActionResult> UnPlannedOrderDetails(int orderId, bool isnotSuccess = false, string itemName = "")
        {
            // this to know if OutPutDocument was Created Successfully or Not 
            ViewBag.isnotSuccess = isnotSuccess;
            // this to know what the item that dosen't has enough quantity 
            ViewBag.itemName = itemName;

            //get the Unplanned orders 
            var unPlannedOrderDetails = await _context.UnPlannedOrder.Include(o => o.Item).Where(o => o.OrderId == orderId).ToListAsync();
            UnPlannedOrderViewModel viewModel = new UnPlannedOrderViewModel();
            viewModel.CommisaryName = "";
            viewModel.OrderId = orderId;
            viewModel.UnPlannedOrderList = unPlannedOrderDetails;
            //
            //this for Taken Quantity
            List<OutPutDocumnetForAnnualViewModel> TakenQuantity = new List<OutPutDocumnetForAnnualViewModel>();
            foreach (var item in unPlannedOrderDetails)
            {
                OutPutDocumnetForAnnualViewModel model = new OutPutDocumnetForAnnualViewModel();
                model.TakenQuantity = 0;
                model.InputQuantity = 0;
                model.ItemId = item.ItemId;
                TakenQuantity.Add(model);
            }

            var outPutDoument = await _context.OutPutDocument.Where(o => o.OrderId == orderId).ToListAsync();
            if (outPutDoument == null)
            {
                viewModel.QuantityModel = TakenQuantity;
                return View(viewModel);
            }

            foreach (var element in outPutDoument)
            {
                var OutPutDocumnetDetails = await _context.OutPutDocumentDetails.Where(o => o.OutPutDocumentId == element.OutPutDocumentID).ToListAsync();
                for (int i = 0; i < OutPutDocumnetDetails.Count; i++)
                {
                    TakenQuantity[i].TakenQuantity += OutPutDocumnetDetails[i].Quantity;
                }
            }
            viewModel.QuantityModel = TakenQuantity;
            return View(viewModel);
        }

        /// <summary>
        /// this Action has a few steps 
        /// 1_ check the item quantity 
        ///     if the quantity dosent enough we return to Unplannedorder View with message explain what happen to user
        ///     
        /// 2_ if every thing go right we will create outPutDocumnet and insert it to outPutDocumnet Table 
        ///     for Creating outputDocumentDetails table    
        ///     
        /// 3_loop    
        ///   -Update Item Quantity 
        ///   -Create OutputDocumnetDetails
        /// 
        /// 4_ Finally update the State of order to 3 to make the order Finish
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateOutPutDoucmentForUnPlannedOrder(UnPlannedOrderViewModel viewModel)
        {
            try
            {
                var UnPlannedOrders = await _context.UnPlannedOrder.Include(o => o.Item).Where(o => o.OrderId == viewModel.OrderId).ToListAsync();
                viewModel.UnPlannedOrderList = UnPlannedOrders;

                //_1_
                //this logic to check if any item less than requier quanitity
                foreach (var element in UnPlannedOrders)
                {
                    var item = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == element.ItemId);
                    if (item.Quantity < element.Quantity)
                    {
                        ViewBag.errorMessage = "إن كمية مادة" + item.Name + "أقل من المطلوب";
                        return RedirectToAction(nameof(UnPlannedOrderDetails), new { orderId = viewModel.OrderId, isnotSuccess = true, itemName = item.Name });
                    }
                }

                //_2_
                //here we will create outpPutDocument for UnPlannedOrder 
                //lets go
                //first create an outputdocumnet with order in OutPutDocumnetTable
                GraduationProject.Data.Models.OutPutDocument outPutDocument = new Data.Models.OutPutDocument();
                outPutDocument.OrderId = viewModel.OrderId;
                _context.Add(outPutDocument);
                await _context.SaveChangesAsync();

                // then we need to create the details for OutPutDocument
                foreach (var element in viewModel.QuantityModel)
                {
                    OutPutDocumentDetails outPutDocumentDetails = new OutPutDocumentDetails()
                    {
                        CommissaryName = viewModel.CommisaryName,
                        CreatedAt = DateTime.Now.Date,
                        Quantity = element.InputQuantity,
                        OutPutDocumentId = outPutDocument.OutPutDocumentID,
                        ItemId = element.ItemId,
                    };

                    //_3_
                    // we need to Update Item Quantity 
                    Items item = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == element.ItemId);
                    item.Quantity = item.Quantity - element.InputQuantity;
                    //check if item Exceded the MiniumumRange
                    if (item.MinimumRange > item.Quantity)
                    {
                        item.ExceededMinimumRange = item.ExceededMinimumRange + 1;
                    }
                    _context.Update(item);
                    await _context.SaveChangesAsync();

                    // add outPutDocumentDetails to database
                    _context.Add(outPutDocumentDetails);
                    await _context.SaveChangesAsync();
                }
                //_4_
                // we need to Update Order State 
                if (checkifUnPlannedOrderDone(viewModel.OrderId))
                {
                    //if check method was true we need to Update OrderState
                    var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderID == viewModel.OrderId);
                    order.State = OrderState.Finishid;
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("Orders", "OutPutDocument");
            }
            catch
            {
                throw;
            }
        }

        //get OutPutDocumentDetailsForUnPlannedOrders
        [HttpGet]
        public async Task<IActionResult> OutPutDocumentDetailsForUnPlannedOrders(int? outPutDocumentId)
        {
            if (outPutDocumentId == null)
            {
                return NotFound();
            }
            var OutPutDocumnetDetails = await _context.OutPutDocumentDetails.Include(o => o.Item).Where(o => o.OutPutDocumentId == outPutDocumentId).ToListAsync();
            return View(OutPutDocumnetDetails);
        }


        /// <summary>
        /// this method for check if the Annual Order was Done or not 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>flase if not otherwise true</returns>
        private bool checkifUnPlannedOrderDone(int orderId)
        {
            var outPutDoument = _context.OutPutDocument.Where(o => o.OrderId == orderId).ToList();
            var unPlannedOrder = _context.UnPlannedOrder.Where(o => o.OrderId == orderId).ToList();
            bool check = false;

            List<OutPutDocumnetForAnnualViewModel> TakenQuantity = new List<OutPutDocumnetForAnnualViewModel>();
            foreach (var item in unPlannedOrder)
            {
                OutPutDocumnetForAnnualViewModel model = new OutPutDocumnetForAnnualViewModel();
                model.TakenQuantity = 0;
                model.InputQuantity = 0;
                model.ItemId = item.ItemId;
                TakenQuantity.Add(model);
            }
            foreach (var element in outPutDoument)
            {
                var OutPutDocumnetDetails = _context.OutPutDocumentDetails.Where(o => o.OutPutDocumentId == element.OutPutDocumentID).ToList();
                for (int i = 0; i < OutPutDocumnetDetails.Count; i++)
                {
                    TakenQuantity[i].TakenQuantity += OutPutDocumnetDetails[i].Quantity;
                }
            }

            for (int i = 0; i < unPlannedOrder.Count; i++)
            {
                int totalQuantity = unPlannedOrder[i].Quantity;
                if (TakenQuantity[i].TakenQuantity != totalQuantity)
                {
                    check = false;
                    break;
                }
                else
                {
                    check = true;
                }
            }
            return check;
        }


        /// <summary>
        /// this method for check if the Annual Order was Done or not 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>flase if not otherwise true</returns>
        private bool checkifAnnualOrderDone(int orderId)
        {
            var outPutDoument = _context.OutPutDocument.Where(o => o.OrderId == orderId).ToList();
            var annualOrder = _context.AnnualOrder.Where(o => o.OrderId == orderId).ToList();
            bool check = true;

            List<OutPutDocumnetForAnnualViewModel> TakenQuantity = new List<OutPutDocumnetForAnnualViewModel>();
            foreach (var item in annualOrder)
            {
                OutPutDocumnetForAnnualViewModel model = new OutPutDocumnetForAnnualViewModel();
                model.TakenQuantity = 0;
                model.InputQuantity = 0;
                model.ItemId = item.ItemId;
                TakenQuantity.Add(model);
            }
            foreach (var element in outPutDoument)
            {
                var OutPutDocumnetDetails = _context.OutPutDocumentDetails.Where(o => o.OutPutDocumentId == element.OutPutDocumentID).ToList();
                for (int i = 0; i < OutPutDocumnetDetails.Count; i++)
                {
                    TakenQuantity[i].TakenQuantity += OutPutDocumnetDetails[i].Quantity;
                }
            }

            for (int i = 0; i < annualOrder.Count; i++)
            {
                int totalQuantity = annualOrder[i].FirstSemQuantity + annualOrder[i].SecondSemQuantity + annualOrder[i].ThirdSemQuantity;
                if (TakenQuantity[i].TakenQuantity != totalQuantity)
                {
                    check = false;
                    break;
                }
            }
            return check;
        }

    }

}

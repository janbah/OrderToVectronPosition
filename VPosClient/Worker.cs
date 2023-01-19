using Order2VPos.Core.Common;
using Order2VPos.Core.IoneApi;
using Order2VPos.Core.IoneApi.Orders;
using Order2VPos.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderToVectronPosition.IOneApi;

namespace Order2VPos.Core.VPosClient
{
    public static class Worker
    {
        static LogWriter logWriter = new LogWriter();

        public static async Task SendActualOrdersToVPosAsync()
        {
            using (CoreDbContext dbContext = CoreDbContext.GetContext())
            {
                IoneClient ioneClient = new IoneClient(new HttpClient());
                //ToDo: Inject this
                
                OrderListResponse currentOrders = await ioneClient.GetOrdersAsync(DateTime.Now.AddHours(-3), DateTime.Now.AddHours(3));

                IEnumerable<OrderListData> toSendOrders = currentOrders.Data.Where(x => !dbContext.Orders.Any(y => y.IoneRefId == x.Id && (y.Status == OrderStatus.Processed || y.Status == OrderStatus.Error))).ToArray();

                if (AppSettings.Default.GcRanges?.Count > 0)
                    toSendOrders = toSendOrders.Where(x => AppSettings.Default.GcRanges.Any(y => Convert.ToInt32(x.TableId) >= y.From && Convert.ToInt32(x.TableId) <= y.To));

                foreach (var toSendOrder in toSendOrders)
                {
                    Order newOrder = new Order { IoneRefId = toSendOrder.Id, IoneId = toSendOrder.IoneId, OrderTotal = toSendOrder.Total.GetDecimal(), OrderDate = toSendOrder.CreatedDate.GetDateTime() };
                    dbContext.Orders.Add(newOrder);
                    await dbContext.SaveChangesAsync();

                    VPosResponse response = await VPosCom.SendReceipt(GetReceiptFromOrder(toSendOrder));
                    if (!response.IsError)
                    {
                        newOrder.Status = OrderStatus.Processed;
                        newOrder.ReceiptMainNo = response.ReceiptMainNo;
                        newOrder.ReceiptTotal = response.SubTotal;
                        newOrder.ReceiptUUId = response.UUId;
                    }
                    else
                    {
                        newOrder.Status = OrderStatus.Error;
                        newOrder.Message = response.Message;
                        newOrder.VPosErrorNumber = response.VPosErrorNumber;
                        newOrder.IsCanceledOnPos = response.IsCanceled;
                    }

                    dbContext.SaveChanges();

                    if (newOrder.Status == OrderStatus.Error)
                    {
                        string errorMessage = $"Fehler beim Buchen einer Online-Bestellung! Tisch: {toSendOrder.TableId}, Bestellnummer: {toSendOrder.IoneId}, Fehler: {newOrder.Message}.";
                        VPosCom.SendMessage(errorMessage);
                        logWriter.WriteEntry(errorMessage, System.Diagnostics.EventLogEntryType.Warning, 11);
                    }
                    else if (newOrder.Status == OrderStatus.Processed)
                    {
                        logWriter.WriteEntry($"Bestellung [{toSendOrder.IoneId}] für Tisch {toSendOrder.TableId} erfolgreich verarbeitet.", System.Diagnostics.EventLogEntryType.Information, 10);
                    }
                }
            }
        }

        static Receipt GetReceiptFromOrder(OrderListData orderData)
        {
            string gcText = $"{orderData.IoneId}";
            if (!string.IsNullOrEmpty(orderData.CustomerNotes))
                gcText += $" - {orderData.CustomerNotes}";
            Receipt newReceipt = new Receipt
            {
                Gc = Convert.ToInt32(orderData.TableId),
                Operator = AppSettings.Default.Operator,
                OperatorCode = AppSettings.Default.OperatorCode,
                MediaNo = AppSettings.Default.ReceiptMediaNo,
                GcText = gcText
            };

            foreach (var orderItem in orderData.OrderItemList)
            {
                if (orderItem.ItemId != -10)
                {
                    Plu newPlu = new Plu
                    {
                        Number = Convert.ToInt32(orderItem.APIObjectId),
                        Quantity = orderItem.Quantity.GetDecimal(),
                        OverridePriceFactor = orderItem.Quantity.GetDecimal(),
                        OverridePriceValue = orderItem.Price.GetDecimal(),
                        ModifyPriceAbsoluteFactor = orderItem.DiscountUnit == "2" ? orderItem.DiscountedQuantity.GetDecimal() : 0,
                        ModifyPriceAbsoluteValue = orderItem.DiscountUnit == "2" ? -orderItem.Discount.GetDecimal() : 0,
                        ModifyPricePercentFactor = orderItem.DiscountUnit == "1" ? orderItem.DiscountedQuantity.GetDecimal() : 0,
                        ModifyPricePercentValue = orderItem.DiscountUnit == "1" ? -orderItem.Discount.GetDecimal() : 0
                    };

                    List<OrderItemListItem> additionalOrderItems = new List<OrderItemListItem>();
                    AddAdditionalItems(ref additionalOrderItems, orderItem.ItemList);

                    foreach (var additionalOrderItem in additionalOrderItems)
                    {
                        newPlu.AdditionalPlus.Add(new Plu
                        {
                            Number = Convert.ToInt32(additionalOrderItem.APIObjectId),
                            Quantity = additionalOrderItem.Quantity.GetDecimal(),
                            OverridePriceFactor = additionalOrderItem.Quantity.GetDecimal(),
                            OverridePriceValue = additionalOrderItem.Price.GetDecimal(),
                            ModifyPriceAbsoluteFactor = additionalOrderItem.DiscountUnit == "2" ? additionalOrderItem.DiscountedQuantity.GetDecimal() : 0,
                            ModifyPriceAbsoluteValue = additionalOrderItem.DiscountUnit == "2" ? additionalOrderItem.Discount.GetDecimal() : 0,
                            ModifyPricePercentFactor = additionalOrderItem.DiscountUnit == "1" ? additionalOrderItem.Discount.GetDecimal() : 0,
                            ModifyPricePercentValue = additionalOrderItem.DiscountUnit == "1" ? additionalOrderItem.Discount.GetDecimal() : 0
                        });
                    }

                    newReceipt.Plus.Add(newPlu);
                }
                else
                    newReceipt.Discounts.Add(new Discount { Number = AppSettings.Default.TipDiscountNumber, Value = orderItem.Total.GetDecimal() });
            }

            return newReceipt;
        }

        static void AddAdditionalItems(ref List<OrderItemListItem> orderItemList, OrderItemListItem[] orderItems)
        {
            if (orderItems != null && orderItems.Length > 0)
            {
                orderItemList.AddRange(orderItems);
                foreach (var subItem in orderItems)
                {
                    AddAdditionalItems(ref orderItemList, subItem.ItemList);
                }
            }
        }
    }
}

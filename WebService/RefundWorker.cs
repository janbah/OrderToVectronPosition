using Order2VPos.Core.Common;
using Order2VPos.Core.IoneApi;
using Order2VPos.Core.IoneApi.Orders;
using Order2VPos.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderToVectronPosition.IOneApi;

namespace Order2VPos.Core.WebService
{
    public static class RefundWorker
    {
        public async static Task<RefundResult> RefundOrder(int receiptMainNo)
        {
            RefundResult result = new RefundResult();

            CoreDbContext dbContext = CoreDbContext.GetContext();
            var order = dbContext.Orders.OrderBy(x => x.OrderDate).FirstOrDefault(x => x.ReceiptMainNo == receiptMainNo);

            if (order != null)
            {
                try
                {
                    IoneClient ioneClient = new IoneClient(new HttpClient());
                    //Todo: Inject this
                    
                    RefundResponse refundResponse = await ioneClient.ProcessRefund(order.IoneRefId);
                    result.Message = refundResponse.Message;
                    switch (refundResponse.StatusCode)
                    {
                        case 1:
                            result.RefundStatus = RefundStatus.Success;
                            break;
                        case 2:
                            result.RefundStatus = RefundStatus.Fail;
                            break;
                        case 3:
                            result.RefundStatus = RefundStatus.Canceled;
                            break;
                        case 4:
                            result.RefundStatus = RefundStatus.Pending;
                            break;
                    }

                    new LogWriter().WriteEntry($"Rückbuchung für Rechnung {receiptMainNo} mit StatusCode {refundResponse.StatusCode} durchgeführt. Meldung: {refundResponse.Message}", System.Diagnostics.EventLogEntryType.Information, 300);
                }
                catch (Exception ex)
                {
                    new LogWriter().WriteEntry($"Rückbuchung für Rechnung {receiptMainNo} fehlgeschlagen. Fehler: {ex.Message}", System.Diagnostics.EventLogEntryType.Information, 301);

                    result.RefundStatus = RefundStatus.Error;
                    result.Message = ex.Message;
                }
            }
            else
                result.RefundStatus = RefundStatus.NoCorrespondingOrder;

            return result;
        }
    }
}

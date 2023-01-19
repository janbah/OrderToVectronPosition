using Newtonsoft.Json;
using Order2VPos.Core.Common;
using Order2VPos.Core.IoneApi;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Order2VPos.Core.WebService
{
    public class WebServer
    {
        HttpListener listener;

        public void Listen()
        {

            listener = new HttpListener();
            listener.Prefixes.Add(AppSettings.Default.WebServiceUrlPrefix);

            listener.Start();
            listener.BeginGetContext(new AsyncCallback(contextReceivedCallback), null);
        }

        public void ShutDown()
        {
            listener.Close();
        }

        private void contextReceivedCallback(IAsyncResult asyncResult)
        {
            HttpListenerContext context = listener.EndGetContext(asyncResult);

            listener.BeginGetContext(new AsyncCallback(contextReceivedCallback), null);
            processRequest(context).Wait();
        }

        private async Task processRequest(HttpListenerContext context)
        {
            if (context.Request.HttpMethod == "GET")
            {
                ResponseMessage responseMessage = new ResponseMessage();

                try
                {
                    if (context.Request.QueryString.HasKeys())
                    {
                        int mainReceiptNo = Convert.ToInt32(context.Request.QueryString["ReceiptMainNo"]);

                        var result = await RefundWorker.RefundOrder(mainReceiptNo);

                        responseMessage.Message = result.Message;

                        if (result.RefundStatus == RefundStatus.Success)
                            responseMessage.RefundSuccess = true;
                        else if (result.RefundStatus == RefundStatus.NoCorrespondingOrder)
                            responseMessage.ReceiptIgnored = true;
                    }
                }
                catch (Exception ex)
                {
                    responseMessage.Message = ex.Message;
                }
                finally
                {
                    string responseText = JsonConvert.SerializeObject(responseMessage);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(responseText);
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 200;
                    context.Response.ContentLength64 = responseBytes.Length;
                    context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                    context.Response.Close();
                }
            }
        }
    }
}

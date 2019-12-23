using Domain.ECommerceIntegration.PagarMe;
using Domain.ECommerceIntegration.Woocommerce;
using Infrastructure.ExcelServices.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Api.Scheduler
{
    public class ScheduleTask : ScheduledProcessor
    {

        private readonly IConfiguration _configuration;
        private IHostingEnvironment _env;

        public ScheduleTask(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, IHostingEnvironment env) 
            : base(serviceScopeFactory)
        {
            _configuration = configuration;
            _env = env;
        }

        protected override string Schedule => "23 15 * * *";

        public override async Task ProcessInScope(IServiceProvider serviceProvider)
        {
            Console.WriteLine("Processing starts here");

            var transactions = new List<TransactionItem>();
            var transactionsToExport = new List<TransactionExport>();

            var fileName = string.Format("{0}.xlsx", DateTime.Now.ToString("dd-MM-yyyy"));

            try
            {

                transactions = await GetTransactions();

                if (transactions.Count == 0)
                {
                    var exc = new Exception($"no-transaction-found");
                    SentrySdk.CaptureException(exc);
                }

                for (int i = 0; i < transactions.Count; i++)
                {
                    transactions[i].payables = await GetTransactionsPayables(transactions[i].id.ToString());
                    var order = await GetEcommerceOrder(transactions[i].metadata.order_number);

                    if (order != null && order.order.line_items.Count > 0)
                    {
                        transactions[i].product = order.order.line_items[0].name;
                    }

                    for (int k = 0; k < transactions[i].payables.Count; k++)
                    {
                        var transaction = new TransactionExport
                        {
                            Status = transactions[i].status,
                            DateCreated = transactions[i].formated_date_created,
                            DateUpdated = transactions[i].formated_date_updated,
                            PaidAmount = transactions[i].paid_amount,
                            Installments = transactions[i].installments.Value,
                            PaymentMethod = transactions[i].payment_method,
                            Id = transactions[i].id,
                            State = transactions[i].address != null ? transactions[i].address.state : "",
                            Name = transactions[i].customer != null ? transactions[i].customer.name : "",
                            Email = transactions[i].customer != null ? transactions[i].customer.email : "",
                            DocumentType = transactions[i].customer != null ? transactions[i].customer.document_type : "",
                            DocumentNumber = transactions[i].customer != null ? transactions[i].customer.document_number : "",
                            Product = transactions[i].product,
                            PayableStatus = transactions[i].customer != null ? transactions[i].payables[k].status : "",
                            PayableType = transactions[i].customer != null ? transactions[i].payables[k].type : "",
                            PayableAmount = transactions[i].payables != null ? transactions[i].payables[k].amount : 0,
                            PayableFee = transactions[i].payables != null ? transactions[i].payables[k].fee : 0,
                            PayableAnticipationFee = transactions[i].payables != null ? transactions[i].payables[k].anticipation_fee : 0,
                            PayableInstallment = transactions[i].payables != null ? transactions[i].payables[k].installment : 0,
                            PayablePaymentDate = transactions[i].payables != null ? transactions[i].payables[k].formated_payment_date : "",
                            PayableAccuralDate = transactions[i].payables != null ? transactions[i].payables[k].formated_accural_date : "",
                            OrderNumber = transactions[i].metadata != null ? transactions[i].metadata.order_number : 0
                        };

                        transactionsToExport.Add(transaction);
                    }
                }


                var bytes = transactionsToExport.ToExcel("answers");
                var webRoot = _env.WebRootPath;
                var reportFolderPath = System.IO.Path.Combine(webRoot, "finance_reports");

                if (!Directory.Exists(reportFolderPath))
                {
                    Directory.CreateDirectory(reportFolderPath);
                }

                var file = System.IO.Path.Combine(webRoot, "finance_reports", fileName);
                File.WriteAllBytes(file, bytes);
            }
            catch(Exception ex)
            {
                string content = JsonConvert.SerializeObject(ex);

                var exc = new Exception($"finance_report_error: {content}");
                SentrySdk.CaptureException(exc);
            }
        }

        private async Task<List<TransactionItem>> GetTransactions()
        {
            var transactions = new List<TransactionItem>();

            var response = await PagarMe.GetTransactions(_configuration);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                transactions = JsonConvert.DeserializeObject<List<TransactionItem>>(
                    content
                );
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync();

                var exc = new Exception($"transaction-not-found: {content}");
                SentrySdk.CaptureException(exc);
            }

            return transactions;
        }

        private async Task<List<Payable>> GetTransactionsPayables(string transactionId)
        {
            var payables = new List<Payable>();

            var response = await PagarMe.GetTransactionsPayables(transactionId, _configuration);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                payables = JsonConvert.DeserializeObject<List<Payable>>(
                    content
                );
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync();

                var exc = new Exception($"payable-not-found: {content}");
                SentrySdk.CaptureException(exc);
            }

            return payables;
        }

        private async Task<EcommerceResponse> GetEcommerceOrder(long orderId)
        {
            var response = await Woocommerce.GetOrderById(
                orderId, _configuration
            );

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                //var parsed = JObject.Parse(content);
                return JsonConvert.DeserializeObject<EcommerceResponse>(
                    content
                );
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync();
                var exc = new Exception($"order-not-found: {content}");
                SentrySdk.CaptureException(exc);
            }

            return null;
        }
    }

    public class TransactionExport
    {
        public string Status { get; set; }
        public string DateCreated { get; set; }
        public string DateUpdated { get; set; }
        public decimal PaidAmount { get; set; }
        public int Installments { get; set; }
        public string PaymentMethod { get; set; }
        public int Id { get; set; }
        public string Product { get; set; }
        public string State { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentType { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public long OrderNumber { get; set; }
        public string PayableStatus { get; set; }
        public string PayableType { get; set; }
        public int PayableAmount { get; set; }
        public int PayableFee { get; set; }
        public int PayableAnticipationFee { get; set; }
        public int? PayableInstallment { get; set; }
        public string PayablePaymentDate { get; set; }
        public string PayableAccuralDate { get; set; }
    }

    public class EcommerceResponse
    {
        public OrderItem order { get; set; }
    }

    public class OrderItem
    {
        public long id { get; set; }
        public string status { get; set; }
        public List<LineItem> line_items { get; set; }
    }

    public class LineItem
    {
        public string name { get; set; }
        public long product_id { get; set; }
    }

}

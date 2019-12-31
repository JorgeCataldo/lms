using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Data;
using MediatR;
using Tg4.Infrastructure.Functional;
using Microsoft.Extensions.Configuration;
using Domain.ECommerceIntegration.PagarMe;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Domain.ECommerceIntegration.Woocommerce;

namespace Domain.Aggregates.Report.Queries
{
    public class GetFinanceReportQuery
    {
        public class Contract : IRequest<Result<List<TransactionItem>>>
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string UserRole { get; set; }
        }

        public class ExtractMovement
        {
            public TransactionItem Transaction { get; set; }

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

        //public class ExtractMovement
        //{
        //    public string OrderId { get; set; }
        //    public DateTime DateCreated { get; set; }
        //    public DateTime DateUpdated { get; set; }
        //    public string PaymentMethod { get; set; }
        //    public decimal PaidAmount { get; set; }
        //    public string Status { get; set; }
        //    public int Instalments { get; set; }
        //    public string Object { get; set; }
        //    public string ObjectId { get; set; }
        //    public Customer Customer { get; set; }
        //}


        public class Handler : IRequestHandler<Contract, Result<List<TransactionItem>>>
        {
            private readonly IDbContext _db;
            private readonly IConfiguration _configuration;

            public Handler(IDbContext db, IConfiguration configuration)
            {
                _db = db;
                _configuration = configuration;
            }

            public async Task<Result<List<TransactionItem>>> Handle(Contract request, CancellationToken cancellationToken)
            {
                var transactions = new List<TransactionItem>();

                if (request.UserRole != "Admin" && request.UserRole != "HumanResources" && request.UserRole != "Recruiter")
                    return Result.Fail<List<TransactionItem>>("Acesso Negado");

                transactions = await GetTransactions(cancellationToken);

                if(transactions.Count == 0)
                {
                    return Result.Fail<List<TransactionItem>>("Não existem transações para o período selecionado.");
                }

                for (int i = 0; i < transactions.Count; i++)
                {
                    transactions[i].payables = await GetTransactionsPayables(transactions[i].id.ToString(), cancellationToken);

                    var order = await GetEcommerceOrder(transactions[i].metadata.order_number, cancellationToken);

                    if (order != null && order.order.line_items.Count > 0)
                    {
                        transactions[i].product = order.order.line_items[0].name;
                    }
                }
                
                return Result.Ok(transactions);
            }

            private async Task<List<TransactionItem>> GetTransactions(CancellationToken cancellationToken)
            {
                var transactions = new List<TransactionItem>();

                var response = await PagarMe.GetTransactions(_configuration);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    //var parsed = JObject.Parse(content);
                    transactions = JsonConvert.DeserializeObject<List<TransactionItem>>(
                        content
                    );
                }
                else
                {
                    string content = await response.Content.ReadAsStringAsync();
                    await CreateErrorLog("transaction-not-found", content, cancellationToken);
                }
                               
                return transactions;
            }

            private async Task<List<Payable>> GetTransactionsPayables(string transactionId, CancellationToken cancellationToken)
            {
                var payables = new List<Payable>();

                var response = await PagarMe.GetTransactionsPayables(transactionId, _configuration);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    //var parsed = JObject.Parse(content);
                    payables = JsonConvert.DeserializeObject<List<Payable>>(
                        content
                    );
                }
                else
                {
                    string content = await response.Content.ReadAsStringAsync();
                    await CreateErrorLog("payable-not-found", content, cancellationToken);
                }

                return payables;
            }

            private async Task<bool> CreateErrorLog(
                string tag, string content, CancellationToken token
            )
            {
                var error = ErrorLog.Create(
                    ErrorLogTypeEnum.PagarMeIntegration,
                    tag, content
                ).Data;

                await _db.ErrorLogCollection.InsertOneAsync(
                    error, cancellationToken: token
                );

                return true;
            }

            private async Task<EcommerceResponse> GetEcommerceOrder(long orderId, CancellationToken token)
            {
                var response = await Woocommerce.GetOrderById(
                    orderId, _configuration
                );

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var parsed = JObject.Parse(content);
                    return JsonConvert.DeserializeObject<EcommerceResponse>(
                        content
                    );
                }
                else
                {
                    string content = await response.Content.ReadAsStringAsync();
                    await CreateErrorLog("order-not-found", content, token);
                }

                return null;
            }
        }

    }
}


using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ECommerceIntegration.PagarMe
{
    public class TransactionItem
    {
        public string status { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_updated { get; set; }
        public decimal paid_amount { get; set; }
        public int? installments { get; set; }
        public string payment_method { get; set; }
        public int id { get; set; }
        public Address address { get; set; }
        public Customer customer { get; set; }
        public Metadata metadata { get; set; }
        public List<Payable> payables { get; set; }
        public string product { get; set; }
        public string formated_date_updated {
            get
            {
                return this.date_updated != null ? this.date_updated.Value.ToString("dd/MM/yyyy") : null;
            }
        }
        public string formated_date_created
        {
            get
            {
                return this.date_created != null ? this.date_created.Value.ToString("dd/MM/yyyy") : null;
            }
        }
    }

    public class Address
    {
        public string state { get; set; }
    }

    public class Customer
    {
        public string document_number { get; set; }
        public string document_type { get; set; }
        public string name { get; set; }
        public string email { get; set; }
    }

    public class Metadata
    {
        public long order_number { get; set; }
    }

    public class Payable
    {
        public string status { get; set; }
        public string type { get; set; }
        public int amount { get; set; }
        public int fee { get; set; }
        public int anticipation_fee { get; set; }
        public int? installment { get; set; }
        public DateTime? payment_date { get; set; }
        public DateTime? accrual_date { get; set; }
        public string formated_payment_date {
            get
            {
                return this.payment_date != null ? this.payment_date.Value.ToString("dd/MM/yyyy") : null;
            }
        }
        public string formated_accural_date {
            get
            {
                return this.accrual_date != null ? this.accrual_date.Value.ToString("dd/MM/yyyy") : null;
            }
        }
    }
}

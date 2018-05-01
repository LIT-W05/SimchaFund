using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication12.Models
{
    public class Transaction
    {
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
}
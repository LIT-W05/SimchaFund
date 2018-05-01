using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimchaFund.Data;

namespace MvcApplication12.Models
{
    public class SimchaIndexViewModel
    {
        public int TotalContributors { get; set; }
        public IEnumerable<Simcha> Simchas { get; set; } 
    }
}
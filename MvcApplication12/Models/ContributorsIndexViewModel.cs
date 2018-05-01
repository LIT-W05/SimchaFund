using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimchaFund.Data;

namespace MvcApplication12.Models
{
    public class ContributorsIndexViewModel
    {
        public IEnumerable<Contributor> Contributors { get; set; } 
    }
}
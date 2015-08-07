using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharitiesOnline.Models
{
    public class GiftAidRepayment
    {
        public int Id { get; set; }
        public string Fore { get; set; }
        public string Sur { get; set; }
        public string House { get; set; }
        public string Postcode { get; set; }
        public string Sponsored { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string GiftAidType { get; set; } //AGG or DON

    }
}

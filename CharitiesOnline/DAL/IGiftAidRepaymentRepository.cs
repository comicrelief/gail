using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CharitiesOnline.Models;

namespace CharitiesOnline.DAL
{
    public interface IGiftAidRepaymentRepository
    {
        GiftAidRepayment GetById(int id);
        IEnumerable<GiftAidRepayment> GetAll();
        IEnumerable<GiftAidRepayment> GetByType(); // necessary?
    }
}

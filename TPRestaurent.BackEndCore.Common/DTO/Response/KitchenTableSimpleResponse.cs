using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class KitchenTableSimpleResponse
    {
        public Guid TableId { get; set; }
        public string TableName { get; set; }
        public Guid TableSessionId { get; set; }
        public int UnCheckedNumberOfDishes { get; set; }
    }
}

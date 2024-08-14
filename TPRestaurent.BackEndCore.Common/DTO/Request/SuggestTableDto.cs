using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class SuggestTableDto
    {
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int NumOfPeople { get; set; }
        public bool IsPrivate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class TotalUserByRankResponseDto
    {
        public UserRank UserRank { get; set; }
        public int Total {  get; set; }
        public double MinimumSpent { get; set; }
    }
}

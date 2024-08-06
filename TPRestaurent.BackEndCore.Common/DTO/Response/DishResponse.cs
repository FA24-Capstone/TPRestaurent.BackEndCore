using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class DishResponse
    {
        public Dish Dish { get; set; } = null!;
        public List<StaticFile> DishImgs { get; set; } = new List<StaticFile>();
        public List<RatingDishResponse> RatingDish { get; set; } = new List<RatingDishResponse>();
    }

    public class RatingDishResponse
    {
        public Rating Rating { get; set; } = null!;
        public List<StaticFile> RatingImgs { get; set; } = new List<StaticFile>();
    }
}

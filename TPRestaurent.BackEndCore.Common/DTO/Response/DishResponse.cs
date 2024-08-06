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
        public List<StaticFile> DishImgs = new List<StaticFile>();
        public List<RatingDishResponse> RatingDish = new List<RatingDishResponse>();  
    }

    public class RatingDishResponse
    {
        public List<Rating> RatingList = new List<Rating>();
        public List<StaticFile> RatingImgs = new List<StaticFile>();
    }
}

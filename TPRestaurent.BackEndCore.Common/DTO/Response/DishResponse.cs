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
        public List<DishSizeDetail> dishSizeDetails { get; set; } = null!;  
        public List<Image> DishImgs { get; set; } = new List<Image>();
        public List<RatingResponse> RatingDish { get; set; } = new List<RatingResponse>();
        public List<DishTag> DishTags = new List<DishTag>();
        public double AverageRating { get; set; } = 0;
        public int NumberOfRating { get; set; } = 0;
    }

    public class RatingResponse
    {
        public Rating Rating { get; set; } = null!;
        public List<Image> RatingImgs { get; set; } = new List<Image>();
    }
}

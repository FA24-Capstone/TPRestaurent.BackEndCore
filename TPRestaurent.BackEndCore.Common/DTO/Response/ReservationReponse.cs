using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class ReservationReponse
    {
        public Reservation Reservation { get; set; }
        public List<ReservationDishDto> ReservationDishes { get; set; } = new List<ReservationDishDto>();
        public List<TableDetail> ReservationTableDetails { get; set; } = new List<TableDetail>();
    }

    public class ReservationDishDto
    {
        public Guid ReservationDishId { get; set; }
        public Guid? DishSizeDetailId { get; set; }
        public DishSizeDetail? DishSizeDetail { get; set; }
        public ComboDishDto? ComboDish { get; set; }
    }

    public class ComboDishDto
    {
        public Guid? ComboId { get; set; }
        [ForeignKey(nameof(ComboId))]
        public Combo? Combo { get; set; }
        public List<DishCombo> DishCombos { get; set; } = new List<DishCombo> { };
    }
}

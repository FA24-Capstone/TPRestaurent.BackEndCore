using Microsoft.EntityFrameworkCore;

namespace TPRestaurent.BackEndCore.Domain.Data
{
    public class TPRestaurentDBContext : DbContext, IDBContext
    {
        public TPRestaurentDBContext()
        {
        }

        public TPRestaurentDBContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Models.Account> Accounts { get; set; } = null!;
        public DbSet<Models.Blog> Blogs { get; set; } = null!;
        public DbSet<Models.Category> Categories { get; set; } = null!;
        public DbSet<Models.Coupon> Coupons { get; set; } = null!;
        public DbSet<Models.CustomerInfo> CustomerInfos { get; set; } = null!;
        public DbSet<Models.Dish> Dishes { get; set; } = null!;
        public DbSet<Models.DishTag> DishTags { get; set; } = null!;
        public DbSet<Models.DishIngredient> DishIngredients { get; set; } = null!;
        public DbSet<Models.Ingredient> Ingredients { get; set; } = null!;
        public DbSet<Models.LoyalPointsHistory> LoyalPointsHistories { get; set; } = null!;
        public DbSet<Models.Order> Orders { get; set; } = null!;
        public DbSet<Models.OrderDetail> OrderDetails { get; set; } = null!;
        public DbSet<Models.OTP> OTPs { get; set; } = null!;
        public DbSet<Models.Rating> Ratings { get; set; } = null!;
        public DbSet<Models.Reservation> Reservations { get; set; } = null!;
        public DbSet<Models.ReservationDish> ReservationDishes { get; set; } = null!;
        public DbSet<Models.StaticFile> StaticFiles { get; set; } = null!;
        public DbSet<Models.Table> Tables { get; set; } = null!;
        public DbSet<Models.TableRating> TableRatings { get; set; } = null!;
        public DbSet<Models.Tag> Tags { get; set; } = null!;
        public DbSet<Models.Token> Tokens { get; set; } = null!;

     

    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TPRestaurent.BackEndCore.Domain.Models;
using TPRestaurent.BackEndCore.Domain.Models.EnumModels;

namespace TPRestaurent.BackEndCore.Domain.Data
{
    public class TPRestaurentDBContext : IdentityDbContext<Account>, IDBContext
    {
        public TPRestaurentDBContext()
        {
        }

        public TPRestaurentDBContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Models.Account> Accounts { get; set; } = null!;
        public DbSet<Models.Blog> Blogs { get; set; } = null!;
        public DbSet<Models.Coupon> Coupons { get; set; } = null!;
        public DbSet<Models.CustomerInfo> CustomerInfos { get; set; } = null!;
        public DbSet<Models.Dish> Dishes { get; set; } = null!;
        public DbSet<Models.DishTag> DishTags { get; set; } = null!;
        public DbSet<Models.LoyalPointsHistory> LoyalPointsHistories { get; set; } = null!;
        public DbSet<Models.Order> Orders { get; set; } = null!;
        public DbSet<Models.OrderDetail> OrderDetails { get; set; } = null!;
        public DbSet<Models.OTP> OTPs { get; set; } = null!;
        public DbSet<Models.Rating> Ratings { get; set; } = null!;
        public DbSet<Models.TableDetail> ReservationTableDetail { get; set; } = null!;
        public DbSet<Models.StaticFile> StaticFiles { get; set; } = null!;
        public DbSet<Models.Table> Tables { get; set; } = null!;
        public DbSet<Models.Room> TableRatings { get; set; } = null!;
        public DbSet<Models.Tag> Tags { get; set; } = null!;
        public DbSet<Models.Token> Tokens { get; set; } = null!;
        public DbSet<Models.Configuration> Configurations { get; set; } = null!;
        public DbSet<Models.Combo> Combos { get; set; } = null!;
        public DbSet<Models.DishCombo> DishCombos { get; set; } = null!;
        public DbSet<Models.StoreCredit> StoreCredits { get; set; } = null!;
        public DbSet<Models.DishSizeDetail> DishSizeDetails { get; set; } = null!;
        public DbSet<Models.CustomerLovedDish> CustomerLovedDishes { get; set; } = null!;
        public DbSet<Models.CustomerSavedCoupon> CustomerSavedCoupons { get; set; } = null!;
        public DbSet<Models.CustomerInfoAddress> CustomerInfoAddress { get; set; } = null!;
        public DbSet<Models.Transaction> Transactions { get; set; } = null!;
        public DbSet<Models.Device> Devices { get; set; } = null!;
        public DbSet<Models.ComboOrderDetail> DishComboComboDetails { get; set; } = null!;
        public DbSet<Models.ComboOptionSet> ComboOptionSets { get; set; } = null!;
        public DbSet<Models.EnumModels.OrderStatus> OrderStatuses { get; set; } = null!;
        public DbSet<Models.EnumModels.OTPType> OTPTypes { get; set; } = null!;
        public DbSet<Models.EnumModels.PaymentMethod> PaymentMethods { get; set; } = null!;
        public DbSet<Models.EnumModels.RatingPoint> RatingPoints { get; set; } = null!;
        public DbSet<Models.EnumModels.DishItemType> DishItemTypes { get; set; } = null!;
        public DbSet<Models.EnumModels.ReservationRequestStatus> ReservationRequestStatuses { get; set; } = null!;
        public DbSet<Models.EnumModels.TableSize> TableSizes { get; set; } = null!;
        public DbSet<Models.EnumModels.ComboCategory> ComboCategories { get; set; } = null!;
        public DbSet<Models.EnumModels.DishSize> DishSizes { get; set; } = null!;
        public DbSet<Models.EnumModels.ReservationStatus> ReservationStatuses { get; set; } = null!;
        public DbSet<Models.EnumModels.PreListOrderStatus> PreListOrderStatuses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
           
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "85b6791c-49d8-4a61-ad0b-8274ec27e764",
                Name = "STAFF",
                ConcurrencyStamp = "85b6791c-49d8-4a61-ad0b-8274ec27e764",
                NormalizedName = "staff"
            });
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "814f9270-78f5-4503-b7d3-0c567e5812ba",
                Name = "MANAGER",
                ConcurrencyStamp = "814f9270-78f5-4503-b7d3-0c567e5812ba",
                NormalizedName = "manager"
            });
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "000f9270-78f5-4503-b7d3-0c567e5812ba",
                Name = "CHEF",
                ConcurrencyStamp = "000f9270-78f5-4503-b7d3-0c567e5812ba",
                NormalizedName = "chef"
            });
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "02962efa-1273-46c0-b103-7167b1742ef3",
                Name = "CUSTOMER",
                ConcurrencyStamp = "02962efa-1273-46c0-b103-7167b1742ef3",
                NormalizedName = "customer"
            });
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "12962efa-1273-46c0-b103-7167b1742ef3",
                Name = "DEVICE",
                ConcurrencyStamp = "12962efa-1273-46c0-b103-7167b1742ef3",
                NormalizedName = "device"
            });


            base.OnModelCreating(builder);

            SeedEnumTable<Models.EnumModels.OrderStatus, Enums.OrderStatus>(builder);
            SeedEnumTable<Models.EnumModels.OTPType, Enums.OTPType>(builder);
            SeedEnumTable<Models.EnumModels.PaymentMethod, Enums.PaymentMethod>(builder);
            SeedEnumTable<Models.EnumModels.RatingPoint, Enums.RatingPoint>(builder);
            SeedEnumTable<Models.EnumModels.DishItemType, Enums.DishItemType>(builder);
            SeedEnumTable<Models.EnumModels.ReservationRequestStatus, Enums.ReservationRequestStatus>(builder);
            SeedEnumTable<Models.EnumModels.TableSize, Enums.TableSize>(builder);
            SeedEnumTable<Models.EnumModels.ComboCategory, Enums.ComboCategory>(builder);
            SeedEnumTable<Models.EnumModels.DishSize, Enums.DishSize>(builder);
            SeedEnumTable<Models.EnumModels.ReservationStatus, Enums.ReservationStatus>(builder);
            SeedEnumTable<Models.EnumModels.PreListOrderStatus, Enums.PreListOrderStatus>(builder);
            SeedEnumTable<Models.EnumModels.TransationStatus, Enums.TransationStatus>(builder);

        }

        private static void SeedEnumTable<TEntity, TEnum>(ModelBuilder modelBuilder)
                 where TEntity : class
                 where TEnum : System.Enum
        {
            var enumType = typeof(TEnum);
            var entityType = typeof(TEntity);

            if (!enumType.IsEnum)
            {
                throw new ArgumentException("TEnum must be an enum type.");
            }

            var enumValues = System.Enum.GetValues(enumType).Cast<TEnum>();

            foreach (var enumValue in enumValues)
            {
                var entityInstance = Activator.CreateInstance(entityType);
                entityType.GetProperty("Id")!.SetValue(entityInstance, enumValue);
                entityType.GetProperty("Name")!.SetValue(entityInstance, enumValue.ToString());
                modelBuilder.Entity<TEntity>().HasData(entityInstance!);
            }
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    IConfiguration config = new ConfigurationBuilder()
        //                   .SetBasePath(Directory.GetCurrentDirectory())
        //                   .AddJsonFile("appsettings.json", true, true)
        //                   .Build();
        //    //string cs = config["ConnectionStrings:DB"];
        //    //if (!optionsBuilder.IsConfigured)
        //    //{
        //    //    optionsBuilder.UseSqlServer(cs);
        //    //}
        //    optionsBuilder.UseSqlServer(
        //    "server=.;database=TPRestaurent;uid=sa;pwd=12345;TrustServerCertificate=True;MultipleActiveResultSets=True;");
        //}
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TPRestaurent.BackEndCore.Domain.Models;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

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
        public DbSet<Models.Blog> Blogs { get; set; } = null!;
        public DbSet<Models.CouponProgram> CouponPrograms { get; set; } = null!;
        public DbSet<Models.Coupon> Coupons { get; set; } = null!;
        public DbSet<Models.Account> Accounts { get; set; } = null!;
        public DbSet<Models.Dish> Dishes { get; set; } = null!;
        public DbSet<Models.DishTag> DishTags { get; set; } = null!;
        public DbSet<Models.LoyalPointsHistory> LoyalPointsHistories { get; set; } = null!;
        public DbSet<Models.Order> Orders { get; set; } = null!;
        public DbSet<Models.OrderDetail> OrderDetails { get; set; } = null!;
        public DbSet<Models.Rating> Ratings { get; set; } = null!;
        public DbSet<Models.TableDetail> ReservationTableDetail { get; set; } = null!;
        public DbSet<Models.Image> StaticFiles { get; set; } = null!;
        public DbSet<Models.Invoice> Invoices { get; set; } = null!;
        public DbSet<Models.Table> Tables { get; set; } = null!;
        public DbSet<Models.Room> TableRatings { get; set; } = null!;
        public DbSet<Models.Tag> Tags { get; set; } = null!;
        public DbSet<Models.Token> Tokens { get; set; } = null!;
        public DbSet<Models.Configuration> Configurations { get; set; } = null!;
        public DbSet<Models.Combo> Combos { get; set; } = null!;
        public DbSet<Models.DishCombo> DishCombos { get; set; } = null!;
        public DbSet<Models.DishSizeDetail> DishSizeDetails { get; set; } = null!;
        public DbSet<Models.CustomerInfoAddress> CustomerInfoAddress { get; set; } = null!;
        public DbSet<Models.Transaction> Transactions { get; set; } = null!;
        public DbSet<Models.ComboOrderDetail> DishComboComboDetails { get; set; } = null!;
        public DbSet<Models.ComboOptionSet> ComboOptionSets { get; set; } = null!;
        public DbSet<Models.ConfigurationVersion> ConfigurationVersions { get; set; } = null!;
        public DbSet<Models.GroupedDishCraft> GroupedDishCrafts { get; set; } = null!;
        public DbSet<Models.NotificationMessage> NotificationMessages { get; set; } = null!;
        public DbSet<Models.OrderAssignedRequest> OrderAssignedRequests { get; set; } = null!;
        public DbSet<Models.EnumModels.OrderStatus> OrderStatuses { get; set; } = null!;
        public DbSet<Models.EnumModels.OTPType> OTPTypes { get; set; } = null!;
        public DbSet<Models.EnumModels.PaymentMethod> PaymentMethods { get; set; } = null!;
        public DbSet<Models.EnumModels.RatingPoint> RatingPoints { get; set; } = null!;
        public DbSet<Models.EnumModels.DishItemType> DishItemTypes { get; set; } = null!;
        public DbSet<Models.EnumModels.TableSize> TableSizes { get; set; } = null!;
        public DbSet<Models.EnumModels.ComboCategory> ComboCategories { get; set; } = null!;
        public DbSet<Models.EnumModels.DishSize> DishSizes { get; set; } = null!;
        public DbSet<Models.EnumModels.OrderDetailStatus> OrderDetailStatuses { get; set; } = null!;
        public DbSet<Models.EnumModels.TransactionType> TransactionTypes { get; set; } = null!;
        public DbSet<Models.EnumModels.OrderSessionStatus> OrderSessionStatuses { get; set; } = null!;
        public DbSet<Models.EnumModels.DishComboDetailStatus> DishComboDetailStatuses { get; set; } = null!;
        public DbSet<Models.EnumModels.OrderAssignedStatus> OrderAssignedStatuses { get; set; } = null!;
        public DbSet<Models.EnumModels.TableStatus> TableStatuses { get; set; } = null!;
        public DbSet<Models.EnumModels.CouponProgramType> CouponProgramTypes { get; set; } = null!;
        public DbSet<Models.EnumModels.UserRank> UserRanks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "85b6791c-49d8-4a61-ad0b-8274ec27e764",
                Name = "SHIPPER",
                ConcurrencyStamp = "85b6791c-49d8-4a61-ad0b-8274ec27e764",
                NormalizedName = "shipper"
            });
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "814f9270-78f5-4503-b7d3-0c567e5812ba",
                Name = "ADMIN",
                ConcurrencyStamp = "814f9270-78f5-4503-b7d3-0c567e5812ba",
                NormalizedName = "admin"
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
            IConfiguration enums = GetVietnameseNames();
            SeedEnumTable<Models.EnumModels.OrderStatus, Enums.OrderStatus>(builder, enums);
            SeedEnumTable<Models.EnumModels.OTPType, Enums.OTPType>(builder, enums);
            SeedEnumTable<Models.EnumModels.PaymentMethod, Enums.PaymentMethod>(builder, enums);
            SeedEnumTable<Models.EnumModels.RatingPoint, Enums.RatingPoint>(builder, enums);
            SeedEnumTable<Models.EnumModels.DishItemType, Enums.DishItemType>(builder, enums);
            SeedEnumTable<Models.EnumModels.TableSize, Enums.TableSize>(builder, enums);
            SeedEnumTable<Models.EnumModels.ComboCategory, Enums.ComboCategory>(builder, enums);
            SeedEnumTable<Models.EnumModels.DishSize, Enums.DishSize>(builder, enums);
            SeedEnumTable<Models.EnumModels.TransationStatus, Enums.TransationStatus>(builder, enums);
            SeedEnumTable<Models.EnumModels.OrderDetailStatus, Enums.OrderDetailStatus>(builder, enums);
            SeedEnumTable<Models.EnumModels.OrderType, Enums.OrderType>(builder, enums);
            SeedEnumTable<Models.EnumModels.TransactionType, Enums.TransactionType>(builder, enums);
            SeedEnumTable<Models.EnumModels.OrderSessionStatus, Enums.OrderSessionStatus>(builder, enums);
            SeedEnumTable<Models.EnumModels.DishComboDetailStatus, Enums.DishComboDetailStatus>(builder, enums);
            SeedEnumTable<Models.EnumModels.OrderAssignedStatus, Enums.OrderAssignedStatus>(builder, enums);
            SeedEnumTable<Models.EnumModels.TableStatus, Enums.TableStatus>(builder, enums);
            SeedEnumTable<Models.EnumModels.CouponProgramType, Enums.CouponProgramType>(builder, enums);
            SeedEnumTable<Models.EnumModels.UserRank, Enums.UserRank>(builder, enums);
        }

        private static void SeedEnumTable<TEntity, TEnum>(ModelBuilder modelBuilder, IConfiguration enums)
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
                string? vietnameseName = enums[$"{enumValue.ToString()}"];
                Console.WriteLine(enumValue.ToString() + " " + vietnameseName);
                entityType.GetProperty("Id")!.SetValue(entityInstance, enumValue);
                entityType.GetProperty("Name")!.SetValue(entityInstance, enumValue.ToString());
                entityType.GetProperty("VietnameseName")!.SetValue(entityInstance, vietnameseName);
                modelBuilder.Entity<TEntity>().HasData(entityInstance!);
            }
        }
        public static IConfigurationSection GetVietnameseNames()
        {
            var configuration = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("vietnamesenames.json", optional: false, reloadOnChange: true)
                     .Build();
            return configuration.GetSection("VietnameseNames");
        }
    }
}
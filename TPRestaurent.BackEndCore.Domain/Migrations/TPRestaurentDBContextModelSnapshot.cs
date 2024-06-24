﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TPRestaurent.BackEndCore.Domain.Data;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    [DbContext(typeof(TPRestaurentDBContext))]
    partial class TPRestaurentDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.22")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NormalizedName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("IdentityRole");

                    b.HasData(
                        new
                        {
                            Id = "6a32e12a-60b5-4d93-8306-82231e1232d7",
                            ConcurrencyStamp = "6a32e12a-60b5-4d93-8306-82231e1232d7",
                            Name = "ADMIN",
                            NormalizedName = "admin"
                        },
                        new
                        {
                            Id = "85b6791c-49d8-4a61-ad0b-8274ec27e764",
                            ConcurrencyStamp = "85b6791c-49d8-4a61-ad0b-8274ec27e764",
                            Name = "STAFF",
                            NormalizedName = "staff"
                        },
                        new
                        {
                            Id = "814f9270-78f5-4503-b7d3-0c567e5812ba",
                            ConcurrencyStamp = "814f9270-78f5-4503-b7d3-0c567e5812ba",
                            Name = "SHIPPER",
                            NormalizedName = "shipper"
                        },
                        new
                        {
                            Id = "02962efa-1273-46c0-b103-7167b1742ef3",
                            ConcurrencyStamp = "02962efa-1273-46c0-b103-7167b1742ef3",
                            Name = "CUSTOMER",
                            NormalizedName = "customer"
                        });
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Account", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Gender")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RefreshTokenExpiryTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("VerifyCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Blog", b =>
                {
                    b.Property<Guid>("BlogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CreateBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Image")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UpdateBy")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("BlogId");

                    b.HasIndex("CreateBy");

                    b.HasIndex("UpdateBy");

                    b.ToTable("Blogs");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Category", b =>
                {
                    b.Property<Guid>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CategoryId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Coupon", b =>
                {
                    b.Property<Guid>("CouponId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DiscountPercent")
                        .HasColumnType("int");

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<double>("MinimumAmount")
                        .HasColumnType("float");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.HasKey("CouponId");

                    b.ToTable("Coupons");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.CustomerInfo", b =>
                {
                    b.Property<Guid>("CustomerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AccountId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("LoyaltyPoint")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CustomerId");

                    b.HasIndex("AccountId");

                    b.ToTable("CustomerInfos");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Dish", b =>
                {
                    b.Property<Guid>("DishId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Discount")
                        .HasColumnType("float");

                    b.Property<string>("Image")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Price")
                        .HasColumnType("float");

                    b.HasKey("DishId");

                    b.ToTable("Dishes");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.DishIngredient", b =>
                {
                    b.Property<Guid>("DishIngredientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("DishId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("IngredientId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("DishIngredientId");

                    b.HasIndex("DishId");

                    b.HasIndex("IngredientId");

                    b.ToTable("DishIngredients");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.DishTag", b =>
                {
                    b.Property<Guid>("DishTagId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("DishId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("TagId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("DishTagId");

                    b.HasIndex("DishId");

                    b.HasIndex("TagId");

                    b.ToTable("DishTags");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.EnumModels.OrderStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("OrderStatus");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Pending"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Processing"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Completed"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Cancelled"
                        });
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.EnumModels.OTPType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("OTPType");

                    b.HasData(
                        new
                        {
                            Id = 0,
                            Name = "Login"
                        },
                        new
                        {
                            Id = 1,
                            Name = "Register"
                        },
                        new
                        {
                            Id = 2,
                            Name = "ForgotPassword"
                        },
                        new
                        {
                            Id = 3,
                            Name = "ChangePassword"
                        },
                        new
                        {
                            Id = 4,
                            Name = "ChangeEmail"
                        },
                        new
                        {
                            Id = 5,
                            Name = "ChangePhone"
                        },
                        new
                        {
                            Id = 6,
                            Name = "ConfirmEmail"
                        },
                        new
                        {
                            Id = 7,
                            Name = "ConfirmPhone"
                        },
                        new
                        {
                            Id = 8,
                            Name = "ConfirmPayment"
                        });
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.EnumModels.PaymentMethod", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("PaymentMethod");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Cash"
                        },
                        new
                        {
                            Id = 2,
                            Name = "VNPAY"
                        },
                        new
                        {
                            Id = 3,
                            Name = "MOMO"
                        },
                        new
                        {
                            Id = 4,
                            Name = "ZALOPAY"
                        });
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.EnumModels.RatingPoint", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("RatingPoint");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "One"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Two"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Three"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Four"
                        },
                        new
                        {
                            Id = 5,
                            Name = "Five"
                        });
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Ingredient", b =>
                {
                    b.Property<Guid>("IngredientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Image")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IngredientId");

                    b.ToTable("Ingredients");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.LoyalPointsHistory", b =>
                {
                    b.Property<Guid>("LoyalPointsHistoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("NewBalance")
                        .HasColumnType("int");

                    b.Property<Guid?>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("PointChanged")
                        .HasColumnType("int");

                    b.Property<DateTime>("TransactionDate")
                        .HasColumnType("datetime2");

                    b.HasKey("LoyalPointsHistoryId");

                    b.HasIndex("OrderId");

                    b.ToTable("LoyalPointsHistories");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Order", b =>
                {
                    b.Property<Guid>("OrderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AccountId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid?>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("LoyalPointsHistoryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("PaymentMethod")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<double>("TotalAmount")
                        .HasColumnType("float");

                    b.HasKey("OrderId");

                    b.HasIndex("AccountId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("LoyalPointsHistoryId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.OrderDetail", b =>
                {
                    b.Property<Guid>("OrderDetailId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("DishId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Price")
                        .HasColumnType("float");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.HasKey("OrderDetailId");

                    b.HasIndex("DishId");

                    b.HasIndex("OrderId");

                    b.ToTable("OrderDetails");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.OTP", b =>
                {
                    b.Property<Guid>("OTPId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AccountId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ExpiredTime")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsUsed")
                        .HasColumnType("bit");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("OTPId");

                    b.HasIndex("AccountId");

                    b.ToTable("OTPs");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Rating", b =>
                {
                    b.Property<Guid>("RatingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AccountId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CreateBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("DishId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Point")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UpdateBy")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("RatingId");

                    b.HasIndex("AccountId");

                    b.HasIndex("CreateBy");

                    b.HasIndex("DishId");

                    b.HasIndex("UpdateBy");

                    b.ToTable("Ratings");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Reservation", b =>
                {
                    b.Property<Guid>("ReservationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Deposit")
                        .HasColumnType("float");

                    b.Property<int>("NumberOfPeople")
                        .HasColumnType("int");

                    b.Property<DateTime>("ReservationDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("TableId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ReservationId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("TableId");

                    b.ToTable("Reservations");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.ReservationDish", b =>
                {
                    b.Property<Guid>("ReservationDishId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("DishId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Note")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<Guid?>("ReservationId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ReservationDishId");

                    b.HasIndex("DishId");

                    b.HasIndex("ReservationId");

                    b.ToTable("ReservationDishes");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.StaticFile", b =>
                {
                    b.Property<Guid>("StaticFileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("BlogId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("DishId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("RatingId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("StaticFileId");

                    b.HasIndex("BlogId");

                    b.HasIndex("DishId");

                    b.HasIndex("RatingId");

                    b.ToTable("StaticFiles");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Table", b =>
                {
                    b.Property<Guid>("TableId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Capacity")
                        .HasColumnType("int");

                    b.Property<string>("TableName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("TableRatingId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TableId");

                    b.HasIndex("TableRatingId");

                    b.ToTable("Tables");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.TableRating", b =>
                {
                    b.Property<Guid>("TableRatingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TableRatingId");

                    b.ToTable("TableRatings");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Tag", b =>
                {
                    b.Property<Guid>("TagId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TagId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Token", b =>
                {
                    b.Property<Guid>("TokenId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AccessTokenValue")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AccountId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreateDateAccessToken")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreateRefreshToken")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ExpiryTimeAccessToken")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ExpiryTimeRefreshToken")
                        .HasColumnType("datetime2");

                    b.Property<string>("RefreshTokenValue")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TokenId");

                    b.HasIndex("AccountId");

                    b.ToTable("Tokens");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Account", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.CustomerInfo", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId");

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Blog", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Account", "CreateByAccount")
                        .WithMany()
                        .HasForeignKey("CreateBy")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Account", "UpdateByAccount")
                        .WithMany()
                        .HasForeignKey("UpdateBy");

                    b.Navigation("CreateByAccount");

                    b.Navigation("UpdateByAccount");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.CustomerInfo", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId");

                    b.Navigation("Account");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.DishIngredient", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Dish", "Dish")
                        .WithMany()
                        .HasForeignKey("DishId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Ingredient", "Ingredient")
                        .WithMany()
                        .HasForeignKey("IngredientId");

                    b.Navigation("Dish");

                    b.Navigation("Ingredient");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.DishTag", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Dish", "Dish")
                        .WithMany()
                        .HasForeignKey("DishId");

                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Tag", "Tag")
                        .WithMany()
                        .HasForeignKey("TagId");

                    b.Navigation("Dish");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.LoyalPointsHistory", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Order", "Order")
                        .WithMany()
                        .HasForeignKey("OrderId");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Order", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId");

                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.CustomerInfo", "CustomerInfo")
                        .WithMany()
                        .HasForeignKey("CustomerId");

                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.LoyalPointsHistory", "LoyalPointsHistory")
                        .WithMany()
                        .HasForeignKey("LoyalPointsHistoryId");

                    b.Navigation("Account");

                    b.Navigation("CustomerInfo");

                    b.Navigation("LoyalPointsHistory");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.OrderDetail", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Dish", "Dish")
                        .WithMany()
                        .HasForeignKey("DishId");

                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Order", "Order")
                        .WithMany()
                        .HasForeignKey("OrderId");

                    b.Navigation("Dish");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.OTP", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Rating", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Account", "CreateByAccount")
                        .WithMany()
                        .HasForeignKey("CreateBy")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Dish", "Dish")
                        .WithMany()
                        .HasForeignKey("DishId");

                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Account", "UpdateByAccount")
                        .WithMany()
                        .HasForeignKey("UpdateBy");

                    b.Navigation("Account");

                    b.Navigation("CreateByAccount");

                    b.Navigation("Dish");

                    b.Navigation("UpdateByAccount");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Reservation", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.CustomerInfo", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId");

                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Table", "Table")
                        .WithMany()
                        .HasForeignKey("TableId");

                    b.Navigation("Customer");

                    b.Navigation("Table");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.ReservationDish", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Dish", "Dish")
                        .WithMany()
                        .HasForeignKey("DishId");

                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Reservation", "Reservation")
                        .WithMany()
                        .HasForeignKey("ReservationId");

                    b.Navigation("Dish");

                    b.Navigation("Reservation");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.StaticFile", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Blog", "Blog")
                        .WithMany()
                        .HasForeignKey("BlogId");

                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Dish", "Dish")
                        .WithMany()
                        .HasForeignKey("DishId");

                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Rating", "Rating")
                        .WithMany()
                        .HasForeignKey("RatingId");

                    b.Navigation("Blog");

                    b.Navigation("Dish");

                    b.Navigation("Rating");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Table", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.TableRating", "TableRating")
                        .WithMany()
                        .HasForeignKey("TableRatingId");

                    b.Navigation("TableRating");
                });

            modelBuilder.Entity("TPRestaurent.BackEndCore.Domain.Models.Token", b =>
                {
                    b.HasOne("TPRestaurent.BackEndCore.Domain.Models.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });
#pragma warning restore 612, 618
        }
    }
}

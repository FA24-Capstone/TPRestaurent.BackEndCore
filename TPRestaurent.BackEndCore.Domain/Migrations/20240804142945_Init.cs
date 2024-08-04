using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TPRestaurent.BackEndCore.Domain.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "ComboCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    ConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActiveValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActiveDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.ConfigurationId);
                });

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    CouponId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MinimumAmount = table.Column<double>(type: "float", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Img = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.CouponId);
                });

            migrationBuilder.CreateTable(
                name: "Dishes",
                columns: table => new
                {
                    DishId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Discount = table.Column<double>(type: "float", nullable: false),
                    DishItemType = table.Column<int>(type: "int", nullable: false),
                    isAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dishes", x => x.DishId);
                });

            migrationBuilder.CreateTable(
                name: "DishItemTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishItemTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OTPTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RatingPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingPoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReservationRequestStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationRequestStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TableRatings",
                columns: table => new
                {
                    TableRatingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableRatings", x => x.TableRatingId);
                });

            migrationBuilder.CreateTable(
                name: "TableSizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.TagId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Combos",
                columns: table => new
                {
                    ComboId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<double>(type: "float", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combos", x => x.ComboId);
                    table.ForeignKey(
                        name: "FK_Combos_ComboCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ComboCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Tables",
                columns: table => new
                {
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableSizeId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    TableRatingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tables", x => x.TableId);
                    table.ForeignKey(
                        name: "FK_Tables_TableRatings_TableRatingId",
                        column: x => x.TableRatingId,
                        principalTable: "TableRatings",
                        principalColumn: "TableRatingId");
                    table.ForeignKey(
                        name: "FK_Tables_TableSizes_TableSizeId",
                        column: x => x.TableSizeId,
                        principalTable: "TableSizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "DishTags",
                columns: table => new
                {
                    DishTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DishId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishTags", x => x.DishTagId);
                    table.ForeignKey(
                        name: "FK_DishTags_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "DishId");
                    table.ForeignKey(
                        name: "FK_DishTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "TagId");
                });

            migrationBuilder.CreateTable(
                name: "DishCombos",
                columns: table => new
                {
                    DishComboId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HasOptions = table.Column<bool>(type: "bit", nullable: false),
                    OptionSetNumber = table.Column<int>(type: "int", nullable: true),
                    DishId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComboId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishCombos", x => x.DishComboId);
                    table.ForeignKey(
                        name: "FK_DishCombos_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "ComboId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_DishCombos_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "DishId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifyCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    BlogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdateBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.BlogId);
                    table.ForeignKey(
                        name: "FK_Blogs_AspNetUsers_CreateBy",
                        column: x => x.CreateBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Blogs_AspNetUsers_UpdateBy",
                        column: x => x.UpdateBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerInfos",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoyaltyPoint = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerInfos", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_CustomerInfos_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OTPs",
                columns: table => new
                {
                    OTPId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiredTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPs", x => x.OTPId);
                    table.ForeignKey(
                        name: "FK_OTPs_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    RatingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Point = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DishId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdateBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.RatingId);
                    table.ForeignKey(
                        name: "FK_Ratings_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Ratings_AspNetUsers_CreateBy",
                        column: x => x.CreateBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Ratings_AspNetUsers_UpdateBy",
                        column: x => x.UpdateBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Ratings_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "DishId");
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfPeople = table.Column<int>(type: "int", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerAccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Deposit = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.ReservationId);
                    table.ForeignKey(
                        name: "FK_Reservations_AspNetUsers_CustomerAccountId",
                        column: x => x.CustomerAccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessTokenValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDateAccessToken = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryTimeAccessToken = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefreshTokenValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateRefreshToken = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryTimeRefreshToken = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_Tokens_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "StoreCredits",
                columns: table => new
                {
                    StoreCreditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    CustomerInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreCredits", x => x.StoreCreditId);
                    table.ForeignKey(
                        name: "FK_StoreCredits_CustomerInfos_CustomerInfoId",
                        column: x => x.CustomerInfoId,
                        principalTable: "CustomerInfos",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "StaticFiles",
                columns: table => new
                {
                    StaticFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DishId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ComboId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BlogId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RatingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaticFiles", x => x.StaticFileId);
                    table.ForeignKey(
                        name: "FK_StaticFiles_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "BlogId");
                    table.ForeignKey(
                        name: "FK_StaticFiles_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "ComboId");
                    table.ForeignKey(
                        name: "FK_StaticFiles_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "DishId");
                    table.ForeignKey(
                        name: "FK_StaticFiles_Ratings_RatingId",
                        column: x => x.RatingId,
                        principalTable: "Ratings",
                        principalColumn: "RatingId");
                });

            migrationBuilder.CreateTable(
                name: "ReservationDishes",
                columns: table => new
                {
                    ReservationDishId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DishId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ComboId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationDishes", x => x.ReservationDishId);
                    table.ForeignKey(
                        name: "FK_ReservationDishes_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "ComboId");
                    table.ForeignKey(
                        name: "FK_ReservationDishes_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "DishId");
                    table.ForeignKey(
                        name: "FK_ReservationDishes_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "ReservationId");
                });

            migrationBuilder.CreateTable(
                name: "StoreCreditHistories",
                columns: table => new
                {
                    StoreCreditHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInput = table.Column<bool>(type: "bit", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    StoreCreditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreCreditHistories", x => x.StoreCreditHistoryId);
                    table.ForeignKey(
                        name: "FK_StoreCreditHistories_StoreCredits_StoreCreditId",
                        column: x => x.StoreCreditId,
                        principalTable: "StoreCredits",
                        principalColumn: "StoreCreditId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "LoyalPointsHistories",
                columns: table => new
                {
                    LoyalPointsHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PointChanged = table.Column<int>(type: "int", nullable: false),
                    NewBalance = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyalPointsHistories", x => x.LoyalPointsHistoryId);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    LoyalPointsHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_CustomerInfos_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerInfos",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_Orders_LoyalPointsHistories_LoyalPointsHistoryId",
                        column: x => x.LoyalPointsHistoryId,
                        principalTable: "LoyalPointsHistories",
                        principalColumn: "LoyalPointsHistoryId");
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DishId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ComboId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.OrderDetailId);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "ComboId");
                    table.ForeignKey(
                        name: "FK_OrderDetails_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "DishId");
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "000f9270-78f5-4503-b7d3-0c567e5812ba", "000f9270-78f5-4503-b7d3-0c567e5812ba", "CHEF", "chef" },
                    { "02962efa-1273-46c0-b103-7167b1742ef3", "02962efa-1273-46c0-b103-7167b1742ef3", "CUSTOMER", "customer" },
                    { "814f9270-78f5-4503-b7d3-0c567e5812ba", "814f9270-78f5-4503-b7d3-0c567e5812ba", "MANAGER", "manager" },
                    { "85b6791c-49d8-4a61-ad0b-8274ec27e764", "85b6791c-49d8-4a61-ad0b-8274ec27e764", "STAFF", "staff" }
                });

            migrationBuilder.InsertData(
                table: "ComboCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "HOTPOT" },
                    { 1, "BBQ" },
                    { 2, "BOTH" }
                });

            migrationBuilder.InsertData(
                table: "DishItemTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "APPETIZER" },
                    { 1, "SOUP" },
                    { 2, "HOTPOT" },
                    { 3, "BBQ" },
                    { 4, "HOTPOT_BROTH" },
                    { 5, "HOTPOT_MEAT" },
                    { 6, "HOTPOT_SEAFOOD" },
                    { 7, "HOTPOT_VEGGIE" },
                    { 8, "BBQ_MEAT" },
                    { 9, "BBQ_SEAFOOD" },
                    { 10, "HOTPOT_TOPPING" },
                    { 11, "BBQ_TOPPING" },
                    { 12, "SIDEDISH" },
                    { 13, "DRINK" },
                    { 14, "DESSERT" },
                    { 15, "SAUCE" }
                });

            migrationBuilder.InsertData(
                table: "OTPTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Login" },
                    { 1, "Register" },
                    { 2, "ForgotPassword" },
                    { 3, "ChangePassword" },
                    { 4, "ChangeEmail" },
                    { 5, "ChangePhone" },
                    { 6, "ConfirmEmail" },
                    { 7, "ConfirmPhone" },
                    { 8, "ConfirmPayment" }
                });

            migrationBuilder.InsertData(
                table: "OrderStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Pending" },
                    { 2, "Processing" },
                    { 3, "Completed" },
                    { 4, "Cancelled" }
                });

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Cash" },
                    { 2, "VNPAY" },
                    { 3, "MOMO" },
                    { 4, "ZALOPAY" }
                });

            migrationBuilder.InsertData(
                table: "RatingPoints",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "One" },
                    { 2, "Two" }
                });

            migrationBuilder.InsertData(
                table: "RatingPoints",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 3, "Three" },
                    { 4, "Four" },
                    { 5, "Five" }
                });

            migrationBuilder.InsertData(
                table: "ReservationRequestStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "PENDING" },
                    { 1, "SUCCESSFUL" },
                    { 2, "FAILED" }
                });

            migrationBuilder.InsertData(
                table: "TableSizes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 2, "TWO" },
                    { 4, "FOUR" },
                    { 6, "SIX" },
                    { 8, "EIGHT" },
                    { 10, "TEN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CustomerId",
                table: "AspNetUsers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_CreateBy",
                table: "Blogs",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_UpdateBy",
                table: "Blogs",
                column: "UpdateBy");

            migrationBuilder.CreateIndex(
                name: "IX_Combos_CategoryId",
                table: "Combos",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInfos_AccountId",
                table: "CustomerInfos",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DishCombos_ComboId",
                table: "DishCombos",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_DishCombos_DishId",
                table: "DishCombos",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_DishTags_DishId",
                table: "DishTags",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_DishTags_TagId",
                table: "DishTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyalPointsHistories_OrderId",
                table: "LoyalPointsHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ComboId",
                table: "OrderDetails",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_DishId",
                table: "OrderDetails",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AccountId",
                table: "Orders",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_LoyalPointsHistoryId",
                table: "Orders",
                column: "LoyalPointsHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OTPs_AccountId",
                table: "OTPs",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_AccountId",
                table: "Ratings",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_CreateBy",
                table: "Ratings",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_DishId",
                table: "Ratings",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UpdateBy",
                table: "Ratings",
                column: "UpdateBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationDishes_ComboId",
                table: "ReservationDishes",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationDishes_DishId",
                table: "ReservationDishes",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationDishes_ReservationId",
                table: "ReservationDishes",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CustomerAccountId",
                table: "Reservations",
                column: "CustomerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_StaticFiles_BlogId",
                table: "StaticFiles",
                column: "BlogId");

            migrationBuilder.CreateIndex(
                name: "IX_StaticFiles_ComboId",
                table: "StaticFiles",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_StaticFiles_DishId",
                table: "StaticFiles",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_StaticFiles_RatingId",
                table: "StaticFiles",
                column: "RatingId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreCreditHistories_StoreCreditId",
                table: "StoreCreditHistories",
                column: "StoreCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreCredits_CustomerInfoId",
                table: "StoreCredits",
                column: "CustomerInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_TableRatingId",
                table: "Tables",
                column: "TableRatingId");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_TableSizeId",
                table: "Tables",
                column: "TableSizeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_AccountId",
                table: "Tokens",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_CustomerInfos_CustomerId",
                table: "AspNetUsers",
                column: "CustomerId",
                principalTable: "CustomerInfos",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoyalPointsHistories_Orders_OrderId",
                table: "LoyalPointsHistories",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerInfos_AspNetUsers_AccountId",
                table: "CustomerInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_AccountId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CustomerInfos_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_LoyalPointsHistories_Orders_OrderId",
                table: "LoyalPointsHistories");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropTable(
                name: "DishCombos");

            migrationBuilder.DropTable(
                name: "DishItemTypes");

            migrationBuilder.DropTable(
                name: "DishTags");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "OrderStatuses");

            migrationBuilder.DropTable(
                name: "OTPs");

            migrationBuilder.DropTable(
                name: "OTPTypes");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "RatingPoints");

            migrationBuilder.DropTable(
                name: "ReservationDishes");

            migrationBuilder.DropTable(
                name: "ReservationRequestStatuses");

            migrationBuilder.DropTable(
                name: "StaticFiles");

            migrationBuilder.DropTable(
                name: "StoreCreditHistories");

            migrationBuilder.DropTable(
                name: "Tables");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Combos");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "StoreCredits");

            migrationBuilder.DropTable(
                name: "TableRatings");

            migrationBuilder.DropTable(
                name: "TableSizes");

            migrationBuilder.DropTable(
                name: "ComboCategories");

            migrationBuilder.DropTable(
                name: "Dishes");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CustomerInfos");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "LoyalPointsHistories");
        }
    }
}

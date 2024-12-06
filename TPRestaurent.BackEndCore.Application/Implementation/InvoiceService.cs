using Newtonsoft.Json;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;
using static Humanizer.In;
using static System.Collections.Specialized.BitVector32;
using Twilio.TwiML.Messaging;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using System.Diagnostics.Contracts;
using Newtonsoft.Json;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class InvoiceService : GenericBackendService, IInvoiceService
    {
        private IGenericRepository<Invoice> _repository;
        private IGenericRepository<Transaction> _transactionRepository;
        private IUnitOfWork _unitOfWork;
        private IFileService _fileService;
        private IFirebaseService _firebaseService;
        public InvoiceService(IGenericRepository<Invoice> repository, IGenericRepository<Transaction> transactionRepository, IUnitOfWork unitOfWork, IFileService fileService, IFirebaseService firebaseService, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _repository = repository;
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _firebaseService = firebaseService;
        }

        public async Task<AppActionResult> GenerateGeneralInvoice(InvoiceFilterRequest dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var invoiceDb = await _repository.GetAllDataByExpression(i => i.Date >= dto.StartTime.Date && i.Date <= dto.EndTime.Date
                                                                              && (!dto.OrderType.HasValue || i.OrderTypeId == dto.OrderType.Value)
                                                                              , dto.pageNumber, dto.pageSize, null, false, null);
                var invoiceHtml = GenerateInvoiceSummaryHtml(invoiceDb.Items, dto.StartTime, dto.EndTime);
                var random = new Random();
                var invoiceContent = _fileService.ConvertHtmlToPdf(invoiceHtml, $"{dto.StartTime.ToString("dd-mm-yyyy")}-{dto.EndTime.ToString("dd-mm-yyyy")}.pdf");
                var upload = await _firebaseService.UploadFileToFirebase(invoiceContent, $"{SD.FirebasePathName.INVOICE_PREFIX}{dto.StartTime.ToString("dd-mm-yyyy")}-{dto.EndTime.ToString("dd-mm-yyyy")}", false);
                result.Result = Convert.ToString(upload.Result);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        [Hangfire.Queue("generate-invoice")]
        public async Task GenerateInvoice()
        {
            try
            {
                List<Invoice> data = new List<Invoice> { };
                var orderRepository = Resolve<IGenericRepository<Order>>();
                var orderService = Resolve<IOrderService>();
                var utility = Resolve<Utility>();
                var currentDate = utility.GetCurrentDateInTimeZone();
                var orderHasCreatedInvoice = await _repository.GetAllDataByExpression(o => o.Date.AddDays(1) == currentDate, 0, 0, null, false, null);
                var orderIds = orderHasCreatedInvoice.Items.Select(o => o.OrderId).ToList();
                var orderDb = await orderRepository.GetAllDataByExpression(o => (o.OrderDate.Date.AddDays(1) == currentDate || o.MealTime.Value.Date.AddDays(1) == currentDate)
                                                                                && o.StatusId == Domain.Enums.OrderStatus.Completed
                                                                                && !orderIds.Contains(o.OrderId), 0, 0, null, false, null);
                if (orderDb.Items.Count > 0)
                {
                    foreach (var item in orderDb.Items)
                    {
                        var orderDetail = await orderService.GetAllOrderDetail(item.OrderId);
                        var transactionInfo = await _transactionRepository.GetByExpression(o => o.OrderId == item.OrderId && o.TransactionTypeId == Domain.Enums.TransactionType.Order && o.TransationStatusId == Domain.Enums.TransationStatus.SUCCESSFUL, o => o.PaymentMethod);
                        if (transactionInfo == null)
                        {
                            continue;
                        }
                        if (orderDetail.IsSuccess && orderDetail.Result != null)
                        {
                            var invoice = new Invoice
                            {
                                InvoiceId = Guid.NewGuid(),
                                OrderId = item.OrderId,
                                OrderTypeId = item.OrderTypeId,
                                TotalAmount = item.TotalAmount,
                                Date = currentDate.AddDays(-1).Date,
                                OrderDetailJson = JsonConvert.SerializeObject(orderDetail.Result as OrderWithDetailReponse)
                            };

                            var invoiceHtmlScript = GetInvoiceHtmlScript(orderDetail.Result as OrderWithDetailReponse, transactionInfo);
                            Console.WriteLine(invoiceHtmlScript);
                            var invoiceContent = _fileService.ConvertHtmlToPdf(invoiceHtmlScript, $"{invoice.InvoiceId}.pdf");
                            var upload = await _firebaseService.UploadFileToFirebase(invoiceContent, $"{SD.FirebasePathName.INVOICE_PREFIX}{invoice.InvoiceId}", false);
                            invoice.pdfLink = Convert.ToString(upload.Result);
                            data.Add(invoice);
                        }
                    }
                    await _repository.InsertRange(data);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<AppActionResult> GetAllInvoice(InvoiceFilterRequest dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var invoiceDb = await _repository.GetAllDataByExpression(i => i.Date >= dto.StartTime.Date && i.Date <= dto.EndTime.Date
                                                                              && (!dto.OrderType.HasValue || i.OrderTypeId == dto.OrderType.Value)
                                                                              , dto.pageNumber, dto.pageSize, null, false, null);
                result.Result = invoiceDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetInvoiceById(Guid id)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var invoiceDb = await _repository.GetById(id);
                result.Result = invoiceDb;
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        private string GetInvoiceHtmlScript(OrderWithDetailReponse response, Transaction transaction)
        {
            int i = 1;
            string customerDetails = response.Order.Account != null
                ? @"
        <table class='info-table'>
      <tr><th>Tên khách hàng:</th><td>" + response.Order.Account.FirstName + " " + response.Order.Account.LastName + @"</td></tr>
      <tr><th>Số điện thoại:</th><td> (+84)" + response.Order.Account.PhoneNumber + @"</td></tr>
      <tr><th>Email:</th><td>" + response.Order.Account.Email + @"</td></tr>
      <tr><th>Địa chỉ:</th><td>" + response.Order.CustomerInfoAddress?.CustomerInfoAddressName + @"</td></tr>
        </table>"
                : "<p class='no-info'>Không có thông tin khách hàng trong hệ thống</p>";

            return @"
<!DOCTYPE html>
<html>
    <head>
    <style>
      body {
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        background-color: #f9f9f9;
        margin: 0;
        padding: 20px;
      }
        .page {
           contain: size;
        }
      .container {
        background-color: #fff;
        max-width: 900px;
        margin: 20px auto;
        border-radius: 8px;
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        overflow: hidden;
      }
      .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        background-color: #A1011A;
        padding: 20px;
        color: white;
      }
      .logo {
        max-width: 100px;
      }
      .restaurant-info h1 {
        margin: 0;
        font-size: 26px;
        font-weight: bold;
      }
      .restaurant-info p {
        margin: 4px 0;
        font-size: 14px;
      }
      .mainBody {
        padding: 20px;
      }
      .section-title {
        font-size: 20px;
        margin-bottom: 10px;
        color: #333;
        border-bottom: 2px solid #A1011A;
        padding-bottom: 5px;
      }
      .info-section {
        display: flex;
        justify-content: space-between;
        gap: 20px;
      }
      .info-table {
        width: 48%;
        border-collapse: collapse;
        margin-bottom: 15px;
      }
      .info-table th, .info-table td {
        padding: 8px;
        border: 1px solid #ddd;
        text-align: left;
      }
      .info-table th {
        background-color: #f5f5f5;
        font-weight: 600;
      }
      .info-table td {
        background-color: #fcfcfc;
      }
      .dish-table {
        width: 100%;
        border-collapse: collapse;
        margin-bottom: 15px;
      }
      .dish-table th, .dish-table td {
        padding: 10px;
        border: 1px solid #ddd;
        text-align: center;
      }
      .dish-table th {
        background-color: #A1011A;
        color: #fff;
        font-weight: 600;
      }
      .info-table td {
        background-color: #fcfcfc;
      }
      .dish-table {
        width: 100%;
        border-collapse: collapse;
        margin-bottom: 15px;
      }
      .dish-table th, .dish-table td {
        padding: 10px;
        border: 1px solid #ddd;
        text-align: center;
      }
      .dish-table th {
        background-color: #A1011A;
        color: #fff;
        font-weight: 600;
      }
      .dish-table tr:nth-child(even) {
        background-color: #f9f9f9;
      }
      .dish-table tr:hover {
        background-color: #f1f1f1;
      }
      .dish-table td {
        background-color: #fcfcfc;
      }
      .total-info {
        text-align: right;
        font-size: 18px;
        margin-top: 20px;
      }
      .total-info p {
        margin: 5px 0;
      }
      .order-summary {
        background-color: #A1011A;
        padding: 15px;
        text-align: center;
        color: #fff;
        font-weight: 600;
        border-radius: 0 0 8px 8px;
      }
      .no-info {
        color: #b30000;
        font-style: italic;
      }
      .info-table {
          width: 100%;
        }
      @media (max-width: 768px) {
        .header, .mainBody {
          padding: 15px;
        }
        .logo {
          max-width: 80px;
        }
        .section-title {
          font-size: 18px;
        }
        .info-table th, .info-table td,
        .dish-table th, .dish-table td {
          padding: 6px;
        }
        .info-section {
          flex-direction: column;
        }
        .info-table {
          width: 100%;
        }
        .info-section {
          flex-direction: column;
        }
        .info-table {
          width: 100%;
        }
      }
    </style>
  </head>
  <body>
    <div class=''>
      <div class='header'>
        <img src='https://thienphurestaurant.vercel.app/icon.png' alt='Restaurant Logo' class='logo'>
        <div class='restaurant-info'>
          <h1>Nhà hàng Thiên Phú</h1>
          <p>Số điện thoại: 091 978 24 44</p>
          <p>Địa chỉ: 78 Đường Lý Tự Trọng, Phường 2, Đà Lạt, Vietnam</p>
        </div>
      </div>
      <div class='mainBody'>
        <div class='info-section'>
          <div>
            <h2 class='section-title'>Thông tin đơn hàng</h2>
        <table class='info-table'>
              <tr><th>Mã đơn:</th><td>" + response.Order.OrderId.ToString().Substring(0, 6) + @"</td></tr>
              <tr><th>Ngày đặt/dùng bữa:</th><td>" + response.Order.OrderDate.ToString("yyyy-MM-dd") + @"</td></tr>
              <tr><th>Phương thức thanh toán:</th><td>" + transaction.PaymentMethod.VietnameseName + @"</td></tr>
              <tr><th>Thời gian thanh toán:</th><td>" + transaction.PaidDate.Value.ToString("yyyy-MM-dd HH:mm") + @"</td></tr>
              <tr><th>Tổng đơn:</th><td>" + response.Order.TotalAmount.ToString("#,0.## VND", System.Globalization.CultureInfo.InvariantCulture) + @"</td></tr>
              <tr><th>Loại đơn hàng:</th><td>" + response.Order.OrderType.VietnameseName + @"</td></tr>
              <tr><th>Ghi chú:</th><td>" + (string.IsNullOrEmpty(response.Order.Note) ? "No notes" : response.Order.Note) + @"</td></tr>
        </table>
          </div>
          <div>
            <h2 class='section-title'>Thông tin khách hàng</h2>
        " + customerDetails + @"
          </div>
        </div>

        <h2 class='section-title'>Danh sách món</h2>
        <table class='dish-table'>
          <thead>
            <tr>
              <th>No</th>
              <th>Tên món</th>
              <th>Kích thước</th>
              <th>Số lượng</th>
              <th>Giá (VND)</th>
              <th>Giảm giá (%)</th>
              <th>Thành tiền</th>
            </tr>
          </thead>
          <tbody>
            " + string.Join("", response.OrderDishes.Select(o =>
            {
                if (o.ComboDish != null)
                {
                    var comboDetail = string.Join("<br>", o.ComboDish.DishCombos.Select(d => "<i>- " + d.DishSizeDetail.Dish.Name + " x " + d.Quantity + "</i>"));
                    return @"
                    <tr>
                      <td>" + (i++) + @"</td>
                      <td>" + o.ComboDish.Combo.Name + "<br>" + comboDetail + @"</td>
                      <td>N/A</td>
                      <td>" + o.Quantity + @"</td>
                      <td>" + (o.ComboDish.Combo.Price).ToString("#,0 VND") + @"</td>
                      <td>" + o.ComboDish.Combo.Discount + @"%</td>
                      <td>" + (Math.Ceiling(o.ComboDish.Combo.Price * o.Quantity * (1 - o.ComboDish.Combo.Discount / 100) / 1000) * 1000).ToString("#,0.## VND", System.Globalization.CultureInfo.InvariantCulture) + @"</td>
                    </tr>";
                }
                return @"
            <tr>
              <td>" + (i++) + @"</td>
              <td>" + o.DishSizeDetail.Dish.Name + @"</td>
              <td>" + o.DishSizeDetail.DishSize.VietnameseName + @"</td>
              <td>" + o.Quantity + @"</td>
                  <td>" + (o.DishSizeDetail.Price).ToString("#,0 VND") + @"</td>
                  <td>" + o.DishSizeDetail.Discount + @"%</td>
                  <td>" + (Math.Ceiling(o.DishSizeDetail.Price * o.Quantity * (1 - o.DishSizeDetail.Discount / 100) / 1000) * 1000).ToString("#,0.## VND", System.Globalization.CultureInfo.InvariantCulture) + @"</td>
                </tr>";
            })) + @"
          </tbody>
        </table>

        <div class='total-info'>
          <p><strong>Tổng (đã bao gồm thuế và phí):</strong> " + response.Order.TotalAmount.ToString("#,0.## VND", System.Globalization.CultureInfo.InvariantCulture) + @"</p>
          <p>Số tiền này đã bao gồm tất cả các loại thuế và phí bổ sung.</p>
        </div>
      </div>
      <div class='order-summary'>
        <p>Cảm ơn bạn đã chọn Nhà hàng Thiên Phú! Mọi thắc mắc xin vui lòng liên hệ bộ phận hỗ trợ của chúng tôi.</p>
      </div>
    </div>
  </body>
</html>";
        }
        //        public string GenerateInvoiceSummaryHtml(List<Invoice> invoices, DateTime startDate, DateTime endDate)
        //        {
        //            var totalRevenue = invoices.Sum(i => i.TotalAmount);
        //            var orderCount = invoices.Count;
        //            var groupedByDate = invoices
        //                .GroupBy(i => i.Date.Date)
        //                .Select(g => new { Date = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
        //                .OrderBy(g => g.Date)
        //                .ToList();

        //            // Calculate maximum revenue for Y-axis scaling
        //            var maxRevenue = groupedByDate.Any() ? groupedByDate.Max(g => g.Revenue) : 0;
        //            var chartHeight = 300; // Height of the chart area in pixels

        //            // Generate bars for the chart
        //            var bars = string.Join("\n", groupedByDate.Select((g, index) =>
        //            {
        //                double barHeight = (g.Revenue / maxRevenue) * chartHeight;
        //                double xPosition = 60 + (index * 40); // 40 pixels between each bar
        //                return $"<rect x='{xPosition}' y='{350 - barHeight}' width='30' height='{barHeight}' fill='#A1011A'></rect>" +
        //                       $"<text x='{xPosition + 15}' y='370' font-size='12' text-anchor='middle'>{g.Date:MM-dd}</text>" +
        //                       $"<text x='{xPosition + 15}' y='{350 - barHeight - 5}' font-size='12' text-anchor='middle'>{g.Revenue:#,0} VND</text>";
        //            }));

        //            return $@"
        //<!DOCTYPE html>
        //<html>
        //  <head>
        //    <style>
        //      body {{
        //        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        //        background-color: #f9f9f9;
        //        margin: 0;
        //        padding: 20px;
        //      }}
        //      .container {{
        //        background-color: #fff;
        //        max-width: 900px;
        //        margin: 20px auto;
        //        border-radius: 8px;
        //        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        //        overflow: hidden;
        //      }}
        //      .header {{
        //        display: flex;
        //        justify-content: space-between;
        //        align-items: center;
        //        background-color: #A1011A;
        //        padding: 20px;
        //        color: white;
        //      }}
        //      .logo {{
        //        max-width: 100px;
        //      }}
        //      .restaurant-info {{
        //        text-align: right;
        //      }}
        //      .restaurant-info h1 {{
        //        margin: 0;
        //        font-size: 26px;
        //        font-weight: bold;
        //      }}
        //      .restaurant-info p {{
        //        margin: 4px 0;
        //        font-size: 14px;
        //        color: #fff;
        //      }}
        //      .mainBody {{
        //        padding: 20px;
        //      }}
        //      .section-title {{
        //        font-size: 20px;
        //        margin-bottom: 10px;
        //        color: #333;
        //        border-bottom: 2px solid #A1011A;
        //        padding-bottom: 5px;
        //      }}
        //      .summary-table {{
        //        width: 100%;
        //        border-collapse: collapse;
        //        margin-bottom: 20px;
        //      }}
        //      .summary-table th, .summary-table td {{
        //        padding: 10px;
        //        border: 1px solid #ddd;
        //        text-align: left;
        //      }}
        //      .summary-table th {{
        //        background-color: #f5f5f5;
        //        font-weight: 600;
        //      }}
        //      .chart-container {{
        //        margin-top: 20px;
        //        text-align: center;
        //      }}
        //      .chart-svg {{
        //        width: 100%;
        //        max-width: 800px;
        //        height: 400px;
        //      }}
        //      .chart-axis {{
        //        stroke: #333;
        //        stroke-width: 1;
        //      }}
        //      .chart-label {{
        //        fill: #333;
        //      }}
        //    </style>
        //  </head>
        //  <body>
        //    <div class='container'>
        //      <div class='header'>
        //        <img src='https://thienphurestaurant.vercel.app/icon.png' alt='Restaurant Logo' class='logo'>
        //        <div class='restaurant-info'>
        //          <h1>Nhà hàng Thiên Phú</h1>
        //          <p>Số điện thoại: 091 978 24 44</p>
        //          <p>Địa chỉ: 78 Đường Lý Tự Trọng, Phường 2, Đà Lạt, Vietnam</p>
        //        </div>
        //      </div>
        //      <div class='mainBody'>
        //        <h2 class='section-title'>Order Summary</h2>
        //        <table class='summary-table'>
        //          <tr>
        //            <th>Order Type</th>
        //            <td>{(invoices.FirstOrDefault()?.OrderTypeId.ToString() ?? "N/A")}</td>
        //          </tr>
        //          <tr>
        //            <th>Total Revenue</th>
        //            <td>{totalRevenue.ToString("#,0.## VND", System.Globalization.CultureInfo.InvariantCulture)}</td>
        //          </tr>
        //          <tr>
        //            <th>Start Date</th>
        //            <td>{startDate:yyyy-MM-dd}</td>
        //          </tr>
        //          <tr>
        //            <th>End Date</th>
        //            <td>{endDate:yyyy-MM-dd}</td>
        //          </tr>
        //          <tr>
        //            <th>Quantity of Orders</th>
        //            <td>{orderCount}</td>
        //          </tr>
        //        </table>
        //        <div class='chart-container'>
        //          <svg class='chart-svg' viewBox='0 0 800 400'>
        //            <!-- Y-axis -->
        //            <line x1='50' y1='50' x2='50' y2='350' class='chart-axis'></line>
        //            <line x1='50' y1='350' x2='750' y2='350' class='chart-axis'></line>
        //            <text x='20' y='350' font-size='12' class='chart-label'>0</text>
        //            <!-- Bars -->
        //            {bars}
        //          </svg>
        //        </div>
        //      </div>
        //      <div class='order-summary'>
        //        <p>Cảm ơn bạn đã chọn Nhà hàng Thiên Phú! Mọi thắc mắc xin vui lòng liên hệ bộ phận hỗ trợ của chúng tôi.</p>
        //      </div>
        //    </div>
        //  </body>
        //</html>";
        //        }

        public string GenerateInvoiceSummaryHtml(List<Invoice> invoices, DateTime startDate, DateTime endDate)
        {
            var totalRevenue = invoices.Sum(i => i.TotalAmount);
            var orderCount = invoices.Count;
            var groupedByDate = invoices
                .GroupBy(i => i.Date.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
                .OrderBy(g => g.Date)
                .ToList();

            var orderTypeVietnameseName = "N/A";
            if (invoices.All(i => i.OrderTypeId == Domain.Enums.OrderType.Delivery))
                orderTypeVietnameseName = "Giao hàng tận nơi";
            else if (invoices.All(i => i.OrderTypeId == Domain.Enums.OrderType.MealWithoutReservation))
                orderTypeVietnameseName = "Dùng tại quán";
            else if (invoices.All(i => i.OrderTypeId == Domain.Enums.OrderType.Reservation))
                orderTypeVietnameseName = "Dùng tại quán có đặt bàn trước";
            else
                orderTypeVietnameseName = "Giao hàng tận nơi, Dùng tại quán, Dùng tại quán có đặt bàn trước";

            var maxRevenue = groupedByDate.Any() ? groupedByDate.Max(g => g.Revenue) : 0;
            var chartHeight = 300;

            int totalBars = groupedByDate.Count;
            int svgWidth = Math.Max(800, totalBars * 60); // Base width 800px, 60px per bar
            int barWidth = Math.Max(20, 600 / Math.Max(totalBars, 1)); // Ensure minimum 20px width

            // Generate bars with dynamic positioning and text size
            var bars = string.Join("\n", groupedByDate.Select((g, index) =>
            {
                double barHeight = maxRevenue > 0 ? (g.Revenue / maxRevenue) * chartHeight : 0;
                double xPosition = 60 + (index * (barWidth + 10));
                string fontSize = totalBars > 15 ? "10" : "12";

                return $"<rect x='{xPosition}' y='{350 - barHeight}' width='{barWidth}' height='{barHeight}' fill='#A1011A'></rect>" +
                       $"<text x='{xPosition + barWidth / 2}' y='370' font-size='{fontSize}' text-anchor='middle'>{g.Date:MM-dd}</text>" +
                       $"<text x='{xPosition + barWidth / 2}' y='{350 - barHeight - 5}' font-size='{fontSize}' text-anchor='middle'>{g.Revenue:#,0} VND</text>";
            }));

            return $@"
<!DOCTYPE html>
<html>
<head>
  <style>
    body {{
      font-family: Arial, sans-serif;
      background-color: #f9f9f9;
      margin: 0;
      padding: 20px;
    }}
        .page {{
           contain: size;
        }}
    .container {{
      background-color: #fff;
      width: 800px; /* Fixed width for PDF rendering */
      margin: 0 auto;
      padding: 20px;
      border: 1px solid #ddd;
    }}
    .header {{
      display: flex;
      align-items: center;
      background-color: #A1011A;
      padding: 20px;
      color: white;
    }}
    .logo {{
        width: 150px;
        height: auto;
        margin-right: 20px;
    }}
    .restaurant-info {{
        text-align: right;
        flex-grow: 1;
    }}
    .restaurant-info h1 {{
      margin: 0;
      font-size: 24px;
      font-weight: bold;
    }}
    .restaurant-info p {{
      margin: 4px 0;
      font-size: 12px;
    }}
    .mainBody {{
      padding: 20px;
    }}
    .section-title {{
      font-size: 18px;
      margin-bottom: 10px;
      color: #333;
      border-bottom: 2px solid #A1011A;
      padding-bottom: 5px;
    }}
    .summary-table {{
      width: 100%;
      border-collapse: collapse;
      margin-bottom: 20px;
    }}
    .summary-table th, .summary-table td {{
      padding: 8px;
      border: 1px solid #ddd;
      text-align: left;
      font-size: 12px;
    }}
    .summary-table th {{
      background-color: #f5f5f5;
      font-weight: bold;
    }}
    .chart-container {{
      margin-top: 20px;
      text-align: center;
    }}
    .chart-svg {{
      width: 100%;
      max-width: {svgWidth}px;
      height: 400px;
    }}
    .chart-axis {{
      stroke: #333;
      stroke-width: 1;
    }}
    .chart-label {{
      fill: #333;
    }}
  </style>
</head>
<body>
  <div class=''>
    <div class='header'>
      <img src='https://thienphurestaurant.vercel.app/icon.png' alt='Restaurant Logo' class='logo'>
      <div class='restaurant-info'>
        <h1>Nhà hàng Thiên Phú</h1>
        <p>Số điện thoại: 091 978 24 44</p>
        <p>Địa chỉ: 78 Đường Lý Tự Trọng, Phường 2, Đà Lạt, Vietnam</p>
      </div>
    </div>
    <div class='mainBody'>
      <h2 class='section-title'>Tổng quan đơn hàng</h2>
      <table class='summary-table'>
        <tr>
          <th>Loại đơn</th>
          <td>{orderTypeVietnameseName}</td>
        </tr>
        <tr>
          <th>Tổng doanh thu</th>
          <td>{totalRevenue:#,0 VND}</td>
        </tr>
        <tr>
          <th>Ngày bắt đầu</th>
          <td>{startDate:yyyy-MM-dd}</td>
        </tr>
        <tr>
          <th>Ngày kết thúc</th>
          <td>{endDate:yyyy-MM-dd}</td>
        </tr>
        <tr>
          <th>Số lượng đơn</th>
          <td>{orderCount}</td>
        </tr>
      </table>
      <div class='chart-container'>
        <svg class='chart-svg' viewBox='0 0 {svgWidth} 400'>
          <!-- Y-axis -->
          <line x1='50' y1='50' x2='50' y2='350' class='chart-axis'></line>
          <line x1='50' y1='350' x2='{svgWidth - 50}' y2='350' class='chart-axis'></line>
          <text x='20' y='350' font-size='12' class='chart-label'>0</text>
          <!-- Bars -->
          {bars}
        </svg>
      </div>
    </div>
    <div class='order-summary'>
      <p>Cảm ơn bạn đã chọn Nhà hàng Thiên Phú! Mọi thắc mắc xin vui lòng liên hệ bộ phận hỗ trợ của chúng tôi.</p>
    </div>
  </div>
</body>
</html>";
        }
    }
}
using Castle.Core.Resource;
using MailKit.Search;
using MathNet.Numerics.Distributions;
using Microsoft.VisualBasic;
using NPOI.HPSF;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;
using static Humanizer.In;
using static System.Collections.Specialized.BitVector32;
using Twilio.TwiML.Messaging;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using System.Diagnostics.Contracts;
using Newtonsoft.Json;

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
                var orderDb = await orderRepository.GetAllDataByExpression(o => (o.OrderDate.Date.AddDays(1) == currentDate || o.MealTime.Value.Date.AddDays(1) == currentDate) && o.StatusId == Domain.Enums.OrderStatus.Completed && !orderIds.Contains(o.OrderId), 0, 0, null, false, null);
                if (orderDb.Items.Count > 0)
                {
                    foreach (var item in orderDb.Items)
                    {
                        var orderDetail = await orderService.GetAllOrderDetail(item.OrderId);
                        var transactionInfo = await _transactionRepository.GetByExpression(o => o.OrderId == item.OrderId && o.TransactionTypeId == Domain.Enums.TransactionType.Order && o.TransationStatusId == Domain.Enums.TransationStatus.SUCCESSFUL, o => o.PaymentMethod);
                        if (transactionInfo == null) {
                            continue;
                        }
                        if (orderDetail.IsSuccess && orderDetail.Result != null) {
                            var invoice = new Invoice
                            {
                                InvoiceId = Guid.NewGuid(),
                                OrderId = item.OrderId,
                                OrderTypeId = item.OrderTypeId,
                                TotalAmount = item.TotalAmount,
                                Date = currentDate.AddDays(-1).Date,
                                OrderDetailJson = JsonConvert.SerializeObject(orderDetail.Result as ReservationReponse)
                            };

                            var invoiceHtmlScript = GetInvoiceHtmlScript(orderDetail.Result as ReservationReponse, transactionInfo);
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

        private string GetInvoiceHtmlScript(ReservationReponse response, Transaction transaction)
        {
            int i = 1;
            string customerDetails = response.Order.Account != null
                ? @"
    <table class='info-table'>
      <tr><th>Tên khách hàng:</th><td>" + response.Order.Account.FirstName + " " + response.Order.Account.LastName + @"</td></tr>
      <tr><th>Số điện thoại:</th><td>" + response.Order.Account.PhoneNumber + @"</td></tr>
      <tr><th>Email:</th><td>" + response.Order.Account.Email + @"</td></tr>
      <tr><th>Địa chỉ:</th><td>" + response.Order.CustomerInfoAddress?.CustomerInfoAddressName + @"</td></tr>
    </table>"
                : "<p class='no-info'>Không có thông tin khách hàng trong hệ thống</p>";

            return @"
<html>
  <head>
    <style>
      body {
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        background-color: #f9f9f9;
        margin: 0;
        padding: 20px;
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
        background-color: #ff9800;
        padding: 20px;
        color: white;
      }
      .logo {
        max-width: 100px;
      }
      .restaurant-info {
        text-align: right;
      }
      .restaurant-info h1 {
        margin: 0;
        font-size: 26px;
        font-weight: bold;
      }
      .restaurant-info p {
        margin: 4px 0;
        font-size: 14px;
        color: #fff;
      }
      .mainBody {
        padding: 20px;
      }
      .section-title {
        font-size: 20px;
        margin-bottom: 10px;
        color: #333;
        border-bottom: 2px solid #ff9800;
        padding-bottom: 5px;
      }
      .info-table, .dish-table {
        width: 100%;
        border-collapse: collapse;
        margin-bottom: 15px;
      }
      .info-table th, .info-table td, .dish-table th, .dish-table td {
        padding: 12px;
        border: 1px solid #ddd;
        text-align: left;
      }
      .info-table th, .dish-table th {
        background-color: #f5f5f5;
        font-weight: 600;
      }
      .info-table td, .dish-table td {
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
        background-color: #ff9800;
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
        .info-table th, .info-table td, .dish-table th, .dish-table td {
          padding: 8px;
        }
      }
    </style>
  </head>
  <body>
    <div class='container'>
      <div class='header'>
        <img src='https://thienphurestaurant.vercel.app/icon.png' alt='Restaurant Logo' class='logo'>
        <div class='restaurant-info'>
          <h1>Nhà hàng Thiên Phú</h1>
          <p>Số điện thoại: 091 978 24 44</p>
          <p>Địa chỉ: 78 Đường Lý Tự Trọng, Phường 2, Đà Lạt, Vietnam</p>
        </div>
      </div>
      <div class='mainBody'>
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

        <h2 class='section-title'>Thông tin khách hàng(Nếu có)</h2>
        " + customerDetails + @"

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
                      <td>" + o.ComboDish.Combo.Price + @"</td>
                      <td>" + o.ComboDish.Combo.Discount + @"%</td>
                      <td>" + (Math.Ceiling((1 - o.ComboDish.Combo.Discount) * o.ComboDish.Combo.Price * o.Quantity / 1000) * 1000).ToString("#,0.## VND", System.Globalization.CultureInfo.InvariantCulture) + @"</td>

                    </tr>";
                }
                return @"
                <tr>
                  <td>" + (i++) + @"</td>
                  <td>" + o.DishSizeDetail.Dish.Name + @"</td>
                  <td>" + o.DishSizeDetail.DishSize.VietnameseName + @"</td>
                  <td>" + o.Quantity + @"</td>
                  <td>" + o.DishSizeDetail.Price + @"</td>
                  <td>" + o.DishSizeDetail.Discount + @"%</td>
                  <td>" + (Math.Ceiling((1 - o.DishSizeDetail.Discount) * o.DishSizeDetail.Price * o.Quantity / 1000) * 1000).ToString("#,0.## VND", System.Globalization.CultureInfo.InvariantCulture) + @"%</td>
                </tr>";
            })) + @"
          </tbody>
        </table>

        <div class='total-info'>
          <p><strong>Tổng (đã bao gồm thuế và phí):</strong> " + response.Order.TotalAmount.ToString("#,0.## VND", System.Globalization.CultureInfo.InvariantCulture) + @" </p>
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

    }
}

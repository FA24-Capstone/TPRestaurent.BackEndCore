using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.Utils;

public class TemplateMappingHelper
{
    public enum ContentEmailType
    {
        VERIFICATION_CODE,
        FORGOTPASSWORD,
        CONTRACT_CODE,
        TOURGUIDE_ACCOUNT_CREATION,
        INSUFFICIENT_COUPON_QUANTITY
    }

    public static string GetTemplateOTPEmail(ContentEmailType type, string body, string name)
    {
        var content = "";
        switch (type)
        {
            case ContentEmailType.VERIFICATION_CODE:
                content = @"
  var content = @""
<!DOCTYPE html>
<html lang=""""vi"""">

<head>
  <meta charset=""""UTF-8"""">
  <meta name=""""viewport"""" content=""""width=device-width, initial-scale=1.0"""">
  <title>Thông báo hoàn tiền</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      margin: 0;
      padding: 0;
      background-color: #f9f9f9;
      color: #333;
    }

    .container {
      max-width: 600px;
      margin: 20px auto;
      background: #ffffff;
      padding: 20px;
      border-radius: 8px;
      border: 1px solid #ddd;
      box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }

    .header {
      background-color: #B71C1C;
      color: #ffffff;
      text-align: center;
      padding: 15px 20px;
      border-radius: 8px 8px 0 0;
    }

    .header h1 {
      display: inline-block;
      vertical-align: middle;
      margin: 0;
      font-size: 20px;
    }

    .content {
      margin: 20px 0;
      line-height: 1.6;
    }

    .highlight {
      color: #B71C1C;
      font-weight: bold;
    }

    .table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }

    .table th,
    .table td {
      border: 1px solid #ddd;
      padding: 10px;
      text-align: left;
    }

    .table th {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .table td {
      background-color: #ffffff;
    }

    .btn {
      display: inline-block;
      margin-top: 20px;
      padding: 10px 20px;
      background-color: #FFD54F;
      color: #B71C1C;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
      text-align: center;
    }

    .btn:hover {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .footer {
      text-align: center;
      margin-top: 20px;
      font-size: 14px;
      color: #555;
    }
  </style>
</head>

<body>
  <div class="""""""">
    <div class=""""header"""">
      <img
        src=""""https://firebasestorage.googleapis.com/v0/b/hcqs-project.appspot.com/o/dish%2Ff25019dc-3a64-4677-87cb-63b0f3dbcef7.jpg.png?alt=media&token=c784cf86-52e6-4314-bd8a-28d898feb7f5""""
        alt=""""Logo Nhà hàng Thiên Phú"""" style=""""height: 100px; margin-right: 10px; vertical-align: middle;"""">
      <h1 style=""""display: inline-block; vertical-align: middle;"""">Nhà hàng Thiên Phú</h1>
      <p style=""""text-transform: uppercase; font-weight: 600; color: #ffffff;font-size: 18px;"""">Thông báo hoàn tiền đặt
        hàng</p>
    </div>
    <div class=""""content"""">
      <p>Kính gửi <span class=""""highlight""""> " + name + @"</span>,</p>
        <p class=""""emailBody"""">
          Bên dưới là mã xác nhận của bạn:
          <b><i> " + body + @"</i></b>
        </p>
   
    </div>
    <div class=""""footer"""">
      <p>Trân trọng,<br>Đội ngũ Nhà hàng Thiên Phú</p>
    </div>
  </div>
</body>

</html>
"";
";

                break;

        

            case ContentEmailType.FORGOTPASSWORD:
                content = @"
<!DOCTYPE html>
<html lang=""vi"">

<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Thông báo hoàn tiền</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      margin: 0;
      padding: 0;
      background-color: #f9f9f9;
      color: #333;
    }

    .container {
      max-width: 600px;
      margin: 20px auto;
      background: #ffffff;
      padding: 20px;
      border-radius: 8px;
      border: 1px solid #ddd;
      box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }

    .header {
      background-color: #B71C1C;
      color: #ffffff;
      text-align: center;
      padding: 15px 20px;
      border-radius: 8px 8px 0 0;
    }

    .header h1 {
      display: inline-block;
      vertical-align: middle;
      margin: 0;
      font-size: 20px;
    }

    .content {
      margin: 20px 0;
      line-height: 1.6;
    }

    .highlight {
      color: #B71C1C;
      font-weight: bold;
    }

    .table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }

    .table th,
    .table td {
      border: 1px solid #ddd;
      padding: 10px;
      text-align: left;
    }

    .table th {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .table td {
      background-color: #ffffff;
    }

    .btn {
      display: inline-block;
      margin-top: 20px;
      padding: 10px 20px;
      background-color: #FFD54F;
      color: #B71C1C;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
      text-align: center;
    }

    .btn:hover {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .footer {
      text-align: center;
      margin-top: 20px;
      font-size: 14px;
      color: #555;
    }
  </style>
</head>

<body>
  <div class="""">
    <div class=""header"">
      <img
        src=""https://firebasestorage.googleapis.com/v0/b/hcqs-project.appspot.com/o/dish%2Ff25019dc-3a64-4677-87cb-63b0f3dbcef7.jpg.png?alt=media&token=c784cf86-52e6-4314-bd8a-28d898feb7f5""
        alt=""Logo Nhà hàng Thiên Phú"" style=""height: 100px; margin-right: 10px; vertical-align: middle;"">
      <h1 style=""display: inline-block; vertical-align: middle;"">Nhà hàng Thiên Phú</h1>
      <p style=""text-transform: uppercase; font-weight: 600; color: #ffffff;font-size: 18px;"">XÁC THỰC OTP</p>
    </div>
    <div class=""content"">
      <p>Kính gửi <span class=""highlight"">"+name+ @"</span>,</p>
      <p>Mã xác thực OTP của bạn là: <strong>"+body+@"</strong></p>
    <p>Mã xác thực có hiệu lực trong vòng 5 phút</p>
   
    </div>
    <div class=""footer"">
      <p>Trân trọng,<br>Đội ngũ Nhà hàng Thiên Phú</p>
    </div>
  </div>
</body>

</html>



";
                break;

         

            case ContentEmailType.INSUFFICIENT_COUPON_QUANTITY:
                content = @"
<!DOCTYPE html>
<html lang=""vi"">

<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Thông báo hoàn tiền</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      margin: 0;
      padding: 0;
      background-color: #f9f9f9;
      color: #333;
    }

    .container {
      max-width: 600px;
      margin: 20px auto;
      background: #ffffff;
      padding: 20px;
      border-radius: 8px;
      border: 1px solid #ddd;
      box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }

    .header {
      background-color: #B71C1C;
      color: #ffffff;
      text-align: center;
      padding: 15px 20px;
      border-radius: 8px 8px 0 0;
    }

    .header h1 {
      display: inline-block;
      vertical-align: middle;
      margin: 0;
      font-size: 20px;
    }

    .content {
      margin: 20px 0;
      line-height: 1.6;
    }

    .highlight {
      color: #B71C1C;
      font-weight: bold;
    }

    .table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }

    .table th,
    .table td {
      border: 1px solid #ddd;
      padding: 10px;
      text-align: left;
    }

    .table th {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .table td {
      background-color: #ffffff;
    }

    .btn {
      display: inline-block;
      margin-top: 20px;
      padding: 10px 20px;
      background-color: #FFD54F;
      color: #B71C1C;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
      text-align: center;
    }

    .btn:hover {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .footer {
      text-align: center;
      margin-top: 20px;
      font-size: 14px;
      color: #555;
    }
  </style>
</head>

<body>
  <div class="""">
    <div class=""header"">
      <img
        src=""https://firebasestorage.googleapis.com/v0/b/hcqs-project.appspot.com/o/dish%2Ff25019dc-3a64-4677-87cb-63b0f3dbcef7.jpg.png?alt=media&token=c784cf86-52e6-4314-bd8a-28d898feb7f5""
        alt=""Logo Nhà hàng Thiên Phú"" style=""height: 100px; margin-right: 10px; vertical-align: middle;"">
      <h1 style=""display: inline-block; vertical-align: middle;"">Nhà hàng Thiên Phú</h1>
      <p style=""text-transform: uppercase; font-weight: 600; color: #ffffff;font-size: 18px;"">Thông báo sự cố mã giảm giá</p>
    </div>
    <div class=""content"">
     <p class="""" emailBody"""">
    Đây là thông báo về quy trình tự động gán mã giảm giá. Hiện tại, số lượng của các coupon sau không đủ và cần được
    cập nhật:
  </p>
  <p class="""" emailBody"""">
    Vui lòng đăng nhập vào hệ thống và cập nhật số lượng coupon để đảm bảo quy trình không bị gián đoạn.
  </p>
    </div>
    <div class=""footer"">
      <p>Trân trọng,<br>Hệ thống tự động</p>
    </div>
  </div>
</body>

</html>
";
                break;
        }
        return content;
    }

    public static string GetTemplateMailToCancelReservation(string username, Order order, TableDetail tableDetail)
    {
        var orderTime = order.OrderTypeId == Domain.Enums.OrderType.Reservation ? order.ReservationDate.Value : order.OrderDate;
      var content = @"
<!DOCTYPE html>
<html lang=""vi"">

<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Thông báo hoàn tiền</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      margin: 0;
      padding: 0;
      background-color: #f9f9f9;
      color: #333;
    }

    .container {
      max-width: 600px;
      margin: 20px auto;
      background: #ffffff;
      padding: 20px;
      border-radius: 8px;
      border: 1px solid #ddd;
      box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }

    .header {
      background-color: #B71C1C;
      color: #ffffff;
      text-align: center;
      padding: 15px 20px;
      border-radius: 8px 8px 0 0;
    }

    .header h1 {
      display: inline-block;
      vertical-align: middle;
      margin: 0;
      font-size: 20px;
    }

    .content {
      margin: 20px 0;
      line-height: 1.6;
    }

    .highlight {
      color: #B71C1C;
      font-weight: bold;
    }

    .table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }

    .table th,
    .table td {
      border: 1px solid #ddd;
      padding: 10px;
      text-align: left;
    }

    .table th {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .table td {
      background-color: #ffffff;
    }

    .btn {
      display: inline-block;
      margin-top: 20px;
      padding: 10px 20px;
      background-color: #FFD54F;
      color: #B71C1C;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
      text-align: center;
    }

    .btn:hover {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .footer {
      text-align: center;
      margin-top: 20px;
      font-size: 14px;
      color: #555;
    }
  </style>
</head>

<body>
  <div class="""">
    <div class=""header"">
      <img
        src=""https://firebasestorage.googleapis.com/v0/b/hcqs-project.appspot.com/o/dish%2Ff25019dc-3a64-4677-87cb-63b0f3dbcef7.jpg.png?alt=media&token=c784cf86-52e6-4314-bd8a-28d898feb7f5""
        alt=""Logo Nhà hàng Thiên Phú"" style=""height: 100px; margin-right: 10px; vertical-align: middle;"">
      <h1 style=""display: inline-block; vertical-align: middle;"">Nhà hàng Thiên Phú</h1>
      <p style=""text-transform: uppercase; font-weight: 600; color: #ffffff;font-size: 18px;"">Thông báo hoàn tiền đặt
        hàng</p>
    </div>
    <div class=""content"">
      <p>Kính gửi <span class=""highlight"">"+username +@"</span>,</p>
      <p>Chúng tôi rất tiếc vì đã gặp phải sự cố dẫn đến việc phải hoàn tiền cho đơn hàng của bạn. Dưới đây là thông tin
        chi tiết:</p>
      <h3>Thông tin hoàn tiền</h3>
      <ul>
        <li><strong>Số tiền hoàn: </strong><span class=""highlight"">[Số tiền] VNĐ</span></li>
        <li><strong>Thời gian dùng bữa: </strong><span class=""highlight"">[Ngày giờ hoàn]</span></li>
        <li><strong>Mã hóa đơn: </strong><span class=""highlight"">[Mã hóa đơn]</span></li>
      </ul>
     <p class=""""emailBody"""">
         Mã đơn: <b>"+order.OrderId.ToString().Substring(0, 5) +@"</b><br>
           Thời gian đặt: <b>"+orderTime+@"</b>
           Thời gian dùng bữa tại nhà hàng(Nếu có): <b>" +order.MealTime  +@"</b>
           Loại phòng: "+tableDetail.Table!.Room! +@"
           Loại bàn: "+tableDetail.Table!.TableSize!+@"
         </p>

    
    </div>
    <div class=""footer"">
      <p>Trân trọng,<br>Đội ngũ Nhà hàng Thiên Phú</p>
    </div>
  </div>
</body>

</html>
";
        return content;
    }

    public static string GetTemplateNotification(string username, Order order)
    {

        var content = @"

<!DOCTYPE html>
<html lang=""vi"">

<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Thông báo hoàn tiền</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      margin: 0;
      padding: 0;
      background-color: #f9f9f9;
      color: #333;
    }

    .container {
      max-width: 600px;
      margin: 20px auto;
      background: #ffffff;
      padding: 20px;
      border-radius: 8px;
      border: 1px solid #ddd;
      box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }

    .header {
      background-color: #B71C1C;
      color: #ffffff;
      text-align: center;
      padding: 15px 20px;
      border-radius: 8px 8px 0 0;
    }

    .header h1 {
      display: inline-block;
      vertical-align: middle;
      margin: 0;
      font-size: 20px;
    }

    .content {
      margin: 20px 0;
      line-height: 1.6;
    }

    .highlight {
      color: #B71C1C;
      font-weight: bold;
    }

    .table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }

    .table th,
    .table td {
      border: 1px solid #ddd;
      padding: 10px;
      text-align: left;
    }

    .table th {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .table td {
      background-color: #ffffff;
    }

    .btn {
      display: inline-block;
      margin-top: 20px;
      padding: 10px 20px;
      background-color: #FFD54F;
      color: #B71C1C;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
      text-align: center;
    }

    .btn:hover {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .footer {
      text-align: center;
      margin-top: 20px;
      font-size: 14px;
      color: #555;
    }
  </style>
</head>

<body>
  <div class="""">
    <div class=""header"">
      <img
        src=""https://firebasestorage.googleapis.com/v0/b/hcqs-project.appspot.com/o/dish%2Ff25019dc-3a64-4677-87cb-63b0f3dbcef7.jpg.png?alt=media&token=c784cf86-52e6-4314-bd8a-28d898feb7f5""
        alt=""Logo Nhà hàng Thiên Phú"" style=""height: 100px; margin-right: 10px; vertical-align: middle;"">
      <h1 style=""display: inline-block; vertical-align: middle;"">Nhà hàng Thiên Phú</h1>
      <p style=""text-transform: uppercase; font-weight: 600; color: #ffffff;font-size: 18px;"">Chúng ta có hẹn tại nhà hàng Thiên Phú</p>
    </div>
    <div class=""content"">
      <p>Kính gửi <span class=""highlight""> "+username+ @"</span>,</p>
      <p>Bạn ơi, đừng quên chúng ta có hẹn tại nhà hàng hôm nay nhé.</p>
                <p class=""emailBody"">
                 Mã đặt chỗ: <b>"+ order.OrderId+@"</b><br>
                 Thời gian đặt chỗ: <b>" +order.ReservationDate+@"</b>
              </p>
               <p class=""emailBody"">
                Chúng tôi rất mong được chào đón bạn và hy vọng bạn sẽ có một trải nghiệm tuyệt vời.
              </p>

    </div>
    <div class=""footer"">
      <p>Trân trọng,<br>Đội ngũ Nhà hàng Thiên Phú</p>
    </div>
  </div>
</body>

</html>



";
        return content;
    }

    public static string GetTemplateOrderConfirmation(string username, Order order)
    {
  var content = @"
<!DOCTYPE html>
<html lang=""vi"">

<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Thông báo hoàn tiền</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      margin: 0;
      padding: 0;
      background-color: #f9f9f9;
      color: #333;
    }

    .container {
      max-width: 600px;
      margin: 20px auto;
      background: #ffffff;
      padding: 20px;
      border-radius: 8px;
      border: 1px solid #ddd;
      box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }

    .header {
      background-color: #B71C1C;
      color: #ffffff;
      text-align: center;
      padding: 15px 20px;
      border-radius: 8px 8px 0 0;
    }

    .header h1 {
      display: inline-block;
      vertical-align: middle;
      margin: 0;
      font-size: 20px;
    }

    .content {
      margin: 20px 0;
      line-height: 1.6;
    }

    .highlight {
      color: #B71C1C;
      font-weight: bold;
    }

    .table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }

    .table th,
    .table td {
      border: 1px solid #ddd;
      padding: 10px;
      text-align: left;
    }

    .table th {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .table td {
      background-color: #ffffff;
    }

    .btn {
      display: inline-block;
      margin-top: 20px;
      padding: 10px 20px;
      background-color: #FFD54F;
      color: #B71C1C;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
      text-align: center;
    }

    .btn:hover {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .footer {
      text-align: center;
      margin-top: 20px;
      font-size: 14px;
      color: #555;
    }
  </style>
</head>

<body>
  <div class="""">
    <div class=""header"">
      <img
        src=""https://firebasestorage.googleapis.com/v0/b/hcqs-project.appspot.com/o/dish%2Ff25019dc-3a64-4677-87cb-63b0f3dbcef7.jpg.png?alt=media&token=c784cf86-52e6-4314-bd8a-28d898feb7f5""
        alt=""Logo Nhà hàng Thiên Phú"" style=""height: 100px; margin-right: 10px; vertical-align: middle;"">
      <h1 style=""display: inline-block; vertical-align: middle;"">Nhà hàng Thiên Phú</h1>
      <p style=""text-transform: uppercase; font-weight: 600; color: #ffffff;font-size: 18px;"">Thông báo đặt hàng thành công</p>
    </div>
    <div class=""content"">
      <p>Kính gửi <span class=""highlight"">"+ username+ @"</span>,</p>
      <p>
           Chúng tôi vui mừng xác nhận rằng đặt chỗ của bạn tại <b><i>Nhà hàng Thiên Phú</i></b> đã được tạo thành công!

</p>
      <h3>Dưới đây là thông tin đặt hàng của bạn</h3>
      <ul>
        <li><strong>Thời gian đặt hàng: </strong><span class=""highlight"">"+ order.OrderDate+@" </span></li>
        <li><strong>Đã thanh toán: </strong><span class=""highlight"">"+order.TotalAmount+@"</span></li>
        <li><strong>Mã hóa đơn: </strong><span class=""highlight"">"+order.OrderId+@"</span></li>
      </ul>
 
   <p><strong>Quy khách mua à</strong> </p>
   
    
    </div>
    <div class=""footer"">
      <p>Trân trọng,<br>Đội ngũ Nhà hàng Thiên Phú</p>
    </div>
  </div>
</body>

</html>
";
        return content;
    }

    public static string GetTemplateReservationConfirmation(string username, Order order)
    {
      var content = @"
<!DOCTYPE html>
<html lang=""vi"">

<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Thông báo hoàn tiền</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      margin: 0;
      padding: 0;
      background-color: #f9f9f9;
      color: #333;
    }

    .container {
      max-width: 600px;
      margin: 20px auto;
      background: #ffffff;
      padding: 20px;
      border-radius: 8px;
      border: 1px solid #ddd;
      box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }

    .header {
      background-color: #B71C1C;
      color: #ffffff;
      text-align: center;
      padding: 15px 20px;
      border-radius: 8px 8px 0 0;
    }

    .header h1 {
      display: inline-block;
      vertical-align: middle;
      margin: 0;
      font-size: 20px;
    }

    .content {
      margin: 20px 0;
      line-height: 1.6;
    }

    .highlight {
      color: #B71C1C;
      font-weight: bold;
    }

    .table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }

    .table th,
    .table td {
      border: 1px solid #ddd;
      padding: 10px;
      text-align: left;
    }

    .table th {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .table td {
      background-color: #ffffff;
    }

    .btn {
      display: inline-block;
      margin-top: 20px;
      padding: 10px 20px;
      background-color: #FFD54F;
      color: #B71C1C;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
      text-align: center;
    }

    .btn:hover {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .footer {
      text-align: center;
      margin-top: 20px;
      font-size: 14px;
      color: #555;
    }
  </style>
</head>

<body>
  <div class="""">
    <div class=""header"">
      <img
        src=""https://firebasestorage.googleapis.com/v0/b/hcqs-project.appspot.com/o/dish%2Ff25019dc-3a64-4677-87cb-63b0f3dbcef7.jpg.png?alt=media&token=c784cf86-52e6-4314-bd8a-28d898feb7f5""
        alt=""Logo Nhà hàng Thiên Phú"" style=""height: 100px; margin-right: 10px; vertical-align: middle;"">
      <h1 style=""display: inline-block; vertical-align: middle;"">Nhà hàng Thiên Phú</h1>
      <p style=""text-transform: uppercase; font-weight: 600; color: #ffffff;font-size: 18px;"">Thông báo đặt chỗ thành công</p>
    </div>
    <div class=""content"">
      <p>Kính gửi <span class=""highlight"">"+ username+ @"</span>,</p>
      <p>
           Chúng tôi vui mừng xác nhận rằng đặt chỗ của bạn tại <b><i>Nhà hàng Thiên Phú</i></b> đã được tạo thành công!

</p>
      <h3>Dưới đây là thông tin đặt chỗ của bạn</h3>
      <ul>
        <li><strong>Thời gian dùng bữa: </strong><span class=""highlight"">"+ order.MealTime+@" </span></li>
        <li><strong>Đã thanh toán cọc: </strong><span class=""highlight"">"+order.Deposit+@"</span></li>
        <li><strong>Mã hóa đơn: </strong><span class=""highlight"">"+order.OrderId+@"</span></li>
      </ul>
 
   <p><strong>Quy khách vui lòng đến đúng giờ, chúng tôi có thể giữ chỗ tối đa thời gian cho quý khách trong vòng 30 phút. Sau 30 phút tiền cọc quý khách sẽ không được hoàn trả</strong> </p>
   
    
    </div>
    <div class=""footer"">
      <p>Trân trọng,<br>Đội ngũ Nhà hàng Thiên Phú</p>
    </div>
  </div>
</body>

</html>
";
        return content;
    }

    public static string GetTemplateBirthdayCoupon(string username, CouponProgram couponProgram)
    {
       var content = @"
<!DOCTYPE html>
<html lang=""vi"">

<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Thông báo hoàn tiền</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      margin: 0;
      padding: 0;
      background-color: #f9f9f9;
      color: #333;
    }

    .container {
      max-width: 600px;
      margin: 20px auto;
      background: #ffffff;
      padding: 20px;
      border-radius: 8px;
      border: 1px solid #ddd;
      box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }

    .header {
      background-color: #B71C1C;
      color: #ffffff;
      text-align: center;
      padding: 15px 20px;
      border-radius: 8px 8px 0 0;
    }

    .header h1 {
      display: inline-block;
      vertical-align: middle;
      margin: 0;
      font-size: 20px;
    }

    .content {
      margin: 20px 0;
      line-height: 1.6;
    }

    .highlight {
      color: #B71C1C;
      font-weight: bold;
    }

    .table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }

    .table th,
    .table td {
      border: 1px solid #ddd;
      padding: 10px;
      text-align: left;
    }

    .table th {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .table td {
      background-color: #ffffff;
    }

    .btn {
      display: inline-block;
      margin-top: 20px;
      padding: 10px 20px;
      background-color: #FFD54F;
      color: #B71C1C;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
      text-align: center;
    }

    .btn:hover {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .footer {
      text-align: center;
      margin-top: 20px;
      font-size: 14px;
      color: #555;
    }
  </style>
</head>

<body>
  <div class="""">
    <div class=""header"">
      <img
        src=""https://firebasestorage.googleapis.com/v0/b/hcqs-project.appspot.com/o/dish%2Ff25019dc-3a64-4677-87cb-63b0f3dbcef7.jpg.png?alt=media&token=c784cf86-52e6-4314-bd8a-28d898feb7f5""
        alt=""Logo Nhà hàng Thiên Phú"" style=""height: 100px; margin-right: 10px; vertical-align: middle;"">
      <h1 style=""display: inline-block; vertical-align: middle;"">Nhà hàng Thiên Phú</h1>
      <p style=""text-transform: uppercase; font-weight: 600; color: #ffffff;font-size: 18px;"">Tặng mã giảm giá nhân ngày sinh nhật</p>
    </div>
    <div class=""content"">
      <p>Kính gửi <span class=""highlight""> " +username +@"</span>,</p>
        <p class=""""emailBody"""">
         Nhân dịp sinh nhật bạn chúng tôi kính chúc quý khách hàng có một ngày sinh nhật vui vẻ hạnh phúc và bình an bên gia đình! Chúng tôi kính gửi quý khách mã voucher bên dưới nhân ngày đặc biệt.
        </p>

         <p class=""""emailBody"""">
           Mã coupon: <b>" +couponProgram.Code +@"</b><br>
           Giảm giá: <b>"+couponProgram.DiscountPercent + @"%</b><br>
           Ngày sử dụng: <b>"+couponProgram.StartDate+@"</b><br>
           Hạn sử dụng: <b>"+ couponProgram.ExpiryDate+@"</b>
         </p>
   
    </div>
    <div class=""footer"">
      <p>Trân trọng,<br>Đội ngũ Nhà hàng Thiên Phú</p>
    </div>
  </div>
</body>

</html>
";
        return content;
    }

    public static string GetTemplateFirstRegistrationCoupon(string username, CouponProgram couponProgram)
    {
   var content = @"
<!DOCTYPE html>
<html lang=""vi"">

<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Thông báo hoàn tiền</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      margin: 0;
      padding: 0;
      background-color: #f9f9f9;
      color: #333;
    }

    .container {
      max-width: 600px;
      margin: 20px auto;
      background: #ffffff;
      padding: 20px;
      border-radius: 8px;
      border: 1px solid #ddd;
      box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }

    .header {
      background-color: #B71C1C;
      color: #ffffff;
      text-align: center;
      padding: 15px 20px;
      border-radius: 8px 8px 0 0;
    }

    .header h1 {
      display: inline-block;
      vertical-align: middle;
      margin: 0;
      font-size: 20px;
    }

    .content {
      margin: 20px 0;
      line-height: 1.6;
    }

    .highlight {
      color: #B71C1C;
      font-weight: bold;
    }

    .table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }

    .table th,
    .table td {
      border: 1px solid #ddd;
      padding: 10px;
      text-align: left;
    }

    .table th {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .table td {
      background-color: #ffffff;
    }

    .btn {
      display: inline-block;
      margin-top: 20px;
      padding: 10px 20px;
      background-color: #FFD54F;
      color: #B71C1C;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
      text-align: center;
    }

    .btn:hover {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .footer {
      text-align: center;
      margin-top: 20px;
      font-size: 14px;
      color: #555;
    }
  </style>
</head>

<body>
  <div class="""">
    <div class=""header"">
      <img
        src=""https://firebasestorage.googleapis.com/v0/b/hcqs-project.appspot.com/o/dish%2Ff25019dc-3a64-4677-87cb-63b0f3dbcef7.jpg.png?alt=media&token=c784cf86-52e6-4314-bd8a-28d898feb7f5""
        alt=""Logo Nhà hàng Thiên Phú"" style=""height: 100px; margin-right: 10px; vertical-align: middle;"">
      <h1 style=""display: inline-block; vertical-align: middle;"">Nhà hàng Thiên Phú</h1>
      <p style=""text-transform: uppercase; font-weight: 600; color: #ffffff;font-size: 18px;"">Thông báo về coupon của hệ thống</p>
    </div>
    <div class=""content"">
      <p>Kính gửi <span class=""highlight""> " +username +@"</span>,</p>
        <p class=""""emailBody"""">
         Để chào đón bạn, chúng tôi xin gửi tặng bạn một coupon đặc biệt cho lần đăng ký đầu tiên của bạn!
        </p>

         <p class=""""emailBody"""">
           Mã coupon: <b>" +couponProgram.Code +@"</b><br>
           Giảm giá: <b>"+couponProgram.DiscountPercent + @"%</b><br>
           Ngày sử dụng: <b>"+couponProgram.StartDate+@"</b><br>
           Hạn sử dụng: <b>"+ couponProgram.ExpiryDate+@"</b>
         </p>
   
    </div>
    <div class=""footer"">
      <p>Trân trọng,<br>Đội ngũ Nhà hàng Thiên Phú</p>
    </div>
  </div>
</body>

</html>
";
        return content;
    }
    public static string GetTemplateRefundDuplicatedPaymentForCustomer(string username, Order order)
    {
        var content = @"
<!DOCTYPE html>
<html lang=""vi"">

<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Thông báo hoàn tiền</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      margin: 0;
      padding: 0;
      background-color: #f9f9f9;
      color: #333;
    }

    .container {
      max-width: 600px;
      margin: 20px auto;
      background: #ffffff;
      padding: 20px;
      border-radius: 8px;
      border: 1px solid #ddd;
      box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }

    .header {
      background-color: #B71C1C;
      color: #ffffff;
      text-align: center;
      padding: 15px 20px;
      border-radius: 8px 8px 0 0;
    }

    .header h1 {
      display: inline-block;
      vertical-align: middle;
      margin: 0;
      font-size: 20px;
    }

    .content {
      margin: 20px 0;
      line-height: 1.6;
    }

    .highlight {
      color: #B71C1C;
      font-weight: bold;
    }

    .table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }

    .table th,
    .table td {
      border: 1px solid #ddd;
      padding: 10px;
      text-align: left;
    }

    .table th {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .table td {
      background-color: #ffffff;
    }

    .btn {
      display: inline-block;
      margin-top: 20px;
      padding: 10px 20px;
      background-color: #FFD54F;
      color: #B71C1C;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
      text-align: center;
    }

    .btn:hover {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .footer {
      text-align: center;
      margin-top: 20px;
      font-size: 14px;
      color: #555;
    }
  </style>
</head>

<body>
  <div class="""">
    <div class=""header"">
      <img
        src=""https://firebasestorage.googleapis.com/v0/b/hcqs-project.appspot.com/o/dish%2Ff25019dc-3a64-4677-87cb-63b0f3dbcef7.jpg.png?alt=media&token=c784cf86-52e6-4314-bd8a-28d898feb7f5""
        alt=""Logo Nhà hàng Thiên Phú"" style=""height: 100px; margin-right: 10px; vertical-align: middle;"">
      <h1 style=""display: inline-block; vertical-align: middle;"">Nhà hàng Thiên Phú</h1>
      <p style=""text-transform: uppercase; font-weight: 600; color: #ffffff;font-size: 18px;"">Thông báo hoàn tiền đặt
        hàng</p>
    </div>
    <div class=""content"">
      <p>Kính gửi <span class=""highlight"">[Tên khách hàng]</span>,</p>
      <p>Chúng tôi rất tiếc vì đã gặp phải sự cố dẫn đến việc phải hoàn tiền cho đơn hàng của bạn. Dưới đây là thông tin
        chi tiết:</p>
      <h3>Thông tin hoàn tiền</h3>
      <ul>
        <li><strong>Số tiền hoàn: </strong><span class=""highlight"">[Số tiền] VNĐ</span></li>
        <li><strong>Thời gian dùng bữa: </strong><span class=""highlight"">[Ngày giờ hoàn]</span></li>
        <li><strong>Mã hóa đơn: </strong><span class=""highlight"">[Mã hóa đơn]</span></li>
      </ul>
      <h3>Thông tin khách hàng</h3>
      <table class=""table"">
        <tr>
          <th>Tên khách hàng</th>
          <td>[Tên khách hàng]</td>
        </tr>
        <tr>
          <th>Số điện thoại</th>
          <td>[Số điện thoại]</td>
        </tr>
        <tr>
          <th>Email</th>
          <td>[Email]</td>
        </tr>
        <tr>
          <th>Địa chỉ</th>
          <td>[Địa chỉ]</td>
        </tr>
      </table>

      <p><strong>Số tiền được hoàn tiền sẽ tự động chuyển về ""<a
            href=""https://thienphurestaurant.vercel.app/user/transaction-history""
            style=""color: #B71C1C; text-decoration: none;"">Tài khoản Ví</a>"" của khách hàng trên hệ thống Website <a
            href=""https://thienphurestaurant.vercel.app/"" style=""color: #B71C1C; text-decoration: none;"">Nhà Hàng Thiên
            Phú</a> trong 4 đến 8 giờ tới.</strong> Nếu quý khách muốn hoàn <strong> TIỀN MẶT </strong>, vui lòng trả
        lời Mail này để được Hỗ trợ.</p>

      <p>Kiểm tra số tiền đã được hoàn vào ví của bạn tại đường dẫn:</p>
      <a class=""btn"" href=""https://thienphurestaurant.vercel.app/user/transaction-history"">Xem lịch sử giao dịch</a>
    </div>
    <div class=""footer"">
      <p>Trân trọng,<br>Đội ngũ Nhà hàng Thiên Phú</p>
    </div>
  </div>
</body>

</html>



";
        return content;
    }
    public static string GetTemplateRefundDuplicatedPaymentForAdmin(string username, Order order)
    {
        var content = @"

<!DOCTYPE html>
<html lang=""vi"">

<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Thông báo hoàn tiền</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      margin: 0;
      padding: 0;
      background-color: #f9f9f9;
      color: #333;
    }

    .container {
      max-width: 600px;
      margin: 20px auto;
      background: #ffffff;
      padding: 20px;
      border-radius: 8px;
      border: 1px solid #ddd;
      box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    }

    .header {
      background-color: #B71C1C;
      color: #ffffff;
      text-align: center;
      padding: 15px 20px;
      border-radius: 8px 8px 0 0;
    }

    .header h1 {
      display: inline-block;
      vertical-align: middle;
      margin: 0;
      font-size: 20px;
    }

    .content {
      margin: 20px 0;
      line-height: 1.6;
    }

    .highlight {
      color: #B71C1C;
      font-weight: bold;
    }

    .table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }

    .table th,
    .table td {
      border: 1px solid #ddd;
      padding: 10px;
      text-align: left;
    }

    .table th {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .table td {
      background-color: #ffffff;
    }

    .btn {
      display: inline-block;
      margin-top: 20px;
      padding: 10px 20px;
      background-color: #FFD54F;
      color: #B71C1C;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
      text-align: center;
    }

    .btn:hover {
      background-color: #B71C1C;
      color: #ffffff;
    }

    .footer {
      text-align: center;
      margin-top: 20px;
      font-size: 14px;
      color: #555;
    }
  </style>
</head>

<body>
  <div class="""">
    <div class=""header"">
      <img
        src=""https://firebasestorage.googleapis.com/v0/b/hcqs-project.appspot.com/o/dish%2Ff25019dc-3a64-4677-87cb-63b0f3dbcef7.jpg.png?alt=media&token=c784cf86-52e6-4314-bd8a-28d898feb7f5""
        alt=""Logo Nhà hàng Thiên Phú"" style=""height: 100px; margin-right: 10px; vertical-align: middle;"">
      <h1 style=""display: inline-block; vertical-align: middle;"">Nhà hàng Thiên Phú</h1>
      <p style=""text-transform: uppercase; font-weight: 600; color: #ffffff;font-size: 18px;"">Thông báo hoàn tiền đặt
        hàng</p>
    </div>
    <div class=""content"">
      <p>Kính gửi <span class=""highlight"">[Tên khách hàng]</span>,</p>
      <p>Chúng tôi rất tiếc vì đã gặp phải sự cố dẫn đến việc phải hoàn tiền cho đơn hàng của bạn. Dưới đây là thông tin
        chi tiết:</p>
      <h3>Thông tin hoàn tiền</h3>
      <ul>
        <li><strong>Số tiền hoàn: </strong><span class=""highlight"">[Số tiền] VNĐ</span></li>
        <li><strong>Thời gian dùng bữa: </strong><span class=""highlight"">[Ngày giờ hoàn]</span></li>
        <li><strong>Mã hóa đơn: </strong><span class=""highlight"">[Mã hóa đơn]</span></li>
      </ul>
      <h3>Thông tin khách hàng</h3>
      <table class=""table"">
        <tr>
          <th>Tên khách hàng</th>
          <td>[Tên khách hàng]</td>
        </tr>
        <tr>
          <th>Số điện thoại</th>
          <td>[Số điện thoại]</td>
        </tr>
        <tr>
          <th>Email</th>
          <td>[Email]</td>
        </tr>
        <tr>
          <th>Địa chỉ</th>
          <td>[Địa chỉ]</td>
        </tr>
      </table>

      <p><strong>Số tiền được hoàn tiền sẽ tự động chuyển về ""<a
            href=""https://thienphurestaurant.vercel.app/user/transaction-history""
            style=""color: #B71C1C; text-decoration: none;"">Tài khoản Ví</a>"" của khách hàng trên hệ thống Website <a
            href=""https://thienphurestaurant.vercel.app/"" style=""color: #B71C1C; text-decoration: none;"">Nhà Hàng Thiên
            Phú</a> trong 4 đến 8 giờ tới.</strong> Nếu quý khách muốn hoàn <strong> TIỀN MẶT </strong>, vui lòng trả
        lời Mail này để được Hỗ trợ.</p>

      <p>Kiểm tra số tiền đã được hoàn vào ví của bạn tại đường dẫn:</p>
      <a class=""btn"" href=""https://thienphurestaurant.vercel.app/user/transaction-history"">Xem lịch sử giao dịch</a>
    </div>
    <div class=""footer"">
      <p>Trân trọng,<br>Đội ngũ Nhà hàng Thiên Phú</p>
    </div>
  </div>
</body>

</html>
";

        return content;
    }
}
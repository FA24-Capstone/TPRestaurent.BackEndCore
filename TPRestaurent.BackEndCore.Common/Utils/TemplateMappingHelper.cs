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
<html>
  <head>
    <style>
      * {
        margin: 0;
        padding: 0;
      }

      body {
        font-family: Arial, sans-serif;
        background-color: #f4f4f4; /* Background color for the entire email */
      }

      .container {
        max-width: 900px;
        margin: 20 auto;
        /* padding: 20px; */
        border-radius: 5px;
        box-shadow: 0px 0px 5px 2px #ccc; /*Add a shadow to the content */
      }

      .header {
        Text-align: center;
        background-color: #ffba00; /* Header background color */
        padding: 20px;
      }
      .header-title {
        Text-align: left;
        background-color: #2ad65e; /* Header background color */
        padding: 20px;
        color: white;
      }
      .title {
        color: black; /* Text color for the title */
        font-size: 30px;
        font-weight: bold;
      }

      .greeting {
        font-size: 18px;
        margin: 10 5;
      }
      .emailBody {
        margin: 5 5;
      }
      .support {
        font-size: 15px;
        font-style: italic;
        margin: 5 5;
      }

      .mainBody {
        background-color: #ffffff; /* Main content background color */
        padding: 20px;
        /* border-radius: 5px; */
        /* box-shadow: 0px 0px 5px 2px #ccc; Add a shadow to the content */
      }
      .body-content {
        /* display: flex;
        flex-direction: column; */
        border: 1px #fff8ea;
        border-radius: 5px;
        margin: 10 5;
        padding: 10px;
        /* background-color: #fff8ea; */
        box-shadow: 0px 0px 5px 2px #ccc;
      }
      .title-content {
        font-weight: bold;
      }

      u i {
        color: blue;
      }

      .footer {
        font-size: 14px;
        Text-align: center;
        background-color: #ffba00; /* Header background color */
        padding: 10px;
        display: flex;
        justify-content: center;
        flex-direction: column;
      }
      .footer-Text {
        font-weight: 600;
      }
      .signature {
        Text-align: right;
        font-size: 16px;
        margin: 5 5;
      }
    </style>
  </head>
  <body>
    <div class=""container"">
      <div
        style=""
          height: 100px;
          display: flex;
          align-items: center;
          justify-content: center;
          background-color: white;
        ""
      >
        <p
          style=""
            color: #515151;
            Text-align: center;
            margin: auto 0;
            font-size: 30px;
          ""
        >
          Nhà hàng lẩu - nướng Thiên Phú
        </p>
      </div>
      <div class=""mainBody"">
        <!-- <div class=""header-title"">
        </div> -->
        <h2 class=""emailBody"">Hello " + name + @" ,</h2>
        <p class=""greeting""></p>

        <p class=""emailBody"">
          Hiện tại bạn đang đăng ký tài khoản tại <b><i>Nhà hàng Thiên Phú </i></b>.
        </p>
        <p class=""emailBody"">
          Bên dưới là mã xác nhận của bạn:
          <b><i> " + body + @"</i></b>
        </p>

        <p class=""emailBody"">
          Vui lòng nhập mã xác nhận vào hệ thống để đến bước tiếp theo
          <a href=""https://www.thienphurestaurant.com/""
            ><span style=""font-weight: bold; Text-transform: uppercase""
              >Tại đây</span
            ></a
          >
        </p>
        <p class=""support"">
          Cảm ơn bạn đã quan tâm đến dịch vụ của <b><i>Nhà hàng Thiên Phú</i></b
          >, nếu có bất kỳ thắc mắc nào, vui lòng liên hệ
          <u><i>qk.backend@gmail.com</i></u> để được hỗ trợ
        </p>
        <div class=""signature"">
          <p>Thân chào,</p>
          <p>
            <b><i>Nhà hàng Thiên Phú</i></b>
          </p>
        </div>
      </div>
      <div style=""height: 100px"">

      </div>
    </div>
  </body>
</html>

";
                break;

            case ContentEmailType.CONTRACT_CODE:
                content = @"
<html>
  <head>
    <style>
      * {
        margin: 0;
        padding: 0;
      }

      body {
        font-family: Arial, sans-serif;
        background-color: #f4f4f4; /* Background color for the entire email */
      }

      .container {
        max-width: 900px;
        margin: 20 auto;
        /* padding: 20px; */
        border-radius: 5px;
        box-shadow: 0px 0px 5px 2px #ccc; /*Add a shadow to the content */
      }

      .header {
        Text-align: center;
        background-color: #ffba00; /* Header background color */
        padding: 20px;
      }
      .header-title {
        Text-align: left;
        background-color: #2ad65e; /* Header background color */
        padding: 20px;
        color: white;
      }
      .title {
        color: black; /* Text color for the title */
        font-size: 30px;
        font-weight: bold;
      }

      .greeting {
        font-size: 18px;
        margin: 10 5;
      }
      .emailBody {
        margin: 5 5;
      }
      .support {
        font-size: 15px;
        font-style: italic;
        margin: 5 5;
      }

      .mainBody {
        background-color: #ffffff; /* Main content background color */
        padding: 20px;
        /* border-radius: 5px; */
        /* box-shadow: 0px 0px 5px 2px #ccc; Add a shadow to the content */
      }
      .body-content {
        /* display: flex;
        flex-direction: column; */
        border: 1px #fff8ea;
        border-radius: 5px;
        margin: 10 5;
        padding: 10px;
        /* background-color: #fff8ea; */
        box-shadow: 0px 0px 5px 2px #ccc;
      }
      .title-content {
        font-weight: bold;
      }

      u i {
        color: blue;
      }

      .footer {
        font-size: 14px;
        Text-align: center;
        background-color: #ffba00; /* Header background color */
        padding: 10px;
        display: flex;
        justify-content: center;
        flex-direction: column;
      }
      .footer-Text {
        font-weight: 600;
      }
      .signature {
        Text-align: right;
        font-size: 16px;
        margin: 5 5;
      }
    </style>
  </head>
  <body>
    <div class=""container"">
      <div
        style=""
          height: 100px;
          display: flex;
          align-items: center;
          justify-content: center;
          background-color: white;
        ""
      >
        <p
          style=""
            color: #515151;
            Text-align: center;
            margin: auto 0;
            font-size: 30px;
          ""
        >
         Nhà hàng Thiên Phú
        </p>
      </div>
      <div class=""mainBody"">
        <!-- <div class=""header-title"">
        </div> -->
        <h2 class=""emailBody"">Hello " + name + @" ,</h2>
        <p class=""greeting""></p>

        <p class=""emailBody"">
          You are in the process of completing contract procedures through <b><i>Nhà hàng Thiên Phú </i></b>.
        </p>
        <p class=""emailBody"">
          Below is your OTP information:
          <b><i> " + body + @"</i></b>
        </p>

        <p class=""emailBody"">
          Please enter the code above into the system to proceed to the next step
          <a href=""https://www.thienphurestaurant.com/""
            ><span style=""font-weight: bold; Text-transform: uppercase""
              >here</span
            ></a
          >
        </p>
        <p class=""support"">
          Thank you for your interest in the services of <b><i>Nhà hàng Thiên Phú</i></b
          >, for any inquiries, please contact
          <u><i>qk.backend@gmail.com</i></u> for support
        </p>
        <div class=""signature"">
          <p>Best regards,</p>
          <p>
            <b><i>Nhà hàng Thiên Phú Team</i></b>
          </p>
        </div>
      </div>
      <div style=""height: 100px"">

      </div>
    </div>
  </body>
</html>

";
                break;

            case ContentEmailType.FORGOTPASSWORD:
                content = @"
<html>
  <head>
    <style>
      * {
        margin: 0;
        padding: 0;
      }

      body {
        font-family: Arial, sans-serif;
        background-color: #f4f4f4; /* Background color for the entire email */
      }

      .container {
        max-width: 900px;
        margin: 20 auto;
        /* padding: 20px; */
        border-radius: 5px;
        box-shadow: 0px 0px 5px 2px #ccc; /*Add a shadow to the content */
      }

      .header {
        Text-align: center;
        background-color: #ffba00; /* Header background color */
        padding: 20px;
      }
      .header-title {
        Text-align: left;
        background-color: #2ad65e; /* Header background color */
        padding: 20px;
        color: white;
      }
      .title {
        color: black; /* Text color for the title */
        font-size: 30px;
        font-weight: bold;
      }

      .greeting {
        font-size: 18px;
        margin: 10 5;
      }
      .emailBody {
        margin: 5 5;
      }
      .support {
        font-size: 15px;
        font-style: italic;
        margin: 5 5;
      }

      .mainBody {
        background-color: #ffffff; /* Main content background color */
        padding: 20px;
        /* border-radius: 5px; */
        /* box-shadow: 0px 0px 5px 2px #ccc; Add a shadow to the content */
      }
      .body-content {
        /* display: flex;
        flex-direction: column; */
        border: 1px #fff8ea;
        border-radius: 5px;
        margin: 10 5;
        padding: 10px;
        /* background-color: #fff8ea; */
        box-shadow: 0px 0px 5px 2px #ccc;
      }
      .title-content {
        font-weight: bold;
      }

      u i {
        color: blue;
      }

      .footer {
        font-size: 14px;
        Text-align: center;
        background-color: #ffba00; /* Header background color */
        padding: 10px;
        display: flex;
        justify-content: center;
        flex-direction: column;
      }
      .footer-Text {
        font-weight: 600;
      }
      .signature {
        Text-align: right;
        font-size: 16px;
        margin: 5 5;
      }
    </style>
  </head>
  <body>
    <div class=""container"">
      <div
        style=""
          height: 100px;
          display: flex;
          align-items: center;
          justify-content: center;
          background-color: white;
        ""
      >
        <p
          style=""
            color: #515151;
            Text-align: center;
            margin: auto 0;
            font-size: 30px;
          ""
        >
          Nhà hàng Thiên Phú
        </p>
      </div>
      <div class=""mainBody"">
        <!-- <div class=""header-title"">
        </div> -->
        <h2 class=""emailBody"">Hello " + name + @" ,</h2>
        <p class=""greeting""></p>

        <p class=""emailBody"">
         You have accidentally forgotten your password through <b><i>Nhà hàng Thiên Phú </i></b>.
        </p>
        <p class=""emailBody"">
          Below is your OTP information:
          <b><i> " + body + @"</i></b>
        </p>

        <p class=""emailBody"">
          Please enter the code above into the system to proceed to the next step
          <a href=""https://www.thienphurestaurant.com/""
            ><span style=""font-weight: bold; Text-transform: uppercase""
              >here</span
            ></a
          >
        </p>
        <p class=""support"">
          Thank you for your interest in the services of <b><i>Nhà hàng Thiên Phú</i></b
          >, for any inquiries, please contact
          <u><i>qk.backend@gmail.com</i></u> for support
        </p>
        <div class=""signature"">
          <p>Best regards,</p>
          <p>
            <b><i>Nhà hàng Thiên Phú Team</i></b>
          </p>
        </div>
      </div>
      <div style=""height: 100px"">

      </div>
    </div>
  </body>
</html>

";
                break;

                return content;

            case ContentEmailType.TOURGUIDE_ACCOUNT_CREATION:
                content = @"
<html>
  <head>
    <style>
      * {
        margin: 0;
        padding: 0;
      }

      body {
        font-family: Arial, sans-serif;
        background-color: #f4f4f4; /* Background color for the entire email */
      }

      .container {
        max-width: 900px;
        margin: 20 auto;
        /* padding: 20px; */
        border-radius: 5px;
        box-shadow: 0px 0px 5px 2px #ccc; /*Add a shadow to the content */
      }

      .header {
        Text-align: center;
        background-color: #ffba00; /* Header background color */
        padding: 20px;
      }
      .header-title {
        Text-align: left;
        background-color: #2ad65e; /* Header background color */
        padding: 20px;
        color: white;
      }
      .title {
        color: black; /* Text color for the title */
        font-size: 30px;
        font-weight: bold;
      }

      .greeting {
        font-size: 18px;
        margin: 10 5;
      }
      .emailBody {
        margin: 5 5;
      }
      .support {
        font-size: 15px;
        font-style: italic;
        margin: 5 5;
      }

      .mainBody {
        background-color: #ffffff; /* Main content background color */
        padding: 20px;
        /* border-radius: 5px; */
        /* box-shadow: 0px 0px 5px 2px #ccc; Add a shadow to the content */
      }
      .body-content {
        /* display: flex;
        flex-direction: column; */
        border: 1px #fff8ea;
        border-radius: 5px;
        margin: 10 5;
        padding: 10px;
        /* background-color: #fff8ea; */
        box-shadow: 0px 0px 5px 2px #ccc;
      }
      .title-content {
        font-weight: bold;
      }

      u i {
        color: blue;
      }

      .footer {
        font-size: 14px;
        Text-align: center;
        background-color: #ffba00; /* Header background color */
        padding: 10px;
        display: flex;
        justify-content: center;
        flex-direction: column;
      }
      .footer-Text {
        font-weight: 600;
      }
      .signature {
        Text-align: right;
        font-size: 16px;
        margin: 5 5;
      }
    </style>
  </head>
  <body>
    <div class=""container"">
      <div
        style=""
          height: 100px;
          display: flex;
          align-items: center;
          justify-content: center;
          background-color: white;
        ""
      >
        <p
          style=""
            color: #515151;
            Text-align: center;
            margin: auto 0;
            font-size: 30px;
          ""
        >
          Nhà hàng Thiên Phú
        </p>
      </div>
      <div class=""mainBody"">
        <!-- <div class=""header-title"">
        </div> -->
        <h2 class=""emailBody"">Xin chào " + name + @" ,</h2>
        <p class=""greeting""></p>

        <p class=""emailBody"">
         Tài khoản hướng dẫn viên của bạn đã được tọ thành công <b><i>Nhà hàng Thiên Phú </i></b>.
        </p>
        <p class=""emailBody"">
          Đây là thông tin tài khoản của bạn, hãy thay đỗi mật khẩu
          <b><i> " + body + @"</i></b>
        </p>

        <p class=""emailBody"">
          Please enter the code above into the system to proceed to the next step
          <a href=""https://www.thienphurestaurant.com/""
            ><span style=""font-weight: bold; Text-transform: uppercase""
              >here</span
            ></a
          >
        </p>
        <p class=""support"">
          Thank you for your interest in the services of <b><i>Nhà hàng Thiên Phú</i></b
          >, for any inquiries, please contact
          <u><i>qk.backend@gmail.com</i></u> for support
        </p>
        <div class=""signature"">
          <p>Best regards,</p>
          <p>
            <b><i>Nhà hàng Thiên Phú Team</i></b>
          </p>
        </div>
      </div>
      <div style=""height: 100px"">

      </div>
    </div>
  </body>
</html>

";
                break;

            case ContentEmailType.INSUFFICIENT_COUPON_QUANTITY:
                content = @"
<html>
  <head>
    <style>
      * {
        margin: 0;
        padding: 0;
      }

      body {
        font-family: Arial, sans-serif;
        background-color: #f4f4f4;
      }

      .container {
        max-width: 900px;
        margin: 20 auto;
        border-radius: 5px;
        box-shadow: 0px 0px 5px 2px #ccc;
      }

      .header {
        text-align: center;
        background-color: #ffba00;
        padding: 20px;
      }

      .title {
        color: black;
        font-size: 30px;
        font-weight: bold;
      }

      .mainBody {
        background-color: #ffffff;
        padding: 20px;
      }

      .emailBody {
        margin: 5 5;
        font-size: 16px;
        color: #333;
      }

      .couponList {
        margin: 10 5;
        padding: 10px;
        border: 1px solid #ddd;
        border-radius: 5px;
        list-style-type: none;
      }

      .couponList li {
        margin: 5px 0;
        padding: 10px;
        background-color: #fff8ea;
        border-radius: 3px;
        box-shadow: 0px 0px 3px 1px #ccc;
      }

      .support {
        font-size: 15px;
        font-style: italic;
        margin: 5 5;
      }

      .signature {
        text-align: right;
        font-size: 16px;
        margin: 5 5;
      }

      .footer {
        text-align: center;
        background-color: #ffba00;
        padding: 10px;
        font-size: 14px;
        font-weight: 600;
      }
    </style>
  </head>
  <body>
    <div class=""container"">
      <div class=""header"">
        <p class=""title"">Nhà hàng Thiên Phú</p>
      </div>
      <div class=""mainBody"">
        <h2 class=""emailBody"">Kính gửi Quản trị viên,</h2>
        <p class=""emailBody"">
          Đây là thông báo về quy trình tự động gán mã giảm giá. Hiện tại, số lượng của các coupon sau không đủ và cần được cập nhật:
        </p>
        " + body + @"
        <p class=""emailBody"">
          Vui lòng đăng nhập vào hệ thống và cập nhật số lượng coupon để đảm bảo quy trình không bị gián đoạn.
        </p>
        <p class=""support"">
          Nếu cần hỗ trợ, vui lòng liên hệ <u><i>qk.backend@gmail.com</i></u>.
        </p>
        <div class=""signature"">
          <p>Trân trọng,</p>
          <p><b>Hệ thống Nhà hàng Thiên Phú</b></p>
        </div>
      </div>
      <div class=""footer"">
        <p>Email này được gửi tự động. Vui lòng không trả lời trực tiếp.</p>
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

//         var content = $@"
// <html>
//   <head>
//     <style>
//       * {{
//         margin: 0;
//         padding: 0;
//       }}
//
//       body {{
//         font-family: Arial, sans-serif;
//         background-color: #f4f4f4; /* Màu nền cho toàn bộ email */
//       }}
//
//       .container {{
//         max-width: 900px;
//         margin: 20px auto;
//         border-radius: 5px;
//         box-shadow: 0px 0px 5px 2px #ccc; /* Thêm bóng cho nội dung */
//       }}
//
//       .header {{
//         text-align: center;
//         background-color: #ffba00; /* Màu nền cho tiêu đề */
//         padding: 20px;
//       }}
//       .header-title {{
//         text-align: left;
//         background-color: #2ad65e; /* Màu nền cho tiêu đề */
//         padding: 20px;
//         color: white;
//       }}
//       .title {{
//         color: black; /* Màu chữ cho tiêu đề */
//         font-size: 30px;
//         font-weight: bold;
//       }}
//
//       .greeting {{
//         font-size: 18px;
//         margin: 10px 5px;
//       }}
//       .emailBody {{
//         margin: 5px 5px;
//       }}
//       .support {{
//         font-size: 15px;
//         font-style: italic;
//         margin: 5px 5px;
//       }}
//
//       .mainBody {{
//         background-color: #ffffff; /* Màu nền cho nội dung chính */
//         padding: 20px;
//       }}
//       .body-content {{
//         border: 1px #fff8ea;
//         border-radius: 5px;
//         margin: 10px 5px;
//         padding: 10px;
//         box-shadow: 0px 0px 5px 2px #ccc;
//       }}
//       .title-content {{
//         font-weight: bold;
//       }}
//
//       u i {{
//         color: blue;
//       }}
//
//       .footer {{
//         font-size: 14px;
//         text-align: center;
//         background-color: #ffba00; /* Màu nền cho chân trang */
//         padding: 10px;
//         display: flex;
//         justify-content: center;
//         flex-direction: column;
//       }}
//       .footer-Text {{
//         font-weight: 600;
//       }}
//       .signature {{
//         text-align: right;
//         font-size: 16px;
//         margin: 5px 5px;
//       }}
//     </style>
//   </head>
//   <body>
//     <div class=""container"">
//       <div
//         style=""
//           height: 100px;
//           display: flex;
//           align-items: center;
//           justify-content: center;
//           background-color: white;
//         ""
//       >
//         <p
//           style=""
//             color: #515151;
//             text-align: center;
//             margin: auto 0;
//             font-size: 30px;
//           ""
//         >
//           Nhà hàng Thiên Phú
//         </p>
//       </div>
//       <div class=""mainBody"">
//         <h2 class=""emailBody"">Xin chào {username},</h2>
//
//         <p class=""emailBody"">
//           Chúng tôi rất tiếc phải thông báo rằng đặt bàn của bạn đã bị hủy tại <b><i>Nhà hàng Thiên Phú</i></b>.
//         </p>
//
//         <p class=""emailBody"">
//           Mã đơn: <b>{order.OrderId.ToString().Substring(0, 5)}</b><br>
//           Thời gian đặt: <b>{orderTime}</b>
//           Thời gian dùng bữa tại nhà hàng(Nếu có): <b> {order.MealTime ?? order.MealTime.Value}</b>
//           Loại phòng: {tableDetail.Table!.Room!}
//           Loại bàn: {tableDetail.Table!.TableSize!}
//         </p>
//
//         <p class=""emailBody"">
//           Nếu đây là một sai sót hoặc nếu bạn muốn thực hiện một đặt chỗ/đơn giao tận nơi khác, vui lòng truy cập website của chúng tôi hoặc liên hệ trực tiếp với chúng tôi.
//         </p>
//
//         <p class=""emailBody"">
//           Để biết thêm thông tin, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi qua
//           <u><i>qk.backend@gmail.com</i></u>.
//         </p>
//
//         <p class=""support"">
//           Cảm ơn bạn đã thông cảm, và chúng tôi hy vọng được phục vụ bạn trong tương lai.
//         </p>
//         <div class=""signature"">
//           <p>Trân trọng,</p>
//           <p>
//             <b><i>Đội ngũ Nhà hàng Thiên Phú</i></b>
//           </p>
//         </div>
//       </div>
//       <div style=""height: 100px""></div>
//     </div>
//   </body>
// </html>";
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
        var content = $@"
<html>
  <head>
    <style>
      * {{
        margin: 0;
        padding: 0;
      }}

      body {{
        font-family: Arial, sans-serif;
        background-color: #f4f4f4; /* Màu nền cho toàn bộ email */
      }}

      .container {{
        max-width: 900px;
        margin: 20px auto;
        border-radius: 5px;
        box-shadow: 0px 0px 5px 2px #ccc; /* Thêm bóng cho nội dung */
      }}

      .header {{
        text-align: center;
        background-color: #ffba00; /* Màu nền cho tiêu đề */
        padding: 20px;
      }}

      .mainBody {{
        background-color: #ffffff; /* Màu nền cho nội dung chính */
        padding: 20px;
      }}

      .emailBody {{
        margin: 5px 5px;
      }}

      .support {{
        font-size: 15px;
        font-style: italic;
        margin: 5px 5px;
      }}

      .footer {{
        font-size: 14px;
        text-align: center;
        background-color: #ffba00; /* Màu nền cho chân trang */
        padding: 10px;
      }}

      .signature {{
        text-align: right;
        font-size: 16px;
        margin: 5px 5px;
      }}
    </style>
  </head>
  <body>
    <div class=""container"">
      <div class=""header"">
        <h1 style=""color: #515151;"">Nhà hàng Thiên Phú</h1>
      </div>
      <div class=""mainBody"">
        <h2 class=""emailBody"">Xin chào {username},</h2>

        <p class=""emailBody"">
          Đây là lời nhắc nhở để thông báo rằng thời gian đặt chỗ của bạn tại <b><i>Nhà hàng Thiên Phú</i></b> đang đến gần!
        </p>

        <p class=""emailBody"">
          Mã đặt chỗ: <b>{order.OrderId}</b><br>
          Thời gian đặt chỗ: <b>{order.ReservationDate}</b>
        </p>

        <p class=""emailBody"">
          Chúng tôi rất mong được chào đón bạn và hy vọng bạn sẽ có một trải nghiệm tuyệt vời.
        </p>

        <p class=""emailBody"">
          Nếu bạn cần thay đổi hoặc hủy bỏ đặt chỗ, vui lòng liên hệ với chúng tôi qua email <u><i>qk.backend@gmail.com</i></u>.
        </p>

        <p class=""support"">
          Cảm ơn bạn đã chọn <b><i>Nhà hàng Thiên Phú</i></b>!
        </p>
        <div class=""signature"">
          <p>Trân trọng,</p>
          <p>
            <b><i>Đội ngũ Nhà hàng Thiên Phú</i></b>
          </p>
        </div>
      </div>
      <div style=""height: 100px""></div>
    </div>
  </body>
</html>";
        return content;
    }

    public static string GetTemplateOrderConfirmation(string username, Order order)
    {
//         var content = $@"
// <html>
//   <head>
//     <style>
//       * {{
//         margin: 0;
//         padding: 0;
//       }}
//
//       body {{
//         font-family: Arial, sans-serif;
//         background-color: #f4f4f4; /* Background color for the entire email */
//       }}
//
//       .container {{
//         max-width: 900px;
//         margin: 20px auto;
//         border-radius: 5px;
//         box-shadow: 0px 0px 5px 2px #ccc; /* Shadow for content */
//       }}
//
//       .header {{
//         text-align: center;
//         background-color: #ffba00; /* Header background color */
//         padding: 20px;
//       }}
//
//       .mainBody {{
//         background-color: #ffffff; /* Main content background */
//         padding: 20px;
//       }}
//
//       .emailBody {{
//         margin: 5px 5px;
//       }}
//
//       .support {{
//         font-size: 15px;
//         font-style: italic;
//         margin: 5px 5px;
//       }}
//
//       .footer {{
//         font-size: 14px;
//         text-align: center;
//         background-color: #ffba00; /* Footer background color */
//         padding: 10px;
//       }}
//
//       .signature {{
//         text-align: right;
//         font-size: 16px;
//         margin: 5px 5px;
//       }}
//     </style>
//   </head>
//   <body>
//     <div class=""container"">
//       <div class=""header"">
//         <h1 style=""color: #515151;"">Nhà hàng Thiên Phú</h1>
//       </div>
//       <div class=""mainBody"">
//         <h2 class=""emailBody"">Xin chào {username},</h2>
//
//         <p class=""emailBody"">
//           Chúng tôi vui mừng thông báo rằng đơn hàng của bạn tại <b><i>Nhà hàng Thiên Phú</i></b> đã được tạo thành công!
//         </p>
//
//         <p class=""emailBody"">
//           Mã đơn hàng: <b>{order.OrderId}</b><br>
//           Ngày đặt hàng: <b>{order.OrderDate}</b><br>
//           Tổng số tiền: <b>{order.TotalAmount:C}</b>
//         </p>
//
//         <p class=""emailBody"">
//           Chúng tôi mong rằng bạn sẽ có một trải nghiệm tuyệt vời tại Nhà hàng Thiên Phú.
//         </p>
//
//         <p class=""emailBody"">
//           Nếu bạn cần hỗ trợ hoặc thay đổi đơn hàng, vui lòng liên hệ chúng tôi qua email <u><i>qk.backend@gmail.com</i></u>.
//         </p>
//
//         <p class=""support"">
//           Cảm ơn bạn đã chọn <b><i>Nhà hàng Thiên Phú</i></b>!
//         </p>
//         <div class=""signature"">
//           <p>Trân trọng,</p>
//           <p>
//             <b><i>Đội ngũ Nhà hàng Thiên Phú</i></b>
//           </p>
//         </div>
//       </div>
//       <div style=""height: 100px""></div>
//     </div>
//   </body>
// </html>";
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
//         var content = $@"
// <html>
//   <head>
//     <style>
//       * {{
//         margin: 0;
//         padding: 0;
//       }}
//
//       body {{
//         font-family: Arial, sans-serif;
//         background-color: #f4f4f4; /* Background color for the entire email */
//       }}
//
//       .container {{
//         max-width: 900px;
//         margin: 20px auto;
//         border-radius: 5px;
//         box-shadow: 0px 0px 5px 2px #ccc; /* Shadow for content */
//       }}
//
//       .header {{
//         text-align: center;
//         background-color: #ffba00; /* Header background color */
//         padding: 20px;
//       }}
//
//       .mainBody {{
//         background-color: #ffffff; /* Main content background */
//         padding: 20px;
//       }}
//
//       .emailBody {{
//         margin: 5px 5px;
//       }}
//
//       .support {{
//         font-size: 15px;
//         font-style: italic;
//         margin: 5px 5px;
//       }}
//
//       .footer {{
//         font-size: 14px;
//         text-align: center;
//         background-color: #ffba00; /* Footer background color */
//         padding: 10px;
//       }}
//
//       .signature {{
//         text-align: right;
//         font-size: 16px;
//         margin: 5px 5px;
//       }}
//     </style>
//   </head>
//   <body>
//     <div class=""container"">
//       <div class=""header"">
//         <h1 style=""color: #515151;"">Nhà hàng Thiên Phú</h1>
//       </div>
//       <div class=""mainBody"">
//         <h2 class=""emailBody"">Xin chào {username},</h2>
//
//         <p class=""emailBody"">
//           Chúng tôi vui mừng xác nhận rằng đặt chỗ của bạn tại <b><i>Nhà hàng Thiên Phú</i></b> đã được tạo thành công!
//         </p>
//
//         <p class=""emailBody"">
//           Mã đặt chỗ: <b>{order.OrderId}</b><br>
//           Thời gian đặt chỗ: <b>{order.OrderDate}</b><br>
//           Tổng số tiền: <b>{order.TotalAmount:C}</b>
//         </p>
//
//         <p class=""emailBody"">
//           Chúng tôi rất mong được chào đón bạn vào thời gian đã chọn. Nếu bạn cần thay đổi thông tin đặt chỗ, vui lòng liên hệ với chúng tôi qua website hoặc email.
//         </p>
//
//         <p class=""emailBody"">
//           Để biết thêm thông tin, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi qua
//           <u><i>qk.backend@gmail.com</i></u>.
//         </p>
//
//         <p class=""support"">
//           Cảm ơn bạn đã chọn <b><i>Nhà hàng Thiên Phú</i></b>, và chúng tôi mong muốn mang đến cho bạn một trải nghiệm tuyệt vời.
//         </p>
//         <div class=""signature"">
//           <p>Trân trọng,</p>
//           <p>
//             <b><i>Đội ngũ Nhà hàng Thiên Phú</i></b>
//           </p>
//         </div>
//       </div>
//       <div style=""height: 100px""></div>
//     </div>
//   </body>
// </html>";
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
      <p style=""text-transform: uppercase; font-weight: 600; color: #ffffff;font-size: 18px;"">Thông báo hoàn tiền đặt
        hàng</p>
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
      <p style=""text-transform: uppercase; font-weight: 600; color: #ffffff;font-size: 18px;"">Thông báo hoàn tiền đặt
        hàng</p>
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

}
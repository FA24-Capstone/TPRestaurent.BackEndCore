﻿using System.Xml.Linq;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.Utils;

public class TemplateMappingHelper
{
    public enum ContentEmailType
    {
        VERIFICATION_CODE,
        FORGOTPASSWORD,
        CONTRACT_CODE,
        TOURGUIDE_ACCOUNT_CREATION
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
          <a href=""https://lovehouse.vercel.app/""
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
          <a href=""https://lovehouse.vercel.app/""
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
          <a href=""https://lovehouse.vercel.app/""
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
          <a href=""https://lovehouse.vercel.app/""
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
        }
        return content;
    }

    public static string GetTemplateMailToCancelReservation(string username, Order order)
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
      .header-title {{
        text-align: left;
        background-color: #2ad65e; /* Màu nền cho tiêu đề */
        padding: 20px;
        color: white;
      }}
      .title {{
        color: black; /* Màu chữ cho tiêu đề */
        font-size: 30px;
        font-weight: bold;
      }}

      .greeting {{
        font-size: 18px;
        margin: 10px 5px;
      }}
      .emailBody {{
        margin: 5px 5px;
      }}
      .support {{
        font-size: 15px;
        font-style: italic;
        margin: 5px 5px;
      }}

      .mainBody {{
        background-color: #ffffff; /* Màu nền cho nội dung chính */
        padding: 20px;
      }}
      .body-content {{
        border: 1px #fff8ea;
        border-radius: 5px;
        margin: 10px 5px;
        padding: 10px;
        box-shadow: 0px 0px 5px 2px #ccc;
      }}
      .title-content {{
        font-weight: bold;
      }}

      u i {{
        color: blue;
      }}

      .footer {{
        font-size: 14px;
        text-align: center;
        background-color: #ffba00; /* Màu nền cho chân trang */
        padding: 10px;
        display: flex;
        justify-content: center;
        flex-direction: column;
      }}
      .footer-Text {{
        font-weight: 600;
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
            text-align: center;
            margin: auto 0;
            font-size: 30px;
          ""
        >
          Nhà hàng Thiên Phú
        </p>
      </div>
      <div class=""mainBody"">
        <h2 class=""emailBody"">Xin chào {username},</h2>
        
        <p class=""emailBody"">
          Chúng tôi rất tiếc phải thông báo rằng đặt chỗ của bạn đã bị hủy tại <b><i>Nhà hàng Thiên Phú</i></b>.
        </p>

        <p class=""emailBody"">
          Mã đặt chỗ: <b>{order.OrderId}</b><br>
          Thời gian đặt chỗ: <b>{order.ReservationDate}</b>
        </p>

        <p class=""emailBody"">
          Nếu đây là một sai sót hoặc nếu bạn muốn thực hiện một đặt chỗ khác, vui lòng truy cập website của chúng tôi hoặc liên hệ trực tiếp với chúng tôi.
        </p>

        <p class=""emailBody"">
          Để biết thêm thông tin, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi qua 
          <u><i>qk.backend@gmail.com</i></u>.
        </p>

        <p class=""support"">
          Cảm ơn bạn đã thông cảm, và chúng tôi hy vọng được phục vụ bạn trong tương lai.
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


}
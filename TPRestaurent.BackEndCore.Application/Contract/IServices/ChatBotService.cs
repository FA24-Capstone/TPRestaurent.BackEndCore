using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface ChatBotService
    {
        Task<string> IntroduceAndAskForInfo(string customerName);

        /// <summary>
        /// Hỏi về số người và sở thích ăn uống.
        /// </summary>
        /// <param name="numberOfPeople">Số người trong nhóm.</param>
        /// <param name="specialPreferences">Sở thích ăn uống đặc biệt.</param>
        /// <returns>Thông điệp hỏi về số người và sở thích ăn uống.</returns>
        Task<string> AskForPeopleAndPreferences(int numberOfPeople, string specialPreferences);

        /// <summary>
        /// Thu thập thông tin từ cơ sở dữ liệu dựa trên lịch sử của khách hàng.
        /// </summary>
        /// <param name="customerId">ID của khách hàng.</param>
        /// <returns>Thông điệp gợi ý món ăn dựa trên lịch sử đặt món.</returns>
        Task<string> CollectInfoFromDatabase(Guid customerId);

        /// <summary>
        /// Gợi ý món ăn dựa trên thời tiết.
        /// </summary>
        /// <param name="weatherCondition">Điều kiện thời tiết hiện tại.</param>
        /// <returns>Thông điệp gợi ý món ăn phù hợp với thời tiết.</returns>
        Task<string> SuggestFoodBasedOnWeather(string weatherCondition);

        /// <summary>
        /// Gợi ý món ăn theo yêu cầu đặc biệt của khách hàng.
        /// </summary>
        /// <param name="specialRequest">Yêu cầu đặc biệt của khách hàng.</param>
        /// <returns>Thông điệp gợi ý món ăn phù hợp với yêu cầu đặc biệt.</returns>
        Task<string> SuggestFoodBasedOnSpecialRequest(string specialRequest);

        /// <summary>
        /// Hỏi thêm thông tin nếu cần thiết để tối ưu hóa gợi ý.
        /// </summary>
        /// <param name="additionalInfo">Thông tin thêm từ khách hàng.</param>
        /// <returns>Thông điệp hỏi thêm thông tin để cải thiện gợi ý.</returns>
        Task<string> AskForAdditionalInfo(string additionalInfo);

        /// <summary>
        /// Tối ưu hóa gợi ý tiếp theo dựa trên phản hồi của khách hàng.
        /// </summary>
        /// <param name="customerFeedback">Phản hồi của khách hàng.</param>
        /// <returns>Thông điệp gợi ý món ăn tiếp theo dựa trên phản hồi.</returns>
        Task<string> OptimizeSuggestionBasedOnFeedback(string customerFeedback);

        /// <summary>
        /// Kết thúc cuộc trò chuyện và hỏi về sự hài lòng của khách hàng.
        /// </summary>
        /// <param name="customerName">Tên khách hàng.</param>
        /// <returns>Thông điệp kết thúc và hỏi về sự hài lòng.</returns>
        Task<string> EndConversationAndAskSatisfaction(string customerName);

        /// <summary>
        /// Gợi ý đồ uống kèm theo món ăn.
        /// </summary>
        /// <param name="food">Món ăn đã chọn.</param>
        /// <returns>Thông điệp gợi ý đồ uống phù hợp.</returns>
        Task<string> SuggestDrink(string food);

        /// <summary>
        /// Hỏi về mức độ hài lòng của khách hàng sau khi dùng bữa.
        /// </summary>
        /// <param name="customerId">ID của khách hàng.</param>
        /// <returns>Thông điệp hỏi về mức độ hài lòng.</returns>
        Task<string> AskForSatisfaction(Guid customerId);

        /// <summary>
        /// Thông báo cho khách hàng về các chương trình khuyến mãi hiện tại.
        /// </summary>
        /// <returns>Thông điệp thông báo khuyến mãi.</returns>
        Task<string> NotifyPromotions();

        /// <summary>
        /// Giải đáp thắc mắc của khách hàng về thực đơn, giờ mở cửa, và cách thức thanh toán.
        /// </summary>
        /// <param name="question">Câu hỏi của khách hàng.</param>
        /// <returns>Thông điệp giải đáp thắc mắc.</returns>
        Task<string> AnswerQuestions(string question);
    }
}

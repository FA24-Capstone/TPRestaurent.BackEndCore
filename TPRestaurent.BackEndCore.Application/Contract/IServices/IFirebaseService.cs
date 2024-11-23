using Microsoft.AspNetCore.Http;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IFirebaseService
    {
        Task<AppActionResult> UploadFileToFirebase(IFormFile file, string pathFileName, bool? isPng = true);

        public Task<string> GetUrlImageFromFirebase(string pathFileName);

        public Task<AppActionResult> DeleteFileFromFirebase(string pathFileName);

        Task<string> SendNotificationAsync(string deviceToken, string title, string body, AppActionResult data = null);

        Task<List<string>> SendMulticastAsync(List<string> deviceTokens, string title, string body, AppActionResult data = null);
    }
}
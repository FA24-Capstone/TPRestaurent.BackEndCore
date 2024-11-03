using DinkToPdf.Contracts;
using Firebase.Auth;
using FirebaseAdmin;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.ConfigurationModel;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class FirebaseService : GenericBackendService, IFirebaseService
    {
        private readonly IConverter _pdfConverter;
        private AppActionResult _result;
        private FirebaseConfiguration _firebaseConfiguration;
        private FirebaseAdminSDK _firebaseAdminSdk;
        private readonly IConfiguration _configuration;
        private  FirebaseMessaging _messaging;

        public FirebaseService(IConverter pdfConverter, IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider)
        {
            _pdfConverter = pdfConverter;
            _result = new();
            _firebaseConfiguration = Resolve<FirebaseConfiguration>();
            _firebaseAdminSdk = Resolve<FirebaseAdminSDK>();
            _configuration = configuration;
            var credentials = GoogleCredential.FromFile("FirebaseCredentials/thienphu-app-firebase-adminsdk-g26ik-28f4c707bf.json");
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credentials
                });
                _messaging = FirebaseMessaging.DefaultInstance;

            }
            else
            {
                FirebaseApp.DefaultInstance.Delete();
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credentials
                });
                _messaging = FirebaseMessaging.DefaultInstance;

            }

        }

        public async Task<AppActionResult> DeleteFileFromFirebase(string pathFileName)
        {
            var _result = new AppActionResult();
            try
            {
                var auth = new FirebaseAuthProvider(new FirebaseConfig(_firebaseConfiguration.ApiKey));
                var account = await auth.SignInWithEmailAndPasswordAsync(_firebaseConfiguration.AuthEmail, _firebaseConfiguration.AuthPassword);
                var storage = new FirebaseStorage(
             _firebaseConfiguration.Bucket,
             new FirebaseStorageOptions
             {
                 AuthTokenAsyncFactory = () => Task.FromResult(account.FirebaseToken),
                 ThrowOnCancel = true
             });
                await storage
                    .Child(pathFileName)
                    .DeleteAsync();
                _result.Messages.Add("Delete image successful");
            }
            catch (FirebaseStorageException ex)
            {
                _result.Messages.Add($"Error deleting image: {ex.Message}");
            }
            return _result;
        }

        public async Task<string> SendNotificationAsync(string deviceToken, string title, string body, AppActionResult data = null)
        {
          
            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                    ImageUrl = "https://firebasestorage.googleapis.com/v0/b/thienphu-app.appspot.com/o/icon.png?alt=media&token=5a819b51-28eb-4f22-b303-0a01bfc5638d"
                },
            };

            try
            {
                string response = await _messaging.SendAsync(message);
                return response; // Successfully sent message
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<string>> SendMulticastAsync(List<string> deviceTokens, string title, string body, AppActionResult data = null)
        {
            try
            {
                var messages = new List<Message>();
                foreach (var token in deviceTokens)
                {
                    var message = new Message
                    {
                        Token = token,
                        Notification = new Notification
                        {
                            Title = title,
                            Body = body,
                            ImageUrl = "https://firebasestorage.googleapis.com/v0/b/thienphu-app.appspot.com/o/icon.png?alt=media&token=5a819b51-28eb-4f22-b303-0a01bfc5638d"
                        },
                        Data = data != null ? Utility.ToDictionary(data) : null // Optional data payload
                    };

                    messages.Add(message);
                }

                // Send all messages as a batch
                foreach (var message in messages)
                {
                    await _messaging.SendAsync(message);
                }
                return deviceTokens; // Successfully sent all messages

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> GetUrlImageFromFirebase(string pathFileName)
        {
            var a = pathFileName.Split("/");
            pathFileName = $"{a[0]}%2F{a[1]}";
            var api = $"https://firebasestorage.googleapis.com/v0/b/yogacenter-44949.appspot.com/o?name={pathFileName}";
            if (string.IsNullOrEmpty(pathFileName))
            {
                return string.Empty;
            }

            var client = new RestClient();
            var request = new RestRequest(api);
            var response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var jmessage = JObject.Parse(response.Content);
                var downloadToken = jmessage.GetValue("downloadTokens").ToString();
                return
                    $"https://firebasestorage.googleapis.com/v0/b/{_configuration["Firebase:Bucket"]}/o/{pathFileName}?alt=media&token={downloadToken}";
            }

            return string.Empty;
        }

        public async Task<AppActionResult> UploadFileToFirebase(IFormFile file, string pathFileName, bool? isPng = true)
        {
            var _result = new AppActionResult();
            bool isValid = true;

            if (file == null || file.Length == 0)
            {
                isValid = false;
                _result.Messages.Add("The file is empty");
            }

            if (isValid)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var stream = new MemoryStream(memoryStream.ToArray());
                    var auth = new FirebaseAuthProvider(new FirebaseConfig(_firebaseConfiguration.ApiKey));
                    var account = await auth.SignInWithEmailAndPasswordAsync(_firebaseConfiguration.AuthEmail, _firebaseConfiguration.AuthPassword);
                    string destinationPath = ""; // Add .png extension
                    if (isPng.HasValue && !isPng.Value)
                    {
                        destinationPath = $"{pathFileName}.pdf";
                    }
                    else
                    {
                        destinationPath = $"{pathFileName}.png";
                    }
                    // Since Firebase.Storage doesn't support metadata directly, use a workaround
                    // You could encode metadata in the file path or handle it separately
                    var task = new FirebaseStorage(
                        _firebaseConfiguration.Bucket,
                        new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(account.FirebaseToken),
                            ThrowOnCancel = true
                        })
                        .Child(destinationPath)
                        .PutAsync(stream);

                    var downloadUrl = await task;

                    if (task != null)
                    {
                        _result.Result = downloadUrl;
                        _result.IsSuccess = true;
                    }
                    else
                    {
                        _result.IsSuccess = false;
                        _result.Messages.Add("Upload failed");
                    }
                }
            }

            return _result;
        }
    }
}

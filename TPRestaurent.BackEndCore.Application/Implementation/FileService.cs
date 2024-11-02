using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class FileService : GenericBackendService, IFileService
    {
        private readonly IConverter _pdfConverter;
        public FileService(IConverter pdfConverter, IServiceProvider serviceProvider): base(serviceProvider) 
        { 
            _pdfConverter = pdfConverter;
        }
        public IFormFile ConvertHtmlToPdf(string content, string fileName)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings =
            {
                PaperSize = PaperKind.A4,
                Orientation = Orientation.Portrait,
            },
                Objects =
            {
                new ObjectSettings()
                {
                    PagesCount = true,
                    HtmlContent = content,
                    WebSettings = { DefaultEncoding = "utf-8" },
                }
            }
            };

            byte[] pdfBytes = _pdfConverter.Convert(doc);
            return CreateFormFileFromBytes(pdfBytes, fileName);
        }

        private IFormFile CreateFormFileFromBytes(byte[] content, string fileName)
        {
            var ms = new MemoryStream(content);
            return new FormFile(ms, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };
        }
    }
}

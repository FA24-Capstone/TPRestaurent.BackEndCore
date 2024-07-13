using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IFileService
    {
        public IActionResult GenerateExcelContent<T1, T2>(string fileName, IEnumerable<T1> dataList, IEnumerable<T2> dataList2, List<string> header, string sheetName, List<string> header2 = null, string sheetName2 = null);
        public IActionResult GenerateExcelSingleContent<T>(IEnumerable<T> dataList, string sheetName);
    }
}

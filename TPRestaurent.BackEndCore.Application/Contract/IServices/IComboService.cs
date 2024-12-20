﻿using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IComboService
    {
        Task<AppActionResult> CreateCombo(ComboDto comboDto);

        Task<AppActionResult> UploadComboImages(ComboImageDto comboDto);

        Task<AppActionResult> UpdateCombo(UpdateComboDto comboDto);

        Task<AppActionResult> DeleteComboById(Guid comboId);

        Task<AppActionResult> GetComboById(Guid comboId);

        Task<AppActionResult> GetComboById2(Guid comboId);

        public Task<AppActionResult> GetAllCombo(string? keyword, ComboCategory? category, int? startPrice, int? endPrice, int pageNumber, int pageSize);

        public Task<AppActionResult> ActivateCombo(Guid comboId);
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;

namespace OIMS.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<CreatedCategoryResponseDto> CreateCategoryAsync(
            CreateCategoryRequestDto request,
            int userId
        );
    }
}

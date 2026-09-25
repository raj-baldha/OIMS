using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<CreatedCategoryResponseDto> CreateCategoryAsync(CreateCategoryRequestDto request,int userId);
    }
}

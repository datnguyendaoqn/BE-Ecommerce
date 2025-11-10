using BackendEcommerce.Application.Reviews.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Domain.Contracts.Persistence;
using BackendEcommerce.Infrastructure.Persistence.Models;
using System.Text;

namespace BackendEcommerce.Application.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly IProductRepository _productRepo;

        public ReviewService(IReviewRepository reviewRepo,IProductRepository productRepo)
        {
            _reviewRepo = reviewRepo;
            _productRepo = productRepo;
        }

        // === LOGIC ĐƯỢC DI CHUYỂN TỪ PRODUCTSERVICE SANG ===

        public async Task<ApiResponseDTO<List<ReviewProductResponseDto>>> GetProductReviewsAsync(int productId)
        {
            var productExists = await _productRepo.ExistsAsync(productId);
            if (!productExists)
            {
                // Nếu KHÔNG, trả về 404 (Production-Ready)
                return new ApiResponseDTO<List<ReviewProductResponseDto>>
                {
                    IsSuccess = false,
                    Code = 404, // <-- Mã lỗi 404
                    Message = $"Không tìm thấy sản phẩm."
                };
            }
            // 1. Lấy dữ liệu thô (đã gộp User, Variant)
            var reviews = await _reviewRepo.GetReviewsForProductAsync(productId);
            // 2. Map (Ánh xạ) sang DTO (và xử lý logic)
            var dtos = reviews.Select(r => new ReviewProductResponseDto
            {
                Id = r.Id,
                AuthorName = r.User.FullName,
                Rating = r.Rating,
                Comment = r.CommentText,
                CreatedAt = r.CreatedAt,
                // Chạy logic "Ghép thông tin" (Production-ready)
                VariantInfo = FormatVariantInfo(r.Variant)
            }).ToList();

            return new ApiResponseDTO<List<ReviewProductResponseDto>> { IsSuccess = true, Data = dtos };
        }


       
       
        // Private helper 
        private string FormatVariantInfo(ProductVariant variant)
        {
            var info = new StringBuilder();
            if (!string.IsNullOrEmpty(variant.Color))
            {
                info.Append($"Màu: {variant.Color}");
            }
            if (!string.IsNullOrEmpty(variant.VariantSize))
            {
                if (info.Length > 0) info.Append(", ");
                info.Append($"Size: {variant.VariantSize}");
            }
            return info.Length > 0 ? info.ToString() : "Phiên bản mặc định";
        }
    }
}

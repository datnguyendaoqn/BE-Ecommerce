using BackendEcommerce.Application.Features.Orders.Contracts;
using BackendEcommerce.Application.Features.Products.Contracts;
using BackendEcommerce.Application.Features.Reviews.Contracts;
using BackendEcommerce.Application.Features.Reviews.DTOs;
using BackendEcommerce.Application.Shared.DTOs;
using BackendEcommerce.Infrastructure.Persistence.Models;
using System.Text;

namespace BackendEcommerce.Application.Features.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly IProductRepository _productRepo;
        private readonly IOrderRepository _orderRepo;

        public ReviewService(
             IReviewRepository reviewRepo,
             IProductRepository productRepo,
             IOrderRepository orderRepo) // <-- (BỔ SUNG)
        {
            _reviewRepo = reviewRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo; // <-- (BỔ SUNG)
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
        // === HÀM MỚI (TẠO REVIEW) ===
        public async Task<ApiResponseDTO<ReviewProductResponseDto>> CreateReviewAsync(CreateReviewRequestDto dto, int userId)
        {
            // 1. Validation (Kiểm tra nghiệp vụ)
            var orderItem = await _orderRepo.GetOrderItemForReviewAsync(dto.OrderItemId); // (Cần hàm Repo này)

            // --- SỬA LỖI CONSTRUCTOR ---
            if (orderItem == null)
                return new ApiResponseDTO<ReviewProductResponseDto>
                {
                    IsSuccess = false,
                    Code = 404,
                    Message = "Không tìm thấy món hàng này trong đơn."
                };

            if (orderItem.Order.UserId != userId)
                return new ApiResponseDTO<ReviewProductResponseDto>
                {
                    IsSuccess = false,
                    Code = 403,
                    Message = "Bạn không có quyền review món hàng này."
                };

            if (orderItem.Order.Status.ToLower() != "completed") // Chỉ review đơn đã Hoàn tất
                return new ApiResponseDTO<ReviewProductResponseDto>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = $"Bạn chỉ có thể review đơn hàng ở trạng thái 'Completed'."
                };

            var alreadyReviewed = await _reviewRepo.HasAlreadyReviewedAsync(dto.OrderItemId);
            if (alreadyReviewed)
                return new ApiResponseDTO<ReviewProductResponseDto>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Bạn đã review món hàng này rồi."
                };
            if (orderItem.Variant == null)
            {
                // Dữ liệu rác (Orphaned data). Không thể tạo Review.
                return new ApiResponseDTO<ReviewProductResponseDto>
                { IsSuccess = false, Code = 500, Message = "Lỗi dữ liệu: Món hàng này không tham chiếu đến một phiên bản sản phẩm cụ thể." };
            }
            // --- KẾT THÚC SỬA LỖI ---

            // 2. Tạo Entity Review (Lấy VariantId từ OrderItem)
            var review = new Review
            {
                UserId = userId,
                VariantId = orderItem.ProductVariantId, // Lấy từ OrderItem
                OrderItemId = dto.OrderItemId,
                Rating = dto.Rating,
                CommentText = dto.CommentText,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Lưu vào DB
            await _reviewRepo.AddAsync(review);
            await _reviewRepo.SaveChangesAsync();

            // 4. (QUAN TRỌNG) Tính toán lại Rating cho Product
            // Lấy ProductId thông qua Variant
            var productId = await _productRepo.GetProductIdFromVariantIdAsync(review.VariantId); // (Cần hàm Repo này)
            if (productId.HasValue)
            {
                await RecalculateProductRatingAsync(productId.Value);
            }

            // 5. Trả về DTO (Giống hàm Get)
            // (Tạm map thủ công, hoặc bạn có thể gọi GetReviewById)
            var returnDto = new ReviewProductResponseDto
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.CommentText,
                CreatedAt = review.CreatedAt,
                VariantInfo = FormatVariantInfo(orderItem.Variant) // Tái sử dụng
            };

            return new ApiResponseDTO<ReviewProductResponseDto>
            {
                IsSuccess = true,
                Code = 201, // 201 Created
                Message = "Tạo review thành công.",
                Data = returnDto
            };
        }


        // === HÀM MỚI (PRIVATE HELPER) ===

        /// <summary>
        /// Hàm này chịu trách nhiệm tính toán lại 2 cột trong Product
        /// </summary>
        private async Task RecalculateProductRatingAsync(int productId)
        {
            // 1. Lấy tất cả review của Product này
            // (Tái sử dụng hàm Repo có sẵn của bạn)
            var reviews = await _reviewRepo.GetReviewsForProductAsync(productId);

            // 2. Tính toán
            int newReviewCount = reviews.Count;
            decimal newAverageRating = 0;

            if (newReviewCount > 0)
            {
                // Tính trung bình (chống lỗi chia cho 0)
                newAverageRating = reviews.Average(r => r.Rating);
            }

            // 3. Cập nhật vào bảng Product
            // (Chúng ta cần 1 hàm trong IProductRepository)
            await _productRepo.UpdateProductRatingStatsAsync(productId, newReviewCount, newAverageRating); // (Cần hàm Repo này)
        }




        // Private helper 
        private string FormatVariantInfo(ProductVariant? variant) // Thêm dấu ? (nullable)
        {
            // Nếu Variant đã bị xóa, chúng ta trả về 1 chuỗi an toàn
            if (variant == null)
            {
                // TODO: Bạn có thể lấy thông tin snapshot từ OrderItem
                // (ví dụ: orderItem.VariantName) nếu bạn đã lưu nó
                return "Phiên bản (đã bị xóa)";
            }

            // Nếu không null, chạy logic cũ của bạn
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

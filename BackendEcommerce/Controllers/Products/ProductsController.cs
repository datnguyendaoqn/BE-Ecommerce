using BackendEcommerce.Data;
using BackendEcommerce.DTOs.Product;
using BackendEcommerce.Models;
using BackendEcommerce.Services.Product;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace BackendEcommerce.Controllers.Products
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _service;
        public ProductController(ProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Create(ProductRequest request)
        {
            var result = await _service.CreateAsync(request);
            return result > 0 ? Ok("Inserted") : BadRequest("Insert failed");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return result > 0 ? Ok("Updated") : NotFound("Not found");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return result > 0 ? Ok("Deleted") : NotFound("Not found");
        }
    }
}

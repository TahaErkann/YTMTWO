using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YTM.Core.Entities;
using YTM.Core.Services;

namespace YTM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                _logger.LogInformation("Getting all products");
                var products = await _productService.GetAllProductsAsync();
                
                if (!products.Any())
                {
                    _logger.LogInformation("No products found");
                    return Ok(new List<Product>()); // Boş liste dön
                }
                
                _logger.LogInformation($"Found {products.Count()} products");
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting products: {ex}");
                return StatusCode(500, new { message = "Ürünler yüklenirken bir hata oluştu", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Ürün bulunamadı" });
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting product: {ex}");
                return StatusCode(500, new { message = "Ürün yüklenirken bir hata oluştu", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            try
            {
                await _productService.CreateProductAsync(product);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating product: {ex}");
                return StatusCode(500, new { message = "Ürün oluşturulurken bir hata oluştu", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id, Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    return BadRequest(new { message = "ID uyuşmazlığı" });
                }

                await _productService.UpdateProductAsync(id, product);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product: {ex}");
                return StatusCode(500, new { message = "Ürün güncellenirken bir hata oluştu", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product: {ex}");
                return StatusCode(500, new { message = "Ürün silinirken bir hata oluştu", error = ex.Message });
            }
        }
    }
} 
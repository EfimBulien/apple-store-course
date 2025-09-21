using Microsoft.AspNetCore.Mvc;

namespace TechStoreEll.Web.Controllers;

public class HomeController(IHttpClientFactory httpClientFactory) : Controller
{
    public async Task<IActionResult> Index()
    {
        var client = httpClientFactory.CreateClient("ApiClient");

        try
        {
            var products = await client.GetFromJsonAsync<List<ProductDto>>("api/products");

            if (products == null || products.Count == 0)
            {
                ViewBag.Error = "Нет товаров или не удалось получить ответ от API.";
                return View(new List<ProductDto>());
            }

            return View(products);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Ошибка при загрузке: " + ex.Message;
            return View(new List<ProductDto>());
        }
    }

}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
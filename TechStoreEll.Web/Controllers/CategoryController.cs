using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;
using TechStoreEll.Web.Helpers;

namespace TechStoreEll.Web.Controllers;

[AuthorizeRole("Admin")]
public class CategoryController(
    IGenericRepository<Category> repository,
    ILogger<CategoryController> logger)
    : Controller
{
    // GET: Categories
    public async Task<IActionResult> Index()
    {
        var categories = await repository.GetAllAsync();
        return View(categories);
    }

    // GET: Categories/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var category = await repository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // GET: Categories/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.ParentCategories = (await repository.GetAllAsync())
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToList();
        return View();
    }
    
    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }
        try
        {
            await repository.CreateAsync(category);
            TempData["Success"] = "Категория успешно создана!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании категории");
            ModelState.AddModelError("", "Не удалось создать категорию. Попробуйте позже.");
            return View(category);
        }
    }

    // GET: Categories/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var category = await repository.GetByIdAsync(id);
        ViewBag.ParentCategories = (await repository.GetAllAsync())
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToList();
        
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
        {
            return BadRequest();
        }
        if (!ModelState.IsValid)
        {
            return View(category);
        }
        try
        {
            await repository.UpdateAsync(id, category);
            TempData["Success"] = "Категория успешно обновлена!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении категории с ID {Id}", id);
            ModelState.AddModelError("", "Не удалось обновить категорию. Попробуйте позже.");
            return View(category);
        }
    }

    // GET: Categories/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var category = await repository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await repository.DeleteAsync(id);
            TempData["Success"] = "Категория успешно удалена!";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении категории с ID {Id}", id);
            TempData["Error"] = "Не удалось удалить категорию.";
        }
        return RedirectToAction(nameof(Index));
    }
}

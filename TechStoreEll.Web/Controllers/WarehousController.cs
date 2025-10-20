using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;
using TechStoreEll.Web.Helpers;

namespace TechStoreEll.Web.Controllers;

[AuthorizeRole("Admin")]
public class WarehousController(
    IGenericRepository<Warehouse> repository,
    ILogger<WarehousController> logger)
    : Controller
{
    // GET: Warehouses
    public async Task<IActionResult> Index()
    {
        var warehouses = await repository.GetAllAsync();
        return View(warehouses);
    }

    // GET: Warehouses/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var warehouse = await repository.GetByIdAsync(id);
        if (warehouse == null)
        {
            return NotFound();
        }
        return View(warehouse);
    }

    // GET: Warehouses/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Warehouses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Warehouse warehouse)
    {
        if (!ModelState.IsValid)
        {
            return View(warehouse);
        }

        try
        {
            await repository.CreateAsync(warehouse);
            TempData["Success"] = "Склад успешно создан!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании склада");
            ModelState.AddModelError("", "Не удалось создать склад. Попробуйте позже.");
            return View(warehouse);
        }
    }

    // GET: Warehouses/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var warehouse = await repository.GetByIdAsync(id);
        if (warehouse == null)
        {
            return NotFound();
        }
        return View(warehouse);
    }

    // POST: Warehouses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Warehouse warehouse)
    {
        if (id != warehouse.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(warehouse);
        }

        try
        {
            await repository.UpdateAsync(id, warehouse);
            TempData["Success"] = "Склад успешно обновлён!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении склада с ID {Id}", id);
            ModelState.AddModelError("", "Не удалось обновить склад. Попробуйте позже.");
            return View(warehouse);
        }
    }

    // GET: Warehouses/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var warehouse = await repository.GetByIdAsync(id);
        if (warehouse == null)
        {
            return NotFound();
        }
        return View(warehouse);
    }

    // POST: Warehouses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await repository.DeleteAsync(id);
            TempData["Success"] = "Склад успешно удалён!";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении склада с ID {Id}", id);
            TempData["Error"] = "Не удалось удалить склад.";
        }

        return RedirectToAction(nameof(Index));
    }
}
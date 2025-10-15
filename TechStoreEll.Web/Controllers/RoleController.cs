using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;
using TechStoreEll.Web.Helpers;

namespace TechStoreEll.Web.Controllers;

[AuthorizeRole("Admin")]
public class RoleController(IGenericRepository<Role> repository,
ILogger<RoleController> logger)
: Controller
{
    public async Task<IActionResult> Index()
    {
        var roles = await repository.GetAllAsync();
        return View(roles);
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var role = await repository.GetByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }
        return View(role);
    }
    
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Role role)
    {
        if (!ModelState.IsValid)
        {
            return View(role);
        }

        try
        {
            await repository.CreateAsync(role);
            TempData["Success"] = "Роль успешно создана!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании роли");
            ModelState.AddModelError("", "Не удалось создать роль. Попробуйте позже.");
            return View(role);
        }
    }
    
    public async Task<IActionResult> Edit(int id)
    {
        var role = await repository.GetByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }
        return View(role);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Role role)
    {
        if (id != role.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(role);
        }

        try
        {
            await repository.UpdateAsync(id, role);
            TempData["Success"] = "Роль успешно обновлена!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении роли с ID {Id}", id);
            ModelState.AddModelError("", "Не удалось обновить роль. Попробуйте позже.");
            return View(role);
        }
    }
    
    public async Task<IActionResult> Delete(int id)
    {
        var role = await repository.GetByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }
        return View(role);
    }
    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await repository.DeleteAsync(id);
            TempData["Success"] = "Роль успешно удалена!";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении роли с ID {Id}", id);
            TempData["Error"] = "Не удалось удалить роль.";
        }
        return RedirectToAction(nameof(Index));
    }
}
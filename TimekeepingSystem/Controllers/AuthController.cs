using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimekeepingSystem.Models;
using TimekeepingSystem.Models.ViewModels;

namespace TimekeepingSystem.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("UserId") != null)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == "Admin") return RedirectToAction("Dashboard", "Admin");
            return RedirectToAction("Dashboard", "Worker");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password && u.IsActive);

        if (user == null)
        {
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
            return View(model);
        }

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("FullName", user.FullName);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("Username", user.Username);

        if (user.Role == "Admin")
            return RedirectToAction("Dashboard", "Admin");

        return RedirectToAction("Dashboard", "Worker");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}


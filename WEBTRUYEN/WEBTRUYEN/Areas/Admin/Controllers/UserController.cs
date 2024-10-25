using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WEBTRUYEN.Data.Users;
using WEBTRUYEN.Models;

namespace WEBTRUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = roles.ToList(),
                    AllRoles = _roleManager.Roles.ToList()
                });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRoles(UserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.Roles;

            // Xóa các vai trò cũ
            var removeResult = await _userManager.RemoveFromRolesAsync(user, userRoles);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Không thể xóa vai trò cũ cho người dùng.");
                return View(model);
            }

            // Thêm các vai trò mới
            var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Không thể thêm vai trò mới cho người dùng.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    // Xử lý lỗi nếu không xóa được
                    ModelState.AddModelError("", "Không thể xóa người dùng.");
                    return BadRequest();
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

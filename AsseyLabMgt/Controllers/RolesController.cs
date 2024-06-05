using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using AsseyLabMgt.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsseyLabMgt.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RolesController> _logger;

        public RolesController(UserManager<AppUser> userManager, ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager, ILogger<RolesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var roles = await _context.Roles.ToListAsync();
                return View(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching roles.");
                TempData["ErrorMessage"] = "An error occurred while fetching roles. Please try again later.";
                return View(new List<IdentityRole>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RolesViewModel role)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(role.RoleName))
                {
                    var roleExist = await _roleManager.RoleExistsAsync(role.RoleName);
                    if (!roleExist)
                    {
                        var result = await _roleManager.CreateAsync(new IdentityRole(role.RoleName));
                        if (result.Succeeded)
                        {
                            TempData["SuccessMessage"] = "Role created successfully.";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            _logger.LogWarning("Failed to create role: {RoleName}", role.RoleName);
                            TempData["ErrorMessage"] = "Failed to create role. Please try again.";
                            return View(role);
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Role name already exists.";
                        return View(role);
                    }
                }
            }
            return View(role);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var rolesViewModel = new RolesViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name
            };

            return View(rolesViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, RolesViewModel roleViewModel)
        {
            if (id != roleViewModel.RoleId)
            {
                return BadRequest();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            if (await _roleManager.RoleExistsAsync(roleViewModel.RoleName) && role.Name != roleViewModel.RoleName)
            {
                TempData["ErrorMessage"] = "Role name already exists.";
                return View(roleViewModel);
            }

            role.Name = roleViewModel.RoleName;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Role updated successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                _logger.LogWarning("Failed to update role: {RoleName}", roleViewModel.RoleName);
                TempData["ErrorMessage"] = "Error updating role. Please try again.";
                return View(roleViewModel);
            }
        }
    }
}

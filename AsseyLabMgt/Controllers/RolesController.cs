using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using AsseyLabMgt.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsseyLabMgt.Controllers
{
    public class RolesController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public RolesController(UserManager<AppUser> userManager, ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles.ToListAsync();
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RolesViewModel role)
        {
            if (!string.IsNullOrEmpty(role.RoleName))
            {
                var roleExist = await _roleManager.RoleExistsAsync(role.RoleName);
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole(role.RoleName));
                }
            }
            return RedirectToAction("Index");
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

            var rolesViewmodel = new RolesViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name
            };

            return View(rolesViewmodel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, RolesViewModel roleviewmodel)
        {
            if (id != roleviewmodel.RoleId)
            {
                return BadRequest();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            if (await _roleManager.RoleExistsAsync(roleviewmodel.RoleName) && role.Name != roleviewmodel.RoleName)
            {
                ModelState.AddModelError("", "Role name already exists.");
                return View(roleviewmodel);
            }

            role.Name = roleviewmodel.RoleName;
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Error updating role.");
                return View(roleviewmodel);
            }

            return RedirectToAction("Index");
        }
    }
}


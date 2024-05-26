using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using AsseyLabMgt.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AsseyLabMgt.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            ILogger<UsersController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching list of users.");
                var users = await _context.Users.ToListAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the list of users.");
                TempData["ErrorMessage"] = "An error occurred while fetching the list of users. Please try again later.";
                return View(new List<AppUser>());
            }
        }

        // GET: Users/Create
        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                _logger.LogInformation("Loading create user form.");
                ViewData["Roles"] = new SelectList(_context.Roles, "Id", "Name");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the create user form.");
                TempData["ErrorMessage"] = "An error occurred while loading the create user form. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsersViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Attempting to create a new user with username {UserName}.", model.UserName);
                    var existingUser = await _userManager.FindByNameAsync(model.UserName);
                    if (existingUser != null)
                    {
                        _logger.LogWarning("Username {UserName} already exists.", model.UserName);
                        TempData["ErrorMessage"] = "Username already exists.";
                        ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name");
                        return View(model);
                    }

                    AppUser user = new AppUser
                    {
                        FirstName = model.FirstName,
                        LastName = model.Surname,
                        OtherName = model.OtherName,
                        CreatedOn = DateTime.UtcNow,
                        CreatdBy = "Admin",
                        UserName = model.UserName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        NormalizedEmail = model.Email.ToUpper(),
                        NormalizedUserName = model.UserName.ToUpper(),
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User {UserName} created successfully.", model.UserName);
                        var role = await _roleManager.FindByIdAsync(model.RoleId);
                        if (role != null)
                        {
                            await _userManager.AddToRoleAsync(user, role.Name);
                            TempData["SuccessMessage"] = "User created successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            _logger.LogWarning("Role ID {RoleId} does not exist.", model.RoleId);
                            TempData["ErrorMessage"] = "The specified role does not exist.";
                        }
                    }

                    foreach (var error in result.Errors)
                    {
                        _logger.LogWarning("Error creating user {UserName}: {ErrorDescription}", model.UserName, error.Description);
                        TempData["ErrorMessage"] = error.Description;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while creating the user {UserName}.", model.UserName);
                    TempData["ErrorMessage"] = "An error occurred while creating the user. Please try again later.";
                }
            }

            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name");
            return View(model);
        }

        // GET: Users/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "User ID cannot be null or empty.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Loading edit form for user with id {UserId}.", id);
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new UsersViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    Surname = user.LastName,
                    OtherName = user.OtherName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RoleId = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
                };

                ViewData["Roles"] = new SelectList(_context.Roles, "Id", "Name", model.RoleId);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the edit form for user with id {UserId}.", id);
                TempData["ErrorMessage"] = "An error occurred while loading the edit form. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UsersViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "User ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Attempting to edit user with id {UserId}.", id);
                    var user = await _userManager.FindByIdAsync(id);
                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "User not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    user.FirstName = model.FirstName;
                    user.LastName = model.Surname;
                    user.OtherName = model.OtherName;
                    user.UserName = model.UserName;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    user.NormalizedEmail = model.Email.ToUpper();
                    user.NormalizedUserName = model.UserName.ToUpper();

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        var currentRoles = await _userManager.GetRolesAsync(user);
                        var roleResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        if (roleResult.Succeeded)
                        {
                            var role = await _roleManager.FindByIdAsync(model.RoleId);
                            if (role != null)
                            {
                                await _userManager.AddToRoleAsync(user, role.Name);
                                TempData["SuccessMessage"] = "User updated successfully.";
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                _logger.LogWarning("Role with id {RoleId} does not exist.", model.RoleId);
                                TempData["ErrorMessage"] = "The selected role does not exist.";
                            }
                        }

                        foreach (var error in roleResult.Errors)
                        {
                            _logger.LogWarning("Error updating roles for user with id {UserId}: {ErrorDescription}", id, error.Description);
                            TempData["ErrorMessage"] = error.Description;
                        }
                    }

                    foreach (var error in result.Errors)
                    {
                        _logger.LogWarning("Error editing user with id {UserId}: {ErrorDescription}", id, error.Description);
                        TempData["ErrorMessage"] = error.Description;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while editing the user with id {UserId}.", id);
                    TempData["ErrorMessage"] = "An error occurred while editing the user. Please try again later.";
                }
            }

            ViewData["Roles"] = new SelectList(_context.Roles, "Id", "Name", model.RoleId);
            return View(model);
        }

        // GET: Users/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "User ID cannot be null or empty.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Fetching details for user with id {UserId}.", id);
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new UsersViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    Surname = user.LastName,
                    OtherName = user.OtherName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RoleId = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
                };

                ViewBag.RoleName = model.RoleId ?? "No Role";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching details for user with id {UserId}.", id);
                TempData["ErrorMessage"] = "An error occurred while fetching user details. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Users/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "User ID cannot be null or empty.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Fetching user details for deletion with id {UserId}.", id);
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new UsersViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    Surname = user.LastName,
                    OtherName = user.OtherName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RoleId = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
                };

                ViewBag.RoleName = model.RoleId ?? "No Role";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching details for user with id {UserId} to delete.", id);
                TempData["ErrorMessage"] = "An error occurred while fetching user details for deletion. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "User ID cannot be null or empty.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Attempting to delete user with id {UserId}.", id);
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User with id {UserId} deleted successfully.", id);
                    TempData["SuccessMessage"] = "User deleted successfully.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Error deleting user with id {UserId}: {ErrorDescription}", id, error.Description);
                    TempData["ErrorMessage"] = error.Description;
                }

                // In case of failure, fetch user details again to show the view properly
                var model = new UsersViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    Surname = user.LastName,
                    OtherName = user.OtherName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RoleId = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
                };

                ViewBag.RoleName = model.RoleId ?? "No Role";
                return View("Delete", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user with id {UserId}.", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the user. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

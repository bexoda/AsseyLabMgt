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
                return StatusCode(500, "Internal server error");
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
                return StatusCode(500, "Internal server error");
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
                        ModelState.AddModelError(string.Empty, "Username already exists.");
                        ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name");
                        return View(model);
                    }

                    AppUser user = new AppUser
                    {
                        FirstName = model.FirstName,
                        LastName = model.Surname,
                        OtherName = model.OtherName,
                        CreatedOn = DateTime.UtcNow,
                        CreatdBy = "Ben",
                        UserName = model.UserName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber, // This should now include the international code
                        NormalizedEmail = model.Email.ToUpper(),
                        NormalizedUserName = model.UserName.ToUpper(),
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User {UserName} created successfully.", model.UserName);
                        await _userManager.AddToRoleAsync(user, model.RoleId);
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        _logger.LogWarning("Error creating user {UserName}: {ErrorDescription}", model.UserName, error.Description);
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while creating the user {UserName}.", model.UserName);
                    return StatusCode(500, "Internal server error");
                }
            }

            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "RoleName");
            return View(model);
        }


        // GET: Users/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Edit user request with null or empty id.");
                return BadRequest("User ID cannot be null or empty.");
            }

            try
            {
                _logger.LogInformation("Loading edit form for user with id {UserId}.", id);
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with id {UserId} not found.", id);
                    return NotFound();
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
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UsersViewModel model)
        {
            if (id != model.Id)
            {
                _logger.LogWarning("Edit user request with mismatched user ID.");
                return BadRequest("User ID mismatch.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Attempting to edit user with id {UserId}.", id);
                    var user = await _userManager.FindByIdAsync(id);
                    if (user == null)
                    {
                        _logger.LogWarning("User with id {UserId} not found.", id);
                        return NotFound();
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
                            // Check if the role exists
                            var role = await _roleManager.FindByIdAsync(model.RoleId);
                            if (role != null)
                            {
                                await _userManager.AddToRoleAsync(user, role.Name);
                            }
                            else
                            {
                                _logger.LogWarning("Role with id {RoleId} does not exist.", model.RoleId);
                                ModelState.AddModelError(string.Empty, "The selected role does not exist.");
                            }
                        }

                        _logger.LogInformation("User with id {UserId} edited successfully.", id);
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        _logger.LogWarning("Error editing user with id {UserId}: {ErrorDescription}", id, error.Description);
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while editing the user with id {UserId}.", id);
                    return StatusCode(500, "Internal server error");
                }
            }

            // Repopulate the role dropdown in case of an error
            ViewData["Roles"] = new SelectList(_context.Roles, "Id", "Name", model.RoleId);
            return View(model);
        }



        // GET: Users/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Details request with null or empty id.");
                return BadRequest("User ID cannot be null or empty.");
            }

            try
            {
                _logger.LogInformation("Fetching details for user with id {UserId}.", id);
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with id {UserId} not found.", id);
                    return NotFound();
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

                
                ViewBag.RoleName = model.RoleId != null ? model.RoleId : "No Role";

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching details for user with id {UserId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }


        // GET: Users/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Delete request with null or empty id.");
                return BadRequest("User ID cannot be null or empty.");
            }

            try
            {
                _logger.LogInformation("Fetching user details for deletion with id {UserId}.", id);
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with id {UserId} not found.", id);
                    return NotFound();
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

                // Fetching the role name to display
                var role = await _roleManager.FindByIdAsync(model.RoleId);
                ViewBag.RoleName = role != null ? role.Name : "No Role";

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching details for user with id {UserId} to delete.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("DeleteConfirmed request with null or empty id.");
                return BadRequest("User ID cannot be null or empty.");
            }

            try
            {
                _logger.LogInformation("Attempting to delete user with id {UserId}.", id);
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with id {UserId} not found.", id);
                    return NotFound();
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User with id {UserId} deleted successfully.", id);
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Error deleting user with id {UserId}: {ErrorDescription}", id, error.Description);
                    ModelState.AddModelError(string.Empty, error.Description);
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

                // Fetching the role name to display
                var role = await _roleManager.FindByIdAsync(model.RoleId);
                ViewBag.RoleName = role != null ? role.Name : "No Role";

                return View("Delete", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user with id {UserId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

    }
}

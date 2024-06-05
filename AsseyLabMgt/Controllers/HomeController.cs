using AsseyLabMgt.Data;
using AsseyLabMgt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AsseyLabMgt.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            //if (!User.Identity.IsAuthenticated)
            //{
            //    return this.Redirect("~/identity/account/login");
            //}

            var labRequestsCount = await _context.LabRequests.CountAsync();
            var labResultsCount = await _context.LabResults.CountAsync();
            var usersCount = await _context.Users.CountAsync();

            // Replace Unique Visitors with a relevant metric if needed
            var uniqueVisitorsCount = await _context.Staff.CountAsync();

            var model = new DashboardViewModel
            {
                LabRequestsCount = labRequestsCount,
                LabResultsCount = labResultsCount,
                UsersCount = usersCount,
                UniqueVisitorsCount = uniqueVisitorsCount
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

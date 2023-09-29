using BugBurner.Models;
using BugBurner.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BugBurner.Controllers
{
    public class HomeController : BTBaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBTCompanyService _bTCompanyService;

        public HomeController(ILogger<HomeController> logger, IBTCompanyService bTCompanyService)
        {
            _logger = logger;
            _bTCompanyService = bTCompanyService;
            
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {

            Company company = await _bTCompanyService.GetCompanyInfoAsync(_companyId);

            return View(company);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
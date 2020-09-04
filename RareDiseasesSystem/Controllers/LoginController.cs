using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RareDisease.Data;
using RareDisease.Data.Repository;
using RareDiseasesSystem.Models;

namespace RareDiseasesSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly RareDiseaseDbContext _context;
        private readonly ILogRepository _logRepository;
        public LoginController(ILogger<LoginController> logger, RareDiseaseDbContext context, ILogRepository logRepository)
        {
            _logger = logger;
            _context = context;
            _logRepository = logRepository;
        }
        public IActionResult Index()
        {
            if(!string.IsNullOrEmpty(HttpContext.Request.Query["Logout"]))
            {
                if (!string.IsNullOrEmpty(HttpContext.User.Identity.Name))
                {
                    _logRepository.Add("注销");
                }
                HttpContext.SignOutAsync();
            }   
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Auth([FromBody]LoginModel loginModel)
        {
            try
            {

                var claims = new List<Claim>(){
                                  new Claim(ClaimTypes.Name, loginModel.UserName),new Claim(ClaimTypes.NameIdentifier, loginModel.UserName)
                               };
                //init the identity instances 
                var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "User"));
                //signin 
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30),
                    IsPersistent = false,
                    AllowRefresh = true
                });
                _logRepository.Add("登录", loginModel.UserName);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError("登录失败：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }

        }
        public JsonResult GetUserName()
        {
            try
            {
                var userName = HttpContext.User.Identity.Name;
                return Json(new { success = true, data = userName });
            }
            catch (Exception ex)
            {
                _logger.LogError("获取用户名失败：" + ex.ToString());
                return Json(new { success = false, msg = ex.ToString() });
            }

        }
    }
}
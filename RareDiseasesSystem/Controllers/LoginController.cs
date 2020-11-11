using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NLog;
using RareDisease.Data;
using RareDisease.Data.Model;
using RareDisease.Data.Repository;
using RareDiseasesSystem.Models;

namespace RareDiseasesSystem.Controllers
{
    [EnableCors("_any")]
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly ILogRepository _logRepository;
        private readonly ILocalMemoryCache _localMemoryCache;
        public LoginController(ILogger<LoginController> logger, ILogRepository logRepository, ILocalMemoryCache localMemoryCache)
        {
            _logger = logger;
            _logRepository = logRepository;
            _localMemoryCache = localMemoryCache;
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
            if (!string.IsNullOrEmpty(HttpContext.Request.Query["tk"]))
            {
                LoginModel loginModel = new LoginModel { UserName = "admin", Password = "admin123456" };
                _logRepository.Add("统一平台进入");
                Auth(loginModel);
                HttpContext.Response.Redirect("/Home/Index");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Auth([FromBody]LoginModel loginModel)
        {
            try
            {
                var userList = _localMemoryCache.GetUserList();
                var user = userList.FirstOrDefault(x => x.UserName.ToLower().Equals(loginModel.UserName.ToLower()) && x.Password.ToLower().Equals(loginModel.Password.ToLower()));
                if (user!=null)
                {
                    var claims = new List<Claim>(){
                                  new Claim(ClaimTypes.Name, user.UserName),
                                  new Claim(ClaimTypes.Role,user.Role )
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
                else
                {
                    return Json(new { success = false, msg = "用户名密码错误！" });
                }
             
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
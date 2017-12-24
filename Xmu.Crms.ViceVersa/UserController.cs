using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using System.Linq;
using Xmu.Crms.Web.ViceVersa.VO;

namespace Xmu.Crms.ViceVersa
{
    [Produces("application/json")]
    [Route("")]
    public class UserController : Controller
    {
        private readonly JwtHeader _header;
        private readonly IUserService _userService;
        private readonly ILoginService _loginService;

        public UserController(JwtHeader header, IUserService userService, ILoginService loginService)
        {
            _header = header;
            _userService = userService;
            _loginService = loginService;
        }

        // GET: /me
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            try
            {
                // 调用UserService中的GetUserByUserId方法
                UserInfo userInfo = _userService.GetUserByUserId(User.Id());
                userInfo.Password = null;       //不能明文传输Password到客户端

                //转换为VO对象
                UserVO userVO = userInfo;

                return Json(userVO);
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
        }

        // PUT: /me
        [HttpPut("me")]
        public IActionResult Put(int id, [FromBody] dynamic json)
        {
            School xmu = new School { Id = 12, Name = "厦门大学", Province = "福建", City = "厦门" };
            UserVO dataUser = new UserVO();
            dataUser.Type = json.Type;
            dataUser.Name = json.Name;
            dataUser.Number = json.Number;
            dataUser.Gender = json.Gender;
            dataUser.Title = json.Title;
            dataUser.Avatar = json.Avatar;

            // Update database

            return NoContent();
        }

        // POST: /signin
        [HttpPost("signin")]
        public IActionResult Signin([FromBody] dynamic json)
        {
            UserVO curUser = new UserVO
            {
                Phone = json.Phone,
                //Password = json.Passsword
            };

            // Username & Password Autherization
            // 如果手机号/密码错误
            if (curUser == null)
                return Unauthorized();

            // Get user info from database
            curUser.Id = 3486;
            if (curUser.Phone == "18999999999")
                curUser.Type = "teacher";
            else
                curUser.Type = "student";

            // Create Token
            // Get key from configuration
            // Generate JWT
            string jwt = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJjaWQiOiJPQTAwMDEiLCJpYXQiOjE0ODI2NTcyODQyMjF9.TeJpy936w610Vrrm+c3+RXouCA9k1AX0Bk8qURkYkdo=";
            // Set JWT into cookie
            // Set cookie

            return Ok(curUser);
            // 怎么传JWT？？？
        }

        // POST: /register
        [HttpPost("register")]
        public IActionResult Register([FromBody] dynamic json)
        {
            UserVO curUser = new UserVO
            {
                Phone = json.Phone,
                //Password = json.Password
            };

            // Username & Password Autherization
            // 如果手机号已注册
            if (curUser == null)
                return Unauthorized();

            // Generate user info in database
            curUser.Id = 3486;
            curUser.Type = "unbinded";
            curUser.Name = "";

            // Create Token
            // Get key from configuration
            // Generate JWT
            string jwt = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJjaWQiOiJPQTAwMDEiLCJpYXQiOjE0ODI2NTcyODQyMjF9.TeJpy936w610Vrrm+c3+RXouCA9k1AX0Bk8qURkYkdo=";
            // Set JWT into cookie
            // Set cookie

            return Ok(curUser);
            // 怎么传JWT？？？
        }
    }
}

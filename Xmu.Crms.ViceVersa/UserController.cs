using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using System.Linq;
using Xmu.Crms.Web.ViceVersa.VO;
using System;

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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            try
            {
                // 调用UserService中的GetUserByUserId方法
                UserInfo userInfo = _userService.GetUserByUserId(User.Id());

                //转换为VO对象
                UserVO userVO = userInfo;

                return Json(userVO);
            }
            // 如果用户不存在，返回404
            catch (UserNotFoundException)
            {
                return NotFound(new {msg = "该用户不存在"});
            }
        }

        // PUT: /me
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("me")]
        public IActionResult Put(long id, [FromBody] dynamic json)
        {
            try
            {
                School school = new School { Name = json.School };
                // 修改头像要加吗？？？
                // Avatar = json.Avatar
                UserInfo user = new UserInfo { Name = json.Name, Number = json.Number, School = school, Phone = json.Phone, Email = json.Email };
                if (json.Type == "teacher")
                {
                    user.Type = Shared.Models.Type.Teacher;
                    //修改职称
                    if (json.Title == "教授")
                        user.Title = Title.Professer;
                    else
                        user.Title = Title.Other;
                }
                else if (json.Type == "student")
                {
                    user.Type = Shared.Models.Type.Student;

                    //修改学历
                    if (json.Title == "本科生" || json.Title == "本科")
                        user.Education = Education.Bachelor;
                    else if (json.Title == "研究生")
                        user.Education = Education.Master;
                    else if (json.Title == "博士生" || "博士")
                        user.Education = Education.Doctor;
                    // 其余的填空情况怎么处理？？？
                    else
                        user.Education = null;
                }
                else
                    return BadRequest(new {msg = "用户访问错误：未注册"});        //Type不为学生/老师，说明访问错误
                
                //记录性别
                if (json.Gender == "male")
                    user.Gender = Gender.Male;
                else
                    user.Gender = Gender.Female;

                // Update database
                // 调用UserService中的UpdateUserByUserId方法
                _userService.UpdateUserByUserId(User.Id(), user);

                return NoContent();

            }
            // 如果用户不存在，返回404
            catch (UserNotFoundException)
            {
                return NotFound(new {msg = "该用户不存在"});
            }
        }

        // POST: /signin
        [HttpPost("signin")]
        public IActionResult Signin([FromBody] dynamic json)
        {
            try
            {
                UserInfo curUser = new UserInfo
                {
                    Phone = json.Phone,
                    Password = json.Passsword
                };

                // 调用LoginService的SignInPhone方法
                UserInfo signInUser = _loginService.SignInPhone(curUser);
                HttpContext.SignInAsync(JwtBearerDefaults.AuthenticationScheme, new ClaimsPrincipal());

                // 返回SignInResult对象的Json
                return Json(GenerateJwtAndSignInResult(signInUser));
            }
            // 如果用户不存在，返回404
            catch (UserNotFoundException)
            {
                return NotFound(new {msg = "该用户不存在"});
            }
            // 如果手机号/密码错误，返回401
            catch (PasswordErrorException)
            {
                return Unauthorized(new {msg = "手机号/密码输入错误"});
            }
        }

        // 用来生成正确登陆后的结果
        // 要在signin界面和signup界面中存起来！！！
        private SignInResult GenerateJwtAndSignInResult(UserInfo user)
        {
            SignInResult signInResult = new SignInResult
            {
                Id = user.Id,
                Type = user.Type.ToString().ToLower(),
                Name = user.Name,
                Jwt = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(_header, new JwtPayload(
                        null,
                        null,
                        new[]
                        {
                            new Claim("id", user.Id.ToString()),
                            new Claim("type", user.Type.ToString().ToLower())
                        },
                        null,
                        DateTime.Now.AddDays(7)
                    )))      //jwt的有效时间为7天
            };
            return signInResult;
        }

        // POST: /register
        [HttpPost("register")]
        public IActionResult Register([FromBody] dynamic json)
        {
            try
            {
                UserInfo curUser = new UserInfo
                {
                    Phone = json.Phone,
                    Password = json.Passsword
                };

                // 调用LoginService的SignUpPhone方法
                UserInfo signUpUser = _loginService.SignUpPhone(curUser);

                // 返回SignInResult对象的Json
                return Json(GenerateJwtAndSignInResult(signUpUser));
            }
            // 如果注册用的手机号在数据库中存在，返回409
            catch (PhoneAlreadyExistsException)
            {
                return StatusCode(409, new {msg = "该手机号已注册"});
            }
        }

        public class SignInResult
        {
            public long Id { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string Jwt { get; set; }
        }
    }
}

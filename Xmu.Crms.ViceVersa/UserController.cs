﻿using System.Security.Claims;
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
using Microsoft.AspNetCore.Authentication;

namespace Xmu.Crms.ViceVersa
{
    /// <summary>
    /// API Controller
    /// 存放路由为"/"下的与用户操作相关的方法的控制器
    /// @author Group ViceVersa
    /// </summary>
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

        /// <summary>
        /// GET: /me
        /// </summary>
        /// <returns>返回当前用户的信息</returns>
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
                return NotFound(new {msg = "该用户不存在！"});
            }
        }

        /// <summary>
        /// PUT: /me
        /// </summary>
        /// <param name="json">待修改的用户信息</param>
        /// <returns>返回204，修改用户信息成功</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("me")]
        public IActionResult Put([FromBody] dynamic json)
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
                    if (json.Title != "")
                    {
                        if (json.Title == "教授")
                            user.Title = Title.Professer;
                        else
                            user.Title = Title.Other;
                    }
                    else user.Title = null;
                }
                else if (json.Type == "student")
                {
                    user.Type = Shared.Models.Type.Student;

                    //修改学历
                    if (json.Title != "")
                    {
                        if (json.Title == "本科生" || json.Title == "本科")
                            user.Education = Education.Bachelor;
                        else if (json.Title == "研究生")
                            user.Education = Education.Master;
                        else if (json.Title == "博士生" || json.Title == "博士")
                            user.Education = Education.Doctor;
                        // 其余的填空情况怎么处理？？？
                    }
                    else user.Education=null;
                }
                else
                    return BadRequest(new {msg = "用户访问错误：未注册用户！"});        //Type不为学生/老师，说明访问错误
                
                //记录性别
                if (json.Gender == "male")
                    user.Gender = Gender.Male;
                else
                    user.Gender = Gender.Female;

                // Update database
                // 先去获得这个对象
                UserInfo dataUser = _userService.GetUserByUserId(User.Id());
                
                // 如果是未注册状态
                if (dataUser.Type == Shared.Models.Type.Unbinded)
                {
                    // 调用UserService中的UpdateUserByUserId方法
                    _userService.UpdateUserByUserId(User.Id(), user);

                    dataUser = _userService.GetUserByUserId(User.Id());

                    // 生成正确的Jwt
                    return Json(GenerateJwtAndSignInResult(dataUser));
                }
                else
                {
                    // 调用UserService中的UpdateUserByUserId方法
                    _userService.UpdateUserByUserId(User.Id(), user);

                    return NoContent();
                }
            }
            // 如果用户不存在，返回404
            catch (UserNotFoundException)
            {
                return NotFound(Json(new {msg = "该用户不存在！"}));
            }
            catch (Exception)
            {
                return NotFound(Json(new ErrorMessage{ Msg = "该学校不存在！" }));
            }
        }

        /// <summary>
        /// POST: /signin
        /// </summary>
        /// <param name="json">登录的手机号和密码信息</param>
        /// <returns>返回SignInResult对象的Json</returns>
        [HttpPost("signin")]
        public IActionResult Signin([FromBody] dynamic json)
        {
            try
            {
                UserInfo curUser = new UserInfo
                {
                    Phone = json.Phone,
                    Password = json.Password
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
                return NotFound(new {msg = "该用户不存在！"});
            }
            // 如果手机号/密码错误，返回401
            catch (PasswordErrorException)
            {
                return StatusCode(401, new { msg = "手机号/密码输入错误！" });
            }
        }

        /// <summary>
        /// 用来生成正确登陆后的结果
        /// </summary>
        /// <param name="user">UserInfo对象，为当前用户的信息</param>
        /// <returns>返回SignInResult对象，包括新生成的JWT</returns>
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

        /// <summary>
        /// POST: /register
        /// </summary>
        /// <param name="json">新注册的用户信息</param>
        /// <returns>返回SignInResult对象的Json</returns>
        [HttpPost("register")]
        public IActionResult Register([FromBody] dynamic json)
        {
            try
            {
                UserInfo curUser = new UserInfo
                {
                    Phone = json.Phone,
                    Password = json.Password,
                    Number = json.Number
                };
                // 调用LoginService的SignUpPhone方法
                // 会判断该学生/老师是否在数据库存在了
                UserInfo signUpUser = _loginService.SignUpPhone(curUser);

                // 返回SignInResult对象的Json
                return Json(GenerateJwtAndSignInResult(signUpUser));
            }
            // 如果注册用的手机号在数据库中存在，返回409
            catch (PhoneAlreadyExistsException)
            {
                return StatusCode(409, new {msg = "该手机号已注册！"});
            }
        }

        /// <summary>
        /// SignInResult类，保存JWT中有的三个信息：用户ID、用户类型Type和用户姓名Name
        /// </summary>
        public class SignInResult
        {
            public long Id { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string Jwt { get; set; }
        }
    }
}

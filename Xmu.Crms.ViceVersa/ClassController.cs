﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Web.ViceVersa.VO;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Xmu.Crms.ViceVersa
{
    /// <summary>
    /// API Controller
    /// 存放路由为"/class"下的与班级操作相关的方法的控制器
    /// @author Group ViceVersa
    /// </summary>
    [Produces("application/json")]
    [Route("/class")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ClassController : Controller
    {
        /// <summary>
        /// @author Group ViceVersa
        /// @version 2.00
        /// </summary>
        
        private readonly IClassService _classService;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly IFixGroupService _fixGroupService;

        /// <summary>
        /// 构造函数 services注入
        /// @author Group ViceVersa
        /// </summary>
        /// <param name="classService">classService</param>
        /// <param name="courseService">courseService</param>
        /// <param name="userService">userService</param>
        /// <param name="fixGroupService">fixGroupService</param>
        public ClassController(IClassService classService,ICourseService courseService,IUserService userService,IFixGroupService fixGroupService)
        {
            _classService = classService;
            _courseService = courseService;
            _userService = userService;
            _fixGroupService = fixGroupService;
        }

        /// <summary>
        /// 按courseName/courseTeacher查询班级.
        /// @author Group ViceVersa
        /// </summary>
        /// <param name="courseTeacher">教师姓名</param>
        /// <param name="courseName">课程名称</param>
        /// <returns>list 班级信息</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IClassService.ListClassByUserId(System.Int64)"/>
        // GET: /class?courseName={courseName}&courseTeacher={courseTeacher}
        [HttpGet]
        public IActionResult GetClassListFromQuery([FromQuery]string courseName, [FromQuery]string courseTeacher)
        {
            //列出学生已选课程（班级）
            if (courseName == null && courseTeacher == null)
            {
                IList<ClassInfo> classList = _classService.ListClassByUserId(User.Id());


                List<CourseClassVO> classes = new List<CourseClassVO>();
                foreach (ClassInfo c in classList)
                    classes.Add(new CourseClassVO(c,0));

                // Success
                return Json(classes);
            }

            //学生选课时查询
            else
            {
                try
                {
                    // Fetch selected class list from database
                    IList<ClassInfo> classList = _courseService.ListClassByName(courseName, courseTeacher);

                    //去除已选班级
                    IList<ClassInfo> classSelectedList = _classService.ListClassByUserId(User.Id());
                    foreach (ClassInfo c in classSelectedList)
                        classList.Remove(c);

                    List<CourseClassVO> classes = new List<CourseClassVO>();
                    foreach (ClassInfo c in classList)
                    {
                        //计算该班级共有多少个学生
                        //List<UserInfo> studentList = (List<UserInfo>)_userService.ListUserByClassId(c.Id, null, null);
                        //CourseClassVO courseClassVO = new CourseClassVO(c, studentList.Count);
                        
                        //测试数据
                        CourseClassVO courseClassVO = new CourseClassVO(c, 0);

                        classes.Add(courseClassVO);
                    }

                    // Success
                    return Json(classes);
                }
                catch (CourseNotFoundException) { return NotFound(new {msg = "不存在符合的课程！"}); }
                catch (UserNotFoundException) { return NotFound(new {msg = "用户不存在！"}); }
            }
        }

        /// <summary>
        /// 按classId查询班级.
        /// @author Group ViceVersa
        /// </summary>
        /// <param name="classId">班级Id</param>
        /// <returns>ClassVO 班级信息</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IClassService.GetClassByClassId(System.Int64)"/>
        // GET: /class/{classId}
        [HttpGet("{classId}")]
        public IActionResult GetClass(long classId)
        {
            try
            {
                ClassInfo classinfo = _classService.GetClassByClassId(classId);
                ClassVO classVO = classinfo;
                // Success
                return Json(classVO);
            }
            catch (ClassNotFoundException) { return NotFound(new {msg = "未找到该班级！"}); }
            //classId格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }

        }

        /// <summary>
        /// 按classId修改班级.
        /// @author Group ViceVersa
        /// </summary>
        /// <param name="classId">班级Id</param>
        /// <param name="json">班级信息</param>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IClassService.GetClassByClassId(System.Int64)"/>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IClassService.UpdateClassByClassId(System.Int64,ClassInfo)"/>
        // PUT: /class/{classId}
        [HttpPut("{classId}")]
        public IActionResult PutClass(long classId, [FromBody]dynamic json)
        {
            // Authentication
            // 学生无法修改班级，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return Forbid("1");

            try
            {
                //找到班级
                ClassInfo classInfo = _classService.GetClassByClassId(classId);
                //无法修改他人班级
                if (classInfo.Course.Teacher.Id != User.Id()) return Forbid("2");

                //Change information in database
                classInfo.ClassTime = json.Time;
                classInfo.Site = json.Site;
                classInfo.Name = json.Name;
                if (json.Proportions.Report == "")
                    classInfo.ReportPercentage = null;
                else
                    classInfo.ReportPercentage = json.Proportions.Report;
                if (json.Proportions.Presentation == "")
                    classInfo.PresentationPercentage = null;
                else
                    classInfo.PresentationPercentage = json.Proportions.Presentation;
                if (json.Proportions.A == "")
                    classInfo.ThreePointPercentage = null;
                else
                    classInfo.ThreePointPercentage = json.Proportions.A;
                if (json.Proportions.B == "")
                    classInfo.FourPointPercentage = null;
                else
                    classInfo.FourPointPercentage = json.Proportions.B;
                if (json.Proportions.C == "")
                    classInfo.FivePointPercentage = null;
                else
                    classInfo.FivePointPercentage = json.Proportions.C;



                _classService.UpdateClassByClassId(classId,classInfo);
               
                //Success
                return NoContent();
            }
            catch (ClassNotFoundException) { return NotFound(new {msg = "未找到该班级！"}); }
            //classId格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// 按classId删除班级.
        /// @author Group ViceVersa
        /// </summary>
        /// <param name="classId">班级Id</param>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IClassService.GetClassByClassId(System.Int64)"/>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IClassService.DeleteClassByClassId(System.Int64)"/>
        // DELETE: /class/{classId}
        [HttpDelete("{classId}")]
        public IActionResult DeleteClass(long classId)
        {
            // Authentication
            // 学生无法删除班级，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return Forbid("1");

            try
            {
                //无法删除他人班级
                ClassInfo classInfo = _classService.GetClassByClassId(classId);
               if(classInfo.Course.Teacher.Id!=User.Id()) return Forbid("2");

                _classService.DeleteClassByClassId(classId);
                
                //Success
                return NoContent();
            }
            //If not found, 返回404
            catch (ClassNotFoundException) { return NotFound(new {msg = "未找到该班级！"}); }
            //classId格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }


        /// <summary>
        /// 学生按classId选择课程.
        /// @author Group ViceVersa
        /// </summary>
        /// <param name="classId">班级Id</param>
        /// <param name="json">学生信息</param>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IClassService.InsertCourseSelectionById(System.Int64,System.Int64)"/>
        // Post: /class/{classId}/student
        [HttpPost("{classId}/student")]
        public IActionResult PostStudentUnderClass(long classId, [FromBody]dynamic json)
        {
            //学生无法为他人选课（URL中ID与自身ID不同）

            //if (json.Id!=User.Id()) return Forbid();//不存在
            try
            {
                var classSelectionId= _classService.InsertCourseSelectionById(User.Id(), classId);
                
                //已选过同课程的课
                if (classSelectionId==0)
                   return StatusCode(409);

                // Return class id & student id
                string uri = "/class/" + classId + "/student/" + User.Id();
                return Created(uri, classSelectionId);

            }
            catch (UserNotFoundException){ return NotFound("1"); }
            catch (ClassNotFoundException) { return NotFound("2"); }
        }

        /// <summary>
        /// 学生按classId退选课程.
        /// @author Group ViceVersa
        /// </summary>
        /// <param name="classId">班级Id</param>
        /// <param name="studentId">学生Id</param>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IClassService.DeleteCourseSelectionById(System.Int64,System.Int64)"/>
        // DELETE: /class/{classId}/student/{studentId}
        [HttpDelete("{classId}/student/{studentId}")]
        public IActionResult DeleteStudentUnderClass(long classId, long studentId)
        {
            //学生无法为他人退课（URL中ID与自身ID不同）
            //if (studentId != User.Id()) return Forbid();

            try
            {
                _classService.DeleteCourseSelectionById(User.Id(), classId);

                //Success
                return NoContent();
            }
            catch(UserNotFoundException) { return NotFound("1"); }
            catch (ClassNotFoundException) { return NotFound("2"); }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// 按studentNumber/studentName查询学生.
        /// @author Group ViceVersa
        /// </summary>
        /// <param name="classId">班级Id</param>
        /// <param name="studentNumber">学生学号</param>
        /// <param name="studentName">学生姓名</param>
        /// <returns>studentVO 学生信息</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IUserService.ListUserByClassId(System.Int64,String,String)"/>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IFixGroupService.GetFixedGroupById(System.Int64,System.Int64)"/>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IFixGroupService.ListFixGroupMemberByGroupId(System.Int64)"/>
        // GET: /Class/{classId}/student?numBeginWith={studentNumber}&nameBeginWith={studentName}
        [HttpGet("{classId}/student")]
        public IActionResult GetStudentListUnderClass(long classId,[FromQuery]string studentNumber, [FromQuery]string studentName)
        {
            try
            {
                if (studentNumber == null) studentNumber = "";
                if (studentName == null) studentName = "";
                IList<UserInfo> studentList = _userService.ListUserByClassId(classId, studentNumber, studentName);

                //找到学生所属小组
                FixGroup fixGroup = _fixGroupService.GetFixedGroupById(User.Id(), classId);
                if (fixGroup != null)
                {
                    IList<UserInfo> groupMember = _fixGroupService.ListFixGroupMemberByGroupId(fixGroup.Id);
                    foreach (UserInfo u in groupMember)
                        studentList.Remove(u);
                }

                List<UserVO> studentVO = new List<UserVO>();
                foreach (UserInfo u in studentList)
                    studentVO.Add(u);

                return Json(studentVO);
            }
            catch (ClassNotFoundException) { return NotFound(new { msg = "未找到该班级！" }); }
            catch (UserNotFoundException) { return NotFound(); }
            catch (FixGroupNotFoundException) { return NotFound(); }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            
        }

        /// <summary>
        /// 按classId查询学生固定分组.
        /// @author Group ViceVersa
        /// </summary>
        /// <param name="classId">班级Id</param>
        /// <returns>classGroupVO 固定分组信息</returns>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IFixGroupService.GetFixedGroupById(System.Int64,System.Int64)"/>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IFixGroupService.ListFixGroupMemberByGroupId(System.Int64)"/>
        // GET: /class/{classId}/classgroup
        [HttpGet("{classId}/classgroup")]
        public IActionResult GetClassGroup(long classId)
        {
            //Authentication
            // 教师访问，返回403
            if (User.Type() == Shared.Models.Type.Teacher)
                return Forbid();
            try
            {
                //找到学生所属小组
                FixGroup fixGroup= _fixGroupService.GetFixedGroupById(User.Id(), classId);
                if (fixGroup == null) throw new FixGroupNotFoundException();
                //得到组员
                IList<UserInfo> groupMember = _fixGroupService.ListFixGroupMemberByGroupId(fixGroup.Id);

                ClassGroupVO classGroupVO = new ClassGroupVO(fixGroup, groupMember);
                // Success
                return Json(classGroupVO);
            }
            catch (ClassNotFoundException) { return NotFound(); }
            catch (UserNotFoundException) { return NotFound(); }
            catch (FixGroupNotFoundException) { return NotFound(); }
            //错误的ID格式，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// 添加学生固定分组组员.
        /// @author Group ViceVersa
        /// </summary>
        /// <param name="classId">班级Id</param>
        /// <param name="json">学生信息</param>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IFixGroupService.GetFixedGroupById(System.Int64,System.Int64)"/>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IFixGroupService.InsertStudentIntoGroup(System.Int64,System.Int64)"/>
        // PUT: /class/{classId}/classgroup/add
        [HttpPut("{classId}/classgroup/add")]
        public IActionResult AddMemberIntoClassGroup(long classId, [FromBody]dynamic json)
        {
            try
            {
                //Authentication 学生不是该小组成员
                FixGroup fixGroup = _fixGroupService.GetFixedGroupById(User.Id(), classId);
                if (fixGroup.LeaderId != User.Id())
                    return Forbid();

                long studentId = json.StudentId;
                FixGroup studentGroup = _fixGroupService.GetFixedGroupById(studentId, classId);
                if(studentGroup!=null) return Forbid();

                // Add student in classgroup database
                var addId = _fixGroupService.InsertStudentIntoGroup(studentId, fixGroup.Id);
               
                // Success
                return NoContent();
            }
            catch (FixGroupNotFoundException) { return NotFound("1"); }
            catch (UserNotFoundException) { return NotFound("2"); }
            catch (InvalidOperationException) { return StatusCode(409); }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// 删除学生固定分组组员.
        /// @author Group ViceVersa
        /// </summary>
        /// <param name="classId">班级Id</param>
        /// <param name="json">学生信息</param>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IFixGroupService.GetFixedGroupById(System.Int64,System.Int64)"/>
        /// <seealso cref="M:Xmu.Crms.Shared.Service.IFixGroupService.DeleteFixGroupUserById(System.Int64,System.Int64)"/>
        // PUT: /class/{classId}/classgroup/remove
        [HttpPut("{classId}/classgroup/remove")]
        public IActionResult RemoveMemberIntoClassGroup(long classId, [FromBody]dynamic json)
        {
            try
            {
                //Authentication  权限不足（不是该小组的成员/组长）
                FixGroup fixGroup = _fixGroupService.GetFixedGroupById(User.Id(), classId);
                if (fixGroup.LeaderId!=User.Id() || json.StudentId == User.Id())
                    return Forbid();

                long studentId = json.StudentId;
                // Remove student from this classgroup database
                _fixGroupService.DeleteFixGroupUserById(fixGroup.Id, studentId);
               
                // Success
                return NoContent();
            }
            catch (FixGroupNotFoundException) { return NotFound("1"); }
            catch (UserNotFoundException) { return NotFound("2"); }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }
    }
}

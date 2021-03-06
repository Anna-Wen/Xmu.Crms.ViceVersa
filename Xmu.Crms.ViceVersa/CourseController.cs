﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Xmu.Crms.Shared.Exceptions;
using System;
using Xmu.Crms.Web.ViceVersa.VO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Xmu.Crms.ViceVersa
{
    /// <summary>
    /// API Controller
    /// 存放路由为"/course"下的与课程操作相关的方法的控制器
    /// @author Group ViceVersa
    /// </summary>
    [Produces("application/json")]
    [Route("/course")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CourseController : Controller
    {
        private readonly ICourseService _iCourseService;
        private readonly IClassService _iClassService;
        private readonly IUserService _iUserService;
        private readonly ISeminarService _iSeminarService;
        private readonly ITopicService _iTopicService;
        private readonly IGradeService _iGradeService;
        private readonly ISeminarGroupService _iSeminarGroupService;

        public CourseController(ICourseService iCourseService, IClassService iClassService, IUserService iUserService, ISeminarService iSeminarService, ITopicService iTopicService, IGradeService iGradeService, ISeminarGroupService iSeminarGroupService)
        {
            _iCourseService = iCourseService;
            _iClassService = iClassService;
            _iUserService = iUserService;
            _iSeminarService = iSeminarService;
            _iTopicService = iTopicService;
            _iGradeService = iGradeService;
            _iSeminarGroupService = iSeminarGroupService;
        }

        /// <summary>
        /// GET: /course
        /// </summary>
        /// <returns>与当前用户相关的课程列表</returns>
        [HttpGet]
        public IActionResult GetCourses()
        {
            List<CourseVO> courseVOList = null;
            try
            {
                //根据教师id获得其教的所有课程
                IList<Course> courseList = _iCourseService.ListCourseByUserId(User.Id());
                //针对每个课程查询具体信息
                courseVOList = new List<CourseVO>();
                foreach (var i in courseList)
                {
                    //根据课程获得该课程的所有班级
                    List<ClassInfo> classList = (List<ClassInfo>)_iClassService.ListClassByCourseId(i.Id);
                    int numClass = classList.Count;
                    //计算该课程共有多少个学生
                    int numStudent = 0;
                    foreach (var j in classList)
                    {
                        //将每个班的学生相加
                        List<UserInfo> studentList = (List<UserInfo>)_iUserService.ListUserByClassId(j.Id, null, null);
                        numStudent += studentList.Count;
                    }
                    CourseVO courseVO = new CourseVO(i, numClass, numStudent);
                    courseVOList.Add(courseVO);
                }

                return Json(courseVOList);
            }
            catch (UserNotFoundException)
            {
                return NotFound(new {msg = "该用户不存在！"});
            }catch(ArgumentException)
            {
                return BadRequest();
            }
            //catch
            //{
            //}
        }

        /// <summary>
        /// POST: /course
        /// </summary>
        /// <param name="json">传入课程相关信息</param>
        /// <returns>创建成功，返回课程的ID</returns>
        [HttpPost]
        public IActionResult PostCourse([FromBody]dynamic json)
        {
            // Authentication
            // 学生无法创建课程，返回403
            if(User.Type() == Shared.Models.Type.Student)
                return StatusCode(403, new { msg = "学生无法创建课程！" });

            Course newCourse = null;
            string uri = null;
            try
            {
                //Get information from json
                newCourse = new Course { Name = json.Name, Description = json.Description, StartDate = json.StartTime, EndDate = json.EndTime, ReportPercentage = json.Proportions.Report, PresentationPercentage = json.Proportions.Presentation, FivePointPercentage = json.Proportions.C, FourPointPercentage = json.Proportions.B, ThreePointPercentage = json.Proportions.A };

                // Store course information in server and generate a id for this new course
                long courseId = _iCourseService.InsertCourseByUserId(User.Id(), newCourse);

                // Return course id
                uri = "/course/" + courseId;

                return Created(uri, newCourse);
            }
            catch (UserNotFoundException)
            {
                return NotFound(new {msg = "该用户不存在！"});
            }
            //catch (Exception)
            //{
            //}
        }

        /// <summary>
        /// GET: /course/{courseId}
        /// </summary>
        /// <param name="courseId">课程ID</param>
        /// <returns>根据ID获取的课程信息</returns>
        [HttpGet("{courseId}")]
        public IActionResult GetCourseByCourseId(long courseId)
        {
            // Fetch data from database
            CourseVO courseVO = null;
            try
            {
                Course course = _iCourseService.GetCourseByCourseId(courseId);
                courseVO = new CourseVO(course);

                return Json(courseVO);
            }
            //If not found, 返回404
            catch (CourseNotFoundException)
            {
                return NotFound(new {msg = "未找到该课程！"});
            }
            //courseId格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
            //catch (Exception)
            //{
            //}
        }

        /// <summary>
        /// PUT: /course/{courseId}
        /// </summary>
        /// <param name="courseId">课程ID</param>
        /// <param name="json">修改后的课程信息</param>
        /// <returns>返回204表示修改成功</returns>
        [HttpPut("{courseId}")]
        public IActionResult PutCourseByCourseId(long courseId, [FromBody]dynamic json)
        {
            // Authentication
            // 学生无法修改课程，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return StatusCode(403, new { msg = "学生无法修改课程！" });

            try
            {
                //Get information from json
                Course editedCourse = new Course { Name = json.Name, Description = json.Description, StartDate = json.StartTime, EndDate = json.EndTime };

                //Change information in database
                //怎样验证该userId有权限修改该courseId???
                _iCourseService.UpdateCourseByCourseId(courseId, editedCourse);

                //Success
                return NoContent();
            }
            //If not found, 返回404
            catch (CourseNotFoundException)
            {
                return NotFound(new {msg = "未找到该课程！"});
            }
            //courseId格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
            //catch (Exception)
            //{
            //}
        }

        /// <summary>
        /// DELETE: /course/{courseId}
        /// </summary>
        /// <param name="courseId">需要删除的课程ID</param>
        /// <returns>返回204，课程删除成功</returns>
        [HttpDelete("{courseId}")]
        public IActionResult DeleteCourseByCourseId(long courseId)
        {
            // Authentication
            // 学生无法删除课程，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return StatusCode(403, new { msg = "学生无法删除课程！" });
            long id = courseId;//================
            try
            {
                //Delete course from database
                //怎样验证该userId有权限删除该courseId???
                _iCourseService.DeleteCourseByCourseId(courseId);

                //Success
                return NoContent();
            }
            // 未找到课程，返回404
            catch (CourseNotFoundException)
            {
                return NotFound(new {msg = "未找到该课程！"});
            }
            //错误的ID格式，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
            //catch (Exception)
            //{
            //}
        }

        /// <summary>
        /// GET: /course/{courseId}/class
        /// </summary>
        /// <param name="courseId">需获取班级的课程ID</param>
        /// <returns>按照课程ID获取的班级列表</returns>
        [HttpGet("{courseId}/class")]
        public IActionResult GetClassList(long courseId)
        {
            try
            {
                // Fetch data from database
                // 调用ClassService的方法
                IList<ClassInfo> classList = _iClassService.ListClassByCourseId(courseId);

                List<ClassVO> classes = new List<ClassVO>();
                foreach (ClassInfo i in classList)
                    classes.Add(i);

                // Success
                return Json(classes);
            }
            // 未找到课程，返回404
            catch (CourseNotFoundException)
            {
                return NotFound(new {msg = "未找到该课程！"});
            }
            //错误的ID格式，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }

        /// <summary>
        /// POST: /course/{courseId}/class
        /// </summary>
        /// <param name="courseId">待创建班级的课程ID</param>
        /// <param name="json">新创建班级的信息</param>
        /// <returns>新创建班级的ID</returns>
        [HttpPost("{courseId}/class")]
        public IActionResult PostNewClass(long courseId, [FromBody]dynamic json)
        {
            // Authentication
            // 学生无法创建班级，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return StatusCode(403, new { msg = "学生无法创建班级！" });

            try
            {
                //Get information from json
                ClassInfo newClass = new ClassInfo
                {
                    Name = json.Name,
                    Site = json.Site,
                    ClassTime = json.Time
                };
                if (json.Proportions != null && json.Proportions.Report != "" && json.Proportions.Presentation != "" && json.Proportions.C != "" && json.Proportions.B != "" && json.Proportions.A != "")
                {
                    newClass.ReportPercentage = json.Proportions.Report;
                    newClass.PresentationPercentage = json.Proportions.Presentation;
                    newClass.FivePointPercentage = json.Proportions.C;
                    newClass.FourPointPercentage = json.Proportions.B;
                    newClass.ThreePointPercentage = json.Proportions.A;
                }
                else
                {
                    newClass.ReportPercentage = 0;
                    newClass.PresentationPercentage = 0;
                    newClass.FivePointPercentage = 0;
                    newClass.FourPointPercentage = 0;
                    newClass.ThreePointPercentage = 0;
                }

                // Store class information in server and generate a id for this new class
                long newClassId = _iCourseService.InsertClassById(courseId, newClass);

                // Return class id
                string uri = "/class/" + newClassId;
                return Created(uri, new { id = newClassId });
            }
            // 未找到课程，返回404
            catch (CourseNotFoundException)
            {
                return NotFound(new {msg = "未找到该课程！"});
            }
            // 错误的ID格式，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
            catch(InvalidOperationException)
            {
                return StatusCode(409);
            }
        }

        /// <summary>
        /// GET: /course/{courseId}/seminar?embedGrade=false
        /// </summary>
        /// <param name="courseId">需获取讨论课的课程ID</param>
        /// <param name="embedGrade">是否包含分数（仅小程序需要）</param>
        /// <returns>按照课程ID获取讨论课详情列表</returns>
        [HttpGet("{courseId}/seminar")]
        public IActionResult GetSeminarList(long courseId, [FromQuery]bool embedGrade = false)
        {
            // Authentication
            // 若教师设置embedGrade为true，返回400
            if (User.Type() == Shared.Models.Type.Teacher && embedGrade == true)
                return BadRequest(new { msg = "教师错误的访问！" });

            try
            {
                // Fetch data from database
                IList<Seminar> seminarList = _iSeminarService.ListSeminarByCourseId(courseId);

                List<SeminarVO> seminars = new List<SeminarVO>();
                foreach (Seminar i in seminarList)
                    if (embedGrade == false)
                    {
                        seminars.Add(i);
                    }

                // Success
                return Json(seminars);
            }
            // 未找到课程，返回404
            catch (CourseNotFoundException)
            {
                return NotFound(new {msg = "未找到该课程！"});
            }
            // 错误的ID格式，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }

        /// <summary>
        /// POST: /course/{courseId}/seminar
        /// </summary>
        /// <param name="courseId">待创建讨论课的课程ID</param>
        /// <param name="json">创建的讨论课信息</param>
        /// <returns>新创建的讨论课ID</returns>
        [HttpPost("{courseId}/seminar")]
        public IActionResult PostNewSeminar(long courseId, [FromBody]dynamic json)
        {
            // Authentication
            // 学生无法创建讨论课，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return StatusCode(403, new { msg = "学生无法创建讨论课！" });

            try
            {
                //Convert.ToDateTime(json.StartTime);
                string startTime = json.StartTime;
                startTime= startTime.Replace("-", "/");
                string endTime = json.EndTime;
                endTime= endTime.Replace("-", "/");

                //Get information from json
                // 所以获得的组内限制和组内人数上限存在哪个实体里？？？
                Seminar newSeminar;
                if (json.GroupingMethod == "fixed")
                    newSeminar = new Seminar { Name = json.Name, Description = json.Description, IsFixed = true, StartTime = Convert.ToDateTime(startTime), EndTime = Convert.ToDateTime(endTime) };
                else
                    newSeminar = new Seminar { Name = json.Name, Description = json.Description, IsFixed = false, StartTime = Convert.ToDateTime(startTime), EndTime = Convert.ToDateTime(endTime) };

                // Store seminar information in server and generate a id for this new seminar
                long newSeminarId = _iSeminarService.InsertSeminarByCourseId(courseId, newSeminar);

                // Return seminar id
                string uri = "/seminar/" + newSeminarId;
                return Created(uri, newSeminar);
            }
            // 未找到课程，返回404
            catch (CourseNotFoundException)
            {
                return NotFound(new { msg = "未找到该课程！" });
            }
            // 错误的ID格式，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }

        /// <summary>
        /// GET: /course/{courseId}/grade
        /// </summary>
        /// <param name="courseId">需获取成绩的课程ID</param>
        /// <returns>按照课程ID获取学生的所有讨论课成绩</returns>
        [HttpGet("{courseId}/grade")]
        public IActionResult GetStudentGradeUnderAllSeminar(long courseId)
        {
            try
            {
                // Fetch data from database
                IList<SeminarGroup> seminarGroupList = _iGradeService.ListSeminarGradeByCourseId(User.Id(), courseId);

                // 转换为SeminarGradeDetailVO的List对象
                List<SeminarGradeDetailVO> seminarGrades = new List<SeminarGradeDetailVO>();
                foreach(SeminarGroup i in seminarGroupList)
                {
                    // 为了获得GroupName，要先建一个GroupVO实体
                    GroupVO g = i;

                    //获取Members
                    IList<UserInfo> memberList = _iSeminarGroupService.ListSeminarGroupMemberByGroupId(i.Id);
                    List<UserVO> members = new List<UserVO>();
                    foreach (UserInfo u in memberList)
                        members.Add(u);
                    g.Members = members;

                    //获取Topics和PresentationGrade
                    IList<SeminarGroupTopic> seminarGroupTopicList = _iTopicService.ListSeminarGroupTopicByGroupId(i.Id);
                    List<TopicVO> topics = new List<TopicVO>();
                    List<int> pGrades = new List<int>();
                    foreach (SeminarGroupTopic sgt in seminarGroupTopicList)
                    {
                        topics.Add(sgt.Topic);
                        if (sgt.PresentationGrade != null)
                            pGrades.Add((int)sgt.PresentationGrade);
                        else pGrades.Add(0);
                    }
                    g.Topics = topics;
                    g.Grade.PresentationGrade = pGrades;

                    //获取Name
                    g.GetName();

                    SeminarGradeDetailVO seminarGradeDetailVO = new SeminarGradeDetailVO { SeminarName = i.Seminar.Name, GroupName = g.Name, LeaderName = i.Leader.Name, PresentationGrade = i.PresentationGrade==null?0:(int)i.PresentationGrade, ReportGrade = i.ReportGrade == null ? 0 : (int)i.ReportGrade, Grade = i.FinalGrade == null ? 0 : (int)i.FinalGrade };
                    seminarGrades.Add(seminarGradeDetailVO);
                }

                // Success
                return Json(seminarGrades);
            }
            // 未找到课程，返回404
            catch (CourseNotFoundException)
            {
                return NotFound(new { msg = "未找到该课程！" });
            }
            // 错误的ID格式，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }
    }
}

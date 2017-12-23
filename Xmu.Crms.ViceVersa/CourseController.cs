using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Xmu.Crms.Shared.Exceptions;
using System;
using Xmu.Crms.Web.ViceVersa.VO;

namespace Xmu.Crms.ViceVersa
{
    [Produces("application/json")]
    [Route("/Course")]
    public class CourseController : Controller
    {
        private readonly ICourseService _iCourseService;
        private readonly IClassService _iClassService;

        public CourseController(ICourseService iCourseService, IClassService iClassService)
        {
            _iClassService = iClassService;
            _iCourseService = iCourseService;
        }


        // GET: /course
        [HttpGet]
        public IActionResult GetCourses()
        {
            try
            {
                //根据教师id获得其教的所有课程
                IList<Course> courseList = _iCourseService.ListCourseByUserId(1);  //JWT???
                //针对每个课程查询具体信息
                List<CourseVO> courseVOList = new List<CourseVO>();
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
                        numStudent += 0;   //???UserService???还是新加???
                    }
                    CourseVO courseVO = new CourseVO(i, numClass, numStudent);
                    courseVOList.Add(courseVO);
                }
                    ////根据课程id获得
                    //List<Course> courses = new List<Course>
                    //{
                    //    new Course { Id = 1, Name = "OOAD", NumClass = 3, NumStudent = 60, StartTime = "1/9/2017", EndTime = "1/1/2018" },
                    //    new Course { Id = 2, Name = "J2EE", NumClass = 1, NumStudent = 60, StartTime = "1/9/2017", EndTime = "1/1/2018" }
                    //};

                return Json(courseVOList);
            }catch(CourseNotFoundException ec)
            {
                return NotFound();
            }catch(ClassNotFoundException ecl)
            {
                return NotFound();
            }
            catch
            {
                throw;
            }
        }


        // POST: /course
        [HttpPost]
        public IActionResult PostCourse([FromBody]dynamic json)
        {
            //Authentication
            //When user's permission denied
            //if(false)
            //  return Forbid();

            Course newCourse = null;
            string uri=null;
            try
            {
                //Get information from json
                //if (json.Proportions != null && json.Proportions.Report != "" && json.Proportions.Presentation != "" && json.Proportions.C != "" && json.Proportions.B != "" && json.Proportions.A != "")
                //{
                //    course = new Course { ReportPercentage = json.Proportions.Report, PresentationPercentage = json.Proportions.Presentation, FivePointPercentage = json.Proportions.C, FourPointPercentage = json.Proportions.B, ThreePointPercentage = json.Proportions.A };
                //}
                newCourse = new Course { Name = json.Name, Description = json.Description, StartDate = json.StartTime, EndDate = json.EndTime, ReportPercentage = json.Proportions.Report, PresentationPercentage = json.Proportions.Presentation, FivePointPercentage = json.Proportions.C, FourPointPercentage = json.Proportions.B, ThreePointPercentage = json.Proportions.A };

                //get teacher's id
                long userId = 1;   //JWT
                // Store course information in server and generate a id for this new course
                long courseId = _iCourseService.InsertCourseByUserId(userId, newCourse);

                // Return course id
                uri = "/course/" + courseId;
            }catch(UserNotFoundException eu)
            {
                return NotFound();
            }
            catch (ArgumentException ea)
            {
                return BadRequest();
            }
            catch (Exception e)
            {
            }
            return Created(uri, newCourse);
        }

        //// POST: /course
        //[HttpPost]
        //public IActionResult PostCourse([FromBody]dynamic json)
        //{
        //    //Authentication
        //    //When user's permission denied
        //    //if(false)
        //    //  return Forbid();

        //    //Get information from json
        //    GradeProportion proportions = null;
        //    if (json.Proportions != null && json.Proportions.Report != "" && json.Proportions.Presentation != "" && json.Proportions.C != "" && json.Proportions.B != "" && json.Proportions.A != "")
        //    {
        //        proportions = new GradeProportion { Report = json.Proportions.Report, Presentation = json.Proportions.Presentation, C = json.Proportions.C, B = json.Proportions.B, A = json.Proportions.A };
        //    }
        //    Course newCourse = new Course { Name = json.Name, Description = json.Description, StartTime = json.StartTime, EndTime = json.EndTime, Proportions = proportions };

        //    // Store course information in server and generate a id for this new course
        //    newCourse.Id = 23;

        //    // Return course id
        //    string uri = "/course/" + newCourse.Id;
        //    return Created(uri, newCourse);
        //}


        // GET: /course/{courseId}
        [HttpGet("{courseId}")]
        public IActionResult GetCourseByCourseId(int courseId)
        {
            // Fetch data from database
            CourseVO courseVO = null;
            try
            {
                Course course = _iCourseService.GetCourseByCourseId(courseId);
                courseVO = new CourseVO(course);
            }
            catch (CourseNotFoundException ec)
            {
                //If not found
                return NotFound();
            }catch(ArgumentException ea)
            {
                //courseId格式错误
                return BadRequest();
            }catch(Exception e)
            {
            }
            return Json(courseVO);
        }

        //// GET: /course/{courseId}
        //[HttpGet("{courseId}")]
        //public IActionResult GetCourseByCourseId(int courseId)
        //{
        //    // Fetch data from database
        //    CourseDetail course = new CourseDetail { Id = 1, Name = "OOAD", Description = "面向对象分析与设计", StartTime = "2017-09-01", EndTime = "2018-01-01", TeacherName = "邱明", TeacherEmail = "mingqiu@xmu.edu.cn" };
        //    //If not found
        //    if (course == null)
        //        return NotFound();

        //    return Json(course);
        //}

        // PUT: /course/{courseId}
        [HttpPut("{courseId}")]
        public IActionResult PutCourseByCourseId(int courseId, [FromBody]dynamic json)
        {
            //Authentication
            //When user's permission denied
            //if(false)
            //return Forbid();

            //Get information from json
            Course editedCourse = new Course { Name = json.Name, Description = json.Description, StartDate = json.StartTime, EndDate = json.EndTime };

            //Change information in database
            _iCourseService.UpdateCourseByCourseId(courseId,editedCourse);
            //Success
            return NoContent();
        }

        // DELETE: /course/{courseId}
        [HttpDelete("{courseId}")]
        public IActionResult DeleteCourseByCourseId(int courseId)
        {
            //Authentication
            //When user's permission denied
            //if(false)
            //  return Forbid();

            try
            {
                //Delete course from database
                _iCourseService.DeleteCourseByCourseId(courseId);
            }catch(CourseNotFoundException ec)
            {
                return NotFound();
            }catch(ArgumentException ea)
            {
                return BadRequest();
            }catch(Exception e)
            {
            }
            //Success
            return NoContent();
        }

        //        // GET: /course/{courseId}/class
        //        [HttpGet("{courseId}/class")]
        //        public IActionResult GetClassList(int courseId)
        //        {
        //            // Fetch data from database
        //            List<Class> classes = new List<Class>
        //            {
        //                new Class { Id = 45, Name = "周三1-2节" },
        //                new Class { Id = 48, Name = "周三3-4节" }
        //            };

        //            // If not found
        //            if (classes == null)
        //                return NotFound();

        //            // Success
        //            return Json(classes);
        //        }

        //        // POST: /course/{courseId}/class
        //        [HttpPost("{courseId}/class")]
        //        public IActionResult PostNewClass(int courseId, [FromBody]dynamic json)
        //        {
        //            //Authentication
        //            //When user's permission denied
        //            //if(false)
        //            //return Forbid();

        //            //Get information from json
        //            GradeProportion proportions = null;
        //            if (json.Proportions != null && json.Proportions.Report != "" && json.Proportions.Presentation != "" && json.Proportions.C != "" && json.Proportions.B != "" && json.Proportions.A != "")
        //            {
        //                proportions = new GradeProportion { Report = json.Proportions.Report, Presentation = json.Proportions.Presentation, C = json.Proportions.C, B = json.Proportions.B, A = json.Proportions.A };
        //            }
        //            Class newClass = new Class { Name = json.Name, Site = json.Site, Time = json.Time, Roster = json.Roster, Proportions = proportions };

        //            // Store class information in server and generate a id for this new class
        //            newClass.Id = 45;

        //            // Return class id
        //            string uri = "/class/" + newClass.Id;
        //            return Created(uri, newClass);
        //        }

        //        // GET: /course/{courseId}/seminar?embedGrade=false
        //        [HttpGet("{courseId}/seminar")]
        //        public IActionResult GetSeminarList(int courseId, [FromQuery]bool embedGrade = false)
        //        {
        //            //Authentication
        //            //When user's permission denied
        //            if(embedGrade != false)
        //                return BadRequest();

        //            // Fetch data from database
        //            List<Seminar> seminars = new List<Seminar>();
        //            if (embedGrade == false)
        //            {
        //                seminars.Add(new Seminar { Id = 45, Name = "界面原型设计", Description = "界面原型设计", GroupingMethod = "fixed", StartTime = "25/09/2017", EndTime = "09/10/2017" });
        //                seminars.Add(new Seminar { Id = 48, Name = "概要设计", Description = "模型层与数据库设计", GroupingMethod = "fixed", StartTime = "10/10/2017", EndTime = "24/10/2017" });
        //            }

        //            // If not found
        //            if (seminars == null)
        //                return NotFound();

        //            // Success
        //            return Json(seminars);
        //        }

        //        // POST: /course/{courseId}/seminar
        //        [HttpPost("{courseId}/seminar")]
        //        public IActionResult PostNewSeminar(int courseId, [FromBody]dynamic json)
        //        {
        //            //Authentication
        //            //When user's permission denied
        //            //if(false)
        //            //return Forbid();

        //            //Get information from json
        //            GradeProportion proportions = null;
        //            if (json.Proportions != null && json.Proportions.Report != "" && json.Proportions.Presentation != "" && json.Proportions.C != "" && json.Proportions.B != "" && json.Proportions.A != "")
        //            {
        //                proportions = new GradeProportion { Report = json.Proportions.Report, Presentation = json.Proportions.Presentation, C = json.Proportions.C, B = json.Proportions.B, A = json.Proportions.A };
        //            }
        //            Seminar newSeminar = new Seminar { Name = json.Name, Description = json.Description, GroupingMethod = json.GroupingMethod, StartTime = json.StartTime, EndTime = json.EndTime, Proportions = proportions };

        //            // Store seminar information in server and generate a id for this new seminar
        //            newSeminar.Id = 32;

        //            // Return seminar id
        //            string uri = "/seminar/" + newSeminar.Id;
        //            return Created(uri, newSeminar);
        //        }

        //        // GET: /course/{courseId}/grade
        //        [HttpGet("{courseId}/grade")]
        //        public IActionResult GetStudentGradeUnderAllSeminar(int courseId)
        //        {
        //            // Fetch data from database
        //            List<SeminarGradeDetail> seminarGrades = new List<SeminarGradeDetail>
        //            {
        //                new SeminarGradeDetail { SeminarName = "需求分析", GroupName = "3A2", LeaderName = "张三", PresentationGrade = 4, ReportGrade = 4, Grade = 4 },
        //                new SeminarGradeDetail { SeminarName = "界面原型设计", GroupName = "3A3", LeaderName = "张三", PresentationGrade = 5, ReportGrade = 5, Grade = 5 }
        //            };

        //            // If not found
        //            if (seminarGrades == null)
        //                return NotFound();

        //            // Success
        //            return Json(seminarGrades);
        //        }

    }
}

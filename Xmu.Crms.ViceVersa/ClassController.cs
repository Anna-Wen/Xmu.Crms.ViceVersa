using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Web.ViceVersa.VO;
using System;

namespace Xmu.Crms.ViceVersa
{
    [Produces("application/json")]
    [Route("/class")]
    public class ClassController : Controller
    {
        private readonly IClassService _classService;
        private readonly ICourseService _courseService;

           public ClassController(IClassService classService)
        {
            _classService = classService;
        }

        // GET: /class?courseName={courseName}&courseTeacher={courseTeacher}
        [HttpGet]
        public IActionResult GetClassListFromQuery([FromQuery]string courseName, [FromQuery]string courseTeacher)
        {
            try
            {
                // Fetch selected class list from database
                IList<ClassInfo> classList = _courseService.ListClassByName(courseName, courseTeacher);
                
                // Success
                return Json(classList);
            }
            catch(CourseNotFoundException) { return NotFound(); }
            catch (UserNotFoundException) { return NotFound(); }
        }

        // GET: /class/{classId}
        [HttpGet("{classId}")]
        public IActionResult GetClass(int classId)
        {
            try
            {
                ClassInfo classinfo = _classService.GetClassByClassId(classId);
                ClassVO classVO = classinfo;
                // Success
                return Json(classVO);
            }
            catch (ClassNotFoundException) { return NotFound(); }
            //classId格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }

        }

        // PUT: /class/{classId}
        [HttpPut("{classId}")]
        public IActionResult PutClass(int classId, [FromBody]dynamic json)
        {
            // Authentication
            // 学生无法修改班级，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return Forbid();

            try
            {
                //找到班级
                ClassInfo classInfo = _classService.GetClassByClassId(classId);
                //无法修改他人班级
                if (classInfo.Course.Teacher.Id != User.Id()) return Forbid();

                //Change information in database
                classInfo.ClassTime = json.Time;
                classInfo.Site = json.Site;
                classInfo.Name = json.Name;
                classInfo.ReportPercentage = json.Proportions.Report;
                classInfo.PresentationPercentage = json.Proportions.Presentation;
                classInfo.ThreePointPercentage = json.Proportions.C;
                classInfo.FourPointPercentage = json.Proportions.B;
                classInfo.FivePointPercentage = json.Proportions.A;

                _classService.UpdateClassByClassId(classId,classInfo);
               
                //Success
                return NoContent();
            }
            catch (ClassNotFoundException) { return NotFound(); }
            //classId格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        // DELETE: /class/{classId}
        [HttpDelete("{classId}")]
        public IActionResult DeleteClass(long classId)
        {
            // Authentication
            // 学生无法删除班级，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return Forbid();

            try
            {
                //无法删除他人班级
                ClassInfo classInfo = _classService.GetClassByClassId(classId);
               if(classInfo.Course.Teacher.Id!=User.Id()) return Forbid();

                _classService.DeleteClassByClassId(classId);
                
                //Success
                return NoContent();
            }
            //If not found, 返回404
            catch (ClassNotFoundException) { return NotFound(); }
            //classId格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }



        // Post: /class/{classId}/student
        [HttpPost("{classId}/student")]
        public IActionResult PostStudentUnderClass(int classId, [FromBody]dynamic json)
        {
            try
            {
                //Authentication
                //When user's permission denied
                //if(false)
                //  return Forbid();

                // Get information from json
                //Student newStudentInClass = new Student { Id = json.Id };

                // Judge and store class-student information in server

                // If already select another class under the same course

                var userId = 8;
                _classService.InsertCourseSelectionById(userId, classId);
                return Json(1);
                //  return Conflict(); 

                // Return class id & student id
                //string uri = "/class/" + classId + "/student/" + newStudentInClass.Id;
                //return Created(uri, newStudentInClass);
            }
            catch (UserNotFoundException){ return NotFound(); }
            catch (ClassNotFoundException) { return NotFound(); }
        }


        // DELETE: /class/{classId}/student/{studentId}
        [HttpDelete("{classId}/student/{studentId}")]
        public IActionResult DeleteStudentUnderClass(int classId, int studentId)
        {
            try
            {
                //Authentication
                //When user's permission denied
                //if(false)
                //  return Forbid();

                _classService.DeleteCourseSelectionById(studentId, classId);

                //Success
                return Json(1);
            }
            catch(UserNotFoundException) { return NotFound(); }
            catch (ClassNotFoundException) { return NotFound(); }
        }

        //        // GET: /class/{classId}/classgroup
        //        [HttpGet("{classId}/classgroup")]
        //        public IActionResult GetClassGroup(int classId)
        //        {
        //            //Authentication
        //            //When user's permission denied
        //            //if(false)
        //            //  return Forbid();

        //            // Fetch class group from database
        //            Student leader = new Student { Id = 233, Name = "张三", Number = "24320152202333" };
        //            Student s1 = new Student { Id = 248, Name = "李四", Number = "24320152202345" };
        //            Student s2 = new Student { Id = 256, Name = "王五", Number = "24320152202356" };
        //            List<Student> memberList = new List<Student>
        //            {
        //                s1,
        //                s2
        //            };

        //            ClassGroup classGroup = new ClassGroup { Leader = leader, Members = memberList};

        //            // Success
        //            return Json(classGroup);
        //        }

        //        // PUT: /class/{classId}/classgroup/add
        //        [HttpPut("{classId}/classgroup/add")]
        //        public IActionResult AddMemberIntoClassGroup(int classId, [FromBody]dynamic json)
        //        {
        //            //Authentication
        //            //When user's permission denied (not in this group / not leader)
        //            //if(false)
        //            //  return Forbid();

        //            // Get information from json
        //            Student newStudentInClassGroup = new Student { Id = json.Id };

        //            // Add student in classgroup database

        //            // If already in group
        //            //  return Conflict();

        //            // Success
        //            return NoContent(); 
        //        }

        //        // PUT: /class/{classId}/classgroup/remove
        //        [HttpPut("{classId}/classgroup/remove")]
        //        public IActionResult RemoveMemberIntoClassGroup(int classId, [FromBody]dynamic json)
        //        {
        //            //Authentication
        //            //When user's permission denied (not in this group / not leader)
        //            //if(false)
        //            //  return Forbid();

        //            // Get information from json
        //            Student newStudentInClassGroup = new Student { Id = json.Id };

        //            // Remove student from this classgroup database

        //            // If student is not in this group
        //            //  return Conflict();

        //            // Success
        //            return NoContent();         
        //        }
    }
}

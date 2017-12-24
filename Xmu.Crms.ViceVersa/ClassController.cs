using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Xmu.Crms.Shared.Exceptions;

namespace Xmu.Crms.ViceVersa
{
    [Produces("application/json")]
    [Route("/class")]
    public class ClassController : Controller
    {
        private readonly IClassService _classService;
     

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
                IList<ClassInfo> classList = _classService.ListClassByName(courseName, courseTeacher);
                
                // Success
                return Json(classList);
            }
            catch(ClassNotFoundException) { return NotFound(); }
        }

        // GET: /class/{classId}
        [HttpGet("{classId}")]
        public IActionResult GetClass(int classId)
        {
            return DeleteStudentUnderClass(3, 91);
            //try
            //{
            //    ClassInfo classinfo = _classService.GetClassByClassId(classId);
            //    // Success
            //    return Json(classinfo);
            //}
            //catch (ClassNotFoundException) { return NotFound(); }
        }

        //        // PUT: /class/{classId}
        //        [HttpPut("{classId}")]
        //        public IActionResult PutClass(int classId, [FromBody]dynamic json)
        //        {
        //            //Authentication
        //            //When user's permission denied
        //            //if(false)
        //            //  return Forbid();

        //            //Get information from json
        //            GradeProportion proportions = null;
        //            if (json.Proportions != null && json.Proportions.Report != "" && json.Proportions.Presentation != "" && json.Proportions.C != "" && json.Proportions.B != "" && json.Proportions.A != "")
        //            {
        //                proportions = new GradeProportion { Report = json.Proportions.Report, Presentation = json.Proportions.Presentation, C = json.Proportions.C, B = json.Proportions.B, A = json.Proportions.A };
        //            }
        //            Class editedClass = new Class { Name = json.Name, Site = json.Site, Time = json.Time, Proportions = proportions };

        //            //Change information in database
        //            //if not found
        //            //    return NotFound();

        //            //Success
        //            return NoContent();
        //        }

        // DELETE: /class/{classId}
        [HttpDelete("{classId}")]
        public IActionResult DeleteClass(long classId)
        {
            try
            {
                //Authentication
                //When user's permission denied
                //if(false)
                //  return Forbid();

                _classService.DeleteClassByClassId(classId);


                //Success
                return NoContent();
            }
            catch (ClassNotFoundException) { return NotFound(); }
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

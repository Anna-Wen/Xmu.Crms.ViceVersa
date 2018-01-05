using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Xmu.Crms.Web.ViceVersa.VO;
using System;

namespace Xmu.Crms.ViceVersa
{

    [Produces("application/json")]
    [Route("/school")]

    public class SchoolController : Controller
    {
        public readonly ISchoolService _iSchoolService;
        public SchoolController(ISchoolService iSchoolService)
        {
            _iSchoolService = iSchoolService;
        }

        // GET: /school?city={city}
        [HttpGet]
        public IActionResult GetSchoolViaCity([FromQuery]string city)
        {
            //如果还没有选择城市，则不能触发这个controller选项
            try
            {
                // Fetch selected class list from database
                IList<School> schoolList = _iSchoolService.ListSchoolByCity(city);
                List<SchoolVO> schools = new List<SchoolVO>();
                foreach (var school in schoolList)
                {
                    schools.Add(school);
                }

                // Success
                return Json(schools);
            }
            //错误的ID格式，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }
        
        // POST: /school
        [HttpPost]
        public IActionResult PostNewSchool([FromBody]dynamic json)
        {
            try
            {
                // Get information from json
                School newSchool = new School { Name = json.Name, Province = json.Province, City = json.City };
              
                ////Insert new school
                long schoolId = _iSchoolService.InsertSchool(newSchool);
                // Return school id
                string uri = "/school/" + schoolId;
                return Created(uri, newSchool);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }
    }
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Xmu.Crms.Web.ViceVersa.VO;
using System;

namespace Xmu.Crms.ViceVersa
{
    /// <summary>
    /// API Controllet
    /// 存放路由为"/school"下的与学校信息操作相关的方法控制器
    /// @author Group ViceVersa
    /// </summary>
    [Produces("application/json")]
    [Route("/school")]
    public class SchoolController : Controller
    {
        public readonly ISchoolService _iSchoolService;
        public SchoolController(ISchoolService iSchoolService)
        {
            _iSchoolService = iSchoolService;
        }

        /// <summary>
        /// GET: /school?city={city}
        /// </summary>
        /// <param name="city">城市名称</param>
        /// <returns>返回该城市的学校相关信息</returns>
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
        
        /// <summary>
        /// POST: /school
        /// </summary>
        /// <param name="json">创建学校的信息</param>
        /// <returns>学校id</returns>
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

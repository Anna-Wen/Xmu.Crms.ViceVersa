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
                // If school not found
                if (schoolList == null)
                    return NotFound();

                // Success
                return Json(schools);
            }
            //错误的ID格式，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }
        
        // POST: /school
        [HttpPost]
        public IActionResult PostNewSchool([FromBody]dynamic json)
        {
            // Get information from json
            SchoolVO newSchool = new SchoolVO { Name = json.Name, Province = json.Province, City = json.City };

            // Judge and store school information in server
            newSchool.Id = 38;
            
            // If already has a school with the same name
            //  return Conflict(); 

            // Return school id
            string uri = "/school/" + newSchool.Id;
            return Created(uri, newSchool);
        }

        // GET: /school/province
        [HttpGet("province")]
        public IActionResult GetProvince()
        {
            // Fetch province list from database
            List<string> provinceList = new List<string> { "北京", "天津", "上海", "重庆", "河北", "山西", "辽宁", "吉林", "黑龙江", "江苏", "浙江", "安徽", "福建", "江西", "山东", "河南", "湖北", "湖南", "广东", "海南", "四川", "贵州", "云南", "陕西", "甘肃", "青海", "台湾", "内蒙古自治区", "广西壮族自治区", "西藏自治区", "宁夏回族自治区", "新疆维吾尔自治区", "香港特别行政区", "澳门特别行政区" };

            // Success
            return Json(provinceList);
        }

        // GET: /school/city?province={province}
        [HttpGet("city")]
        public IActionResult GetCityViaProvince([FromQuery]string province)
        {
            // Fetch city list from database
            List<string> cityList = new List<string>();
            if (province == "北京")
                cityList.Add("北京");
            if (province == "广东")
            {
                cityList.Add("广州");
                cityList.Add("深圳");
                cityList.Add("珠海");
            }
            if (province == "福建")
            {
                cityList.Add("福州");
                cityList.Add("厦门");
                cityList.Add("漳州");
            }

            // If province not found
            if (cityList == null)
                return NotFound();

            // Success
            return Json(cityList);
        }

    }
}

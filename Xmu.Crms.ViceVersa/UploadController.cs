using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.ViceVersa
{
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("/upload")]
    public class UploadController : Controller
    {
        private IHostingEnvironment _hostingEnv;
        private readonly ISeminarGroupService _iSeminarGroupService;
        private readonly IUserService _iUserService;
        private readonly IClassService _iClassService;

        private readonly CrmsContext _db;

        public UploadController(IHostingEnvironment env, ISeminarGroupService iSeminarGroupService, IUserService iUserService, IClassService iClassService, CrmsContext db)
        {
            this._hostingEnv = env;
            _iSeminarGroupService = iSeminarGroupService;
            _iUserService = iUserService;
            _iClassService = iClassService;

            _db = db;
            //if (string.IsNullOrWhiteSpace(env.WebRootPath))
            //{
            //    hostingEnv.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            //}
        }

        // POST: /upload/avatar
        [HttpPost("avatar")]
        public IActionResult UploadAvatar([FromBody]dynamic json)
        {
            // Store file in server
            string file = json.File;
            // If encounter a problem (if pics's format is wrong)
            if (file == null)
                return BadRequest();

            // Store avatar file url in user info
            string uri = "/avatar/3486.png";

            // Return avatar file url
            return Created(uri, file);
            // 该返回file吗？还是什么object？
        }

        // /upload/report?seminarId={seminarId}
        [HttpPost("report")]
        public IActionResult UploadReport([FromQuery] long seminarId)
        {
            // 先存入服务端的wwwroot下的report文件夹
            var files = Request.Form.Files;
            string fileUrl = null;
            foreach (var file in files)
            {
                var filename = ContentDispositionHeaderValue
                                .Parse(file.ContentDisposition)
                                .Name
                                .Trim('"');
                //filename = hostingEnv.WebRootPath + $@"\report\{filename}";
                fileUrl = Path.Combine(_hostingEnv.WebRootPath + "/report/" + filename);
                using (FileStream fs = System.IO.File.Create(fileUrl))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
            }

            // 插入当前用户所在的SeminarGroup表中的Report属性
            try
            {
                // 先查SeminarGroup
                SeminarGroup seminarGroup = _iSeminarGroupService.GetSeminarGroupById(seminarId, User.Id());
                // 改SeminarGroup表中的Report属性
                _db.SeminarGroup.Attach(seminarGroup);
                seminarGroup.Report = fileUrl;
                _db.SaveChanges();
            }
            catch (GroupNotFoundException)
            {
                return NotFound();
            }

            return Created(fileUrl, files);
        }

        // /upload/roster?classId={classId}
        [HttpPost("roster")]
        public IActionResult ImportXlsx([FromQuery] long classId)
        {
            IFormFileCollection files = Request.Form.Files;
            string fileUrl = null;
            foreach (IFormFile xlsxFile in files)
            {
                // 先存到wwwroot/roster中
                var filename = ContentDispositionHeaderValue
                                .Parse(xlsxFile.ContentDisposition)
                                .Name
                                .Trim('"');
                fileUrl = Path.Combine(_hostingEnv.WebRootPath + "/roster/" + filename);
                try
                {
                    using (FileStream fs = System.IO.File.Create(fileUrl))
                    {
                        xlsxFile.CopyTo(fs);
                        fs.Flush();
                    }

                    // 创建FileInfo对象
                    FileInfo file = new FileInfo(fileUrl);

                    // 
                    using (ExcelPackage package = new ExcelPackage(file))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                        int rowCount = worksheet.Dimension.Rows;

                        // 先找老师的UserInfo
                        UserInfo teacher = _iUserService.GetUserByUserId(User.Id());

                        for (int row = 1; row <= rowCount; row++)
                        {
                            UserInfo user = new UserInfo { Number = worksheet.Cells[row, 1].Value.ToString(), Name = worksheet.Cells[row, 2].Value.ToString(), Type = Shared.Models.Type.Student, School = teacher.School };

                            // 先去找数据库中是否有这样一个User
                            try
                            {
                                user = _iUserService.GetUserByUserNumber(user.Number);
                            }
                            // 如果不存在这样一个User，就在数据库中插入一条UserInfo
                            catch (UserNotFoundException)
                            {
                                _db.UserInfo.Add(user);
                                _db.SaveChanges();
                            }

                            // 建立这个班级和这个学生之间的关系
                            _iClassService.InsertCourseSelectionById(user.Id, classId);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }

            }

            return NoContent();
        }
    }
}

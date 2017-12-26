using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http.Headers;
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
        private IHostingEnvironment hostingEnv;
        private readonly ISeminarGroupService _iSeminarGroupService;

        private readonly CrmsContext _db;

        public UploadController(IHostingEnvironment env, ISeminarGroupService iSeminarGroupService, CrmsContext db)
        {
            this.hostingEnv = env;
            _iSeminarGroupService = iSeminarGroupService;

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
            long size = 0;
            var files = Request.Form.Files;
            string fileUrl = null;
            foreach (var file in files)
            {
                var filename = ContentDispositionHeaderValue
                                .Parse(file.ContentDisposition)
                                .Name
                                .Trim('"');
                //filename = hostingEnv.WebRootPath + $@"\report\{filename}";
                fileUrl = Path.Combine(hostingEnv.WebRootPath + "/report/" + filename);
                size += file.Length;
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
            catch(GroupNotFoundException)
            {
                return NotFound();
            }

            return Created(fileUrl, files);
        }

    }
}

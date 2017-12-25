using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Service;
using Xmu.Crms.Web.ViceVersa.VO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Xmu.Crms.ViceVersa
{
    [Produces("application/json")]
    [Route("/seminar")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SeminarController : Controller
    {
        private readonly ISeminarService _iSeminarService;
        private readonly ITopicService _iTopicService;
        private readonly ISeminarGroupService _iSeminarGroupService;

        public SeminarController(ISeminarService iSeminarService, ITopicService iTopicService, ISeminarGroupService iSeminarGroupService)
        {
            _iSeminarService = iSeminarService;
            _iTopicService = iTopicService;
            _iSeminarGroupService = iSeminarGroupService;
        }

        // GET: /seminar/{seminarId}
        [HttpGet("{seminarId}", Name = "Get")]
        public IActionResult GetSeminar(int seminarId)
        {
            try
            {
                // Fetch data from database
                // 获得Seminar的基本信息
                Seminar seminarData = _iSeminarService.GetSeminarBySeminarId(seminarId);
                // 获得Seminar下的所有Topic信息
                IList<Topic> topicList = _iTopicService.ListTopicBySeminarId(seminarId);

                // 生成SeminarVO对象
                SeminarVO seminar = seminarData;
                // 如何计算剩余组数？？？
                List<TopicVO> topics = new List<TopicVO>();
                foreach (Topic i in topicList)
                    topics.Add(i);
                //将topics放入SeminarVO对象中
                seminar.Topics = topics;

                // Success
                return Json(seminar);
            }
            //If seminar not found, 返回404
            catch (SeminarNotFoundException)
            {
                return NotFound();
            }
            //seminarId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        // PUT: /seminar/{seminarId}
        [HttpPut("{seminarId}")]
        public IActionResult PutSeminar(int seminarId, [FromBody]dynamic json)
        {
            // Authentication
            // 学生无法修改讨论课，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return Forbid();

            try
            {
                // Get information from json
                Seminar editedSeminar = new Seminar { Id = seminarId, Name = json.Name, Description = json.Description, StartTime = Convert.ToDateTime(json.StartTime), EndTime = Convert.ToDateTime(json.EndTime) };
                // 这里groupingMethod返回的是fixed还是固定分组？？？
                if (json.GroupingMethod == "fixed" || json.GroupingMethod == "固定分组")
                    editedSeminar.IsFixed = true;
                else if (json.GroupingMethod == "random" || json.GroupingMethod == "随机分组")
                    editedSeminar.IsFixed = false;

                //Change information in database
                _iSeminarService.UpdateSeminarBySeminarId(seminarId, editedSeminar);

                //Success
                return NoContent();
            }
            //If seminar not found, 返回404
            catch (SeminarNotFoundException)
            {
                return NotFound();
            }
            //seminarId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        // DELETE: /seminar/{seminarId}
        [HttpDelete("{seminarId}")]
        public IActionResult DeleteSeminar(int seminarId)
        {
            // Authentication
            // 学生无法删除讨论课，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return Forbid();

            try
            {
                // Delete seminar from database
                // 怎么完成无法删除他人讨论课的权限判断？？？
                _iSeminarService.DeleteSeminarBySeminarId(seminarId);

                // Success
                return NoContent();
            }
            //If seminar not found, 返回404
            catch (SeminarNotFoundException)
            {
                return NotFound();
            }
            //seminarId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        //// GET: /seminar/{seminarId}/detail
        //[HttpGet("{seminarId}/detail")]
        //public IActionResult GetSeminarDetails(int seminarId)
        //{
            //// Fetch data from database
            //SeminarDetail seminarDetail = new SeminarDetail { Id = 1, Name = "概要设计", StartTime = "10/10/2017", EndTime = "24/10/2017", Site = "海韵教学楼201", TeacherName = "邱明", TeacherEmail = "mingqiu@xmu.edu.cn" };

            //// If seminar not found
            //if (seminarDetail == null)
            //    return NotFound();

            //// Success
            //return Json(seminarDetail);
        //}

        // GET: /seminar/{seminarId}/topic
        [HttpGet("{seminarId}/topic")]
        public IActionResult GetSeminarTopics(int seminarId)
        {
            try
            {
                // Fetch data from database
                // 获得Seminar下的所有Topic信息
                IList<Topic> topicList = _iTopicService.ListTopicBySeminarId(seminarId);

                // 生成List<TopicVO>对象
                // 如何计算剩余组数？？？
                List<TopicVO> topics = new List<TopicVO>();
                foreach (Topic i in topicList)
                    topics.Add(i);

                // Success
                return Json(topics);
            }
            //If seminar not found, 返回404
            catch (SeminarNotFoundException)
            {
                return NotFound();
            }
            //seminarId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        // POST: /seminar/{seminarId}/topic
        [HttpPost("{seminarId}/topic")]
        public IActionResult PostNewTopicUnderSeminar(int seminarId, [FromBody]dynamic json)
        {
            // Authentication
            // 学生无法创建话题，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return Forbid();

            try
            {
                //Get information from json
                Topic newTopic = new Topic();
                if (json.GroupLimit != "" && json.GroupMemberLimit != "")
                {
                    newTopic = new Topic { Serial = json.Serial, Name = json.Name, Description = json.Description, GroupNumberLimit = json.GroupLimit, GroupStudentLimit = json.GroupMemberLimit };
                }

                // Store topic information in server and generate a id for this new topic
                // 怎么完成无法在他人讨论课创建话题的权限判断？？？
                long newTopicId = _iTopicService.InsertTopicBySeminarId(seminarId, newTopic);

                // Return topic id
                string uri = "/topic/" + newTopicId;
                return Created(uri, newTopic);
            }
            //If seminar not found, 返回404
            catch (SeminarNotFoundException)
            {
                return NotFound();
            }
            //seminarId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        // GET: /seminar/{seminarId}/group?classId={classId}
        [HttpGet("{seminarId}/group")]
        public IActionResult GetSeminarGroups(int seminarId, [FromQuery]bool gradeable, [FromQuery]int classId)
        {
            try
            {
                // 先只实现了不分班级小组和不分用户是否可打分小组的方法

                // Fetch data from database
                IList<SeminarGroup> seminarGroupList = _iSeminarGroupService.ListSeminarGroupBySeminarId(seminarId);

                // 转换成VO对象
                List<GroupVO> groups = new List<GroupVO>();
                foreach (SeminarGroup sg in seminarGroupList)
                    groups.Add(sg);

                // Success
                return Json(groups);
            }
            //If seminar not found, 返回404
            catch (SeminarNotFoundException)
            {
                return NotFound();
            }
            //seminarId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest();
            }
            // Fetch data from database
            //Student l1 = new Student { Id = 233, Name = "张三", Number = "24320152202333" };
            //Student s1 = new Student { Id = 248, Name = "李四", Number = "24320152202345" };
            //Student s2 = new Student { Id = 256, Name = "王五", Number = "24320152202356" };
            //Student l2 = new Student { Id = 233, Name = "小红", Number = "24320152202456" };
            //Student l3 = new Student { Id = 233, Name = "小紫", Number = "24320152202478" };
            //Student l4 = new Student { Id = 233, Name = "小明", Number = "24320152202499" };
            //List<Student> memberList = new List<Student> { s1, s2 };
            //List<Topic> t1 = new List<Topic> { new Topic { Id = 257, Serial = "A", Name = "领域模型与模块", Description = "Domain model 与模块划分", GroupLimit = 5, GroupMemberLimit = 6, GroupLeft = 2 } };
            //List<Topic> t2 = new List<Topic> { new Topic { Id = 258, Serial = "B", Name = "数据库设计", Description = "XXXXXXXX", GroupLimit = 5, GroupMemberLimit = 5, GroupLeft = 1 } };
            //List<Topic> t3 = new List<Topic> { new Topic { Id = 257, Serial = "A", Name = "领域模型与模块", Description = "Domain model 与模块划分", GroupLimit = 5, GroupMemberLimit = 6, GroupLeft = 2 },
            //                                        new Topic { Id = 258, Serial = "B", Name = "数据库设计", Description = "XXXXXXXX", GroupLimit = 5, GroupMemberLimit = 5, GroupLeft = 1 } };
            //SeminarGrade sg1 = new SeminarGrade { PresentationGrade = new List<int> { 5 }, ReportGrade = 5, Grade = 5 };
            //SeminarGrade sg2 = new SeminarGrade { PresentationGrade = new List<int> { 4 }, ReportGrade = 4, Grade = 4 };
            //SeminarGrade sg3 = new SeminarGrade { PresentationGrade = new List<int> { 5 } };
            //SeminarGrade sg4 = new SeminarGrade { PresentationGrade = new List<int> { 4 } };
            //SeminarGrade sg5 = new SeminarGrade { PresentationGrade = new List<int> { 5, 4 } };

            //List<Group> groups = new List<Group> {
            //    new Group { Id = 28, Name = "1-A-1", Leader = l1, Members = memberList, Topics = t1, Report = "/report/233.pdf", Grade = sg1 },
            //    new Group { Id = 29, Name = "1-A-2", Leader = s1, Members = memberList, Topics = t1, Report = "/report/233.pdf", Grade = sg2 },
            //    new Group { Id = 30, Name = "1-B-1", Leader = s2, Members = memberList, Topics = t2, Report = "/report/233.pdf", Grade = sg3 },
            //    new Group { Id = 31, Name = "2-A-1", Leader = l2, Members = memberList, Topics = t1, Report = "/report/233.pdf", Grade = sg4 },
            //    new Group { Id = 32, Name = "2-A-2", Leader = l3, Members = memberList, Topics = t1, Report = "", Grade = sg3 },
            //    new Group { Id = 33, Name = "3-A-1", Leader = l4, Members = memberList, Topics = t3, Report = "/report/233.pdf", Grade = sg5 }
            //};
        }

        // GET: /seminar/{seminarId}/group/my
        [HttpGet("{seminarId}/group/my")]
        public IActionResult GetMySeminarGroup(int seminarId)
        {
            ////Authentication
            ////When user's permission denied
            ////if(false)
            ////  return Forbid();

            //// Fetch data from database
            //Student leader = new Student { Id = 233, Name = "张三", Number = "24320152202333" };
            //Student s1 = new Student { Id = 248, Name = "李四", Number = "24320152202345" };
            //Student s2 = new Student { Id = 256, Name = "王五", Number = "24320152202356" };
            //List<Student> memberList = new List<Student> { s1, s2 };
            //List<Topic> topics = new List<Topic> { new Topic { Id = 257, Serial = "A", Name = "领域模型与模块", Description = "Domain model 与模块划分", GroupLimit = 5, GroupMemberLimit = 6, GroupLeft = 2 } };
            //Group myGroup = new Group { Id = 28, Name = "1-A-1", Leader = leader, Members = memberList, Topics = topics };

            //// If seminar not found or no groups yet
            //if (myGroup == null)
            //    return NotFound();

            //// Success
            //return Json(myGroup);
            throw new NotImplementedException();
        }

    }
}

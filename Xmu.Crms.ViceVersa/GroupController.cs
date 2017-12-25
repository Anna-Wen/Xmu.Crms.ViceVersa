using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using System;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Web.ViceVersa.VO;

namespace Xmu.Crms.ViceVersa
{
    [Produces("application/json")]
    [Route("/group")]
    public class GroupController : Controller
    {
        public readonly ISeminarGroupService _iSeminarGroupService;
        public readonly IFixGroupService _iFixGroupService;
        public GroupController(ISeminarGroupService iSeminarGroupService, IFixGroupService iFixGroupService)
        {
            _iSeminarGroupService = iSeminarGroupService;
            _iFixGroupService = iFixGroupService;
        }
        // GET: /group/{groupId}?embedTopics={true|false}&embedGrade={true|false}
        [HttpGet("{groupId}")]
        public IActionResult GetGroupByGroupId(long groupId, [FromQuery]bool embedTopics, [FromQuery]bool embedGrade)
        {
            // Fetch selected data from database
            try
            {
                GroupVO group= _iSeminarGroupService.GetSeminarGroupByGroupId(groupId);
                return Json(group);
            }
            catch(GroupNotFoundException)
            {
                return NotFound();
            }
            catch(ArgumentException)
            {
                return BadRequest();
            }
            // Success 
        }

        //下面的增加和移除人员都是针对固定分组的
        // PUT: /group/{groupId}/add
        [HttpPut("{groupId}/add")]
        public IActionResult AddGroupMember(long groupId, [FromBody]dynamic json)
        {
            try//无法抛出权限不足的异常
            {
                long userId = json.id;//json里面存的userId是叫这个名字吗
                //检验该学生是否已经在该小组当中
                IList<UserInfo> fixGroupMembers = _iFixGroupService.ListFixGroupMemberByGroupId(groupId);
                foreach(var member in fixGroupMembers)
                {
                    if(member.Id==json.id)
                    {
                        return StatusCode(409);
                    }
                }
                //向该小组插入学生信息
                _iFixGroupService.InsertStudentIntoGroup(userId, groupId);
                return Ok();
            }
            catch(InvalidOperationException)//待添加学生已经在小组里了(如果service写了，我的判断删掉)
            {
                return StatusCode(409);
            }
            catch(UserNotFoundException)
            {
                return NotFound();
            }
            catch (GroupNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        // PUT: /group/{groupId}/remove
        [HttpPut("{groupId}/remove")]
        public IActionResult RemoveGroupMember(int groupId, [FromBody]dynamic json)
        {
            try//注意看service有没有判断存不存在该学生
            {
                long userId = json.id;
                _iFixGroupService.DeleteFixGroupUserById(groupId,userId);
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
            catch (GroupNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            return Ok();
        }

        // POST: /group/{groupId}/topic
        [HttpPost("{groupId}/topic")]
        public IActionResult PostNewTopic(int groupId, [FromBody]dynamic json)
        {
            //Authentication
            //When user's permission denied (not leader)
            //if(false)
            //  return Forbid();

            // Get information from json
            Topic topicSelectedByGroup = new Topic { Id = json.Id };

            // Judge and store group-topic information in server

            //if (topicSelectedByGroup.GroupLeft == 0)
            //    return BadRequest();

            // If group not existed
            //  return NotFound(); 

            // Return group id & topic id
            string uri = "/group/" + groupId + "/topic/" + topicSelectedByGroup.Id;
            return Created(uri, topicSelectedByGroup);
        }

        // DELETE: /group/{groupId}/topic/{topicId}
        [HttpPost("{groupId}/topic/{topicId}")]
        public IActionResult DeleteTopic(int groupId, int topicId, [FromBody]dynamic json)
        {
            //Authentication
            //When user's permission denied (not leader)
            //if(false)
            //  return Forbid();

            //Delete group topic relation from database
            //if not found this relation
            //    return NotFound();

            //Success
            return NoContent();
        }

        // 下面这个API没有用？
        // GET: /group/{groupId}/grade
        [HttpGet("{groupId}/grade")]
        public IActionResult GetGroupSeminarGrade(int groupId)
        {
            // Fetch data from database via groupId
            Student leader = new Student { Id = 233, Name = "张三", Number = "24320152202333" };
            Student s1 = new Student { Id = 248, Name = "李四", Number = "24320152202345" };
            Student s2 = new Student { Id = 256, Name = "王五", Number = "24320152202356" };
            List<Student> memberList = new List<Student> { s1, s2 };
            List<Topic> topics = new List<Topic> { new Topic { Id = 257, Serial = "A", Name = "领域模型与模块", Description = "Domain model 与模块划分", GroupLimit = 5, GroupMemberLimit = 6, GroupLeft = 2 } };
            SeminarGrade sg = new SeminarGrade { PresentationGrade = new List<int> { 5 }, ReportGrade = 5, Grade = 5 };
            Group group = new Group { Id = 28, Name = "1-A-1", Leader = leader, Members = memberList, Topics = topics, Grade = sg };

            // If group not found
            if (group == null)
                return NotFound();

            // Success
            return Json(sg);
        }

        // PUT: /group/{groupId}/grade/report
        [HttpPut("{groupId}/grade/report")]
        public IActionResult PutGroupReportGrade(int groupId, [FromBody]dynamic json)
        {
            //Authentication
            //When user's permission denied
            //if(false)
            //  return Forbid();

            //Get information from json
            int reportGrade = json.reportGrade;

            //Change information in database
            //if group not found
            //    return NotFound();

            //Success
            return NoContent();
        }

    }
}

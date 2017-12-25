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
        public readonly ITopicService _iTopicService;
        public readonly IUserService _iUserService;
        public readonly IGradeService _iGradeService;
        public GroupController(ISeminarGroupService iSeminarGroupService, IFixGroupService iFixGroupService, ITopicService iTopicService, IUserService iUserService, IGradeService iGradeService)
        {
            _iSeminarGroupService = iSeminarGroupService;
            _iFixGroupService = iFixGroupService;
            _iTopicService = iTopicService;
            _iUserService = iUserService;
            _iGradeService = iGradeService;
        }
        // GET: /group/{groupId}?embedTopics={true|false}&embedGrade={true|false}
        [HttpGet("{groupId}")]
        public IActionResult GetGroupByGroupId(long groupId, [FromQuery]bool embedTopics, [FromQuery]bool embedGrade)
        {
            // Fetch selected data from database
            try
            {
                SeminarGroup seminarGroup= _iSeminarGroupService.GetSeminarGroupByGroupId(groupId);
                // 转换成VO对象
                GroupVO myGroup = seminarGroup;

                //获取Members
                IList<UserInfo> memberList = _iSeminarGroupService.ListSeminarGroupMemberByGroupId(seminarGroup.Id);
                List<UserVO> members = new List<UserVO>();
                foreach (UserInfo u in memberList)
                    members.Add(u);
                myGroup.Members = members;

                //获取Topics和PresentationGrade
                IList<SeminarGroupTopic> seminarGroupTopicList = _iTopicService.ListSeminarGroupTopicByGroupId(seminarGroup.Id);
                List<TopicVO> topics = new List<TopicVO>();
                List<int> pGrades = new List<int>();
                foreach (SeminarGroupTopic sgt in seminarGroupTopicList)
                {
                    topics.Add(sgt.Topic);
                    pGrades.Add((int)sgt.PresentationGrade);
                }
                myGroup.Topics = topics;
                myGroup.Grade.PresentationGrade = pGrades;

                //获取Name
                myGroup.GetName();

                // Success
                return Json(myGroup);
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
        public IActionResult RemoveGroupMember(long groupId, [FromBody]dynamic json)
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
            return NoContent();
        }
        
        // POST: /group/{groupId}/topic   //小组按ID选择话题[随机分组还是固定分组？？？]
        [HttpPost("{groupId}/topic")]
        public IActionResult PostNewTopic(long groupId, [FromBody]dynamic json)
        {
            long topicId = json.id;
            try
            {
                TopicVO topicSelectedByGroup = _iTopicService.GetTopicByTopicId(topicId);
                _iSeminarGroupService.InsertTopicByGroupId(groupId, topicId);
                string uri = "/group/" + groupId + "/topic/" + topicId;
                return Created(uri, topicSelectedByGroup);
            }
            catch(TopicNotFoundException)
            {
                return NotFound();
            }
            catch(GroupNotFoundException)
            {
                return NotFound();
            }
        }
        
        // DELETE: /group/{groupId}/topic/{topicId}   //小组按ID选择话题[随机分组还是固定分组？？？]
        [HttpPost("{groupId}/topic/{topicId}")]
        public IActionResult DeleteTopic(long groupId, long topicId, [FromBody]dynamic json)
        {
            try
            {
                //验证是不是组长【json会传UserId回来吗？】
                long userId = json.id;
                //FixGroup fixGroup = _iFixGroupService.GetFixGroupByGroupId(groupId);//没有这个方法
                IList<FixGroupMember> fixGroupMemberList = _iFixGroupService.ListFixGroupByGroupId(groupId);
                if (fixGroupMemberList == null)
                    throw new FixGroupNotFoundException();
                if (fixGroupMemberList[0].FixGroup.Leader.Id == groupId)//注意看service有没有写Include
                {
                    return StatusCode(403);
                }
                _iTopicService.DeleteSeminarGroupTopicById(groupId, topicId);
            }
            catch(FixGroupNotFoundException)
            {
                return NotFound();
            }
            catch(ArgumentException)
            {
                return BadRequest();
            }
            return NoContent();
        }

        // 下面这个API没有用？
        // GET: /group/{groupId}/grade
        [HttpGet("{groupId}/grade")]
        public IActionResult GetGroupSeminarGrade(long groupId)
        {
            try
            {
                SeminarGroup seminarGroup = _iSeminarGroupService.GetSeminarGroupByGroupId(groupId);
                // 转换成VO对象
                GroupVO myGroup = seminarGroup;

                //获取Members
                IList<UserInfo> memberList = _iSeminarGroupService.ListSeminarGroupMemberByGroupId(seminarGroup.Id);
                List<UserVO> members = new List<UserVO>();
                foreach (UserInfo u in memberList)
                    members.Add(u);
                myGroup.Members = members;

                //获取Topics和PresentationGrade
                IList<SeminarGroupTopic> seminarGroupTopicList = _iTopicService.ListSeminarGroupTopicByGroupId(seminarGroup.Id);
                List<TopicVO> topics = new List<TopicVO>();
                List<int> pGrades = new List<int>();
                foreach (SeminarGroupTopic sgt in seminarGroupTopicList)
                {
                    topics.Add(sgt.Topic);
                    pGrades.Add((int)sgt.PresentationGrade);
                }
                myGroup.Topics = topics;
                myGroup.Grade.PresentationGrade = pGrades;

                //获取Name
                myGroup.GetName();

                // Success
                return Json(myGroup);
            }
            catch(SeminarNotFoundException)
            {
                return NotFound();
            }
            catch(GroupNotFoundException)
            {
                return NotFound();
            }
            catch(ArgumentException)
            {
                return BadRequest();
            }
        }

        // PUT: /group/{groupId}/grade/report
        [HttpPut("{groupId}/grade/report")]
        public IActionResult PutGroupReportGrade(long groupId, [FromBody]dynamic json)
        {
            int reportGrade = json.reportGrade;
            try
            {
                _iGradeService.UpdateGroupByGroupId(groupId,reportGrade);
            }
            catch(GroupNotFoundException)
            {
                return NotFound();
            }
            catch(ArgumentException)
            {
                return BadRequest();
            }
            return NoContent();
        }
    }
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using System;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Web.ViceVersa.VO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Xmu.Crms.ViceVersa
{
    /// <summary>
    /// API Controllet
    /// 存放路由为"/group"下的与小组操作相关的方法控制器
    /// @author Group ViceVersa
    /// </summary>
    [Produces("application/json")]
    [Route("/group")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
        /// <summary>
        /// GET: /group/{groupId}?embedTopics={true|false}&embedGrade={true|false}
        /// </summary>
        /// <param name="groupId">小组的Id</param>
        /// <param name="embedTopics">是否包含小组选择的话题</param>
        /// <param name="embedGrade">是否包含小组的成绩</param>
        /// <returns>小组详情</returns>
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
        
        /// <summary>
        /// PUT: /group/{groupId}/add
        /// </summary>
        /// <param name="groupId">小组的Id</param>
        /// <param name="json">添加学生的Id</param>
        /// <returns>操作结果状态</returns>
        [HttpPut("{groupId}/add")]
        public IActionResult AddGroupMember(long groupId, [FromBody]dynamic json)
        {
            try//无法抛出权限不足的异常
            {
                long userId = json.id;
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
        
        /// <summary>
        /// PUT: /group/{groupId}/remove
        /// </summary>
        /// <param name="groupId">小组的Id</param>
        /// <param name="json">需要移除学生的Id</param>
        /// <returns>操作结果状态</returns>
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
        
        /// <summary>
        /// POST: /group/{groupId}/topic
        /// </summary>
        /// <param name="groupId">小组的Id</param>
        /// <param name="json">该小组选择的话题的Id</param>
        /// <returns>操作结果状态</returns>
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
        
        /// <summary>
        /// DELETE: /group/{groupId}/topic/{topicId} 
        /// </summary>
        /// <param name="groupId">小组的Id</param>
        /// <param name="topicId">该小组删除的话题的Id</param>
        /// <param name="json"></param>
        /// <returns>操作结果状态</returns>
        [HttpPost("{groupId}/topic/{topicId}")]
        public IActionResult DeleteTopic(long groupId, long topicId, [FromBody]dynamic json)
        {
            try
            {
                //验证是不是组长
                long userId = User.Id();
                IList<FixGroupMember> fixGroupMemberList = _iFixGroupService.ListFixGroupByGroupId(groupId);
                if (fixGroupMemberList == null)
                    throw new FixGroupNotFoundException();
                if (fixGroupMemberList[0].FixGroup.Leader.Id == groupId)
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

        /// <summary>
        /// GET: /group/{groupId}/grade
        /// </summary>
        /// <param name="groupId">需获取成绩的小组的Id</param>
        /// <returns>该小组讨论课成绩</returns>
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
        
        /// <summary>
        /// PUT: /group/{groupId}/grade/report
        /// </summary>
        /// <param name="groupId">需设置成绩的小组Id</param>
        /// <param name="json">报告成绩</param>
        /// <returns>操作结果状态</returns>
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

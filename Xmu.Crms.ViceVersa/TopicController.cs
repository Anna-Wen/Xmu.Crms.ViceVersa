using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Xmu.Crms.Shared.Exceptions;
using System;
using Xmu.Crms.Shared.Service;
using Xmu.Crms.Web.ViceVersa.VO;

namespace Xmu.Crms.ViceVersa
{
    /// <summary>
    /// API Controller
    /// 存放路由为"/topic"下的与话题操作相关的方法的控制器
    /// @author Group ViceVersa
    /// </summary>
    [Produces("application/json")]
    [Route("/topic")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TopicController : Controller
    {
        private readonly ISeminarGroupService _iSeminarGroupService;
        private readonly ITopicService _iTopicService;

        public TopicController(ISeminarGroupService iSeminarGroupService, ITopicService iTopicService)
        {
            _iSeminarGroupService = iSeminarGroupService;
            _iTopicService = iTopicService;
        }

        /// <summary>
        /// GET: /topic/{topicId}
        /// </summary>
        /// <param name="topicId">需获取话题的ID</param>
        /// <returns>按照ID获取的话题信息</returns>
        [HttpGet("{topicId}")]
        public IActionResult GetTopic(long topicId)
        {
            try
            {
                // Fetch topic from database...
                Topic topic = _iTopicService.GetTopicByTopicId(topicId);

                // 未解决：如何求GroupLeft
                TopicVO topicVO = topic;

                // Success
                return Json(topicVO);
            }
            //If topic not found, 返回404
            catch (TopicNotFoundException)
            {
                return NotFound(new { msg = "未找到该话题！" });
            }
            //topicId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }

        /// <summary>
        /// PUT: /topic/{topicId}
        /// </summary>
        /// <param name="topicId">需修改话题的ID</param>
        /// <param name="json">需修改的话题信息</param>
        /// <returns>返回204，修改话题信息成功</returns>
        [HttpPut("{topicId}")]
        public IActionResult PutTopic(long topicId, [FromBody]dynamic json)
        {
            // Authentication
            // 学生无法修改话题，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return StatusCode(403, new { msg = "学生无法修改话题！" });

            try
            {
                // Get information from json
                Topic editedTopic = new Topic();
                if (json.GroupLimit != "" && json.GroupMemberLimit != "")
                {
                    editedTopic = new Topic { Serial = json.Serial, Name = json.Name, Description = json.Description, GroupNumberLimit = json.GroupLimit, GroupStudentLimit = json.GroupMemberLimit };
                }

                //Change information in database
                // 怎么完成无法修改他人话题的权限判断？？？
                _iTopicService.UpdateTopicByTopicId(topicId, editedTopic);

                //Success
                return NoContent();
            }
            //If topic not found, 返回404
            catch (TopicNotFoundException)
            {
                return NotFound(new { msg = "未找到该话题！" });
            }
            //topicId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }

        /// <summary>
        /// DELETE: /topic/{topicId}
        /// </summary>
        /// <param name="topicId">需删除话题的ID</param>
        /// <returns>返回204，话题删除成功</returns>
        [HttpDelete("{topicId}")]
        public IActionResult DeleteTopic(long topicId)
        {
            // Authentication
            // 学生无法删除话题，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return StatusCode(403, new { msg = "学生无法删除话题！" });

            try
            {
                // Delete seminar from database
                // 怎么完成无法删除他人话题的权限判断？？？
                _iTopicService.DeleteTopicByTopicId(topicId);

                // Success
                return NoContent();
            }
            //If topic not found, 返回404
            catch (TopicNotFoundException)
            {
                return NotFound(new { msg = "未找到该话题！" });
            }
            //topicId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }

        /// <summary>
        /// GET: /topic/{topicId}/group
        /// </summary>
        /// <param name="topicId">需获取小组信息的话题ID</param>
        /// <returns>按照话题ID获取的小组信息</returns>
        [HttpGet("{topicId}/group")]
        public IActionResult GetGroupsUnderTopic(long topicId)
        {
            try
            {
                // Fetch groups belongs to this topic via topicId from database...
                IList<SeminarGroup> seminarGroupList = _iSeminarGroupService.ListGroupByTopicId(topicId);

                // 转换成VO对象
                List<GroupVO> groups = new List<GroupVO>();         //只需要Id和Name
                foreach (SeminarGroup sg in seminarGroupList)
                {
                    GroupVO g = sg;

                    //获取Members
                    IList<UserInfo> memberList = _iSeminarGroupService.ListSeminarGroupMemberByGroupId(sg.Id);
                    List<UserVO> members = new List<UserVO>();
                    foreach (UserInfo u in memberList)
                        members.Add(u);
                    g.Members = members;

                    //获取Topics和PresentationGrade
                    IList<SeminarGroupTopic> seminarGroupTopicList = _iTopicService.ListSeminarGroupTopicByGroupId(sg.Id);
                    List<TopicVO> topics = new List<TopicVO>();
                    List<int> pGrades = new List<int>();
                    foreach (SeminarGroupTopic sgt in seminarGroupTopicList)
                    {
                        topics.Add(sgt.Topic);
                        pGrades.Add((int)sgt.PresentationGrade);
                    }
                    g.Topics = topics;
                    g.Grade.PresentationGrade = pGrades;

                    //获取Name
                    g.GetName();

                    groups.Add(g);
                }

                // Success
                return Json(groups);
            }
            //If topic not found, 返回404
            catch (TopicNotFoundException)
            {
                return NotFound(new { msg = "未找到该话题！" });
            }
            //topicId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }
    }
}

﻿using System;
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
    /// <summary>
    /// API Controller
    /// 存放路由为"/seminar"下的与讨论课操作相关的方法的控制器
    /// @author Group ViceVersa
    /// </summary>
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

        /// <summary>
        /// GET: /seminar/{seminarId}
        /// </summary>
        /// <param name="seminarId">需获取讨论课的ID</param>
        /// <returns>讨论课ID对应的信息</returns>
        [HttpGet("{seminarId}", Name = "Get")]
        public IActionResult GetSeminar(long seminarId)
        {
            try
            {
                long id = seminarId;
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
                return NotFound(new { msg = "未找到该讨论课！" });
            }
            //seminarId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }

        /// <summary>
        /// PUT: /seminar/{seminarId}
        /// </summary>
        /// <param name="seminarId">需修改的讨论课ID</param>
        /// <param name="json">需修改的讨论课信息</param>
        /// <returns>返回204，讨论课修改成功</returns>
        [HttpPut("{seminarId}")]
        public IActionResult PutSeminar(long seminarId, [FromBody]dynamic json)
        {
            // Authentication
            // 学生无法修改讨论课，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return StatusCode(403, new { msg = "学生无法修改讨论课！" });

            try
            {
                string startTime = json.StartTime;
                string endTime = json.EndTime;
                // Get information from json
                Seminar editedSeminar = new Seminar { Id = seminarId, Name = json.Name, Description = json.Description, StartTime = Convert.ToDateTime(startTime), EndTime = Convert.ToDateTime(endTime) };
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
                return NotFound(new { msg = "未找到该讨论课！" });
            }
            //seminarId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }

        /// <summary>
        /// DELETE: /seminar/{seminarId}
        /// </summary>
        /// <param name="seminarId">需删除讨论课的ID</param>
        /// <returns>返回204，删除讨论课成功</returns>
        [HttpDelete("{seminarId}")]
        public IActionResult DeleteSeminar(long seminarId)
        {
            // Authentication
            // 学生无法删除讨论课，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return StatusCode(403, new { msg = "学生无法删除讨论课！" });

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
                return NotFound(new { msg = "未找到该讨论课！" });
            }
            //seminarId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }

        /// <summary>
        /// GET: /seminar/{seminarId}/topic
        /// </summary>
        /// <param name="seminarId">需获取话题的讨论课的ID</param>
        /// <returns>按讨论课ID获取的话题列表</returns>
        [HttpGet("{seminarId}/topic")]
        public IActionResult GetSeminarTopics(long seminarId)
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
                return NotFound(new { msg = "未找到该讨论课！" });
            }
            //seminarId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }

        /// <summary>
        /// POST: /seminar/{seminarId}/topic
        /// </summary>
        /// <param name="seminarId">待创建话题的讨论课ID</param>
        /// <param name="json">待创建话题的信息</param>
        /// <returns>新创建话题的ID</returns>
        [HttpPost("{seminarId}/topic")]
        public IActionResult PostNewTopicUnderSeminar(long seminarId, [FromBody]dynamic json)
        {
            // Authentication
            // 学生无法创建话题，返回403
            if (User.Type() == Shared.Models.Type.Student)
                return StatusCode(403, new { msg = "学生无法创建话题！" });

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
                return Created(uri, newTopicId);
            }
            //If seminar not found, 返回404
            catch (SeminarNotFoundException)
            {
                return NotFound(new { msg = "未找到该讨论课！" });
            }
            //seminarId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }

        /// <summary>
        /// GET: /seminar/{seminarId}/group?classId={classId}
        /// </summary>
        /// <param name="seminarId">需获取小组的讨论课ID</param>
        /// <param name="gradeable">为true时，仅返回当前用户可打分的小组</param>
        /// <param name="classId">仅返回此班级的小组</param>
        /// <returns>按照讨论课ID查找的小组列表</returns>
        [HttpGet("{seminarId}/group")]
        public IActionResult GetSeminarGroups(long seminarId, [FromQuery]bool gradeable, [FromQuery]int classId)
        {
            try
            {
                // 先只实现了不分班级小组和不分用户是否可打分小组的方法

                // Fetch data from database
                IList<SeminarGroup> seminarGroupList = _iSeminarGroupService.ListSeminarGroupBySeminarId(seminarId);

                // 转换成VO对象
                List<GroupVO> groups = new List<GroupVO>();
                foreach (var sg in seminarGroupList)
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
                        if(sgt.Topic!=null)
                        {
                            topics.Add(sgt.Topic);
                            if(sgt.PresentationGrade!=null)
                                pGrades.Add((int)sgt.PresentationGrade);
                        }
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
            //If seminar not found, 返回404
            catch (SeminarNotFoundException)
            {
                return NotFound(new { msg = "未找到该讨论课！" });
            }
            //seminarId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }

        /// <summary>
        /// GET: /seminar/{seminarId}/group/my
        /// </summary>
        /// <param name="seminarId">需获取小组的讨论课ID</param>
        /// <returns>按照讨论课ID获取当前学生所在的小组详情</returns>
        [HttpGet("{seminarId}/group/my")]
        public IActionResult GetMySeminarGroup(long seminarId)
        {
            // Authentication
            // 老师无法使用此API，返回403
            if (User.Type() == Shared.Models.Type.Teacher)
                return StatusCode(403, new { msg = "老师无法获得自己的小组信息！" });

            try
            {
                // Fetch data from database
                SeminarGroup seminarGroup = _iSeminarGroupService.GetSeminarGroupById(seminarId, User.Id());

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
            //If seminar not found, 返回404
            catch (SeminarNotFoundException)
            {
                return NotFound(new { msg = "未找到该讨论课！" });
            }
            //If group not found, 返回404
            catch (GroupNotFoundException)
            {
                return NotFound(new { msg = "讨论课尚未分组！" });
            }
            //seminarId 格式错误，返回400
            catch (ArgumentException)
            {
                return BadRequest(new { msg = "错误的ID格式！" });
            }
        }

    }
}

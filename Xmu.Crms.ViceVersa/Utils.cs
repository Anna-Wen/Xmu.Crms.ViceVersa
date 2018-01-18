using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Xmu.Crms.ViceVersa
{
    /// <summary>
    /// 该类存放一些关于JWT使用的功能性方法
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// 获取当前用户Id的方法
        /// </summary>
        /// <param name="user">JWT生成后的ClaimsPrincipal对象</param>
        /// <returns>当前用户ID</returns>
        public static long Id(this ClaimsPrincipal user) => long.Parse(user.Claims.Single(c => c.Type == "id").Value);

        /// <summary>
        /// 获取当前用户Type的方法
        /// </summary>
        /// <param name="user">JWT生成后的ClaimsPrincipal对象</param>
        /// <returns>当前用户Type</returns>
        public static Shared.Models.Type Type(this ClaimsPrincipal user) => Enum.Parse<Shared.Models.Type>(user.Claims.Single(c => c.Type == "type").Value, true);
    }
}

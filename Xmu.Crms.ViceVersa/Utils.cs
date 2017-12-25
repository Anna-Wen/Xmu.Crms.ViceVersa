using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Xmu.Crms.ViceVersa
{
    public static class Utils
    {
        // 获取当前用户Id的方法
        public static long Id(this ClaimsPrincipal user) => long.Parse(user.Claims.Single(c => c.Type == "id").Value);

        // 获取当前用户Type的方法
        public static Shared.Models.Type Type(this ClaimsPrincipal user) => Enum.Parse<Shared.Models.Type>(user.Claims.Single(c => c.Type == "type").Value, true);
    }
}

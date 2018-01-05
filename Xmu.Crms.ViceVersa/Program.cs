using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xmu.Crms.Shared;

namespace Xmu.Crms.ViceVersa
{
    /// <summary>
    /// 包括程序入口Main函数的程序类
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 程序的入口
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            host.Run();
        }
        
        /// <summary>
        /// 配置所有代理，包括依赖注入
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseIISIntegration()
                .ConfigureServices(services => services.AddViceVersaClassService().AddViceVersaClassDao()
                                                .AddViceVersaGradeDao().AddViceVersaGradeService()
                                                .AddViceVersaCourseDao().AddViceVersaCourseService()
                                                .AddHighGradeSchoolService().AddHighGradeSeminarService()
                                                .AddInsomniaFixedGroupService().AddInsomniaSeminarGroupService().AddInsomniaLoginService()
                                                .AddGroup1TopicService().AddGroup1TopicDao()
                                                .AddGroup1UserService().AddGroup1UserDao()
                                                //.AddInsomniaPbkdf2LoginService().AddInsomniaUserService().AddInsomniaTopicService()
                                                //.AddGroup1SchoolService()
                                                .AddCrmsView("Web.ViceVersa"))
                .UseStartup<Startup>()
                .Build();
    }
}
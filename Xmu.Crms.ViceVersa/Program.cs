using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xmu.Crms.Shared;

namespace Xmu.Crms.ViceVersa
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            host.Run();
        }
        
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
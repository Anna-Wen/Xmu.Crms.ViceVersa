# README for Class Management System  

## by Group ViceVersa  

### 小组成员  

1. 文华婷：24320152202824@stu.xmu.edu.cn  
2. 李思恒：24320152202766@stu.xmu.edu.cn  
3. 刘庆：24320152202781@stu.xmu.edu.cn  
4. 罗旭丹：24320152202791@stu.xmu.edu.cn  

* 注：老师如果您运行有任何问题可以联系我们小组的成员

### 项目浏览前必读  

1. 我们的项目主要由七个部分组成  
    * (1) Xmu.Crms.Web.ViceVersa：  
        * 我们小组的VO层设计与界面设计  
    * (2) Xmu.Crms.ViceVersa：  
        * 我们小组的Controller层设计  
    * (3) Xmu.Crms.Service.ViceVersa：  
        * 我们小组完成的三个模块——CourseService, ClassService, GradeService   
    * (4) Xmu.Crms.Shared：  
        * 标准组统一规定的model, service接口与配置文件  
    * (5) Xmu.Crms.Service.Group1：  
        * 集成Group1小组的UserService, TopicService  
    * (6) Xmu.Crms.Service.HighGrade：  
        * 集成HighGrade小组的SchoolService, SeminarService  
    * (7) Xmu.Crms.Service.Insomnia：  
        * 集成Insomnia小组的Md5LoginService, GroupService  

2. 我们小组在OOAD课上选择实现 CourseService, ClassService 与 GradeService三个部分。  
    * 主要实现了课堂管理系统当中 课程管理，课堂管理 与 分数处理 三个模块的功能。  

3. 其余的UserService, SchoolService, TopicService, SeminarService, LoginService, GroupService部分选择集成其他小组的内容。  

4. 该项目的运行环境为VS2017,使用.NetCore 4.6 + MVC框架 + RESTful API设计。  
    * Model在 Xmu.Crms.Shared 的 Model 文件夹中  
    * View在 Xmu.Crms.Web.ViceVersa 的View文件夹中  
    * MVC Controller在 Xmu.Crms.Web.ViceVersa 的 Controller 文件夹中，API Controller在 Xmu.Crms.ViceVersa 中  

5. 我们上交的项目所连接的数据库为邱明老师的服务器上的数据库。  
    * 连接字符串在Xmu.Crms.ViceVersa中的appsettings.json中。  
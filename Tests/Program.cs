using RBAC.Data;
using RBAC.Service;
using RBAC.ServiceImpl;
using RBAC.Utils.Manager;

namespace RBAC.Tests;

// 这是验证程序，学习时先阅读主项目即可。
internal class Program
{
    private static int _passed;

    private static void Main()
    {
        InMemoryDatabase database = new InMemoryDatabase();
        IRbacService service = new RbacServiceImpl(database);

        Check(!service.HasPermission("report:view"), "未登录没有权限");
        Check(service.GetUsers().Count == 0, "未登录不能查看用户");
        Check(service.CreateUser("bob", "Pass123!").Contains("权限不足"), "未登录不能创建用户");
        Check(service.ChangeRole("alice", "管理员").Contains("权限不足"), "未登录不能修改角色");
        Check(service.ViewReport().Contains("权限不足"), "未登录不能查看报表");

        Check(service.Login(" ADMIN ", "Admin123!"), "管理员登录");
        Check(service.GetUsers().Count == 2, "管理员查看用户");
        Check(!service.HasPermission("unknown"), "未知权限被拒绝");
        Check(service.ChangeRole("admin", "普通用户").Contains("最后一个"), "保留最后一个管理员");
        Check(service.ChangeRole("missing", "普通用户").Contains("用户不存在"), "处理不存在的用户");
        Check(service.ChangeRole("alice", "missing").Contains("角色不存在"), "处理不存在的角色");
        Check(service.CreateUser("ADMIN", "Pass123!").Contains("已存在"), "拒绝重复用户名");
        Check(service.CreateUser("a", "Pass123!").Contains("2～20"), "校验用户名长度");
        Check(service.CreateUser("   ", "Pass123!").Contains("2～20"), "拒绝空白用户名");
        Check(service.CreateUser("bob", "123").Contains("至少 6"), "拒绝短密码");
        Check(service.CreateUser("bob", "      ").Contains("至少 6"), "拒绝全空白密码");
        Check(service.CreateUser(" bob ", "Pass123!").Contains("创建成功"), "创建用户");
        Check(database.Users.Count == 3 && database.Users[2].Role.Name == "普通用户", "新用户默认是普通用户");
        Check(database.Users[2].PasswordHash != "Pass123!", "未保存明文密码");

        Check(!service.Login("admin", "wrong"), "错误密码不能登录");
        Check(service.CurrentUser == null && !service.HasPermission("user:create"), "登录失败清除旧身份");
        Check(!service.Login("missing", "Pass123!"), "不存在的账号不能登录");
        Check(service.Login("bob", "Pass123!"), "新用户可以登录");
        Check(service.GetUsers().Count == 0, "普通用户不能查看用户");
        Check(service.CreateUser("eve", "Pass123!").Contains("权限不足"), "普通用户不能创建用户");
        Check(service.ChangeRole("bob", "管理员").Contains("权限不足"), "普通用户不能给自己提权");
        Check(service.ViewReport().Contains("订单数 12"), "普通用户可以查看报表");

        // 两个业务对象共享数据，模拟管理员修改已登录用户的角色。
        IRbacService adminService = new RbacServiceImpl(database);
        adminService.Login("admin", "Admin123!");
        Check(adminService.ChangeRole("bob", "管理员").Contains("修改成功"), "分配管理员角色");
        Check(service.HasPermission("user:create"), "提升角色立即获得权限");
        Check(adminService.ChangeRole("bob", "普通用户").Contains("修改成功"), "改回普通用户");
        Check(!service.HasPermission("user:create"), "降低角色立即失去权限");
        service.Logout();
        Check(service.CurrentUser == null && !service.HasPermission("report:view"), "注销清除身份与权限");

        // 用几行文本模拟终端输入，覆盖登录、越权提示、报表和注销。
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;
        StringReader input = new StringReader("bad\n1\nadmin\nwrong\n1\nalice\nUser123!\n2\n3\n4\n5\n0\n0\n");
        StringWriter output = new StringWriter();
        try
        {
            Console.SetIn(input);
            Console.SetOut(output);
            new SystemManager(service).Run();
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }

        string text = output.ToString();
        Check(text.Contains("无效选项") && text.Contains("用户名或密码错误"), "菜单处理错误输入");
        Check(text.Contains("不能查看用户") && text.Contains("不能创建用户") && text.Contains("不能修改角色"),
            "菜单显示权限不足提示");
        Check(text.Contains("订单数 12") && text.Contains("已退出登录") && text.Contains("再见"), "终端报表与退出流程");

        // 输入在创建用户的中途结束，也应正常退出且清除会话。
        input = new StringReader("1\nadmin\nAdmin123!\n3\nbob\n");
        output = new StringWriter();
        try
        {
            Console.SetIn(input);
            Console.SetOut(output);
            new SystemManager(service).Run();
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
        Check(output.ToString().Contains("再见") && service.CurrentUser == null, "输入结束时正常清理并退出");

        Console.WriteLine("全部通过，共 " + _passed + " 项检查。");
    }

    private static void Check(bool success, string name)
    {
        if (!success)
        {
            throw new Exception("检查失败：" + name);
        }

        _passed++;
        Console.WriteLine("[通过] " + name);
    }
}

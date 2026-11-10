using RBAC.Models;
using RBAC.Service;

namespace RBAC.Utils.Manager;

// 菜单层只负责读取输入、调用业务方法、显示结果。
public class SystemManager
{
    private IRbacService _service;

    public SystemManager(IRbacService service)
    {
        _service = service;
    }

    public void Run()
    {
        Console.WriteLine("========== RBAC 入门版 ==========");
        Console.WriteLine("管理员：admin / Admin123!");
        Console.WriteLine("普通用户：alice / User123!");

        bool running = true;
        while (running)
        {
            if (_service.CurrentUser == null)
            {
                Console.WriteLine("\n1. 登录  0. 退出程序");
                string? choice = Console.ReadLine();
                if (choice == null || choice == "0")
                {
                    break;
                }

                if (choice == "1")
                {
                    running = Login();
                }
                else
                {
                    Console.WriteLine("无效选项。");
                }
            }
            else
            {
                Console.WriteLine("\n1. 我的信息  2. 查看用户  3. 创建用户");
                Console.WriteLine("4. 修改用户角色  5. 查看报表  0. 退出登录  Q. 退出程序");
                string? choice = Console.ReadLine();
                if (choice == null || choice == "Q" || choice == "q")
                {
                    break;
                }

                switch (choice)
                {
                    case "1":
                        ShowMyInfo();
                        break;
                    case "2":
                        ShowUsers();
                        break;
                    case "3":
                        running = CreateUser();
                        break;
                    case "4":
                        running = ChangeRole();
                        break;
                    case "5":
                        Console.WriteLine(_service.ViewReport());
                        break;
                    case "0":
                        _service.Logout();
                        Console.WriteLine("已退出登录。");
                        break;
                    default:
                        Console.WriteLine("无效选项。");
                        break;
                }
            }
        }

        _service.Logout();
        Console.WriteLine("再见！");
    }

    private bool Login()
    {
        Console.Write("用户名：");
        string? username = Console.ReadLine();
        Console.Write("密码：");
        string? password = Console.ReadLine();
        if (username == null || password == null) return false;

        if (_service.Login(username, password))
        {
            Console.WriteLine("登录成功！");
        }
        else
        {
            Console.WriteLine("用户名或密码错误。");
        }

        return true;
    }

    private void ShowMyInfo()
    {
        User? user = _service.CurrentUser;
        if (user == null) return;

        Console.WriteLine("用户名：" + user.Username);
        Console.WriteLine("角色：" + user.Role.Name);
        Console.WriteLine("拥有的权限：");
        foreach (string permission in user.Role.Permissions)
        {
            Console.WriteLine("  " + permission);
        }
    }

    private void ShowUsers()
    {
        if (!_service.HasPermission("user:view"))
        {
            Console.WriteLine("权限不足，不能查看用户。");
            return;
        }

        foreach (User user in _service.GetUsers())
        {
            Console.WriteLine("用户名：" + user.Username + "，角色：" + user.Role.Name);
        }
    }

    private bool CreateUser()
    {
        if (!_service.HasPermission("user:create"))
        {
            Console.WriteLine("权限不足，不能创建用户。");
            return true;
        }

        Console.Write("新用户名：");
        string? username = Console.ReadLine();
        Console.Write("新密码（至少 6 位）：");
        string? password = Console.ReadLine();
        if (username == null || password == null) return false;

        Console.WriteLine(_service.CreateUser(username, password));
        return true;
    }

    private bool ChangeRole()
    {
        if (!_service.HasPermission("user:change-role"))
        {
            Console.WriteLine("权限不足，不能修改角色。");
            return true;
        }

        Console.Write("要修改的用户名：");
        string? username = Console.ReadLine();
        Console.Write("新角色（管理员/普通用户）：");
        string? roleName = Console.ReadLine();
        if (username == null || roleName == null) return false;

        Console.WriteLine(_service.ChangeRole(username, roleName));
        return true;
    }
}

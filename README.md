# C# 终端 RBAC 入门版

这个版本面向刚学完 C# 基础语法的同学，主要使用类、属性、接口、`List`、`if`、`foreach` 和 `switch`。先练习登录与权限判断，再逐步扩展功能。

**详细学习教程：[从 C# 基础语法到第一个终端 RBAC 项目](docs/TUTORIAL.md)**。包含逐文件讲解、登录与权限的执行过程、Rider 调试步骤，以及带参考答案的练习。

## 运行

在 Rider 中打开 `RBAC.sln`，选择 **RBAC** 运行。也可以在项目目录执行：

```powershell
dotnet run --project RBAC.csproj
```

本机的 `dotnet` 不在 PATH 中时，使用：

```powershell
& "$env:USERPROFILE\.dotnet\dotnet.exe" run --project RBAC.csproj
```

需要 .NET 9 SDK 或支持 `net9.0` 的更高版本 SDK。数据放在内存集合中，退出登录时保留，重启程序后恢复初始状态。

| 账号 | 密码 | 角色 |
| --- | --- | --- |
| admin | Admin123! | 管理员 |
| alice | User123! | 普通用户 |

## 先理解一个关系

```text
用户 User → 角色 Role → 权限 Permissions
alice       普通用户     report:view（查看报表）
admin       管理员       查看用户、创建用户、修改角色、查看报表
```

入门版中，每个用户只有一个角色，角色固定为“管理员”和“普通用户”。权限用字符串表示，例如 `user:create` 就代表“创建用户”。

判断权限时，先取得当前用户，再取出他的角色，最后使用 `foreach` 检查角色的权限列表。没有登录或找不到权限，就返回 `false`。权限检查写在业务层中。

## 保留的分层

```text
RBAC/
├── Program.cs                       程序入口，创建三个对象
├── Models/
│   ├── User.cs                      用户类
│   └── Role.cs                      角色类
├── Data/InMemoryDatabase.cs          List 集合和演示账号
├── Service/IRbacService.cs           业务接口：有哪些功能
├── ServiceImpl/RbacServiceImpl.cs    业务实现：每个功能怎么做
├── Utils/Manager/SystemManager.cs    终端菜单、输入和输出
├── Utils/Security/PasswordHasher.cs  密码工具，初学时可跳过
└── Tests/                           验证程序，初学时可跳过
```

调用顺序是 `菜单 → Service 接口 → ServiceImpl → Data`。

接口可以理解为“功能清单”。`RbacServiceImpl : IRbacService` 表示实现类必须完成接口里声明的方法。菜单通过接口调用这些方法，业务实现再操作数据集合。

## 按这个顺序读代码

1. **User.cs、Role.cs**：理解一个用户如何保存角色，一个角色如何保存权限列表。
2. **InMemoryDatabase.cs**：看怎样 `new` 对象、给属性赋值、用 `Add` 加入集合。
3. **IRbacService.cs、RbacServiceImpl.cs**：先看 `Login` 的循环查找，再看 `HasPermission` 的循环判断。
4. **Program.cs**：看数据对象、业务对象、菜单对象怎样连接起来。
5. **SystemManager.cs**：看 `while` 循环和 `switch` 怎样组成菜单。

`User?` 和 `string?` 中的问号表示“可能为 null”，例如尚未登录时 `CurrentUser` 为 null。构造函数中的参数用于把已经创建的数据对象传给业务对象。

## 动手操作

1. 用 `alice / User123!` 登录，选 `1` 查看信息，选 `5` 查看报表。
2. 选 `2` 查看用户，会提示权限不足。
3. 选 `0` 退出登录，再用 `admin / Admin123!` 登录。
4. 选 `3` 创建用户 `bob`，密码填写 `Bob123!`；新用户默认是普通用户。
5. 选 `4`，把 bob 的角色改成 `管理员`。
6. 切换到 bob 登录，就可以查看和创建用户。

系统会校验重复用户名和短密码，也会保留至少一位管理员。菜单中的 `0` 是退出登录，`Q` 是退出程序。

密码输入采用普通的 `Console.ReadLine()`，会显示输入内容。密码存储保留了独立的散列工具：现阶段只需知道 `Hash` 用于保存密码、`Verify` 用于比较密码，可以暂时跳过工具内部的算法。

## 验证

```powershell
dotnet build RBAC.sln -m:1
dotnet run --project Tests/RBAC.Tests.csproj --no-build
```

`-m:1` 使用单进程构建。验证程序检查登录、权限拦截、创建用户、角色切换和菜单流程。

学会当前版本后，可以依次尝试：增加一个预设角色、增加一个菜单功能、把数据保存到文件、让一个用户拥有多个角色。

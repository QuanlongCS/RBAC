# 从 C# 基础语法到第一个终端 RBAC 项目

这份教程对应当前项目的“入门版”。你已经学过变量、条件判断、循环和方法，接下来会把这些语法连接起来，完成一个有分层的终端程序。

建议边运行、边阅读、边调试。第一遍先看懂登录和权限判断，第二遍再看创建用户、修改角色和菜单。密码算法和测试代码可以放到最后。

## 目录

1. [先运行一次，看看程序能做什么](#1-先运行一次看看程序能做什么)
2. [用生活中的例子理解 RBAC](#2-用生活中的例子理解-rbac)
3. [项目为什么要分层](#3-项目为什么要分层)
4. [先认识类、对象和属性](#4-先认识类对象和属性)
5. [用 List 保存用户和角色](#5-用-list-保存用户和角色)
6. [读懂 Data：初始化数据](#6-读懂-data初始化数据)
7. [读懂 Service：接口和实现](#7-读懂-service接口和实现)
8. [读懂 Program：把对象连接起来](#8-读懂-program把对象连接起来)
9. [读懂登录和注销](#9-读懂登录和注销)
10. [读懂权限检查和报表](#10-读懂权限检查和报表)
11. [读懂查看用户和创建用户](#11-读懂查看用户和创建用户)
12. [读懂修改角色](#12-读懂修改角色)
13. [读懂终端菜单](#13-读懂终端菜单)
14. [把一次操作从头跟到尾](#14-把一次操作从头跟到尾)
15. [在 Rider 中调试](#15-在-rider-中调试)
16. [密码工具先掌握调用方式](#16-密码工具先掌握调用方式)
17. [验证、常见问题和练习](#17-验证常见问题和练习)

## 1. 先运行一次，看看程序能做什么

### 1.1 在 Rider 中运行

1. 打开项目根目录的 `RBAC.sln`。
2. 等待 Rider 加载项目。
3. 选择 **RBAC** 的运行配置，点击运行按钮。
4. 在运行窗口或终端里输入菜单选项，再按回车。

解决方案里还有一个 `RBAC.Tests`，它用来验证代码。体验交互菜单时选择 **RBAC**。

如果 Rider 的运行窗口不能正常输入，可以打开 Rider 内置终端，执行下面的命令。

### 1.2 在终端中运行

在项目根目录执行：

```powershell
dotnet run --project RBAC.csproj
```

如果提示找不到 `dotnet`，本机可以使用完整路径：

```powershell
& "$env:USERPROFILE\.dotnet\dotnet.exe" run --project RBAC.csproj
```

这里的 `&` 是 PowerShell 的调用运算符，用来执行引号中的程序路径。`$env:USERPROFILE` 是当前 Windows 用户目录。

项目使用 .NET 9，目标框架写在 `RBAC.csproj` 的 `<TargetFramework>net9.0</TargetFramework>` 中。`dotnet run` 会先构建项目，再运行程序。

### 1.3 演示账号和菜单

| 用户名 | 密码 | 角色 |
| --- | --- | --- |
| admin | Admin123! | 管理员 |
| alice | User123! | 普通用户 |

先输入 `1` 登录，再输入用户名和密码。登录之后的菜单是：

| 选项 | 功能 |
| --- | --- |
| 1 | 查看自己的用户名、角色和权限 |
| 2 | 查看用户列表 |
| 3 | 创建用户 |
| 4 | 修改用户角色 |
| 5 | 查看固定的演示报表 |
| 0 | 退出登录 |
| Q 或 q | 退出程序 |

没有登录时，`0` 表示退出程序。密码通过 `Console.ReadLine()` 读取，输入内容会显示出来。

### 1.4 完整体验一次

请在**同一次程序运行中**完成以下步骤：

1. 用 `alice / User123!` 登录。
2. 选 `1`，看到角色为“普通用户”，权限为 `report:view`。
3. 选 `5`，看到报表。
4. 选 `2`，看到“权限不足，不能查看用户。”
5. 选 `0` 退出登录，再用管理员账号登录。
6. 选 `3`，创建 `bob`，密码填写 `Bob123!`。
7. 选 `2`，看到 bob 的角色是“普通用户”。
8. 选 `4`，用户名填 `bob`，新角色填 `管理员`。
9. 退出登录，换成 bob 登录，再选 `2`，这次可以查看用户。

你刚刚完成了“创建用户 → 分配角色 → 通过角色获得权限”的过程。

数据保存在内存中。退出登录会保留本次运行的数据；退出程序后重新运行，bob 就不存在了，只会重新生成 admin 和 alice。

## 2. 用生活中的例子理解 RBAC

假设一个图书馆有“读者”和“管理员”。读者可以查询图书，管理员还可以录入图书、维护读者资料。

可以把这个关系分成三部分：

| 概念 | 含义 | 本项目中的例子 |
| --- | --- | --- |
| 用户 User | 谁在使用系统 | admin、alice、bob |
| 角色 Role | 用户具有哪种身份 | 管理员、普通用户 |
| 权限 Permission | 允许执行哪种操作 | 查看用户、创建用户、查看报表 |

RBAC 的英文是 Role-Based Access Control，意思是“基于角色的访问控制”。本项目中，每个用户有一个角色，每个角色有一个权限列表。

```text
admin ──→ 管理员 ──→ user:view
                    user:create
                    user:change-role
                    report:view

alice ──→ 普通用户 ──→ report:view
```

`user:create` 是我们约定的字符串。冒号没有特殊的 C# 功能，只是方便读懂“用户相关功能中的创建操作”。

| 权限字符串 | 含义 |
| --- | --- |
| user:view | 查看用户 |
| user:create | 创建用户 |
| user:change-role | 修改用户角色 |
| report:view | 查看报表 |

还要区分两个问题：

- **登录**：验证用户名和密码，确认你是谁。
- **权限检查**：根据你的角色，判断你能做什么。

alice 能成功登录，但不能创建用户，就是这两个过程的区别。

查看自己的信息是所有已登录用户都能使用的功能，没有单独的权限字符串。

## 3. 项目为什么要分层

先看当前目录：

```text
RBAC/
├── Program.cs
├── Models/
│   ├── User.cs
│   └── Role.cs
├── Data/
│   └── InMemoryDatabase.cs
├── Service/
│   └── IRbacService.cs
├── ServiceImpl/
│   └── RbacServiceImpl.cs
├── Utils/
│   ├── Manager/SystemManager.cs
│   └── Security/PasswordHasher.cs
└── Tests/
    ├── RBAC.Tests.csproj
    └── Program.cs
```

各部分的职责如下：

| 部分 | 回答的问题 | 例子 |
| --- | --- | --- |
| Models | 数据长什么样 | 用户有用户名、密码散列、角色 |
| Data | 数据放在哪里，初始数据是什么 | 用 List 保存用户和角色 |
| Service | 对外提供哪些业务功能 | 登录、创建用户、修改角色 |
| ServiceImpl | 这些业务具体怎么做 | 校验权限、检查重名、添加用户 |
| SystemManager | 怎样和终端用户交互 | 显示菜单、读取输入、输出提示 |
| Program | 程序从哪里开始，怎样组装对象 | 创建数据、业务和菜单对象 |

例如创建用户时，菜单负责问“用户名是什么”，业务层负责判断“能不能创建、是否重名”，Data 保存创建后的对象。

这是一个项目中按文件夹组织的分层。文件夹方便管理文件；真正让职责分开的，是各个类负责的事情和它们之间的调用方式。

`ServiceImpl` 中的 `Impl` 是 Implementation，也就是“实现”的缩写。

学习时可以按下面顺序阅读：

```text
User、Role → Data → Service → ServiceImpl → Program → SystemManager
```

实际运行则从 `Program.Main()` 开始。

## 4. 先认识类、对象和属性

对应文件：`Models/User.cs` 和 `Models/Role.cs`。

### 4.1 类是描述，对象是具体的数据

当前的用户类是：

```csharp
public class User
{
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public Role Role { get; set; } = new Role();
}
```

`User` 描述“一个用户有哪些信息”。要创建某个具体用户，需要 `new`：

```csharp
User user = new User();
user.Username = "alice";
```

逐段理解第一行：

- 左边的 `User`：变量类型。
- `user`：变量名称。
- `new User()`：创建一个新的 User 对象。
- `=`：把新对象的引用交给变量 user 保存。

`User` 是类名，`user` 是变量名，大小写不同。变量可以叫 `alice`、`newUser`，但它的类型仍然是 `User`。

### 4.2 get 和 set 是什么

```csharp
public string Username { get; set; } = "";
```

这是一条属性声明：

- `public`：类外部也可以访问。
- `string`：属性保存字符串。
- `get`：允许读取，例如 `Console.WriteLine(user.Username)`。
- `set`：允许赋值，例如 `user.Username = "bob"`。
- `= ""`：初始值是空字符串。

初学时可以先把属性理解成“对象提供给外部读写的信息”。

### 4.3 一个对象可以保存另一个对象

```csharp
public Role Role { get; set; } = new Role();
```

第一个 `Role` 是类型，第二个 `Role` 是属性名称。

它表达的是：“用户对象中有一个属性，这个属性指向一个角色对象。”因此可以连续访问：

```csharp
user.Role.Name
user.Role.Permissions
```

第一行读取角色名，第二行读取角色的权限列表。

`new Role()` 提供一个初始的空角色。创建演示用户或新用户时，代码还会把它替换成 Data 中预设的角色。

### 4.4 namespace 和 using

```csharp
namespace RBAC.Models;
```

命名空间可以理解为给类加上分类名称。这个 User 类的完整名称是 `RBAC.Models.User`。

其他文件写上：

```csharp
using RBAC.Models;
```

就可以直接使用 `User`。`using` 方便使用类型名称，不会自动创建对象，也不会执行 User 中的代码。

项目开启了 `ImplicitUsings`，一些常用命名空间会自动导入，因此可以直接写 `Console` 和 `List`。

## 5. 用 List 保存用户和角色

### 5.1 为什么需要集合

单个 `User` 变量表示一个用户；系统要保存多个用户，就可以使用 `List<User>`。

```csharp
List<User> users = new List<User>();
```

`List<User>` 的意思是“元素类型为 User 的列表”。尖括号里的类型决定这个列表保存什么。

| 写法 | 保存什么 |
| --- | --- |
| List<User> | 用户对象 |
| List<Role> | 角色对象 |
| List<string> | 字符串，例如权限码 |

角色类中的这行代码就创建了权限列表：

```csharp
public List<string> Permissions { get; set; } = new List<string>();
```

### 5.2 先掌握四种操作

下面是独立的语法示例，不需要额外添加进项目：

```csharp
List<string> permissions = new List<string>();
permissions.Add("report:view");       // 添加一个元素
Console.WriteLine(permissions.Count); // 元素数量：1
Console.WriteLine(permissions[0]);     // 第一个元素

foreach (string permission in permissions)
{
    Console.WriteLine(permission);    // 逐个读取
}
```

索引从 `0` 开始。当前项目先添加管理员角色，再添加普通用户角色，因此：

```text
Roles[0] → 管理员
Roles[1] → 普通用户
```

`Roles[1]` 表示列表中第二个元素。它能被用作新用户的默认角色，是因为初始化代码保证了这个顺序。

### 5.3 只有 get，为什么还能 Add

Data 中的声明是：

```csharp
public List<User> Users { get; } = new List<User>();
```

只有 `get` 表示外部不能把整个属性重新赋成另一份列表：

```csharp
// database.Users = new List<User>(); // 不能这样替换属性
```

但可以修改当前列表的内容：

```csharp
database.Users.Add(user);
```

“不能替换列表”和“不能修改列表内容”是两件不同的事。当前这个属性允许添加元素。

## 6. 读懂 Data：初始化数据

对应文件：`Data/InMemoryDatabase.cs`。

### 6.1 构造函数什么时候执行

```csharp
public InMemoryDatabase()
{
    // 创建角色和用户
}
```

它和类同名，没有返回类型，是构造函数。执行下面这句时，构造函数会自动运行：

```csharp
InMemoryDatabase database = new InMemoryDatabase();
```

当前程序启动时只创建一次这个 database。菜单循环和退出登录都不会重新创建它。

### 6.2 初始化按四步进行

第一步，创建管理员角色并添加权限：

```csharp
Role adminRole = new Role();
adminRole.Name = "管理员";
adminRole.Permissions.Add("user:view");
adminRole.Permissions.Add("user:create");
adminRole.Permissions.Add("user:change-role");
adminRole.Permissions.Add("report:view");
```

第二步，创建普通用户角色，只添加报表权限。

第三步，把两个角色放入 Roles：

```csharp
Roles.Add(adminRole);
Roles.Add(userRole);
```

第四步，创建用户，设置角色，再放入 Users：

```csharp
User admin = new User();
admin.Username = "admin";
admin.PasswordHash = PasswordHasher.Hash("Admin123!");
admin.Role = adminRole;
Users.Add(admin);
```

`PasswordHasher.Hash()` 的用途是生成密码散列。这里先知道它接收密码、返回一个字符串即可。

### 6.3 对象之间通过引用连接

```csharp
admin.Role = adminRole;
```

这句把已有角色对象的引用赋给用户的 Role 属性，不会复制出一个独立的管理员角色。

```text
Roles[0] ────────┐
                ├──→ 同一个管理员角色对象
admin.Role ─────┘
```

同样，`Users.Add(admin)` 保存的也是这个用户对象的引用。之后通过列表找到并修改该用户，修改的是同一个对象。

这能解释后面的问题：“为什么只给 targetUser.Role 赋值，就能修改列表中的用户？”

## 7. 读懂 Service：接口和实现

对应文件：`Service/IRbacService.cs`、`ServiceImpl/RbacServiceImpl.cs`。

### 7.1 接口描述有哪些功能

接口里有这样的声明：

```csharp
bool Login(string username, string password);
string CreateUser(string username, string password);
```

它们规定了方法名称、参数和返回类型，还没有写具体步骤。

例如第一行表示：“提供一个 Login 方法，接收两个字符串，返回 bool。”

`IRbacService` 开头的 `I` 是 C# 接口命名的常见约定。接口文件中的分号意味着这条声明到这里结束。

### 7.2 实现类完成具体功能

```csharp
public class RbacServiceImpl : IRbacService
```

这里的冒号表示 RbacServiceImpl 实现 IRbacService 接口。实现类需要提供接口要求的功能。

接口是类型约定，不会在运行时多出一个单独的“接口对象”。实际执行的方法在 RbacServiceImpl 中。

### 7.3 方法的返回值分别代表什么

| 方法或属性 | 类型 | 在本项目中的含义 |
| --- | --- | --- |
| CurrentUser | User? | 当前登录的用户，未登录时为 null |
| Login | bool | 用户名与密码是否验证成功 |
| Logout | void | 清除当前用户，不返回结果 |
| HasPermission | bool | 当前用户是否有指定权限 |
| GetUsers | List<User> | 可查看的用户列表，无权限时为空列表 |
| CreateUser | string | 创建结果或失败原因 |
| ChangeRole | string | 修改结果或失败原因 |
| ViewReport | string | 报表内容或权限不足提示 |

返回字符串是当前入门版的简单做法。调用者可以直接把它输出到终端。

### 7.4 CurrentUser 的问号和 private set

```csharp
public User? CurrentUser { get; private set; }
```

- `User?`：这个属性可以保存 User 对象，也可以为 null。
- `get`：外部可以读取当前用户。
- `private set`：只有 RbacServiceImpl 内部可以给这个属性赋值。

未登录时没有当前用户，所以使用 null 很自然。

`private set` 限制的是属性赋值。它不意味着这个 User 对象内部所有属性都变成只读；入门版的模型仍然是可以修改的普通对象。

## 8. 读懂 Program：把对象连接起来

对应文件：`Program.cs`。

### 8.1 Main 是主程序入口

```csharp
private static void Main()
```

当前主项目运行时从这里开始。`static` 表示不需要先创建 Program 对象就能调用这个入口。

开头两行设置终端输入输出编码：

```csharp
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
```

它们用于帮助终端正确读写中文，可以先理解用途。

### 8.2 最重要的是四行组装代码

```csharp
InMemoryDatabase database = new InMemoryDatabase();
IRbacService service = new RbacServiceImpl(database);
SystemManager manager = new SystemManager(service);
manager.Run();
```

按顺序理解：

1. 创建 database，构造函数初始化角色和用户。
2. 创建业务对象，把 database 交给它使用。
3. 创建菜单对象，把 service 交给它调用。
4. 启动菜单循环。

第二行左边是接口类型，右边是实现类对象。它表达的是“变量 service 按照 IRbacService 的约定使用 RbacServiceImpl 对象”。

### 8.3 参数怎样保存到类里面

业务类的构造函数是：

```csharp
private InMemoryDatabase _database;

public RbacServiceImpl(InMemoryDatabase database)
{
    _database = database;
}
```

这里有两个不同的名称：

- `database`：构造函数的参数。
- `_database`：类的字段，其他业务方法也可以使用它。

赋值后，字段保存传入对象的引用。构造函数结束后，Login、CreateUser 等方法仍然可以通过 `_database` 访问数据。

以下划线开头是私有字段的常见命名习惯，没有特殊语法作用。

这种通过构造函数传入依赖对象的做法也叫“构造函数依赖注入”。先掌握“创建对象，再把对象传进去”就够了。

主入口显式创建的是数据、业务和菜单三个主要对象；初始化数据时还会创建角色、用户和列表对象。

## 9. 读懂登录和注销

对应文件：`ServiceImpl/RbacServiceImpl.cs`。

### 9.1 Login 的完整逻辑

```csharp
public bool Login(string username, string password)
{
    CurrentUser = null;
    foreach (User user in _database.Users)
    {
        if (user.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase)
            && PasswordHasher.Verify(password, user.PasswordHash))
        {
            CurrentUser = user;
            return true;
        }
    }

    return false;
}
```

第一步，把旧登录状态清空。这样一次登录失败之后不会意外保留之前的身份。

第二步，遍历所有用户。每次循环中的 user 是当前取出的那个用户对象。

第三步，同时比较用户名和密码：

- `username.Trim()` 去掉输入用户名两端的空白。
- `Equals(..., StringComparison.OrdinalIgnoreCase)` 比较字符串时忽略大小写，因此 ADMIN 也能匹配 admin。
- `PasswordHasher.Verify()` 检查输入密码是否匹配已保存的散列。
- `&&` 表示两个条件都成立才算成功；左边不成立时，右边不会执行。

第四步，匹配成功后记录 CurrentUser 并返回 true。

如果循环结束还没找到匹配项，就返回 false。

### 9.2 跟着 alice 的登录走一遍

输入 `alice / User123!` 时：

| 阶段 | 正在比较谁 | 结果 |
| --- | --- | --- |
| 开始 | 清空 CurrentUser | CurrentUser 为 null |
| 第一次循环 | admin | 用户名不匹配，继续 |
| 第二次循环 | alice | 用户名与密码都匹配 |
| 登录成功 | 保存 alice 对象 | 返回 true |

如果密码错误，最终会走到循环后的 `return false`。

### 9.3 return 和 break 的区别

- `return true`：立即结束整个方法，把 true 返回给调用者。
- `break`：结束当前最内层的循环或 switch，继续执行它后面的代码。

登录找到用户后，整个方法的任务已经完成，所以使用 return。

### 9.4 注销只需要清除当前用户

```csharp
public void Logout()
{
    CurrentUser = null;
}
```

这不会清空 `_database.Users`。因此退出登录后，刚创建的用户还在。

## 10. 读懂权限检查和报表

### 10.1 HasPermission 是核心方法

```csharp
public bool HasPermission(string permission)
{
    if (CurrentUser == null)
    {
        return false;
    }

    foreach (string item in CurrentUser.Role.Permissions)
    {
        if (item == permission)
        {
            return true;
        }
    }

    return false;
}
```

先判断有没有登录，再遍历当前角色的权限列表。找到了就返回 true，遍历完还没找到就返回 false。

这段访问链：

```csharp
CurrentUser.Role.Permissions
```

就是“当前用户 → 他的角色 → 角色的权限列表”。它把 RBAC 的概念直接翻译成了 C# 属性访问。

### 10.2 用普通用户举例

alice 的权限列表只有：

```text
report:view
```

因此：

```csharp
service.HasPermission("report:view"); // true
service.HasPermission("user:create"); // false
service.HasPermission("unknown");     // false
```

当前权限码使用 `==` 精确比较，大小写要一致，`REPORT:VIEW` 不会匹配 `report:view`。用户名的比较方式与权限码不同，要以各自的代码为准。

### 10.3 权限来自列表，角色名称用于识别角色

业务是否允许执行，主要由权限列表决定。角色叫“管理员”，也需要拥有对应权限字符串，`HasPermission()` 才会通过。

后面的“保留最后一个管理员”规则会比较角色名，这是该规则额外使用的身份判断。

### 10.4 ViewReport 把权限检查用于业务

```csharp
public string ViewReport()
{
    if (!HasPermission("report:view"))
    {
        return "权限不足，不能查看报表。";
    }

    return "演示报表：订单数 12，销售额 1280 元。";
}
```

`!` 表示取反，所以 `!HasPermission(...)` 就是“没有这个权限”。

先拒绝无权限情况，再执行业务，是一种容易阅读的写法。这里报表是固定文本，重点是掌握业务入口的权限判断。

## 11. 读懂查看用户和创建用户

### 11.1 GetUsers 为什么新建一个列表

```csharp
public List<User> GetUsers()
{
    List<User> result = new List<User>();
    if (HasPermission("user:view"))
    {
        foreach (User user in _database.Users)
        {
            result.Add(user);
        }
    }

    return result;
}
```

如果没有权限，result 保持为空；如果有权限，就把用户逐个放入结果列表。

返回的是一个新列表，但其中的 User 仍然引用原来的用户对象。新列表和用户对象的复制不是同一件事。

菜单层还会先检查一次权限，用于输出清楚的“权限不足”提示。业务层的检查保证即使直接调用 GetUsers，也不会返回无权查看的列表。

### 11.2 CreateUser 按五步处理

第一步：检查 `user:create` 权限。没有权限就返回提示。

第二步：整理和检查输入。

```csharp
username = username.Trim();
if (username.Length < 2 || username.Length > 20 || string.IsNullOrWhiteSpace(username))
{
    return "用户名需要 2～20 个字符，不能全为空白。";
}
```

`||` 表示“或者”。这里任意一个条件成立，就拒绝创建。

`string.IsNullOrWhiteSpace()` 用来识别 null、空字符串或全是空白的字符串。当前用户名先经过 Trim，空白用户名会变成空字符串。

密码要求至少 6 个字符且不能全为空白。密码不会调用 Trim，输入的首尾空格也是密码的一部分。

第三步：检查用户名重复。

```csharp
foreach (User user in _database.Users)
{
    if (user.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
    {
        return "用户名已存在。";
    }
}
```

第四步：创建对象，设置属性，指定默认角色。

```csharp
User newUser = new User();
newUser.Username = username;
newUser.PasswordHash = PasswordHasher.Hash(password);
newUser.Role = _database.Roles[1];
```

第五步：真正保存进集合并返回结果。

```csharp
_database.Users.Add(newUser);
return "创建成功，新用户的角色是普通用户。";
```

注意区分 `new User()` 和 `Users.Add(newUser)`：前者创建对象，后者把它加入系统维护的用户列表。

### 11.3 创建用户的判断顺序

```text
检查权限
  ↓ 通过
检查用户名和密码
  ↓ 通过
检查用户名是否重复
  ↓ 不重复
创建对象 → 设置角色 → 加入 Users → 返回成功提示
```

任何一步失败都会 return，因此后面的创建和保存代码不会执行。

## 12. 读懂修改角色

对应方法：`RbacServiceImpl.ChangeRole()`。

这是业务层最长的方法，可以拆成四段阅读。

### 12.1 先检查权限

```csharp
if (!HasPermission("user:change-role"))
{
    return "权限不足，不能修改角色。";
}
```

普通用户不能调用它把自己改成管理员。

### 12.2 再找到要修改的用户

```csharp
User? targetUser = null;
foreach (User user in _database.Users)
{
    if (user.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase))
    {
        targetUser = user;
        break;
    }
}
```

targetUser 一开始为 null，代表“尚未找到”。匹配后把用户对象的引用保存下来，用 break 结束查找循环。

这里使用 break，是因为找到用户后还要执行后面的角色查找和修改代码。

循环结束后，代码检查 targetUser 是否仍为 null。如果是，就返回“用户不存在”。

### 12.3 查找角色并保留至少一位管理员

接下来遍历 Roles，找出名称和输入相同的角色。角色名输入会先去掉首尾空白，当前只支持预设的“管理员”和“普通用户”。

找到角色后，通过循环统计管理员数量：

```csharp
int adminCount = 0;
foreach (User user in _database.Users)
{
    if (user.Role.Name == "管理员")
    {
        adminCount++;
    }
}
```

`adminCount++` 表示数量加一。

然后检查三个条件：

```csharp
targetUser.Role.Name == "管理员"
role.Name != "管理员"
adminCount == 1
```

它们同时成立，说明“系统唯一的管理员正准备被改成其他角色”，此时拒绝修改。

如果已经有两位管理员，就允许其中一位降为普通用户。

### 12.4 真正的角色修改只有一行

```csharp
targetUser.Role = role;
```

它把目标用户的 Role 属性指向新角色。因为 targetUser 引用列表中的同一个用户对象，所以列表中的用户也随之改变。

权限变化立即生效：每次调用 HasPermission 都读取当前用户的 Role，没有提前缓存旧权限。

修改为同一个角色也会返回“角色修改成功”，当前版本没有额外判断“角色没有变化”。

## 13. 读懂终端菜单

对应文件：`Utils/Manager/SystemManager.cs`。

### 13.1 while 让程序持续等待操作

```csharp
bool running = true;
while (running)
{
    // 显示菜单、读取输入、处理操作
}
```

只要 running 为 true，就继续下一轮。一次操作结束后，程序再次显示菜单。

登录状态决定显示哪种菜单：

```csharp
if (_service.CurrentUser == null)
{
    // 未登录菜单
}
else
{
    // 登录后菜单
}
```

### 13.2 ReadLine 返回字符串

输入数字 `3` 后，`Console.ReadLine()` 读到的是字符串 `"3"`。因此 switch 写成：

```csharp
case "3":
    running = CreateUser();
    break;
```

当前菜单不需要把输入转为 int，直接比较字符串即可。输入其他内容时，default 输出“无效选项”。

### 13.3 两个 Login 方法的返回值不同

项目里有两个同名方法，它们属于不同的类：

| 方法 | 负责什么 | bool 表示什么 |
| --- | --- | --- |
| RbacServiceImpl.Login | 验证账号密码 | 是否登录成功 |
| SystemManager.Login | 读取输入、调用业务、输出提示 | 是否继续菜单循环 |

菜单方法的逻辑是：读取用户名密码，如果输入结束就返回 false；否则执行登录、显示成功或失败提示，然后返回 true。

所以输入错误密码以后，菜单层仍返回 true，表示“继续运行，让用户重试”。这不代表登录成功。

SystemManager 的 CreateUser 和 ChangeRole 也使用这个约定：业务失败仍可继续菜单；终端输入结束才返回 false。

### 13.4 null 和空字符串有什么不同

```csharp
string? username = Console.ReadLine();
```

- 直接按回车，通常得到 `""`，即空字符串。
- 输入流结束，得到 null，例如重定向的输入读完了。

当前菜单遇到输入结束会退出循环，并在 Run 末尾注销、输出“再见”。空字符串则会进入普通的校验或无效选项处理。

### 13.5 switch 中的 break 不会退出整个程序

菜单 case 末尾的 break 只结束 switch。下一步仍会回到 while 循环。

登录后输入 Q 时，代码在 switch 之前执行 break，它结束的是外层 while，因此程序退出。

判断 break 结束哪一层，要看它位于哪个最内层的循环或 switch 中。

### 13.6 界面检查与业务检查

创建用户之前，菜单先通过 HasPermission 判断是否需要读取新用户名和密码。没有权限时立即提示。

业务方法 CreateUser 内部还会再次检查权限。即使以后另一个调用者跳过菜单，直接调用业务方法，也会得到“权限不足”的结果。

界面层负责交互体验，业务层负责保证业务入口执行权限规则。

## 14. 把一次操作从头跟到尾

以管理员创建 bob 为例：

```text
Program.Main()
  ├─ new InMemoryDatabase()      初始化角色、admin、alice
  ├─ new RbacServiceImpl(...)    保存对数据对象的引用
  └─ new SystemManager(...)
       └─ Run()
            ├─ 输入 1 → 菜单 Login() → 业务 Login()
            │                            └─ CurrentUser = admin
            └─ 输入 3 → 菜单 CreateUser()
                           ├─ 检查权限、读取 bob 和密码
                           └─ 业务 CreateUser(...)
                                ├─ 检查权限
                                ├─ 校验输入与重复用户名
                                ├─ 创建 User 并指定普通用户角色
                                └─ Users.Add(newUser)
```

业务方法返回字符串后，调用一层层返回到菜单，Console.WriteLine 输出结果，随后进入下一轮菜单循环。

再想一遍：业务代码中没有 Console.ReadLine，因为它不需要知道用户名是从终端、文件还是其他界面传来的，只负责处理收到的参数。

## 15. 在 Rider 中调试

### 15.1 建议设置的断点

在相应代码行左侧的编辑器边栏点击，设置断点，然后使用 **Debug** 启动 RBAC：

1. `Program.cs` 中创建 database 的一行。
2. `RbacServiceImpl.Login()` 中的 foreach。
3. 登录成功时的 `CurrentUser = user`。
4. `HasPermission()` 中的 `if (item == permission)`。
5. `CreateUser()` 中的 `_database.Users.Add(newUser)`。
6. `ChangeRole()` 中的 `targetUser.Role = role`。

停在某一行时，该行通常还没执行。执行一步之后，再观察变量变化。

### 15.2 先掌握三种调试动作

| 动作 | 用途 |
| --- | --- |
| Step Over，单步跳过 | 执行当前行，停在下一步，不进入调用的方法内部 |
| Step Into，单步进入 | 进入当前调用的方法，查看具体实现 |
| Resume，继续 | 运行到下一个断点或程序结束 |

Rider 快捷键会随键位方案变化，可以直接使用调试工具栏按钮。

### 15.3 三次观察任务

**任务一：观察登录。**

用 alice 登录，在 Login 中观察 username、user.Username、CurrentUser。确认第一次循环是 admin，第二次才是 alice。

**任务二：观察权限。**

用 alice 查看用户，在 HasPermission 中观察 permission 为 `user:view`，item 为 `report:view`，因此最终返回 false。

**任务三：观察保存。**

用 admin 创建 bob，在 Add 前后观察 `_database.Users.Count`：它会从 2 变为 3。展开第三个用户，查看用户名和角色。

可以使用 Variables、Watches 等窗口观察对象。密码散列属于实现细节，当前调试重点放在角色和权限。

## 16. 密码工具先掌握调用方式

对应文件：`Utils/Security/PasswordHasher.cs`。

当前阶段掌握这两个调用即可：

```csharp
string storedHash = PasswordHasher.Hash("Pass123!");
bool correct = PasswordHasher.Verify("Pass123!", storedHash);
```

- Hash：接收原始密码，返回用于保存的散列字符串。
- Verify：接收输入密码和保存的散列，返回是否匹配。

Hash 是静态方法，所以可以用 `PasswordHasher.Hash(...)` 调用，不需要 new 一个 PasswordHasher。

这里使用 PBKDF2、随机盐和 SHA-256。散列用于校验，不能通过“解密”拿回原始密码；随机盐让相同密码两次生成的保存字符串也可能不同，因此登录时使用 Verify，不能简单重新 Hash 后比较两个字符串。

等你熟悉业务流程后，再看字节数组、Base64、异常处理和密码算法。它们不会影响你先理解用户、角色、权限的关系。

## 17. 验证、常见问题和练习

### 17.1 运行已有验证程序

在项目根目录执行：

```powershell
dotnet build RBAC.sln -m:1
dotnet run --project Tests/RBAC.Tests.csproj --no-build
```

本机也可以使用完整路径：

```powershell
& "$env:USERPROFILE\.dotnet\dotnet.exe" build RBAC.sln -m:1
& "$env:USERPROFILE\.dotnet\dotnet.exe" run --project Tests/RBAC.Tests.csproj --no-build
```

`-m:1` 表示单进程构建。第二条命令的 `--no-build` 表示运行刚构建好的版本；修改代码后需要重新构建。

当前初始版本包含 36 项检查，覆盖登录、权限拦截、用户校验、角色变化、最后一位管理员保护以及菜单流程。修改项目后，可以运行它们观察行为是否发生意外变化。

Tests 中也有 Program 和 Main，但它属于另一个 `.csproj`。主项目通过 `<Compile Remove="Tests\**\*.cs" />` 排除这些源文件，因此两个入口各自属于不同项目。

### 17.2 常见问题

| 现象 | 原因或检查方法 |
| --- | --- |
| 找不到 dotnet | 使用前面的完整路径命令，或在 Rider 中运行 |
| 启动后只看到检查结果 | 当前运行的是 RBAC.Tests，切换到 RBAC |
| bob 重启后消失 | 数据只保存在内存，这是当前版本的行为 |
| 普通用户能登录却不能查看用户 | 登录和权限检查是两个过程 |
| 输入“管理员”以外的名称失败 | 当前只预设管理员、普通用户两个角色 |
| 用户名 ADMIN 能登录，权限码大写却不匹配 | 用户名忽略大小写，权限码使用精确比较 |
| 第一个菜单的 0 和登录后的 0 不同 | 前者退出程序，后者退出登录 |
| 修改代码后运行结果没变 | 重新构建，并确认运行的是当前项目 |
| 修改角色后其他账号权限也变化 | 检查是否修改了共享 Role 对象的 Permissions，而不是用户的 Role 引用 |
| 想删除最后一个管理员身份却失败 | ChangeRole 中有保留至少一位管理员的规则 |

### 17.3 练习一：只读代码，预测结果

先不要运行，回答以下问题，再用程序确认：

1. 未登录时，HasPermission("report:view") 返回什么？
2. alice 登录后，GetUsers() 返回什么？
3. 新用户创建后，默认是什么角色？
4. 输入错误密码后，CurrentUser 是什么？
5. 把当前唯一的管理员改成普通用户，会发生什么？
6. 给 bob 设置管理员角色后，为什么 bob 能创建用户？

参考答案：

1. false，因为没有 CurrentUser。
2. 空列表，因为没有 `user:view` 权限。
3. 普通用户，因为使用了 Roles[1]。
4. null，因为 Login 开头清除了旧状态。
5. 返回拒绝修改的提示，原角色保持不变。
6. HasPermission 从 bob.Role.Permissions 中找到了 `user:create`。

### 17.4 练习二：增加一个预设“访客”角色

目标：访客能登录和查看自己的信息，但没有报表权限。

在 `InMemoryDatabase` 构造函数中，放在两个现有角色加入 Roles 之后，添加：

```csharp
Role guestRole = new Role();
guestRole.Name = "访客";
Roles.Add(guestRole);
```

不调用 Permissions.Add，所以它的权限列表为空。把角色放在现有两个角色之后，Roles[1] 仍然代表普通用户。

随后更新以下提示，使它们列出“访客”：

- SystemManager.ChangeRole 中的新角色输入提示。
- RbacServiceImpl.ChangeRole 末尾的“角色不存在”提示。

角色查找已经遍历整个 Roles，因此不需要为访客额外添加一个 if 分支。

验证步骤：创建 bob，将其改为访客，再用 bob 登录。选 1 能查看角色，选 5 应提示权限不足。

### 17.5 练习三：为报表加上标题

先只修改 ViewReport 最后的返回文本，例如添加“本月销售报表”字样。运行后确认菜单 5 显示新内容。

再尝试把报表展示分成两个变量：

```csharp
int orderCount = 12;
decimal sales = 1280m;
return "演示报表：订单数 " + orderCount + "，销售额 " + sales + " 元。";
```

这段应放在权限检查通过后，用来替换原来返回固定报表的那一行。`decimal` 常用于金额，`m` 表示 decimal 字面量。

### 17.6 练习四：补充用户名规则

目标：用户名中不能包含普通空格。

在 CreateUser 已有的用户名长度校验后、重复检查前添加：

```csharp
if (username.Contains(" "))
{
    return "用户名不能包含空格。";
}
```

验证：创建 `a b` 应失败，创建 `bob` 应成功。Contains(" ") 只检查普通空格；等熟悉字符操作后，再练习判断制表符等其他空白。

### 17.7 练习五：增加一个受权限保护的功能

目标：管理员可以查看系统说明，普通用户不可以。按四处改动完成：

1. 在 Data 中给管理员添加 `system:info` 权限。
2. 在 IRbacService 中声明 `string ViewSystemInfo();`。
3. 在 RbacServiceImpl 中实现该方法。
4. 在 SystemManager 中显示菜单 6，并添加对应 case。

业务实现可以写成：

```csharp
public string ViewSystemInfo()
{
    if (!HasPermission("system:info"))
    {
        return "权限不足，不能查看系统说明。";
    }

    return "这是一个使用 C# 编写的终端 RBAC 学习项目。";
}
```

菜单的 switch 中添加：

```csharp
case "6":
    Console.WriteLine(_service.ViewSystemInfo());
    break;
```

重新构建，用 admin 和 alice 分别尝试。只加菜单而没加接口，编译器会提示找不到方法；只写业务方法但没授予权限，管理员也会被拒绝。可以通过这次练习检查自己是否理解了各层如何连接。

### 17.8 读完后检查自己是否理解

可以不看教程，试着解释下面五件事：

- Program 怎样创建并连接数据、业务和菜单对象。
- Login 怎样找到用户，并把登录状态保存到 CurrentUser。
- HasPermission 怎样沿着用户、角色、权限找到结果。
- CreateUser 为什么既需要 new User，也需要 Users.Add。
- ChangeRole 为什么只修改一个属性，就能改变用户的权限。

如果能解释清楚，就可以尝试从空白控制台项目重新搭建一遍：先写模型与数据，再实现登录和权限检查，最后接上终端菜单。

using System.Text;
using RBAC.Data;
using RBAC.Service;
using RBAC.ServiceImpl;
using RBAC.Utils.Manager;

namespace RBAC;

internal class Program
{
    private static void Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        // 只创建三个对象：数据、业务、菜单。
        InMemoryDatabase database = new InMemoryDatabase();
        IRbacService service = new RbacServiceImpl(database);
        SystemManager manager = new SystemManager(service);
        manager.Run();
    }
}

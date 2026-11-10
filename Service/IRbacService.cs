using RBAC.Models;

namespace RBAC.Service;

// 接口是“功能清单”，具体做法写在 ServiceImpl 中。
public interface IRbacService
{
    User? CurrentUser { get; }
    bool Login(string username, string password);
    void Logout();
    bool HasPermission(string permission);
    List<User> GetUsers();
    string CreateUser(string username, string password);
    string ChangeRole(string username, string roleName);
    string ViewReport();
}

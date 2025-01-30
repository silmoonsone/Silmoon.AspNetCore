using Microsoft.AspNetCore.Http;
using Silmoon.Extension.Models.Identities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Services.Interfaces
{
    public interface ISilmoonAuthService
    {
        SilmoonAuthServiceOption Options { get; }
        Task SignIn<TUser>(TUser User, bool AddEnumRole) where TUser : class, IDefaultUserIdentity;
        Task SignIn<TUser>(TUser User, string NameIdentifier = null) where TUser : class, IDefaultUserIdentity;
        Task SignIn<TUser, TTCustomerRoles>(TUser User, bool AddEnumRole, TTCustomerRoles CustomerRoles, string NameIdentifier = null) where TUser : class, IDefaultUserIdentity where TTCustomerRoles : Enum;
        Task SignIn<TUser, TTCustomerRoles>(TUser User, bool AddEnumRole, TTCustomerRoles[] CustomerRoles, string NameIdentifier = null) where TUser : class, IDefaultUserIdentity where TTCustomerRoles : Enum;
        Task SignIn<TUser>(TUser User, bool AddEnumRole, string[] CustomerRoles, string NameIdentifier = null) where TUser : class, IDefaultUserIdentity;
        Task<bool> SignOut();
        Task<TUser> GetUser<TUser>() where TUser : class, IDefaultUserIdentity;
        Task<TUser> GetUser<TUser>(string UserToken, string Name = null, string NameIdentifier = null) where TUser : class, IDefaultUserIdentity;
        Task ReloadUser<TUser>() where TUser : class, IDefaultUserIdentity;
        Task<bool> IsSignIn();
        Task<string[]> GetRoles();

        void SetUserSessionFlag<T>(string Name, T Data);
        T GetUserSessionFlag<T>(string Name);

        bool IsInRole(params string[] Roles);
        bool IsInRole(params Enum[] Roles);
        Task<bool> IsInRoleAsync(params string[] Roles);
        Task<bool> IsInRoleAsync(params Enum[] Roles);


        Task<IDefaultUserIdentity> GetUserData(string Username, string NameIdentifier);
        Task<IDefaultUserIdentity> GetUserData(string Username, string NameIdentifier, string UserToken);
        Task OnSignIn(HttpContext httpContext, RequestDelegate requestDelegate, string username, string password);
        Task OnSignOut(HttpContext httpContext, RequestDelegate requestDelegate);
    }
}

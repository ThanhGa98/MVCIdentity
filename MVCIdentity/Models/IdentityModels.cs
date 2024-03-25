using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using MVCIdentity.App_Start;

namespace MVCIdentity.Models
{
    public class ApplicationUser : IdentityUser
    {

        public string Email { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }

    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole,string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    {
        //IdentityConnection defines the LocalDB in the web.config connection string tag
        public ApplicationDbContext() : base("IdentityConnection")
        {

        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public class IdentityManager
        {
            // Swap ApplicationRole for IdentityRole:
            RoleManager<ApplicationRole> _roleManager = new RoleManager<ApplicationRole>(
                new RoleStore<ApplicationRole>(new ApplicationDbContext()));

            UserManager<ApplicationUser> _userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(new ApplicationDbContext()));

            ApplicationDbContext _db = new ApplicationDbContext();


            public bool RoleExists(string name)
            {
                return _roleManager.RoleExists(name);
            }


            public bool CreateRole(string name, string description = "")
            {
                // Swap ApplicationRole for IdentityRole:
                var idResult = _roleManager.Create(new ApplicationRole(name, description));
                return idResult.Succeeded;
            }


            public bool CreateUser(ApplicationUser user, string password)
            {
                var idResult = _userManager.Create(user, password);
                return idResult.Succeeded;
            }


            public bool AddUserToRole(string userId, string roleName)
            {
                var idResult = _userManager.AddToRole(userId, roleName);
                return idResult.Succeeded;
            }


            public void ClearUserRoles(string userId)
            {
                var user = _userManager.FindById(userId);
                var currentRoles = new List<IdentityUserRole>();

                currentRoles.AddRange(user.Roles);
                foreach (var role in currentRoles)
                {
                    _userManager.RemoveFromRole(userId, _db.Roles.Find(role.RoleId)?.Id);
                }
            }


            public void RemoveFromRole(string userId, string roleName)
            {
                _userManager.RemoveFromRole(userId, roleName);
            }


            public void DeleteRole(string roleId)
            {
                var roleUsers = _db.Users.Where(u => u.Roles.Any(r => r.RoleId == roleId));
                var role = _db.Roles.Find(roleId);

                foreach (var user in roleUsers)
                {
                    this.RemoveFromRole(user.Id, role.Name);
                }
                _db.Roles.Remove(role);
                _db.SaveChanges();
            }
        }
    }
}



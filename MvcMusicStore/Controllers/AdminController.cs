using MvcMusicStore.DataContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.EntityFramework;
using MvcMusicStore.Models;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace MvcMusicStore.Controllers
{
    public class AdminController : Controller
    {
        private IdentityDb identityDb = new IdentityDb();
        // GET: Admin
        [Authorize(Roles = "AppAdmin")]
        public ActionResult Index()
        {
            return View(identityDb.Roles.OrderBy(r => r.Name));
        }
        // GET: Admin/UsersInRole?name=banana
        [Authorize(Roles = "AppAdmin")]
        public ActionResult UsersInRole(String name)
        {
            // store name of the role in viewbag variable
            ViewBag.RoleName = name;
            // create a list of users who are members of the selected role
            List<IdentityUser> userMembers = new List<IdentityUser>();
            foreach (var user in identityDb.Roles.Single(r => r.Name == name).Users)
            {
                userMembers.Add(identityDb.Users.Find(user.UserId));
            }
            ViewBag.Members = userMembers;
            // the model will contain a list of all users
            return View(identityDb.Users.OrderBy(u => u.UserName));
        }

        // POST:Admin/UsersInRole?name=banana
        [Authorize(Roles = "AppAdmin")]
        [HttpPost]
        public async Task<ActionResult> UsersInRole(String name, FormCollection Form)
        {
            var store = new UserStore<ApplicationUser>(identityDb);
            var manager = new UserManager<ApplicationUser>(store);

            string str = Form["UserNames"].ToString();
            string[] users = str.Split(',');
            foreach (string usr in users)
            {
                bool isChecked = Convert.ToBoolean(Form[usr].Split(',')[0]);
                if (isChecked)
                {
                    if (!await manager.IsInRoleAsync(manager.Users.FirstOrDefault(u => u.UserName == usr).Id, name))
                    {
                        await manager.AddToRoleAsync(manager.Users.FirstOrDefault(u => u.UserName == usr).Id, name);
                    }
                }
                else
                {
                    if (await manager.IsInRoleAsync(manager.Users.FirstOrDefault(u => u.UserName == usr).Id, name))
                    {
                        await manager.RemoveFromRoleAsync(manager.Users.FirstOrDefault(u => u.UserName == usr).Id, name);
                    }
                }
            }
            return RedirectToAction("Index");
        }

    }

   
}
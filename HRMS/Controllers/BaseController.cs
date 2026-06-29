using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        protected string CurrentUserName =>
            User.Identity?.Name
            ?? HttpContext.Session.GetString("UserName")
            ?? "System";
    }
}

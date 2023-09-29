using BugBurner.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BugBurner.Controllers
{
    [Controller]
    public abstract class BTBaseController : Controller
    {
        protected int _companyId => User.Identity!.GetCompanyId();
    }
}

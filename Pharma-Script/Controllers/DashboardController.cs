using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _uow;

        public DashboardController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var orgId = User.GetOrganizationId();

            int orgsCount = 0;
            int branchesCount = 0;
            int deptsCount = 0;
            int usersCount = 0;
            int specsCount = 0;

            if (isPlatformOwner)
            {
                var orgList = await _uow.Organizations.GetAllAsync();
                orgsCount = orgList.Count();

                var branchList = await _uow.Branches.GetAllAsync();
                branchesCount = branchList.Count();

                var deptList = await _uow.Departments.GetAllAsync();
                deptsCount = deptList.Count();

                var userList = await _uow.Users.GetAllAsync();
                usersCount = userList.Count();
            }
            else if (orgId.HasValue)
            {
                orgsCount = 1;

                var branchList = await _uow.Branches.GetByOrganizationIdAsync(orgId.Value);
                branchesCount = branchList.Count();

                var deptList = await _uow.Departments.GetByOrganizationIdAsync(orgId.Value);
                deptsCount = deptList.Count();

                var userList = await _uow.Users.GetByOrganizationIdAsync(orgId.Value);
                usersCount = userList.Count();
            }

            var specList = await _uow.Specializations.GetAllAsync();
            specsCount = specList.Count();

            ViewBag.OrgsCount = orgsCount;
            ViewBag.BranchesCount = branchesCount;
            ViewBag.DeptsCount = deptsCount;
            ViewBag.UsersCount = usersCount;
            ViewBag.SpecsCount = specsCount;

            return View();
        }
    }
}

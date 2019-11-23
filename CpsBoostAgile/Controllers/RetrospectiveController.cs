using System.Web.Mvc;
using CpsBoostAgile.BL;
using CpsBoostAgile.BL.API;
using Microsoft.AspNet.Identity;

namespace CpsBoostAgile.Controllers
{
    public class RetrospectiveController : Controller
    {
        private IRetrospectiveService _retrospectiveService;

        public RetrospectiveController()
        {
            _retrospectiveService = new RetrospectiveService();
        }

        [Authorize]
        public ActionResult ModeratorView(string id)
        {
            var retrospective = _retrospectiveService.GetRetrospective(id);

            if (string.IsNullOrEmpty(retrospective.Id))
            {
                return HttpNotFound();
            }

            if (User.Identity.GetUserId() == retrospective.CreatedBy)
            {
                retrospective.Url = Url.Action("UserView", "Retrospective", new { id }, Request.Url.Scheme);
                return View(retrospective);
            }

            return RedirectToAction("UserView", "Retrospective", new {id});
        }

        public ActionResult UserView(string id)
        {
            var retrospective = _retrospectiveService.GetRetrospective(id);

            if (string.IsNullOrEmpty(retrospective.Id))
            {
                return HttpNotFound();
            }

            return View(retrospective);
        }
    }
}
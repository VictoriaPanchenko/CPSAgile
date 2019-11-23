using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CpsBoostAgile.BL;
using CpsBoostAgile.BL.API;
using CpsBoostAgile.Enumeration;
using CpsBoostAgile.Models.PokerPlanning;
using Microsoft.AspNet.Identity;

namespace CpsBoostAgile.Controllers
{
    public class PokerPlanningController : Controller
    {
        private IPokerPlanningService _pokerPlanningServiceService;

        public PokerPlanningController()
        {
            _pokerPlanningServiceService = new PokerPlanningService();
        }

        [Authorize]
        public ActionResult CreatePP()
        {
            return View();
        }


        [HttpPost]
        [Authorize]
        public ActionResult CreatePP(PPViewModels createdEvent)
        {
            if (ModelState.IsValid)
            {
                var id = _pokerPlanningServiceService.CreatePokerPlanningEvent(model: createdEvent, eventType: EventTypeEnum.PokerPlanning, createdBy: User.Identity.GetUserId());

                return RedirectToAction("ModeratorViewPoker", "PokerPlanning", new { id });
            }
            return View(createdEvent);
        }

        [Authorize]
        public ActionResult ModeratorViewPoker(string id)
        {
            var ppEvent = _pokerPlanningServiceService.GetPokerPlanningEvent(id);

            if (string.IsNullOrEmpty(ppEvent.Id))
            {
                return HttpNotFound();
            }

            if (User.Identity.GetUserId() == ppEvent.CreatedBy)
            {
                ppEvent.Url = Url.Action("UserViewPP", "PokerPlanning", new { id }, Request.Url.Scheme);
                return View(ppEvent);
            }

            return RedirectToAction("UserViewPP", "PokerPlanning", new { id });
        }

        public ActionResult UserViewPP(string id)
        {
            var ppEvent = _pokerPlanningServiceService.GetPokerPlanningEvent(id);
            ppEvent.Url = Url.Action("UserViewPP", "PokerPlanning", new { id }, Request.Url.Scheme);
            if (string.IsNullOrEmpty(ppEvent.Id))
            {
                return HttpNotFound();
            }

            return View(ppEvent);
        }

        [Authorize]
        public ActionResult SaveUserStory(string id, string text, string ppEventId)
        {
            if(string.IsNullOrWhiteSpace(text))
                return Json(new { isSaved = false });

            var userStoryId = _pokerPlanningServiceService.SaveUserStory(text, ppEventId);

            return Json(new { isSaved = true, userStoryId });
        }

        [Authorize]
        public ActionResult DeleteUserStory(string id)
        {
            if (_pokerPlanningServiceService.DeleteUserStory(id))
                return Json(new {success = true});
            else
                return Json(new {success = false});
        }
    }
}
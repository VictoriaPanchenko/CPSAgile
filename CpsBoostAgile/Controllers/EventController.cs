using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CpsBoostAgile.BL;
using CpsBoostAgile.BL.API;
using CpsBoostAgile.DAO;
using CpsBoostAgile.DbContext;
using CpsBoostAgile.Enumeration;
using CpsBoostAgile.Models.Retrospective;
using Microsoft.AspNet.Identity;

namespace CpsBoostAgile.Controllers
{
    public class EventController : Controller
    {
        private IEventService _eventService;
        private IFileManager _fileManager;

        public EventController()
        {
            _eventService = new EventService();
            _fileManager = new FileManager(new RetrospectiveService(), new PokerPlanningService());
        }

        [Authorize]
        public ActionResult Events()
        {
            var events = _eventService.GetUsersEvents(User.Identity.GetUserId()).OrderByDescending(o => o.StartedDate).ToList();
            return View(events);
        }

        [Authorize]
        public ActionResult CreateEvent()
        {
            return View();
        }


        public ActionResult Export(string id)
        {
            var eventType = _eventService.GetEventType(id);
            
            string path = System.Web.HttpContext.Current.Server.MapPath("~/ExcelTemplates/" +
                           "Export_error.xlsx");

            if (eventType != null)
            {
                if(eventType.Value == EventTypeEnum.Retrospective)
                    path = _fileManager.ExportRetrospective(id);

                if (eventType.Value == EventTypeEnum.PokerPlanning)
                    path = _fileManager.ExportPokerPlanning(id);
            } 
                

            return File(path, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Path.GetFileName(path));
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteEvent(string id)
        {
            if(_eventService.DeleteEvent(id)) 
                return Json(new { success = true });
            else 
                return Json(new { success = false });
        }

        [HttpPost]
        [Authorize]
        public ActionResult CreateEvent(RetrospectiveViewModels createdEvent)
        {
            if (ModelState.IsValid)
            {
                var id = _eventService.CreateEvent(model: createdEvent, eventType: EventTypeEnum.Retrospective, createdBy: User.Identity.GetUserId());

                return RedirectToAction("ModeratorView", "Retrospective", new { id });
            }
            return View(createdEvent);
        }

    }
}
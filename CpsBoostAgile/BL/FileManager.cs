using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using CpsBoostAgile.BL.API;
using OfficeOpenXml;

namespace CpsBoostAgile.BL
{
    public class FileManager : IFileManager
    {
        private IRetrospectiveService _retrospectiveService;
        private IPokerPlanningService _pokerService;

        public FileManager(IRetrospectiveService retrospectiveService, IPokerPlanningService pokerService)
        {
            _retrospectiveService = retrospectiveService;
            _pokerService = pokerService;
        }

        public string ExportRetrospective(string retrospectiveId)
        {
            try
            {
                // delete existing files
                List<FileInfo> previousFiles = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/Temp/"))
                    .GetFiles("*Retrospective*").ToList();

                previousFiles.ForEach(x => x.Delete());

                var retrospective = _retrospectiveService.GetRetrospective(retrospectiveId);

                string name = "Export_Retrospective_" +
                              retrospective
                                  .RetrospectiveName + "_" + retrospective.CreatedDate.GetValueOrDefault().ToString("ddMMyyyy");
                string path = (HttpContext.Current.Server.MapPath("~/Temp/") + name + ".xlsx");
                string pathTemplate = (HttpContext.Current.Server.MapPath("~/ExcelTemplates/") +
                                       "Export_template_Retrospective.xlsx");

                File.Copy(pathTemplate, path);
                var file = new FileInfo(path);

                using (var excel = new ExcelPackage(file))
                {
                    //Get the Worksheet  
                    var ws = excel.Workbook.Worksheets["Retrospective"];

                    ws.Cells["A2"].Value = retrospective.RetrospectiveName;
                    ws.Cells["B2"].Value = retrospective.Project;
                    ws.Cells["C2"].Value = retrospective.Team;
                    ws.Cells["D2"].Value = retrospective.Sprint;
                    ws.Cells["E2"].Value = retrospective.Comment;
                    ws.Cells["F2"].Value = retrospective.CreatedDate != null
                        ? retrospective.CreatedDate.GetValueOrDefault().ToString("g")
                        : "";
                    ws.Cells["G2"].Value = retrospective.FinishedDate != null
                        ? retrospective.FinishedDate.GetValueOrDefault().ToString("g")
                        : "";

                    var retrospectiveItems = retrospective.RetrospectiveItems.OrderBy(o => o.Group).ToList();
                    for (int i = 0; i < retrospectiveItems.Count; i++)
                    {
                        var cellIndex = i + 8;
                        ws.Cells["A" + cellIndex].Value = retrospectiveItems[i].Group;
                        ws.Cells["B" + cellIndex].Value = retrospectiveItems[i].Text;
                        ws.Cells["C" + cellIndex].Value = retrospectiveItems[i].Rating;
                        ws.Cells["D" + cellIndex].Value = retrospectiveItems[i].CreatedBy;
                    }

                    excel.Save();
                }

                return path;
            }
            catch (Exception e)
            {
                return HttpContext.Current.Server.MapPath("~/ExcelTemplates/") +
                        "Export_template_Retrospective.xlsx";
            }
        }

        public string ExportPokerPlanning(string id)
        {
            try
            {
                // delete existing files
                List<FileInfo> previousFiles = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/Temp/"))
                    .GetFiles("*PokerPlanning*").ToList();

                previousFiles.ForEach(x => x.Delete());

                var ppEvent = _pokerService.GetPokerPlanningEvent(id);

                string name = "Export_PokerPlanning_" +
                              ppEvent
                                  .EventName + "_" + ppEvent.CreatedDate.GetValueOrDefault().ToString("ddMMyyyy");
                string path = (HttpContext.Current.Server.MapPath("~/Temp/") + name + ".xlsx");
                string pathTemplate = (HttpContext.Current.Server.MapPath("~/ExcelTemplates/") +
                                       "Export_template_PokerPlanning.xlsx");

                File.Copy(pathTemplate, path);
                var file = new FileInfo(path);

                using (var excel = new ExcelPackage(file))
                {
                    //Get the Worksheet  
                    var ws = excel.Workbook.Worksheets["PokerPlanning"];
                    
                    ws.Cells["A2"].Value = ppEvent.EventName;
                    ws.Cells["B2"].Value = ppEvent.Project;
                    ws.Cells["C2"].Value = ppEvent.Team;
                    ws.Cells["D2"].Value = ppEvent.Sprint;
                    ws.Cells["E2"].Value = ppEvent.Comment;
                    ws.Cells["F2"].Value = ppEvent.CreatedDate != null
                        ? ppEvent.CreatedDate.GetValueOrDefault().ToString("g")
                        : "";
                    ws.Cells["G2"].Value = ppEvent.FinishedDate != null
                        ? ppEvent.FinishedDate.GetValueOrDefault().ToString("g")
                        : "";

                    var userStories = ppEvent.UserStoryList.ToList();
                    for (int i = 0; i < userStories.Count; i++)
                    {
                        var cellIndex = i + 8;
                        ws.Cells["A" + cellIndex].Value = userStories[i].Description;
                        ws.Cells["B" + cellIndex].Value = userStories[i].Comment;
                        ws.Cells["C" + cellIndex].Value = userStories[i].FinalEstimation != null 
                            ? userStories[i].FinalEstimation.ToString()
                            : "Not Estimated";
                    }

                    excel.Save();
                }

                return path;
            }
            catch (Exception e)
            {
                return HttpContext.Current.Server.MapPath("~/ExcelTemplates/") +
                        "Export_template_PokerPlanning.xlsx";
            }
        }
    }
}
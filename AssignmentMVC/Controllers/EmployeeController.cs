using AssignmentMVC.Data;
using AssignmentMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AssignmentMVC.Controllers
{

    public class EmployeeController : Controller
    {

        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(ILogger<EmployeeController> logger)
        {
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {

            try
            {
                List<EmployeeViewModel> employees = new List<EmployeeViewModel>();

                using (var client = new HttpClient())
                {
                    var responseMessage = await client.GetAsync(EmployeeAPIUrl.URL);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var jsonContent = await responseMessage.Content.ReadAsStringAsync();
                        var json = JArray.Parse(jsonContent);

                        //Ensure all items are correctly parsed and included 
                        var employeeEntries = json.Children<JObject>().Select(item => new
                        {
                            EmployeeName = item["EmployeeName"].ToString(),
                            StartTime = DateTime.Parse(item["StarTimeUtc"].ToString()),
                            EndTime = DateTime.Parse(item["EndTimeUtc"].ToString())
                        }).ToList();


                        //Processing JSON data 
                        var workingTimes = json.Children<JObject>().Select(item => new
                        {
                            EmployeeName = item["EmployeeName"].ToString(),
                            StartTime = DateTime.Parse(item["StarTimeUtc"].ToString()),
                            EndTime = DateTime.Parse(item["EndTimeUtc"].ToString())
                        })
                            .GroupBy(emp => emp.EmployeeName)
                            .Select(g => new EmployeeViewModel
                            {
                                EmployeeName = g.Key,
                                TotalWorkingTime = Math.Round(g.Sum(e => (e.EndTime - e.StartTime).TotalHours),2)

                            })
                            .OrderBy(e => e.TotalWorkingTime)
                            .ToList();

                        return View(workingTimes);

                    }

                    else
                    {
                        ViewData["HTTPErrorMessage"] = "An error occurred while fetching data from the server.";
                    }
                }

                return View(employees);
            }

            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Network error");
                ViewData["HTTPErrorMessage"] = "An error occured while processing your HTTP request. Please, try again.";
            }

            catch (JsonReaderException e)
            {
                _logger.LogError(e, "Data parsing error");
                ViewData["ErrorMessage"] = "An error occured with JSON file. Please, try again.";
            }

            catch (Exception e)
            {
                _logger.LogError(e, "An unexpected error occured");
                ViewData["ErrorMessage"] = "An error occured while processing your request. Please, try again.";
            }

            return View("Error");

        }

        public async Task<IActionResult> Searching(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                return View(new List<EmployeeViewModel>());
            }
            else
            {
                DataInfo data = new DataInfo();
                var employees = await data.SearchEmp(searchString); 
                return View("Index", employees);
            }
        }


    }



}


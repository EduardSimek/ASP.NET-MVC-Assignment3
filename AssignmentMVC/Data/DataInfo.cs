using AssignmentMVC.Controllers;
using AssignmentMVC.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;

namespace AssignmentMVC.Data
{
    public class DataInfo
    {

        public async Task<List<EmployeeViewModel>>  SearchEmp(string searchString)
        {

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(EmployeeAPIUrl.URL);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();
                        var json = JArray.Parse(jsonData);

                        

                        var workingTimes = json.Children<JObject>()
                            .Where(jo=> jo["EmployeeName"] != null && jo["EmployeeName"].ToString().IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                            .GroupBy(emp=> emp["EmployeeName"].ToString())
                            .Select(g=> new EmployeeViewModel 
                            { 
                                
                                EmployeeName = g.Key,
                                TotalWorkingTime = g.Sum(e =>
                                     (DateTime.Parse(e["EndTimeUtc"].ToString()) - DateTime.Parse(e["StarTimeUtc"].ToString())).TotalHours)
                                     //Math.Round((DateTime.Parse(e["EndTimeUtc"].ToString()) - DateTime.Parse(e["StartTimeUtc"].ToString())).TotalHours, 2))
                            })
                            .OrderBy(e=> e.EmployeeName)
                            .ToList();

                        return workingTimes;


                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");
                       
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"An error occurred while retrieving data: {ex.Message}");
                  
                }
            }

            return new List<EmployeeViewModel>();


        }

    }
}

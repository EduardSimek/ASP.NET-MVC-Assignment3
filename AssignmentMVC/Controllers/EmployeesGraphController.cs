using AssignmentMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace AssignmentMVC.Controllers
{
    public class EmployeesGraphController : Controller
    {

        public async Task<IActionResult> Index()
        {
            var employees = await FetchEmployeeWorkTimes(EmployeeAPIUrl.URL);
            var stream = GeneratePieChart(employees);
            return File(stream.ToArray(), "image/png");
        }

        private async Task<List<EmployeeViewModel>> FetchEmployeeWorkTimes(string url)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(EmployeeAPIUrl.URL);
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var json = JArray.Parse(jsonContent);

                    return json.Children<JObject>()
                        .Select(item => new
                        {
                            EmployeeName = item["EmployeeName"].ToString(),
                            StartTime = DateTime.Parse(item["StarTimeUtc"].ToString()),
                            EndTime = DateTime.Parse(item["EndTimeUtc"].ToString())
                        })
                        .GroupBy(emp => emp.EmployeeName)
                        .Select(g => new EmployeeViewModel
                        {
                            EmployeeName = g.Key,
                            TotalWorkingTime = g.Sum(e => (e.EndTime - e.StartTime).TotalHours)
                        })
                        .OrderBy(e => e.EmployeeName)
                        .ToList();
                }
            }
            return new List<EmployeeViewModel>();

        }

        private MemoryStream GeneratePieChart(List<EmployeeViewModel> employees)
        {
            const int width = 1000;
            const int height = 1000;
            var bitmap = new Bitmap(width, height);
            var g = Graphics.FromImage(bitmap);

            g.Clear(Color.White);

            float totalHours = employees.Sum(e => (float)e.TotalWorkingTime);
            float startAngle = 0;
            List<Color> colors = new List<Color>();

            // Draw pie chart
            foreach (var employee in employees)
            {
                float sweepAngle = (float)(employee.TotalWorkingTime / totalHours) * 360;
                Color color = GetRandomColor();
                colors.Add(color); // Add color to list for legend
                g.FillPie(new SolidBrush(color), new Rectangle(50, 50, width - 100, height - 150), startAngle, sweepAngle);

                startAngle += sweepAngle;
            }

            // Draw legend
            int legendPosition = height - 200;
            for (int i = 0; i < employees.Count; i++)
            {
                //Calculating percentages
                float percentage = (float)((employees[i].TotalWorkingTime / totalHours) * 100); 

                // Draw color box
                g.FillRectangle(new SolidBrush(colors[i]), 50, legendPosition, 20, 10);
                //Draw text label with hours
                //g.DrawString(employees[i].EmployeeName + " - " + employees[i].TotalWorkingTime.ToString("N2") + " hours", new Font("Arial", 8), Brushes.Black, new PointF(75, legendPosition - 5));

                //Draw text label with percentage 
                g.DrawString($"{employees[i].EmployeeName} - {percentage:N2}%", new Font("Arial", 8), Brushes.Black, new PointF(75, legendPosition - 5));
                legendPosition += 15;
            }

            var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0; // Reset stream position to the beginning

            g.Dispose();
            bitmap.Dispose();

            return stream;
        }

        private Color GetRandomColor()
        {
            Random rand = new Random();
            return Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
        }
    }
}

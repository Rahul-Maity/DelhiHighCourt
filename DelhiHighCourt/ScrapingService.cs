using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace DelhiHighCourt;

public class ScrapingService
{
    private readonly AppDbContext _context;
    public ScrapingService(AppDbContext context)
    {

        _context = context;

    }
    public async Task ScrapeDataAsync()
    {
        using (IWebDriver driver = new ChromeDriver())
        {
            string url = "https://tdsat.gov.in/Delhi/services/judgment.php";
            driver.Navigate().GoToUrl(url);

            DateTime today = DateTime.Today;
            DateTime oneMonthAgo = today.AddMonths(-1);

            // Format dates as DD/MM/YYYY
            string fromDate = "01/09/2024";
            string toDate = "12/09/2024";

            try
            {
                // Use JavaScript to set the value for the readonly fields
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                // Fill the 'from date' field
                IWebElement fromDateElement = driver.FindElement(By.Id("mydate"));
                js.ExecuteScript("arguments[0].value = arguments[1];", fromDateElement, fromDate);

                // Fill the 'to date' field
                IWebElement toDateElement = driver.FindElement(By.Id("mydate1"));
                js.ExecuteScript("arguments[0].value = arguments[1];", toDateElement, toDate);

                // Scroll to the submit button and click it
                IWebElement submitButton = driver.FindElement(By.Id("submit1"));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", submitButton);
                submitButton.Click();

                // Wait for 5 seconds to observe the result
                System.Threading.Thread.Sleep(5000);


                // Locate the form that contains the table
                IWebElement form = driver.FindElement(By.XPath("//form[@name='frm3']"));

                // Locate the table with width='100%' inside the form
                IWebElement table = form.FindElement(By.XPath(".//table[@width='100%']"));

                // Get all rows of the table
                IList<IWebElement> rows = table.FindElements(By.XPath(".//tr"));

                // Prepare a list to hold the extracted data
                var extractedData = new List<Dictionary<string, string>>();

                // Loop through the rows and extract data
                for (int i = 1; i < rows.Count; i++) // Start from 1 to skip header
                {
                    // Get all columns (cells) for the current row
                    IList<IWebElement> cols = rows[i].FindElements(By.TagName("td"));

                    if (cols.Count > 0)
                    {
                        var rowData = new Dictionary<string, string>
                        {
                            {"Serial No.", cols[0].Text.Trim()},
                            {"Case No.", cols[1].Text.Trim()},
                            {"Member Name", cols[2].Text.Trim()},
                            {"Party Detail", cols[3].Text.Replace("\n", " ").Trim()},
                            {"Order Date", cols[4].Text.Trim()},
                            {"Download", cols[5].FindElement(By.TagName("a")).GetAttribute("href")}
                        };

                        extractedData.Add(rowData);
                    }
                }

                // Convert the extracted data to JSON format
                string jsonResult = JsonConvert.SerializeObject(extractedData, Newtonsoft.Json.Formatting.Indented);
                Console.WriteLine(jsonResult);
                await SaveCasesAsync(extractedData);
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine($"Error finding an element: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

    private async Task SaveCasesAsync(List<Dictionary<string, string>> caseData)
    {
        foreach (var item in caseData)
        {
            // Extract and map the data to your CaseDetail entity
            var newCaseDetail = new caseDetail
            {
                CaseNo = item.GetValueOrDefault("Case No."),
                Coram = item.GetValueOrDefault("Member Name"),
                Petitioner = ParsePetitionerOrRespondent(item.GetValueOrDefault("Party Detail"), true),
                Respondent = ParsePetitionerOrRespondent(item.GetValueOrDefault("Party Detail"), false),
                Dated = (DateTime)TryParseDate(item.GetValueOrDefault("Order Date")),
                PdfLink = item.GetValueOrDefault("Download"),

                // Set other fields to default or null
                Filename = null,
                Court = null,
                Abbr = null,
                CaseName = null,
                Counsel = null,
                Overrule = null,
                OveruleBy = null,
                Citation = null,
                Act = null,
                Bench = null,
                Result = null,
                Headnotes = null,
                CaseReferred = null,
                Ssd = null,
                Reportable = false,
                Type = null,
                CoramCount = 0,
                BlaCitation = null,
                QrLink = null
            };

            _context.caseDetails.Add(newCaseDetail);
        }

        await _context.SaveChangesAsync();
    }

    private DateTime? TryParseDate(string? dateString)
    {
        if (string.IsNullOrEmpty(dateString)) return null;

        string[] formats = { "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "dd/MM/yyyy" };

        if (DateTime.TryParseExact(dateString, formats, null, System.Globalization.DateTimeStyles.None, out var date))
        {
            // Convert to UTC
            return DateTime.SpecifyKind(date.ToUniversalTime(), DateTimeKind.Utc);
        }

        // Log or handle invalid date format
        Console.WriteLine($"Unable to parse date: {dateString}");
        return null;
    }

    private string ParsePetitionerOrRespondent(string partyDetail, bool isPetitioner)
    {
        if (string.IsNullOrEmpty(partyDetail)) return null;

        var parts = partyDetail.Split(new[] { "VS", "AND OTHERS" }, StringSplitOptions.None);

        if (isPetitioner)
        {
            return parts.FirstOrDefault()?.Trim();
        }
        else
        {
            return parts.Length > 1 ? parts[1].Trim() : null;
        }
    }
}

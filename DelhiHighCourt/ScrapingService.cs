﻿using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace DelhiHighCourt
{
    public class ScrapingService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _pdfSavePath = @"C:\CourtDetails\DelhiCourt";
        private const int BatchSize = 50; // Define batch size for database operations

        public ScrapingService(AppDbContext context, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task ScrapeDataAsync()
        {
            using (IWebDriver driver = new ChromeDriver())
            {
                string url = "https://tdsat.gov.in/Delhi/services/judgment.php";
                driver.Navigate().GoToUrl(url);

                // Define the date range
                string fromDate = "01/09/2000";
                string toDate = "30/09/2024";

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

                    // Wait for the page to load
                    await Task.Delay(5000);

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

                    // Save data in batches
                    await SaveCasesInBatchesAsync(extractedData);
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

        private async Task SaveCasesInBatchesAsync(List<Dictionary<string, string>> caseData)
        {
            var batch = new List<caseDetail>();
            foreach (var item in caseData)
            {
                string rawDate = item.GetValueOrDefault("Order Date");
                Console.WriteLine($"Raw Date: {rawDate}");
                Console.WriteLine(item.GetValueOrDefault("Member Name"));

                var orderDateStr = item.GetValueOrDefault("Order Date");
                var formattedDate = ConvertDateFormat(orderDateStr);
                //var memberNames = item.GetValueOrDefault("Member Name")
                //         ?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) // Split by commas and remove any empty entries
                //         .Select(name => name.Trim()) // Trim any extra spaces from each name
                //         .ToList();

                var memberNames = ParseMemberNames(item.GetValueOrDefault("Member Name"));
                var petitioner = ParsePetitionerOrRespondent(item.GetValueOrDefault("Party Detail"), true);
                var respondent = ParsePetitionerOrRespondent(item.GetValueOrDefault("Party Detail"), false);
                var caseName = $"{petitioner} vs {respondent}";

                // Extract and map the data to your CaseDetail entity
                var newCaseDetail = new caseDetail
                {
                    CaseNo = item.GetValueOrDefault("Case No."),
                   Coram = memberNames,
                    Petitioner = petitioner,
                    Respondent = respondent,
                    Dated = formattedDate,
                    PdfLink = string.Empty,

                    // Set other fields to default or null
                    Filename = null,
                    Court = "Telecom Disputes Settlement And Appellate Tribunal",
                    Abbr = "TDSAT",
                    CaseName = caseName,
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
                    CoramCount = memberNames?.Length ?? 0,
                    BlaCitation = null,
                    QrLink = null
                };

                var pdfLink = item.GetValueOrDefault("Download");
                if (!string.IsNullOrEmpty(pdfLink))
                {
                    // Download and get the local path of the PDF
                    var localPdfPath = await DownloadAndSavePdfAsync(pdfLink, newCaseDetail.CaseNo);

                    // Set the PdfLink property with the local path
                    newCaseDetail.PdfLink = localPdfPath;
                }

                batch.Add(newCaseDetail);

                // Save in batches
                if (batch.Count >= BatchSize)
                {
                    await SaveBatchAsync(batch);
                    batch.Clear();
                }
            }

            // Save any remaining records
            if (batch.Count > 0)
            {
                await SaveBatchAsync(batch);
            }
        }

        private static string[] ParseMemberNames(string input)
        {
            var members = new List<string>();

            foreach (var part in input.Split(','))
            {
                var trimmedPart = part.Trim();

                // Remove everything after the last opening parenthesis
                int openParenthesisIndex = trimmedPart.LastIndexOf('(');
                if (openParenthesisIndex > -1)
                {
                    trimmedPart = trimmedPart.Substring(0, openParenthesisIndex).Trim();
                }

                // Remove titles like "MR.", "JUSTICE"
                var words = trimmedPart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var filteredWords = words.SkipWhile(w => w.Equals("MR.", StringComparison.OrdinalIgnoreCase)
                                                        || w.Equals("JUSTICE", StringComparison.OrdinalIgnoreCase))
                                         .ToArray();

                // Add cleaned member name
                if (filteredWords.Length > 0)
                {
                    members.Add(string.Join(" ", filteredWords));
                }
            }

            return members.ToArray(); // Convert List<string> to string[]
        }

        private async Task SaveBatchAsync(List<caseDetail> batch)
        {
            try
            {
                _context.caseDetails.AddRange(batch);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving batch: {ex.Message}");
            }
        }

        private string ConvertDateFormat(string? dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
                return string.Empty;

            // Define an array of acceptable date formats
            string[] formats = { "MM-dd-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "dd/MM/yyyy" };

            // Attempt to parse the date with any of the formats
            if (DateTime.TryParseExact(dateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                // Convert to the desired format
                return parsedDate.ToString("dd-MM-yyyy");
            }

            // Return an empty string if parsing fails
            return string.Empty;
        }

        private async Task<string> DownloadAndSavePdfAsync(string url, string? caseNo)
        {
            try
            {
                // Ensure the directory exists
                if (!Directory.Exists(_pdfSavePath))
                {
                    Directory.CreateDirectory(_pdfSavePath);
                }

                // Ensure caseNo is not null and sanitize it if needed
                var sanitizedCaseNo = string.IsNullOrWhiteSpace(caseNo) ? "DefaultName" : caseNo;
                sanitizedCaseNo = Path.GetInvalidFileNameChars().Aggregate(sanitizedCaseNo, (current, c) => current.Replace(c.ToString(), "_"));

                var filePath = Path.Combine(_pdfSavePath, $"{sanitizedCaseNo}.pdf");

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await response.Content.CopyToAsync(fileStream);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                // Handle exceptions such as network errors or file access issues
                Console.WriteLine($"Error downloading PDF: {ex.Message}");
                return string.Empty;
            }
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
}

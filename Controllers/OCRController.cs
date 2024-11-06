using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Tesseract;
using TradingApp.Models;
using TradingApp.Services;

namespace TradingApp.Controllers;

public class OCRController : Controller
{
    private readonly OcrService _ocrService;

    public OCRController()
    {
        _ocrService = new OcrService();
    }
    [HttpGet]
    public IActionResult ExtractText()
    {

        return View();

    }

    [HttpPost]
    public async Task<IActionResult> ExtractText(IFormFile uploadedImage, string actiontype)
    {
        if (uploadedImage == null || uploadedImage.Length == 0)
        {
            return BadRequest("Please upload an image file.");
        }

        string filePath = Path.Combine(Path.GetTempPath(), uploadedImage.FileName);
        try
        {
            // Save the uploaded image to a temporary location
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                uploadedImage.CopyTo(stream);
            }

            // Extract text from the image
            string ocrText;
            if (string.Equals(actiontype, "OCR", StringComparison.OrdinalIgnoreCase))
            {
                ocrText = _ocrService.ExtractTextFromImage(filePath);
                if (string.IsNullOrEmpty(ocrText))
                {
                    return StatusCode(500, "Failed to extract text from the image.");
                }
                // Return the extracted text for OCR action
                return Ok(new { Text = ocrText });
            }
            else if (string.Equals(actiontype, "Parse", StringComparison.OrdinalIgnoreCase))
            {
                // Initialize Tesseract OCR Engine
                string tessdataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
                using var engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(filePath);
                using var page = engine.Process(img);
                ocrText = page.GetText();

                if (string.IsNullOrEmpty(ocrText))
                {
                    return StatusCode(500, "Failed to extract text from the image.");
                }

                // Send OCR text to the Python script for NLP processing
                var pythonOutput = await RunPythonScript(ocrText);

                // Deserialize the JSON output from Python
                var processedData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(pythonOutput);

                // Return the processed data along with the OCR text
                return Ok(new { Text = ocrText, ProcessedData = processedData });
            }
            else
            {
                return BadRequest("Invalid action type. Please specify 'OCR' or 'Parse'.");
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Error: {e.Message}");
        }
        finally
        {
            // Delete the temporary file after processing
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }


    private string ExtractValue(string line, string key, bool isCurrency = false)
    {
        var index = line.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            var value = line[(index + key.Length)..].Trim();
            if (isCurrency)
            {
                value = value.Replace("$", "").Trim();
            }
            return value;
        }
        return string.Empty;
    }

    private bool TryExtractDate(string line, out DateTime date)
    {
        date = DateTime.MinValue;

        // Regex to match common date formats like MM/dd/yyyy, dd/MM/yyyy, yyyy-MM-dd, etc.
        var dateMatch = Regex.Match(line, @"\b(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})\b");
        if (dateMatch.Success && DateTime.TryParse(dateMatch.Value, out date))
        {
            return true;
        }

        // Handle improperly formatted dates like "1110212019" (possibly MMddyyyy or ddMMyyyy)
        var potentialDateMatch = Regex.Match(line, @"\b(\d{8})\b");
        if (potentialDateMatch.Success)
        {
            string potentialDate = potentialDateMatch.Value;

            // Attempt to parse as MMddyyyy or ddMMyyyy
            if (TryParseManualDate(potentialDate, "MMddyyyy", out date) ||
                TryParseManualDate(potentialDate, "ddMMyyyy", out date))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryParseManualDate(string dateStr, string format, out DateTime date)
    {
        date = DateTime.MinValue;

        if (DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return true;
        }

        return false;
    }

    public async Task<string> RunPythonScript(string text)
    {
        // Path to Python executable
        var pythonPath = @"C:\Users\123\AppData\Local\Programs\Python\Python310\python.exe";  // Update this path as needed
        var scriptPath = @"D:\Codebase\TradingApp\PythonScript\process_text.py";  // Update this path as needed

        // Create the process start information
        var psi = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"\"{scriptPath}\" \"{text}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Start the process and read output
        using (var process = Process.Start(psi))
        {
            if (process == null)
            {
                throw new Exception("Failed to start the Python process.");
            }

            // Read the output
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            // Check if there was an error
            if (process.ExitCode != 0)
            {
                throw new Exception($"Python script error: {error}");
            }

            return output;
        }
    }



}

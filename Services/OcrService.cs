using System;
using System.IO;
using Tesseract;
namespace TradingApp.Services
{
   

    public class OcrService
    {
        public string ExtractTextFromImage(string imagePath)
        {
            try
            {
                string tessdataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
                using (var engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default))
                {
                    // Load the image from the provided path
                    using (var img = Pix.LoadFromFile(imagePath))
                    {
                        // Process the image to extract text
                        using (var page = engine.Process(img))
                        {
                            return page.GetText(); // Extracted text from the image
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return null;
            }
        }
    }

}

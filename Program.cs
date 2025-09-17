using System;
using System.IO;
using System.Threading.Tasks;

namespace FbaApp;

/// <summary>
/// A simple file-based application template for .NET 10+
/// This template provides a starting point for applications that work with files
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Welcome to File-based App!");
        Console.WriteLine("This template helps you create applications that work with files.");
        
        // Example: Create a sample file
        var fileName = "sample.txt";
        var content = $"File created at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        
        try
        {
            await File.WriteAllTextAsync(fileName, content);
            Console.WriteLine($"Created file: {fileName}");
            
            // Example: Read the file back
            var readContent = await File.ReadAllTextAsync(fileName);
            Console.WriteLine($"File content: {readContent}");
            
            // Example: Check if file exists
            if (File.Exists(fileName))
            {
                Console.WriteLine("File exists and can be processed.");
            }
            
            // Clean up
            File.Delete(fileName);
            Console.WriteLine("Sample file cleaned up.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error working with files: {ex.Message}");
        }
        
        Console.WriteLine("\nReplace this template code with your file-based application logic!");
    }
}
namespace MultiFileStream.Demo
{
    using System;
    using System.IO;
    using System.Reflection;
    using static System.IO.Path;

    class Program
    {
        static void Main(string[] args)
        {
            string mainFilePath = Combine(GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"someTextFile.txt");
            MultiPartFile.Delete(mainFilePath);
            string contentToWrite = "This content will be saved to multiple files and each file size will be limited up to 40 bytes";

            using (MultiPartFileStream multiFileStream = MultiPartFile.Create(mainFilePath, perFileLength: 40))
            using (var writer = new StreamWriter(multiFileStream))
            {
                writer.Write(contentToWrite);
                writer.Flush();

                WriteLine("Files were created:", ConsoleColor.Yellow);
                foreach (string createdFile in multiFileStream.Files)
                {
                    WriteLine(createdFile);
                }
            }

            using (MultiPartFileStream multiFileStream = MultiPartFile.OpenRead(mainFilePath))
            using (var reader = new StreamReader(multiFileStream))
            {
                string result = reader.ReadToEnd();
                WriteLine($"Read content: {result}", ConsoleColor.Yellow);
                WriteLine($"Content is {(contentToWrite == result? "equal" : "not equal")} to content which was written.", ConsoleColor.Green);

                WriteLine("Found files for reading:", ConsoleColor.Yellow);
                foreach (string foundFile in multiFileStream.Files)
                {
                    WriteLine(foundFile);
                }
            }

            Console.ReadKey();
        }

        private static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Chill;
using FluentAssertions;
using For_MultiPartFile_While_Writing;
using MultiFileStream;
using MultiFileStream.Helpers.Extensions;
using MultiFileStream.Specs.Helpers;


namespace For_MultiPartFile_While_Reading
{
    public class Given_Few_Partial_Files : Given_Content_And_Main_File_Path<string>
    {
        public Given_Few_Partial_Files()
        {
            Given(() =>
            {
                using (MultiPartFileStream multiFileStream = MultiPartFile.Create(TheNamed<string>("mainFilePath"), 10))
                using (var writer = new StreamWriter(multiFileStream))
                {
                    writer.Write(TheNamed<string>("content"));
                    writer.Flush();
                    UseThe(multiFileStream.Files, "writtenFiles");
                }
            });
        }
    }

    public class When_Reading_Partial_Files : Given_Few_Partial_Files
    {
        public When_Reading_Partial_Files()
        {
            Given(() =>
            {
                UseThe(MultiPartFile.OpenRead(TheNamed<string>("mainFilePath")));
                UseThe(new StreamReader(The<MultiPartFileStream>()));
            });

            When(() => The<StreamReader>().ReadToEnd());
        }

        [Fact]
        public void Then_Read_Content_Should_Be_Equal_To_Files_Content()
        {
            Result.Should().Be(TheNamed<string>("content"));
        }

        [Fact]
        public void Then_Should_Detect_The_Same_Amount_Of_Files_As_Count_Of_Partial_Files()
        {
            The<MultiPartFileStream>().Files.Count.Should().Be(TheNamed<IReadOnlyList<string>>("writtenFiles").Count);
        }
    }

    public class Given_Few_Non_Partial_Files : GivenWhenThen<string>
    {
        public Given_Few_Non_Partial_Files()
        {
            Given(() =>
            {
                UseThe(TempDirectory.InBaseDirectory());
                UseThe(The<TempDirectory>().AddFile("someFile.txt"), "file1");
                UseThe(The<TempDirectory>().AddFile("anotherFile.txt"), "file2");
                UseThe($"Some dummy content for non partial file #1 to write: {string.Join("", Enumerable.Range(1, 100))}", "content1");
                UseThe($"Some dummy content for non partial file #2 to write: {string.Join("", Enumerable.Range(100, 200))}", "content2");

                File.WriteAllText(TheNamed<string>("file1"), TheNamed<string>("content1"));
                File.WriteAllText(TheNamed<string>("file2"), TheNamed<string>("content2"));
            });
        }
    }

    public class When_Reading_Non_Partial_Files : Given_Few_Non_Partial_Files
    {
        public When_Reading_Non_Partial_Files()
        {
            Given(() =>
            {
                UseThe(MultiPartFile.OpenRead(TheNamed<string>("file1"), TheNamed<string>("file2")));
                UseThe(new StreamReader(The<MultiPartFileStream>()));
            });

            When(() => The<StreamReader>().ReadToEnd());
        }

        [Fact]
        public void Then_Read_Content_Should_Be_Equal_To_Files_Content()
        {
            Result.Should().Be(TheNamed<string>("content1") + TheNamed<string>("content2"));
        }

        [Fact]
        public void Then_Should_Detect_The_Same_Amount_Of_Files_As_Count_Of_Partial_Files()
        {
            The<MultiPartFileStream>().Files.Count.Should().Be(2);
        }
    }
}

namespace For_MultiPartFile_While_Writing
{
    public class Given_Content_And_Main_File_Path<T> : GivenWhenThen<T>
    {
        public Given_Content_And_Main_File_Path()
        {
            Given(() =>
            {
                UseThe(TempDirectory.InBaseDirectory());
                UseThe(The<TempDirectory>().AddFile("mainFile.txt"), "mainFilePath");
                UseThe($"Some dummy content for file to write: {string.Join("", Enumerable.Range(1, 100))}", "content");
            });
        }
    }

    public class When_Write_To_File_With_Small_File_Limit : Given_Content_And_Main_File_Path<IReadOnlyList<string>>
    {
        private long fileSizeLimitInBytes = 10;

        public When_Write_To_File_With_Small_File_Limit()
        {
            Given(() =>
            {
                UseThe(MultiPartFile.Create(TheNamed<string>("mainFilePath"), perFileLength: fileSizeLimitInBytes));
                UseThe(new StreamWriter(The<MultiPartFileStream>()));
            });

            When(() =>
            {
                The<StreamWriter>().Write(TheNamed<string>("content"));
                The<StreamWriter>().Flush();
                The<StreamWriter>().Dispose();
                return The<MultiPartFileStream>().Files;
            });
        }

        [Fact]
        public void Then_It_Should_Create_Multiple_Files()
        {
            Result.Count.Should().BeGreaterThan(1);
        }

        [Fact]
        public void Then_Created_Files_Should_Be_Named_With_Incremented_Part_Suffix()
        {
            Result.Skip(1)
                .Select(file => Regex.Match(file, @"\.part(?<partIndex>\d+)").FindGroup<int>("partIndex"))
                .Should()
                .BeInAscendingOrder();
        }

        [Fact]
        public void Then_All_Files_Should_Have_Size_Smaller_Or_Equal_Then_File_Limit()
        {
            foreach (long fileSize in Result.Select(file => new FileInfo(file).Length))
            {
                fileSize.Should().BeGreaterOrEqualTo(fileSizeLimitInBytes);
            }
        }

        [Fact]
        public void Then_Content_From_All_Files_Should_Be_Equal_To_Written_Content()
        {
            var writtenContent = string.Join("", Result.Select(File.ReadAllText));
            writtenContent.Should().Be(TheNamed<string>("content"));
        }
    }

    public class When_Write_To_File_With_Big_File_Limit : Given_Content_And_Main_File_Path<IReadOnlyList<string>>
    {
        private static readonly long OneMegaByte = 1 * 1024 * 1024;
        private static readonly long fileSizeLimitInBytes = OneMegaByte;

        public When_Write_To_File_With_Big_File_Limit()
        {
            Given(() =>
            {
                UseThe(MultiPartFile.Create(TheNamed<string>("mainFilePath"), perFileLength: fileSizeLimitInBytes));
                UseThe(new StreamWriter(The<MultiPartFileStream>()));
            });

            When(() =>
            {
                The<StreamWriter>().Write(TheNamed<string>("content"));
                The<StreamWriter>().Flush();
                The<StreamWriter>().Dispose();
                return The<MultiPartFileStream>().Files;
            });
        }

        [Fact]
        public void Then_It_Should_Create_Single_File()
        {
            Result.Count.Should().Be(1);
        }

        [Fact]
        public void Then_Content_From_All_Files_Should_Be_Equal_To_Written_Content()
        {
            File.ReadAllText(TheNamed<string>("mainFilePath")).Should().Be(TheNamed<string>("content"));
        }
    }
}

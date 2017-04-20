namespace MultiFileStream
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Helpers.Extensions;
    using static System.IO.Path;

    public sealed class MultiPartFile
    {
        private readonly List<PartFileInfo> files;
        private PartFileInfo lastWrittenFile;

        internal MultiPartFile(string mainFilePath)
            : this(FindFiles(mainFilePath))
        {
            
        }

        internal MultiPartFile(string[] files)
            : this(files.Select(file => new PartFileInfo(file)).ToList())
        {
            
        }

        private MultiPartFile(List<PartFileInfo> files)
        {
            this.files = files;
        }

        internal IReadOnlyList<string> Files => files.Select(file => file.Path).ToList();

        public static MultiPartFileStream OpenRead(string mainFilePath)
        {
            var parts = new MultiPartFile(mainFilePath);
            return new MultiPartFileStream(parts, new ConcatenatedStream(parts.Files.Select(File.OpenRead)));
        }

        public static MultiPartFileStream OpenRead(params string[] files)
        {
            var parts = new MultiPartFile(files);
            return new MultiPartFileStream(parts, new ConcatenatedStream(parts.Files.Select(File.OpenRead)));
        }

        public static MultiPartFileStream Create(string mainFilePath, long perFileLength)
        {
            var parts = new MultiPartFile(mainFilePath);
            return new MultiPartFileStream(parts, new SplittedStream(perFileLength, parts.OpenWriteNextFile));
        }

        public static void Delete(string mainFilePath)
        {
            var fileToDelete = new PartFileInfo(mainFilePath);
            while (fileToDelete.Exists)
            {
                fileToDelete.Delete();
                fileToDelete = fileToDelete.NextSequentialFile;
            }
        }

        public static IReadOnlyList<string> SplitFile(string iputFile, string mainOutputFilePath, long perFileLenghtInBytes)
        {
            using (FileStream inputStream = File.Open(iputFile, FileMode.Open))
            using (MultiPartFileStream multiFileStream = Create(mainOutputFilePath, perFileLenghtInBytes))
            {
                inputStream.CopyTo(multiFileStream);
                return multiFileStream.Files;
            }
        }

        private FileStream OpenWriteNextFile()
        {
            bool isWriting = !lastWrittenFile.Equals(default(PartFileInfo));
            PartFileInfo mainFile = files.First();
            PartFileInfo file = lastWrittenFile = isWriting ? lastWrittenFile.NextSequentialFile : mainFile;
            if (!file.Equals(mainFile))
            {
                files.Add(file);
            }
            return File.OpenWrite(file.Path);
        }

        private static List<PartFileInfo> FindFiles(string mainFilePath)
        {
            var mainPart = new PartFileInfo(mainFilePath);
            var subsequentFiles = mainPart.Directory.GetFiles()
                .Select(file => new PartFileInfo(file))
                .Where(i => i.IsSubsequentOf(mainPart))
                .OrderBy(i => i.Number);

            return new[] { mainPart }.Union(subsequentFiles).ToList();
        }

        private struct PartFileInfo : IEquatable<PartFileInfo>
        {
            private const string PrefixGroupName = "mainFilePrefix";
            private const string FileNumberGroupName = "fileNumber";
            private const string MaxFilesGroupName = "maxFiles";
            private const string ExtensionGroupName = "extension";
            private static readonly Regex FileNameRegex = new Regex($@"^(?<{PrefixGroupName}>.*?)" +
                                                                    $@"(\.part(?<{FileNumberGroupName}>\d+)((\s)?of(\s)?(?<{MaxFilesGroupName}>\d+))?)?" +
                                                                    $@"(\.(?<{ExtensionGroupName}>\w{{3}}))?$");

            private readonly Match match;
            private readonly FileInfo fileInfo;

            public PartFileInfo(string file) : this(new FileInfo(file)) {}

            public PartFileInfo(FileInfo file)
            {
                match = FileNameRegex.Match(file.Name);
                fileInfo = file;
            }

            private string Extension => match.FindGroup(ExtensionGroupName);
            private string MainFilePrefix => match.FindGroup(PrefixGroupName);
            public DirectoryInfo Directory => fileInfo.Directory;
            public string Path => fileInfo?.FullName;
            public int Number => match.FindGroup(FileNumberGroupName, defaultValue: 1);
            public bool Exists => fileInfo.Exists;

            public PartFileInfo NextSequentialFile
            {
                get
                {
                    string file = $"{MainFilePrefix}.part{Number + 1}{(Extension == null ? "" : $".{Extension}")}";
                    return new PartFileInfo(Combine(Directory.FullName, file));
                }
            }

            public bool IsSubsequentOf(PartFileInfo file)
            {
                return file.MainFilePrefix != null && file.MainFilePrefix == MainFilePrefix && file.Number < Number;
            }

            public void Delete() => fileInfo.Delete();

            public override bool Equals(object obj) => !ReferenceEquals(null, obj) && obj is PartFileInfo && Equals((PartFileInfo)obj);
            public override int GetHashCode() => Path?.GetHashCode() ?? 0;
            public bool Equals(PartFileInfo other) => Equals(Path, other.Path);
        }
    }
}

# MultiFileStream

[![Build status](https://ci.appveyor.com/api/projects/status/omyq41n5eqqf8cxk?svg=true)](https://ci.appveyor.com/project/iarovyi/multifilestream)
[![NuGet](https://img.shields.io/nuget/v/MultiFileStream.svg)](https://www.nuget.org/packages/MultiFileStream/)

Easy way to treat multiple files as one file while reading or writing.

With MultiFileStream you can write to multiple files using one Stream:
```
using (MultiPartFileStream multiFileStream = MultiPartFile.Create(@"C:\textFile.txt", perFileLength: OneMegaByte))
using (var writer = new StreamWriter(multiFileStream))
{
	writer.Write(hugeText);
	writer.Flush();
	//Will create following files while writing using single stream:
	//C:\textFile.txt         (1Mb)
	//C:\textFile.part2.txt   (1Mb)
	//C:\textFile.part3.txt   (1Mb)
	//...
}

```

With MultiFileStream you can read from multiple files using one Stream:
```
using (MultiPartFileStream multiFileStream = MultiPartFile.OpenRead(@"C:\textFile.txt"))
using (var reader = new StreamReader(multiFileStream))
{
	string result = reader.ReadToEnd();
	//Will read as one stream all files:
	//C:\textFile.txt         (1Mb)
	//C:\textFile.part2.txt   (1Mb)
	//C:\textFile.part3.txt   (1Mb)
	//...
}

```

With MultiFileStream you can read list of any given files as one Stream:
```
using (MultiPartFileStream multiFileStream = MultiPartFile.OpenRead(@"C:\someFile.txt", @"C:\anotherFile.txt"))
using (var reader = new StreamReader(multiFileStream))
{
	string result = reader.ReadToEnd();
	//Will read both files as one stream
}

```




### Build Script

#### From a powershell prompt

Command     | Description
:-----------| :----------
`build.ps1` | Builds the entire solution and run tests.

#### From a bash prompt

Command                | Description
:----------------------| :----------
`build.bat` | Builds the entire solution and run tests.

#### Build targets

Target            | Description
:-----------------| :----------
`Build`           | Build solution with semantic version and run tests.
`Package`         | Create nuget package based on semantic version.
`Publish   `      | Publish nuget package to nuget.org. (require ApiKey present as environment variable)

Example:  `.\build.ps1 -target Package`



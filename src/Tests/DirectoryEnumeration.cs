using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Win32.SafeHandles;

namespace Tests
{
    // |    Method |     Mean |     Error |    StdDev |   Gen 0 | Allocated |
    // |---------- |---------:|----------:|----------:|--------:|----------:|
    // | Classical | 7.478 ms | 0.1389 ms | 0.1231 ms | 15.6250 |    144 KB |
    // |  Win32Api | 4.456 ms | 0.0561 ms | 0.0525 ms |  7.8125 |     59 KB |
    // |  IoRedist | 3.258 ms | 0.0328 ms | 0.0307 ms | 11.7188 |     75 KB |
    [MemoryDiagnoser]
    public class DirectionEnumerationTests
    {
        [Benchmark]
        public void Classical()
        {
            string directory = GetDirectory();

            var files = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
            }
        }

        [Benchmark]
        public void Win32Api()
        {
            string directory = GetDirectory();

            DirectoryEnumeration.EnumerateDirectoryRecursive(directory, dir => true, file => { });
        }

        [Benchmark]
        public void IoRedist()
        {
            string directory = GetDirectory();

            var files = Microsoft.IO.Directory.EnumerateFiles(directory, "*", new Microsoft.IO.EnumerationOptions() { RecurseSubdirectories = true });
            foreach (var file in files)
            {
            }
        }

        public static string GetDirectory()
        {
            var directory = Assembly.GetExecutingAssembly().Location;
            directory = Path.GetDirectoryName(directory);
            directory = Path.GetDirectoryName(directory);
            directory = Path.GetDirectoryName(directory);
            directory = Path.GetDirectoryName(directory);
            return directory;
        }
    }

    public class DirectoryEnumeration
    {
        public class FileSystemEntryData
        {
            public string Directory { get; set; }
            public string Name { get; set; }
            public string FullPath => !string.IsNullOrEmpty(Directory) && Directory[Directory.Length - 1] == '\\'
                ? Directory + Name
                : Directory + "\\" + Name; // faster than Path.Combine
        }

        [ThreadStatic]
        private static FileSystemEntryData current;

        public static Task EnumerateDirectoryRecursiveAsync(
            string directory,
            Func<FileSystemEntryData, bool> directoryCallback,
            Action<FileSystemEntryData> fileCallback)
        {
            List<Task> tasks = null;

            EnumerateDirectory(
                directory,
                d =>
                {
                    if (directoryCallback?.Invoke(d) ?? true)
                    {
                        if (tasks == null)
                        {
                            tasks = new List<Task>();
                        }

                        string fullPath = d.FullPath;
                        var subdirectoryTask = Task.Run(() => EnumerateDirectoryRecursiveAsync(fullPath, directoryCallback, fileCallback));
                        tasks.Add(subdirectoryTask);
                    }
                },
                fileCallback);

            if (tasks == null)
            {
                return Task.CompletedTask;
            }
            else if (tasks.Count == 1)
            {
                return tasks[0];
            }
            else
            {
                return Task.WhenAll(tasks);
            }
        }

        public static void EnumerateDirectoryRecursive(
            string directory,
            Func<FileSystemEntryData, bool> directoryCallback,
            Action<FileSystemEntryData> fileCallback)
        {
            EnumerateDirectory(
                directory,
                d =>
                {
                    if (directoryCallback?.Invoke(d) ?? true)
                    {
                        EnumerateDirectoryRecursive(d.FullPath, directoryCallback, fileCallback);
                    }
                },
                fileCallback);
        }

        public static void EnumerateDirectory(
            string directory,
            Func<FileSystemEntryData, bool> directoryCallback,
            Action<FileSystemEntryData> fileCallback,
            SearchOption searchOption,
            bool parallel = true)
        {
            if (searchOption == SearchOption.AllDirectories)
            {
                if (parallel)
                {
                    EnumerateDirectoryRecursiveAsync(directory, directoryCallback, fileCallback).Wait();
                }
                else
                {
                    EnumerateDirectoryRecursive(directory, directoryCallback, fileCallback);
                }
            }
            else
            {
                void action(FileSystemEntryData f) => directoryCallback(f);
                EnumerateDirectory(directory, action, fileCallback);
            }
        }

        public static void EnumerateDirectory(
            string directory,
            Action<FileSystemEntryData> directoryCallback,
            Action<FileSystemEntryData> fileCallback)
        {
            if (current == null)
            {
                current = new FileSystemEntryData();
            }

            string lookupDirectory;
            if (directory.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
            {
                lookupDirectory = directory.Replace(@"\\", @"\\?\UNC\") + "\\*";
            }
            else
            {
                lookupDirectory = "\\\\?\\" + directory + "\\*";
            }

            WIN32_FIND_DATA w32FindData;

            using (SafeFindFileHandle fileHandle = FindFirstFileEx(
                lookupDirectory,
                FINDEX_INFO_LEVELS.Basic,
                out w32FindData,
                FINDEX_SEARCH_OPS.SearchNameMatch,
                IntPtr.Zero,
                FindExAdditionalFlags.LargeFetch))
            {
                if (fileHandle.IsInvalid)
                {
                    return;
                }

                do
                {
                    var fileName = w32FindData.cFileName;
                    if (fileName == "." || fileName == "..")
                    {
                        continue;
                    }

                    // need to set it here on every iteration because it could have been changed
                    // by recursive calls on the same thread
                    current.Directory = directory;
                    current.Name = fileName;

                    if ((w32FindData.dwFileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        directoryCallback?.Invoke(current);
                    }
                    else
                    {
                        // long fileSize = w32FindData.nFileSizeLow + ((long)w32FindData.nFileSizeHigh << 32);
                        fileCallback?.Invoke(current);
                    }
                } while (FindNextFile(fileHandle, out w32FindData));
            }
        }

        internal enum FINDEX_INFO_LEVELS
        {
            Standard = 0,
            Basic = 1
        }

        internal enum FINDEX_SEARCH_OPS
        {
            SearchNameMatch = 0,
            SearchLimitToDirectories = 1,
            SearchLimitToDevices = 2
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WIN32_FIND_DATA
        {
            public FileAttributes dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public readonly uint dwReserved0;
            private readonly uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        internal enum FindExAdditionalFlags
        {
            None = 0,
            CaseSensitive = 1,
            LargeFetch = 2
        }

        [DllImport("kernel32.dll", SetLastError = false, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FindClose(IntPtr hFindFile);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "FindFirstFileExW")]
        internal static extern SafeFindFileHandle FindFirstFileEx([MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                                                                  FINDEX_INFO_LEVELS fInfoLevelId,
                                                                  out WIN32_FIND_DATA lpFindFileData,
                                                                  FINDEX_SEARCH_OPS fSearchOp,
                                                                  IntPtr lpSearchFilter,
                                                                  FindExAdditionalFlags dwAdditionalFlags);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "FindNextFileW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FindNextFile(SafeFindFileHandle hFindFile, out WIN32_FIND_DATA lpFindFileData);
    }

    internal sealed class SafeFindFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeFindFileHandle() : base(true)
        {
        }

        public SafeFindFileHandle(IntPtr handle, bool ownsHandle = true) : base(ownsHandle)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            bool retValue = true;
            if (!IsInvalid)
            {
                // See https://github.com/Wintellect/FastFileFinder/issues/12
                retValue = DirectoryEnumeration.FindClose(handle);
            }

            return retValue;
        }
    }
}
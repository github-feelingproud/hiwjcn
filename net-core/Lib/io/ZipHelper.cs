using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lib.io
{
    /// <summary>压缩解压缩</summary>
    public static class ZipHelper
    {
        #region Zip
        /// <summary>压缩到档案</summary>
        /// <param name="outputFile">档案位置</param>
        /// <param name="fileOrDirectory">文件或文件夹</param>
        public static void Zip(string outputFile, string fileOrDirectory)
        {
            Zip(outputFile, fileOrDirectory, null, null);
        }

        /// <summary>压缩到档案</summary>
        /// <param name="outputFile">档案位置</param>
        /// <param name="baseDirectory">基目录</param>
        /// <param name="filesOrDirectories">文件或文件夹列表</param>
        public static void Zip(string outputFile, string baseDirectory, IEnumerable<string> filesOrDirectories)
        {
            Zip(outputFile, baseDirectory, filesOrDirectories, null);
        }

        /// <summary>压缩到档案</summary>
        /// <param name="outputFile">档案位置</param>
        /// <param name="baseDirectory">基目录</param>
        /// <param name="filesOrDirectories">文件或文件夹列表</param>
        /// <param name="ignoreFilesOrDirectories">过滤的文件或文件夹列表</param>
        public static void Zip(string outputFile, string baseDirectory, IEnumerable<string> filesOrDirectories, IEnumerable<string> ignoreFilesOrDirectories)
        {
            using (var output = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            {
                Zip(output, baseDirectory, filesOrDirectories, ignoreFilesOrDirectories);
            }
        }

        /// <summary>压缩到档案</summary>
        /// <param name="output">输出流</param>
        /// <param name="fileOrDirectory">文件或文件夹</param>
        public static void Zip(Stream output, string fileOrDirectory)
        {
            fileOrDirectory = Path.GetFullPath(fileOrDirectory);

            if (Directory.Exists(fileOrDirectory))
                Zip(output, fileOrDirectory, null);
            else if (File.Exists(fileOrDirectory))
                Zip(output, Path.GetDirectoryName(fileOrDirectory), new[] { fileOrDirectory });
        }

        /// <summary>压缩到档案</summary>
        /// <param name="output">输出流</param>
        /// <param name="baseDirectory">基目录</param>
        /// <param name="filesOrDirectories">文件或文件夹列表</param>
        public static void Zip(Stream output, string baseDirectory, IEnumerable<string> filesOrDirectories)
        {
            Zip(output, baseDirectory, filesOrDirectories, null);
        }

        /// <summary>压缩到档案</summary>
        /// <param name="output">输出流</param>
        /// <param name="baseDirectory">基目录</param>
        /// <param name="filesOrDirectories">文件或文件夹列表</param>
        /// <param name="ignoreFilesOrDirectories">过滤的文件或文件夹列表</param>
        public static void Zip(Stream output, string baseDirectory, IEnumerable<string> filesOrDirectories, IEnumerable<string> ignoreFilesOrDirectories)
        {
            using (var outputStream = new ZipOutputStream(output))
            {
                if (filesOrDirectories == null || filesOrDirectories.FirstOrDefault() == null)
                    ZipDirectory(outputStream, baseDirectory, null, ignoreFilesOrDirectories);
                else
                    foreach (var fileOrDirectory in filesOrDirectories)
                    {
                        var fullPath = fileOrDirectory;
                        if (string.Compare(Path.GetFullPath(fileOrDirectory), fileOrDirectory, true) != 0)
                            fullPath = Path.Combine(baseDirectory, fileOrDirectory);

                        if (Directory.Exists(fullPath))
                            ZipDirectory(outputStream, baseDirectory, fullPath, ignoreFilesOrDirectories);
                        else if (File.Exists(fullPath))
                            ZipFile(outputStream, baseDirectory, fullPath, ignoreFilesOrDirectories);
                    }
            }
        }

        private static void ZipDirectory(ZipOutputStream outputStream, string baseDirectory, string directoryPath, IEnumerable<string> ignoreFilesOrDirectories)
        {
            foreach (var file in Directory.GetFiles(directoryPath ?? baseDirectory))
            {
                ZipFile(outputStream, baseDirectory, file, ignoreFilesOrDirectories);
            }

            foreach (var dir in Directory.GetDirectories(directoryPath ?? baseDirectory))
            {
                if (ignoreFilesOrDirectories == null || !ignoreFilesOrDirectories.Any(fd => string.Compare(fd, dir, true) == 0))
                    ZipDirectory(outputStream, baseDirectory, dir, ignoreFilesOrDirectories);
            }
        }

        private static void ZipFile(ZipOutputStream outputStream, string baseDirectory, string fileFullPath, IEnumerable<string> ignoreFilesOrDirectories)
        {
            if (ignoreFilesOrDirectories == null || !ignoreFilesOrDirectories.Any(fd => string.Compare(fd, fileFullPath, true) == 0))
                using (var input = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read))
                {
                    var entry = new ZipEntry(fileFullPath.Substring(baseDirectory.Length).TrimStart('\\'));

                    var fileInfo = new FileInfo(fileFullPath);
                    entry.DateTime = fileInfo.LastWriteTime;

                    outputStream.PutNextEntry(entry);

                    int readLength;
                    byte[] buffer = new byte[1024 * 1024];
                    while ((readLength = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outputStream.Write(buffer, 0, readLength);

                        outputStream.Flush();
                    }
                }
        }
        #endregion

        #region Dezip
        /// <summary>解压缩到档案</summary>
        /// <param name="inputFile">输入文档</param>
        /// <param name="outputDirectory">输出目录</param>
        public static IList<string> Dezip(string inputFile, string outputDirectory)
        {
            using (var input = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                return Dezip(input, outputDirectory);
            }
        }

        /// <summary>解压缩到档案</summary>
        /// <param name="inputStream">输入流</param>
        /// <param name="outputDirectory">输出目录</param>
        public static IList<string> Dezip(Stream inputStream, string outputDirectory)
        {
            ZipEntry entry;
            using (var zipStream = new ZipInputStream(inputStream))
            {
                var files = new List<string>();
                while ((entry = zipStream.GetNextEntry()) != null)
                {
                    var path = Path.Combine(outputDirectory, entry.Name.Replace('/', '\\'));

                    try
                    {
                        var dir = Path.GetDirectoryName(path);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                    }
                    catch { }

                    if (entry.IsFile)
                    {
                        files.Add(entry.Name.Replace('/', '\\'));

                        using (var output = new FileStream(path, FileMode.Create, FileAccess.Write))
                        {
                            var readLength = 0;
                            byte[] data = new byte[1024 * 1024];
                            while ((readLength = zipStream.Read(data, 0, data.Length)) > 0)
                            {
                                output.Write(data, 0, readLength);

                                output.Flush();
                            }
                        }
                    }
                }

                return files;
            }
        }
        #endregion

        #region GetFileList
        /// <summary>获得文档中的所有文件名称</summary>
        /// <param name="inputFile">输入文档</param>
        public static IList<string> GetFileList(string inputFile)
        {
            using (var input = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                return GetFileList(inputFile);
            }
        }

        /// <summary>获得文档中的所有文件名称</summary>
        /// <param name="inputFile">输入流</param>
        public static IList<string> GetFileList(Stream input)
        {
            ZipEntry entry;
            using (var inputStream = new ZipInputStream(input))
            {
                var files = new List<string>();
                while ((entry = inputStream.GetNextEntry()) != null)
                {
                    if (entry.IsFile)
                        files.Add(entry.Name.Replace('/', '\\'));
                }
                return files;
            }
        }
        #endregion
    }
}

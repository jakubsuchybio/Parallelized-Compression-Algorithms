using CP.Storage.Compressors;
using System;
using System.IO;
using System.IO.Compression;

namespace CP.Storage
{
    public class ZipArchivist : IArchivist
    {
        private readonly ICompressor _compressor;

        public ZipArchivist(ICompressor compressor) =>
            _compressor = compressor;

        public void MakeArchive(string sourceDirectory, string archiveFileName, ArchivationSpeed archivationSpeed)
        {
            string directoryForZip = string.Empty;
            string iniFile = Path.Combine(sourceDirectory, Constants.FileNames.IniHeader);
            try
            {
                if (File.Exists(archiveFileName))
                    File.Delete(archiveFileName);

                switch (archivationSpeed)
                {
                    case ArchivationSpeed.Fastest:
                        directoryForZip = sourceDirectory;
                        break;
                    case ArchivationSpeed.Average:
                        directoryForZip = CopyAndCompressFilesRecursively(sourceDirectory, 5);
                        break;
                    case ArchivationSpeed.Best:
                        directoryForZip = CopyAndCompressFilesRecursively(sourceDirectory, 10);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(archivationSpeed), archivationSpeed, null);
                }

                // Create final mewzip
                ZipFile.CreateFromDirectory(directoryForZip, archiveFileName, CompressionLevel.NoCompression, false);
            }
            catch (Exception)
            {
            }
            finally
            {
                // We performed zipping from source directory, so clean ini file
                if (sourceDirectory == directoryForZip)
                    File.Delete(iniFile);

                // We performed zipping from temp, so clean up
                if (sourceDirectory != directoryForZip)
                    Directory.Delete(directoryForZip);
            }
        }

        public void RestoreArchive(string archiveFileName, string destinationDirectory)
        {
            string workingDirectory = Path.Combine(Path.GetTempPath(), "DeflateArchivist", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

            try
            {
                ZipFile.ExtractToDirectory(archiveFileName, workingDirectory);

                DecompressFilesInPlaceIfNeeded(workingDirectory);

                Directory.Move(workingDirectory, destinationDirectory);
            }
            catch (Exception)
            {
            }
            finally
            {
                Directory.Delete(workingDirectory);
            }
        }

        public Stream RestoreFile(string archiveFileName, string fileNameInArchive)
        {
            using (var archive = ZipFile.OpenRead(archiveFileName))
            {
                // Restore uncompressed file from Zip
                var entry = archive.GetEntry(fileNameInArchive);
                if (entry != null)
                {
                    using (var entryStream = entry.Open())
                    {
                        var memoryStream = new MemoryStream();
                        entryStream.CopyTo(memoryStream);
                        return memoryStream;
                    }
                }

                // Restore compressed file from Zip
                string compressedFileName = $"{fileNameInArchive}{_compressor.CompressedFileExtension}";
                entry = archive.GetEntry(compressedFileName);
                if (entry != null)
                {
                    using (var entryStream = entry.Open())
                    {
                        var memoryStream = new MemoryStream();
                        _compressor.Decompress(entryStream, memoryStream);
                        return memoryStream;
                    }
                }

                // File not found inside Zip
                return null;
            }
        }

        public void UpdateFile(string archiveFileName, string fileNameInArchive, Stream stream, ArchivationSpeed archivationSpeed)
        {
            using (var archive = ZipFile.Open(archiveFileName, ZipArchiveMode.Update))
            {
                // Update uncompressed file from Zip
                var entry = archive.GetEntry(fileNameInArchive);
                if (entry != null)
                {
                    entry.Delete();
                    var newEntry = archive.CreateEntry(fileNameInArchive);
                    using (var entryStream = newEntry.Open())
                    {
                        stream.CopyTo(entryStream);
                    }
                }

                // Update compressed file from Zip
                string compressedFileName = $"{fileNameInArchive}{_compressor.CompressedFileExtension}";
                entry = archive.GetEntry(compressedFileName);
                if (entry != null)
                {
                    entry.Delete();
                    var newEntry = archive.CreateEntry(compressedFileName);
                    using (var entryStream = newEntry.Open())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            _compressor.Compress(stream, memoryStream, archivationSpeed.ToIntCompressionLevel());
                            memoryStream.CopyTo(entryStream);
                        }
                    }
                }
            }
        }

        private string CopyAndCompressFilesRecursively(string sourceDirectory, int compressionLevel)
        {
            string workingDirectory = Path.Combine(Path.GetTempPath(), "DeflateArchivist", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            Directory.CreateDirectory(workingDirectory);

            // Crypt all files from source directory into working directory
            var di = new DirectoryInfo(sourceDirectory);
            foreach (var file in di.GetFiles("*", SearchOption.AllDirectories))
            {
                string relativeFilePath = file.FullName.Replace($"{sourceDirectory}{Path.DirectorySeparatorChar}", "");
                string destinationFilePath = Path.Combine(workingDirectory, relativeFilePath + _compressor.CompressedFileExtension);

                string destinationDir = Path.GetDirectoryName(destinationFilePath);
                if (destinationDir != null && !Directory.Exists(destinationDir))
                    Directory.CreateDirectory(destinationDir);

                using (var input = File.OpenRead(file.FullName))
                using (var output = File.Create(destinationFilePath))
                {
                    _compressor.Compress(input, output, compressionLevel);
                }
            }

            return workingDirectory;
        }

        private void DecompressFilesInPlaceIfNeeded(string workingDirectory)
        {
            var di = new DirectoryInfo(workingDirectory);
            foreach (var file in di.GetFiles($"*{_compressor.CompressedFileExtension}", SearchOption.AllDirectories))
            {
                string relativeCompressedFilePath = file.FullName.Replace($"{workingDirectory}{Path.DirectorySeparatorChar}", string.Empty);
                string relativeFilePath = relativeCompressedFilePath.Replace(_compressor.CompressedFileExtension, string.Empty);
                string fullFilePath = Path.Combine(workingDirectory, relativeFilePath);

                using (var source = File.OpenRead(file.FullName))
                using (var destination = File.Create(fullFilePath))
                {
                    _compressor.Decompress(source, destination);
                }

                File.Delete(file.FullName);
            }
        }
    }
}

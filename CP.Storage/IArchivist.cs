using System.IO;

namespace CP.Storage
{
    public interface IArchivist
    {
        void MakeArchive(string sourceDirectory, string archiveFileName, ArchivationSpeed archivationSpeed);
        void RestoreArchive(string archiveFileName, string destinationDirectory);

        Stream RestoreFile(string archiveFileName, string fileNameInArchive);
        void UpdateFile(string archiveFileName, string fileNameInArchive, Stream sourceStream, ArchivationSpeed archivationSpeed);
    }
}
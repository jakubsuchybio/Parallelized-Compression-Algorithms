namespace CP.Storage
{
    public enum ArchivationSpeed
    {
        Fastest,
        Average,
        Best
    }

    public enum ArchivationMethod : byte
    {
        Sequential = 0,
        Parallel = 1
    }

    public enum ArchivationAlgorithm : byte
    {
        Deflate = 0
    }
}
namespace CP.Storage
{
    public static class Constants
    {
        public static class Extensions
        {
            public const string Deflate = ".def";
            public const string GZip = ".gz";
            public const string LZ4 = ".lz4";
        }

        public static class FileNames
        {
            public const string IniHeader = "=info.ini";
            public const string CryptoXml = "examination.xml.crypto";
        }

        public static class Sizes
        {
            public const int KB = 1024;
            public const int MB = 1024 * 1024;
            public const int GB = 1024 * 1024 * 1024;
        }
    }
}
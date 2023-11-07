using System;

namespace TetrifactClient
{
    public class PackageDiffResponse
    {
        public PackageDiff PackageDiff { get; set; }

        public PackageDiffResponseTypes ResponseType { get; set; }

        public Exception Exception { get; set; }

        public string Message { get; set; }
    }
}

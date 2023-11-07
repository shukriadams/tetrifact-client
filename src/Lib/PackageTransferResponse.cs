using System;

namespace TetrifactClient
{
    public class PackageTransferResponse
    {
        public bool Succeeded { get; set; }

        public PackageTransferResultTypes Result { get; set; }

        public string Message { get; set; }

        /// <summary>
        /// Todo : we probably don't want to be passing exceptions back up this way. Either convert to expected enum member, or rethrow
        /// </summary>
        public Exception Exception { get; set; }
    }
}

namespace TetrifactClient.Models
{
    /// <summary>
    /// Represents a local version of a package. 
    /// </summary>
    public class LocalPackage
    {
        #region PROPERTIES

        /// <summary>
        /// Package id this object is connected to
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Ignore { get; set; }

        public BuildTransferStates TransferState { get; set; }

        /// <summary>
        /// If transfer or integrity check failed, message describing failure.
        /// </summary>
        public string ErrorSummary { get; set; }

        #endregion

        #region METHODS

        public bool IsPlayable()
        {
            return this.TransferState == BuildTransferStates.DoNotDelete ||
                this.TransferState == BuildTransferStates.Downloaded;
        }

        #endregion
    }
}

namespace TetrifactClient.Models
{
    public class Build
    {
        public string Key { get; set; }

        public bool Ignore { get; set; }
    
        public BuildTransferStates TransferState { get; set; }

        /// <summary>
        /// If transfer or integrity check failed, message describing failure.
        /// </summary>
        public string ErrorSummary { get; set; }

        public bool IsPlayable() 
        {
            return this.TransferState == BuildTransferStates.DoNotDelete ||
                this.TransferState == BuildTransferStates.Downloaded;
        }
    }
}

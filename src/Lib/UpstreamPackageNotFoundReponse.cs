namespace TetrifactClient
{
    /// <summary>
    /// An error response from tetrifact where the upstream package in a diff lookup was not found.
    /// </summary>
    public class UpstreamPackageNotFoundReponse : IPackageDiffQueryResponse
    {
        public string UpstreamPackageId { get; set; }
    }
}

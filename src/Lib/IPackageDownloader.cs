namespace TetrifactClient
{
    public interface IPackageDownloader : ICancel
    {
        PackageTransferResponse Download();
    }
}

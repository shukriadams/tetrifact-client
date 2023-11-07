namespace TetrifactClient
{
    public enum PackageTransferResultTypes : int
    {
        Success = 0,
        FilePathTooLong = 1,
        LocalCopyFail = 2,
        DonorFileNotFount = 3,
        FileDownloadFailed= 4
    }
}

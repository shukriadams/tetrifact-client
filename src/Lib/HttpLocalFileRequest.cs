namespace TetrifactClient
{
    public class HttpLocalFileRequest
    {
        public string SavePath { get; private set; }

        public double TimeTaken { get; private set; }

        public int Retries { get; private set; }

        public bool Succeeded { get; private set; }

        public string Error { get; private set; }
    }
}

namespace TetrifactClient
{
    public class Payload<T>
    {
        public T Success { get; set; }

        public string Error { get; set; }
    }
}

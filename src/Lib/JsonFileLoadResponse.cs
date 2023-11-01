using System;

namespace TetrifactClient
{
    /// <summary>
    /// Response from attempting to load an object from a file 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonFileLoadResponse<T>
    {
        public T Payload { get; private set; }

        public JsonFileLoadResponseErrorTypes ErrorType { get; private set; }
        
        public Exception Exception { get; private set; }

        public JsonFileLoadResponse() 
        {

        }

        public JsonFileLoadResponse(T payload)
        {
            this.Payload = payload;
        }

        public JsonFileLoadResponse(JsonFileLoadResponseErrorTypes error) 
        {
            this.ErrorType = error;
        }

        public JsonFileLoadResponse(JsonFileLoadResponseErrorTypes error, Exception ex)
        {
            this.ErrorType = error;
            this.Exception = ex;
        }
    }
}

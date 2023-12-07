using System.Collections.Generic;

namespace TetrifactClient
{
    /// <summary>
    /// Event called when package verification ends.
    /// </summary>
    /// <param name="errors"></param>
    public delegate void OnVerifyEnd(IEnumerable<string> errors);
}

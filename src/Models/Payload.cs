﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class Payload<T>
    {
        public T Success { get; set; }

        public string Error { get; set; }
    }
}

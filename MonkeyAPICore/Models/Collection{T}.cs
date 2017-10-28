using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyAPICore.Models
{
    public class Collection<T> : Resource
    {
        public T[] Value { get; set; }

    }
}

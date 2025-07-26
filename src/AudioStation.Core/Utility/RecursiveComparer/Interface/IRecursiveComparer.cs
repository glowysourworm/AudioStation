using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioStation.Core.Utility.RecursiveComparer.Interface
{
    public interface IRecursiveComparer
    {
        bool Compare<T>(T object1, T object2);
    }
}

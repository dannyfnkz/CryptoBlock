using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Tables.StringTransformers
{
    public interface IStringTransformer
    {
        string TransformToString(object obj);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Tables.StringTransformers
{
    // transforms object provided as argument into a string
    public interface IStringTransformer
    {
        /// <summary>
        /// returns a <see cref="string"/> representation of <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>
        /// <see cref="string"/> representation of <paramref name="obj"/>
        /// </returns>
        string TransformToString(object obj);
    }
}

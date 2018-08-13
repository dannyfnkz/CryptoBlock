using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace CommandHandling.Arguments
    {
        public interface ICommandArgumentConstraint
        {
            bool IsValid(String[] commandArgumentArray);
            void OnInvalidCommandArgumentArray(string[] commandArgumentArray);
        }
    }
}


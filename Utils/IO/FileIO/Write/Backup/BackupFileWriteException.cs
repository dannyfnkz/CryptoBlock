using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.FileIO.Write.Backup
    {
        /// <summary>
        /// thrown if an exception occurs during file write with backup operation.
        /// </summary>
        public class BackupFileWriteException : FileWriteException
        {
            public BackupFileWriteException(
                string filePath,
                Exception innerException = null,
                string additionalDetails = null
                )
                : base(filePath, innerException, additionalDetails)
            {

            }
        }
    }

}

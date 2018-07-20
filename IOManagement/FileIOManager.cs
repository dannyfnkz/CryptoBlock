using System.IO;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace IOManagement
    {
        public class FileIOManager
        {
            private static FileIOManager instance = new FileIOManager();

            private const string DATA_FILE_EXTENSION = ".json";

            private FileIOManager()
            {

            }

            public static FileIOManager Instance
            {
                get { return instance; }
            }

            public async Task WriteTextToDataFileAsync(string fileName, string text)
            {
                string filePath = getDataFilePath(fileName);
                await writeTextToFileAsync(filePath, text);
            }

            public void WriteTextToDataFile(string fileName, string text)
            {
                string filePath = getDataFilePath(fileName);
                writeTextToFile(filePath, text);
            }

            public bool DataFileExists(string fileName)
            {
                string filePath = getDataFilePath(fileName);

                return File.Exists(filePath);
            }

            public string ReadTextFromDataFile(string fileName)
            {
                string filePath = getDataFilePath(fileName);

                return readTextFromFile(filePath);
            }

            private string getDataFilePath(string fileName)
            {
                return fileName + DATA_FILE_EXTENSION;
            }

            private async Task writeTextToFileAsync(string filePath, string text)
            {
                using (StreamWriter writer = File.CreateText(filePath))
                {
                    await writer.WriteAsync(text);
                }
            }

            private void writeTextToFile(string filePath, string text)
            {
                File.WriteAllText(filePath, text);              
            }

            private string readTextFromFile(string filePath)
            {
                string text = File.ReadAllText(filePath);

                return text;
            }
        }
    }
}
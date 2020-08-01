using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Restless.Plugin.Framework
{
    public class MultiPartStream
    {
        #region Private
        // Specs say that the body of each part and it's header are separated by two CRLFs
        private readonly byte[] separatorBytes = Encoding.UTF8.GetBytes("\r\n\r\n");
        private readonly byte[] headerBytes = new byte[100];
        private readonly Regex contentRegex = new Regex(@"Content-Length:\s?(?<length>[0-9]+)\r\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly BinaryReader reader;
        #endregion

        /************************************************************************/

        #region Constructor
        public MultiPartStream(Stream stream)
        {
            reader = new BinaryReader(new BufferedStream(stream));
        }
        #endregion

        /************************************************************************/

        #region Public methods
        public Task<byte[]> NextPartAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    // every part has it's own headers
                    string headerSection = ReadContentHeaderSection(reader);
                    // let's parse the header section for the content-length
                    int length = GetPartLength(headerSection);
                    // now let's get the image
                    return reader.ReadBytes(length);
                }
                catch
                {
                    return null; 
                }
            });
        }

        /// <summary>
        /// Closes the underlying binary reader.
        /// </summary>
        public void Close()
        {
            reader.Dispose();
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private string ReadContentHeaderSection(BinaryReader stream)
        {
            // headers and content in multi part are separated by two \r\n
            bool found;

            int count = 4;
            stream.Read(headerBytes, 0, 4);
            for (int i = 0; i < headerBytes.Length; i++)
            {
                found = SeparatorBytesExistsInArray(i, headerBytes);
                if (!found)
                {
                    headerBytes[count] = stream.ReadByte();
                    count++;
                }
                else
                    break;
            }
            return Encoding.UTF8.GetString(headerBytes, 0, count);
        }

        private int GetPartLength(string headerSection)
        {
            Match m = contentRegex.Match(headerSection);
            return int.Parse(m.Groups["length"].Value);
        }

        private bool SeparatorBytesExistsInArray(int position, byte[] array)
        {
            bool result = false;
            for (int i = position, j = 0; j < separatorBytes.Length; i++, j++)
            {
                result = array[i] == separatorBytes[j];
                if (!result)
                {
                    break;
                }
            }
            return result;
        }
        #endregion
    }
}
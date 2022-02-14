using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3UPSManager
{
    public partial class Form1 : Form
    {
        // From https://stackoverflow.com/questions/16183788/case-sensitive-directory-exists-file-exists
        // (edited)
        static public bool DirExistsMatchCase(string path)
        {

            // Figure out if the case (of the final part) is the same
            string thisDir = Path.GetFileName(path);
            thisDir = thisDir.Substring(thisDir.LastIndexOf('\\') + 1);
            string actualDir = Path.GetFileName(Directory.GetDirectories(Path.GetDirectoryName(path), thisDir)[0]);
            //DisplayInfo.Print("thisDir: " + thisDir + ", actualDir: " + actualDir);
            return thisDir == actualDir;
        }

        // From https://stackoverflow.com/questions/16183788/case-sensitive-directory-exists-file-exists
        public static bool FileExistsCaseSensitive(string filename)
        {
            string name = Path.GetDirectoryName(filename);

            return name != null
                   && Array.Exists(Directory.GetFiles(name), s => s == Path.GetFullPath(filename));
        }

        // From https://stackoverflow.com/questions/2435695/converting-a-md5-hash-byte-array-to-a-string
        public static string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }

    }
}

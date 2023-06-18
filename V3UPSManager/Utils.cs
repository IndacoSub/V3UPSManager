using System.Text;

namespace V3UPSManager;

public partial class MainWindow : Form
{
    // From https://stackoverflow.com/questions/16183788/case-sensitive-directory-exists-file-exists
    // (edited)
    public static bool DirExistsMatchCase(string path)
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

    // From https://stackoverflow.com/questions/1395205/better-way-to-check-if-a-path-is-a-file-or-a-directory
    public static bool IsDirectory(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            return false;
        }

        // get the file attributes for file or directory
        FileAttributes attr = File.GetAttributes(path);

        if (attr.HasFlag(FileAttributes.Directory))
            return true;
        return false;
    }
}
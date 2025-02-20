using System.Runtime.InteropServices;

namespace Janus.Helpers
{
    public static class PathHelper
    {

        public static string[] PathSplitter(string path)
        {
            return path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
        }


        public static string GetCredentialPath()
        {
            string basePath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: use %APPDATA%
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // MacOS: use /Library/Application Support
                string home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                basePath = Path.Combine(home, "Library", "Application Support");
            }
            else
            {
                // Misc case
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }


            string credentialPath = Path.Combine(basePath, "Janus");

            if (File.Exists(credentialPath))
            {
                throw new IOException($"A file exists at '{credentialPath}', but a directory is expected.");
            }

            if (!Directory.Exists(credentialPath))
            {
                Directory.CreateDirectory(credentialPath);
            }


            return credentialPath;
        }
    }

}

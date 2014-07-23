//npp_toolbar_image Shell32.dll|3
using System;
using System.IO;
using System.Text.RegularExpressions;
using NppScripts;

/**
 * 快速打开PHP文件
 */
public class Script : NppScript
{
    public override void Run() {
        string line, path;
        Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GETCURLINE, 0, out line);
        Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_GETCURRENTDIRECTORY, 0, out path);

        line = Regex.Replace(line, "^[^'|\"]+", "");
        line = Regex.Replace(line, "[^'|\"]+$", "");
        line = line.Replace("'", "");
        line = line.Replace("\"", "");
        path = path + "\\" + line;

        if (!File.Exists(path)) return;

        Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_DOOPEN, 0, path);
    }
}

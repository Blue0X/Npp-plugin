//npp_toolbar_image Shell32.dll|3
//css_inc EditorHelper.cs
using System;
using System.IO;
using System.Text.RegularExpressions;
using NppScripts;
using NppScripts.EditorHelper;

/**
 * 快速打开PHP文件
 */
public class Script : NppScript
{
    public override void Run() {
        string line = Editor.getCurrentLine();
        string path = Editor.getCurrentPath();

        line = Regex.Replace(line, "^[^'|\"]+", "");
        line = Regex.Replace(line, "[^'|\"]+$", "");
        line = line.Replace("'", "");
        line = line.Replace("\"", "");
        path = path + "\\" + line;

        if (!File.Exists(path)) return;
        Editor.open(path);
    }
}

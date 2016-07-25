using System;
using System.IO;

namespace mIRCOptionsEditor
{
    public class Editor
    {
        public static void Main(string[] args)
        {
            string filePath = args[0];
            int autoGetType = Convert.ToInt32(args[1]);          
            if (autoGetType != 1 && autoGetType != 2)
            {
                autoGetType = 1;
            }
            int autoGet = 1;
            int showDialog = 1;
            int limitAutoGet = 1;
            FileInfo file = new FileInfo(filePath);
            if (File.Exists(file.FullName + ".BAK"))
            {
                File.Delete(file.FullName + ".BAK");
            }
            file.CopyTo(file.FullName + ".BAK");

            string[] allLines = File.ReadAllLines(file.FullName);
            for (int i = 0; i < allLines.Length; i++)
            {
                if (allLines[i] == "[options]")
                {
                    string n0Line = allLines[i + 1];
                    string n3Line = allLines[i + 4];
                    string n7Line = allLines[i + 8];

                    //n0 line: Auto-Get setting
                    string[] lineInfo = n0Line.Split('=');
                    string[] lineVariables = lineInfo[1].Split(',');
                    lineVariables[14] = autoGet.ToString();
                    lineInfo[1] = String.Join(",", lineVariables);
                    n0Line = String.Join("=", lineInfo);
                    allLines[i + 1] = n0Line;

                    //n3 line: "Auto-Get type" and "Show get dialog for non-trusted users"
                    lineInfo = n3Line.Split('=');
                    lineVariables = lineInfo[1].Split(',');
                    lineVariables[26] = autoGetType.ToString();
                    lineVariables[33] = showDialog.ToString();
                    lineInfo[1] = String.Join(",", lineVariables);
                    n3Line = String.Join("=", lineInfo);
                    allLines[i + 4] = n3Line;

                    //n7 line: "Limit auto-get to non-trusted users"
                    lineInfo = n7Line.Split('=');
                    lineVariables = lineInfo[1].Split(',');
                    lineVariables[18] = limitAutoGet.ToString();
                    lineInfo[1] = String.Join(",", lineVariables);
                    n7Line = String.Join("=", lineInfo);
                    allLines[i + 8] = n7Line;

                    break;
                }
            }

            File.WriteAllLines(file.FullName, allLines);
            return;
        }
    }
}

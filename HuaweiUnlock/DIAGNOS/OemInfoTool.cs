﻿using System;
using System.Collections.Generic;
using System.IO;
using static HuaweiUnlocker.LangProc;
namespace HuaweiUnlocker.DIAGNOS
{
    public class OemInfoTool
    {
        public static List<string> data = new List<string>();
        public static List<int> Offsets = new List<int>();
        private static FileStream FileAll;
        public static void Decompile(string path, int counttoread, string Header = "4F454D5F494E464F06")
        {
            //UNCORRECT
            Offsets.Clear();
            data.Clear();
            if (Directory.Exists("UnlockFiles/OemInfoData")) Directory.Delete("UnlockFiles/OemInfoData", true);
            Directory.CreateDirectory("UnlockFiles/OemInfoData");
            LOG(0, "Reading File...");
            FileAll = File.OpenRead(path);
            FileAll.Position = 0;
            LOG(0, "Length: " + FileAll.Length);
            byte[] piza = new byte[Header.Length];
            while (FileAll.Read(piza, 0, Header.Length) > 0)
            {
                if (CRC.BytesToHexString(piza).Equals(Header))
                {
                    LOG(0, "Found Offset: " + FileAll.Position);
                    Offsets.Add((int)FileAll.Position);
                }
            }

            for (int i = 0; i < Offsets.Count; i++)
            {
                int curoffset = Offsets[i];
                LOG(0, "Reading OEM_INFO AT: " + curoffset);
                FileAll.Position = curoffset;
                byte[] read;
                if (Offsets.Count > i + 1)
                {
                    int nextoffset = Offsets[i + 1];
                    int countread = nextoffset - curoffset;
                    read = new byte[countread];
                    FileAll.Read(read, 0, countread);
                    data.Add("OEM_INFO_" + curoffset + ".header");
                }
                else
                {
                    read = new byte[FileAll.Length - curoffset];
                    FileAll.Read(read, 0, (int)FileAll.Length - curoffset);
                }

                LOG(0, "Writing: OEM_INFO_" + curoffset + ".header");
                File.WriteAllBytes("UnlockFiles/OemInfoData/OEM_INFO_" + curoffset + ".header", read);
            }
        }
        public static void Compile(string intpath, string outpath)
        {
            //NOT WORK 
            FileStream outfile = new FileStream(outpath, FileMode.OpenOrCreate);
            outfile.Seek(0, SeekOrigin.Begin);
            foreach (var aoffset in Offsets)
            {
                outfile.Seek(aoffset, SeekOrigin.Current);
                LOG(0, "Adding: OEM_INFO_" + aoffset + ".header");
                byte[] filebts = File.ReadAllBytes(intpath + "/OEM_INFO_" + aoffset + ".header");
                outfile.Write(filebts, 0, filebts.Length);
            }
            outfile.Close();
            outfile.Dispose();
            FileAll.Close();
            FileAll.Dispose();
        }
    }
}

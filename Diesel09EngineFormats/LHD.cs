using System;
using System.Collections.Generic;
using System.IO;

namespace Diesel09EngineFormats {
    public class LHDDatabaseEntry {
        public LHDDatabaseEntry(string fclSource, string fullPath) {
            this.fclSource = fclSource;
            this.fullPath = fullPath;

            this.path = Path.ChangeExtension(this.fullPath, "");
            this.extension = Path.GetExtension(this.fullPath);
        }

        public string fclSource;
        public string fullPath;

        public string path;
        public string extension;
    }

    public class LHD {
        public LHD() {}
        public LHD(string path) => Load(path);

        private Dictionary<string, LHDDatabaseEntry> _entries = new Dictionary<string, LHDDatabaseEntry>();

        public Dictionary<string, LHDDatabaseEntry> Entries {
            get {
                return this._entries;
            }
            set {
                this._entries = value;
            }
        }

        public void AddEntry(string fclSource, string fullPath) {
            AddEntry(new LHDDatabaseEntry(fclSource, fullPath));
        }

        public void AddEntry(LHDDatabaseEntry entry) {
            this.Entries[entry.fullPath] = entry;
        }

        public List<LHDDatabaseEntry> GetDatabaseEntries() {
            List<LHDDatabaseEntry> entries = new List<LHDDatabaseEntry>();
            entries.AddRange(this._entries.Values);
            return entries;
        }

        public bool Load(string path) {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                using (var br = new BinaryReader(fs)) {

                    uint stringCount = br.ReadUInt32();
                    string[] stringArray = new string[stringCount];

					for (uint i = 0; i < stringCount; i++) {
                        stringArray[i] = br.ReadNullTerminatedString();
                    }

                    uint assembledStringCount = br.ReadUInt32();
					string[] assembledStringArray = new string[assembledStringCount];

					for (uint i = 0; i < assembledStringCount; i++) {
                        uint stringdex1 = br.ReadUInt32();
                        uint stringdex2 = br.ReadUInt32();
                        br.ReadUInt32();
                        br.ReadUInt32();

                        string string1 = stringArray[stringdex1];
                        string string2 = stringArray[stringdex2];

                        assembledStringArray[i] = string1 + string2;
                    }

                    string fileName = Path.GetFileNameWithoutExtension(path);
                    uint crNumFiles = 0;

                    uint fclCount = br.ReadUInt32();

                    for (uint i = 0; i < fclCount; i++) {
                        uint fclStringdex = br.ReadUInt32();
                        string fclName = stringArray[fclStringdex];
                        uint fileCount = br.ReadUInt32();

                        Console.WriteLine(fclStringdex);
                        Console.WriteLine(fclName);
                        Console.WriteLine(fileCount);
                        Console.WriteLine("----");

                        for (uint fi = 0; fi < fileCount; fi++) {
                            uint assembledPathIndex = br.ReadUInt32();

                            AddEntry(fclName, assembledStringArray[assembledPathIndex]);
                        }

                        for (uint fi = 0; fi < fileCount; fi++) {
                            uint unk2 = br.ReadUInt32();
                        }
                    }

                    Console.WriteLine(br.BaseStream.Position);

				}
            }

            return true;
        }
    }
}

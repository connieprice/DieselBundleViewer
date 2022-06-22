using DieselBundleViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Diesel09EngineFormats {
    public class LHDDatabaseEntry {
        public LHDDatabaseEntry(string fclSource, string fullPath, uint offset, uint size) {
            this.fclSource = fclSource;
            this.fullPath = fullPath;

            this.offset = offset;
            this.size = size;

            this.path = Path.ChangeExtension(this.fullPath, "");
            this.extension = Path.GetExtension(this.fullPath);
        }

        public string fclSource;
        public string fullPath;

        public string path;
        public string extension;

        public uint offset;
        public uint size;
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

        public void AddEntry(string fclSource, string fullPath, uint offset, uint size) {
            AddEntry(new LHDDatabaseEntry(fclSource, fullPath, offset, size));
        }

        public void AddEntry(LHDDatabaseEntry entry) {
            this.Entries[entry.fullPath] = entry;
        }

        private Dictionary<string, uint> _fclEntries = new Dictionary<string, uint>();
        public Dictionary<string, uint> FCLEntries {
            get {
                return this._fclEntries;
            }
            set {
                this._fclEntries = value;
            }
        }

        public void AddFCLEntry(string flcName, uint size) {
            this.FCLEntries[flcName] = size;
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

                    string fclDir = Path.GetDirectoryName(path);

                    uint fclCount = br.ReadUInt32();

                    for (uint i = 0; i < fclCount; i++) {
                        uint fclStringdex = br.ReadUInt32();
                        string fclName = stringArray[fclStringdex];
                        uint fileCount = br.ReadUInt32();

                        uint fclSize = 0;
                        string fclPath = fclDir + "\\" + fclName + ".fcl";
                        using (Stream fclFile = FileEntry.UnpackFCL(fclName, new FileStream(fclPath, FileMode.Open, FileAccess.Read))) {
                            fclSize = (uint)fclFile.Length;
                        }

                        AddFCLEntry(fclName, fclSize);

                        uint[] entryPathIds = new uint[fileCount];
                        uint[] entryOffsets = new uint[fileCount];

                        for (uint fi = 0; fi < fileCount; fi++) {
                            uint assembledPathIndex = br.ReadUInt32();
                            entryPathIds[fi] = assembledPathIndex;
                        }

                        for (uint fi = 0; fi < fileCount; fi++) {
                            uint offset = br.ReadUInt32();
                            entryOffsets[fi] = offset;
                        }

                        for (uint fi = 0; fi < fileCount; fi++) {
                            uint assembledPathIndex = entryPathIds[fi];
                            uint offset = entryOffsets[fi];

                            uint nextFi = fi + 1;
                            uint end = 0;
                            if (nextFi == fileCount) {
                                end = fclSize;
							} else {
                                end = entryOffsets[nextFi];
							}
                            uint size = end - offset;

                            AddEntry(fclName, assembledStringArray[assembledPathIndex], offset, size);
                        }
                    }

                    Console.WriteLine(br.BaseStream.Position);

				}
            }

            return true;
        }
    }
}

using DieselEngineFormats.Bundle;
using System;
using System.IO;

namespace Diesel09EngineFormats {
	class FCLPackageFileEntry : PackageFileEntry {
		public new void ReadEntry(BinaryReader br, bool readLength = false) {
			throw new NotImplementedException();
		}

		public new void WriteEntry(BinaryWriter writer, bool writeLength = false) {
			throw new NotImplementedException();
		}
	}
}

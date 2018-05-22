using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace Core.Helper {
  
  public class ZIPHelper {

    public static bool CreateZipFile(ArrayList filePaths, string outputPath) {
      bool result = true;

      ZipOutputStream pack = new ZipOutputStream(File.Create(outputPath));
      try {

        // set compression level
        pack.SetLevel(5);

        foreach (string filePath in filePaths) {
          FileStream fs = File.OpenRead(filePath);

          // allocate buffer
          byte[] buffer = new byte[fs.Length];
          fs.Read(buffer, 0, buffer.Length);

          // write the zip entry and its data 
          ZipEntry entry = new ZipEntry(filePath.Substring(filePath.LastIndexOf(Path.DirectorySeparatorChar) + 1));
          pack.PutNextEntry(entry);
          pack.Write(buffer, 0, buffer.Length);
        }

      } catch {
        result = false;
      } finally {
        pack.Finish();
        pack.Close();
      }

      return result;
    }

    public static ArrayList ExtractFileFromZipFile(string filePath, string destinationPath, bool isInvoicePackage) {
      ArrayList result = new ArrayList();

      ZipFile zip = new ZipFile(File.OpenRead(filePath));
      try {
        foreach (ZipEntry entry in zip) {
          if (entry.IsFile) {
            string fileName = entry.Name;
            if (isInvoicePackage) {
              fileName = fileName.Replace("unsigned", "signed");
            }

            result.Add(fileName);

            Stream inputStream = zip.GetInputStream(entry);
            FileStream fileStream = new FileStream(destinationPath + fileName, FileMode.Create);
            try {
              CopyStream(inputStream, fileStream);
            } finally {
              fileStream.Close();
              inputStream.Close();
            }
          }
        }
      } finally {
        zip.Close();
      }

      return result;
    }

    private static void CopyStream(Stream input, Stream output) {
      byte[] buffer = new byte[0x1000];
      int read;
      while ((read = input.Read(buffer, 0, buffer.Length)) > 0) {
        output.Write(buffer, 0, read);
      }
    }
  }
}

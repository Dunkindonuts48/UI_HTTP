using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTP_NET_Project
{
    internal class PNG
    {
        private string fileName;  // Filename of the image
        private string contentType;  // Content type, typically "image/png"
        private byte[] data;  // Binary data of the image

        public PNG(string fileName, byte[] data)
        {
            FileName = fileName;
            ContentType = "image/png";  // Default content type for PNG images
            Data = data;
        }

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public string ContentType
        {
            get { return contentType; }
            set { contentType = value; }
        }

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        public override string ToString()
        {
            return $"FileName: {FileName}\n" +
                   $"ContentType: {ContentType}\n" +
                   $"Data Length: {Data.Length} bytes";
        }
 
    }
}

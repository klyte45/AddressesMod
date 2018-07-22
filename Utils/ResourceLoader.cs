using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Klyte.Addresses.Utils
{
    internal class ResourceLoader
    {

        public static Assembly ResourceAssembly
        {
            get {
                //return null;
                return Assembly.GetAssembly(typeof(ResourceLoader));
            }
        }

        public static byte[] loadResourceData(string name, string prefix = "Klyte.Addresses.")
        {
            name = prefix + name;

            UnmanagedMemoryStream stream = (UnmanagedMemoryStream)ResourceAssembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                AdrUtils.doErrorLog("Could not find resource: " + name);
                return null;
            }

            BinaryReader read = new BinaryReader(stream);
            return read.ReadBytes((int)stream.Length);
        }

        public static string loadResourceString(string name, string prefix = "Klyte.Addresses.")
        {
            name = prefix + name;

            UnmanagedMemoryStream stream = (UnmanagedMemoryStream)ResourceAssembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                AdrUtils.doErrorLog("Could not find resource: " + name);
                return null;
            }

            StreamReader read = new StreamReader(stream);
            return read.ReadToEnd();
        }

        public static Texture2D loadTexture(int x, int y, string filename)
        {
            try
            {
                Texture2D texture = new Texture2D(x, y);
                texture.LoadImage(loadResourceData(filename));
                return texture;
            }
            catch (Exception e)
            {
                AdrUtils.doErrorLog("The file could not be read:" + e.Message);
            }

            return null;
        }
    }
}

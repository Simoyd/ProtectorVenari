using System.IO;
using System.Xml.Serialization;

namespace ProtectorVenari
{
    /// <summary>
    /// Basic class used to persist a file.
    /// </summary>
    /// <remarks>NOT THREAD SAFE!</remarks>
    /// <typeparam name="T">The type to serialize to file.</typeparam>
    class PersistanceFile<T>
        where T : class, new()
    {
        /// <summary>
        /// Creates a new instance of PersistanceFile.
        /// </summary>
        /// <param name="dataFile">The path of the file to use for persistance.</param>
        public PersistanceFile(string dataFile)
        {
            m_dataFile = dataFile;
        }

        /// <summary>
        /// XML serializer for the specified type.
        /// </summary>
        private XmlSerializer m_dataXs = new XmlSerializer(typeof(T));

        /// <summary>
        /// The path of the file to use for persistance.
        /// </summary>
        private string m_dataFile;

        /// <summary>
        /// The instance of data to serialize.
        /// </summary>
        private T m_data = null;

        /// <summary>
        /// The instance of data to serialize.
        /// </summary>
        public T Data
        {
            get
            {
                if (m_data == null)
                {
                    if (File.Exists(m_dataFile))
                    {
                        // If it's null and the file exists, then deserialize it from disk
                        using (FileStream fs = new FileStream(m_dataFile, FileMode.Open, FileAccess.Read))
                        {
                            m_data = m_dataXs.Deserialize(fs) as T;
                        }
                    }

                    // Create a new instance so it's never null
                    if (m_data == null)
                    {
                        m_data = new T();
                    }
                }

                return m_data;
            }
            set
            {
                m_data = value;
            }
        }

        /// <summary>
        /// Saves the data to disk
        /// </summary>
        public void Save()
        {
            // Reference instance locally so we don't try to deserialize while we serialize
            T dataTemp = Data;

            // Save and flush to disk
            using (FileStream fs = new FileStream(m_dataFile, FileMode.Create, FileAccess.Write))
            {
                m_dataXs.Serialize(fs, dataTemp);
                fs.Flush();
                fs.Close();
            }
        }
    }
}

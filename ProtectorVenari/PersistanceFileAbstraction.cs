namespace ProtectorVenari
{
    /// <summary>
    /// Class to inheret for using PersistanceFile in a thread safe manner.
    /// </summary>
    /// <typeparam name="T">The type to serialize to file.</typeparam>
    abstract class PersistanceFileAbstraction<T>
        where T : class, new()
    {
        /// <summary>
        /// Creates a new instance of PersistanceFileAbstraction
        /// </summary>
        /// <param name="dataFile">The path of the file to use for persistance.</param>
        public PersistanceFileAbstraction(string dataFile)
        {
            persistanceFile = new PersistanceFile<T>(dataFile);
        }

        /// <summary>
        /// The persistance file for the child class to use
        /// </summary>
        protected PersistanceFile<T> persistanceFile;

        /// <summary>
        /// Saves the file to disk
        /// </summary>
        public void Save()
        {
            lock (persistanceFile)
            {
                persistanceFile.Save();
            }
        }
    }
}

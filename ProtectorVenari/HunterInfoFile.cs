using System.Collections.Generic;
using System.Linq;

namespace ProtectorVenari
{
    /// <summary>
    /// Persistance file for hunter notification configurations
    /// </summary>
    class HunterInfoFile : PersistanceFileAbstraction<List<HunterInfo>>
    {
        /// <summary>
        /// Creates a new instance of HunterInfoFile
        /// </summary>
        public HunterInfoFile() : base("HunterInfo.xml") { }

        /// <summary>
        /// Retrieves the notification configuration for the specified server
        /// </summary>
        /// <param name="guild">The server ID to retrieve the notification configuration for</param>
        /// <returns>The notification configuration for the specified server</returns>
        public HunterInfo GetGuild(ulong guild)
        {
            lock (persistanceFile)
            {
                return persistanceFile.Data.Where(cur => cur.Guild == guild).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets all notification configurations
        /// </summary>
        /// <returns>All notification configurations</returns>
        public HunterInfo[] GetAll()
        {
            lock (persistanceFile)
            {
                return persistanceFile.Data.ToArray();
            }
        }

        /// <summary>
        /// Updates the notification configuration for a server
        /// </summary>
        /// <param name="info">The updated server notification configuration</param>
        public void UpdateInfo(HunterInfo info)
        {
            lock (persistanceFile)
            {
                HunterInfo oldInfo = GetGuild(info.Guild);

                if (oldInfo != null)
                {
                    persistanceFile.Data.Remove(oldInfo);
                }

                persistanceFile.Data.Add(info);
                persistanceFile.Save();
            }
        }

        /// <summary>
        /// Removes the notification configuration for the specified server
        /// </summary>
        /// <param name="guild">The server ID to remove the configuration for</param>
        public void Remove(ulong guild)
        {
            lock (persistanceFile)
            {
                HunterInfo oldInfo = GetGuild(guild);

                if (oldInfo != null)
                {
                    persistanceFile.Data.Remove(oldInfo);
                }

                persistanceFile.Save();
            }
        }
    }
}

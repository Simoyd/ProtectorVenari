using System;
using System.Xml.Serialization;

namespace ProtectorVenari
{
    /// <summary>
    /// Class to store which channel to send notifications to and which role should be pinged per server
    /// </summary>
    [Serializable]
    public class HunterInfo
    {
        /// <summary>
        /// The server to send the message to
        /// </summary>
        [XmlAttribute]
        public ulong Guild { get; set; }

        /// <summary>
        /// The channel ID in the server to send the message to
        /// </summary>
        [XmlAttribute]
        public ulong Channel { get; set; }

        /// <summary>
        /// The role ID to ping
        /// </summary>
        [XmlAttribute]
        public ulong Role { get; set; }
    }
}

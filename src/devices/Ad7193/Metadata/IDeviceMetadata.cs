using System;
using System.Collections.Generic;
using System.Text;

namespace Ad7193.Metadata
{
    /// <summary>
    /// Metadata interface for any supported IoT device
    /// </summary>
    public interface IDeviceMetadata
    {
        /// <summary>
        /// Name of the manufacturer of the device
        /// </summary>
        public string Manufacturer { get; }

        /// <summary>
        /// Name of the product
        /// </summary>
        public string Product { get; }

        /// <summary>
        /// Category of the device
        /// </summary>
        public string ProductCategory { get; }

        /// <summary>
        /// Description of the device
        /// </summary>
        public string ProductDescription { get; }

        /// <summary>
        /// The URI of the datasheet of the device
        /// </summary>
        public string DataSheetURI { get; }
    }
}

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
        string Manufacturer { get; }

        /// <summary>
        /// Name of the product
        /// </summary>
        string Product { get; }

        /// <summary>
        /// Category of the device
        /// </summary>
        string ProductCategory { get; }

        /// <summary>
        /// Description of the device
        /// </summary>
        string ProductDescription { get; }

        /// <summary>
        /// The URI of the datasheet of the device
        /// </summary>
        string DataSheetURI { get; }
    }
}

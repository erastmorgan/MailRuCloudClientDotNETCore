//-----------------------------------------------------------------------------------------------
// <copyright file="Size.cs" company="Erast Korolev">
//     Created in 2017, just under by MIT license. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------------------------

namespace MailRuCloudClient.Data
{
    using System;

    /// <summary>
    /// Defines storage units of disk size.
    /// </summary>
    public enum StorageUnit
    {
        /// <summary>
        /// Byte unit.
        /// </summary>
        Byte,

        /// <summary>
        /// Kilobyte unit.
        /// </summary>
        Kb,

        /// <summary>
        /// Megabyte unit.
        /// </summary>
        Mb,

        /// <summary>
        /// Gigabyte unit.
        /// </summary>
        Gb,

        /// <summary>
        /// Terabyte unit.
        /// </summary>
        Tb
    }

    /// <summary>
    /// Defines item size in cloud.
    /// </summary>
    public class Size
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Size" /> class.
        /// </summary>
        /// <param name="sourceValue">Default value in bytes.</param>
        internal Size(long sourceValue)
        {
            this.DefaultValue = sourceValue;
            this.SetNormalizedValue();
        }

        /// <summary>
        /// Gets default value in bytes.
        /// </summary>
        public long DefaultValue { get; internal set; }

        /// <summary>
        /// Gets normalized auto detected value.
        /// </summary>
        public double NormalizedValue { get; private set; }

        /// <summary>
        /// Gets normalized auto detected storage unit.
        /// </summary>
        public StorageUnit NormalizedType { get; private set; }

        /// <summary>
        /// Detect normalized value and storage unit.
        /// </summary>
        private void SetNormalizedValue()
        {
            if (this.DefaultValue < 1024.0)
            {
                this.NormalizedType = StorageUnit.Byte;
                this.NormalizedValue = this.DefaultValue;
            }
            else if (this.DefaultValue >= 1024.0 && this.DefaultValue < 1048576L)
            {
                this.NormalizedType = StorageUnit.Kb;
                this.NormalizedValue = this.DefaultValue / 1024.0;
            }
            else if (this.DefaultValue >= 1048576L && this.DefaultValue < 1073741824L)
            {
                this.NormalizedType = StorageUnit.Mb;
                this.NormalizedValue = this.DefaultValue / 1024.0 / 1024.0;
            }
            else if (this.DefaultValue >= 1073741824L && this.DefaultValue < 1099511627776L)
            {
                this.NormalizedType = StorageUnit.Gb;
                this.NormalizedValue = this.DefaultValue / 1024.0 / 1024.0 / 1024.0;
            }
            else
            {
                this.NormalizedType = StorageUnit.Tb;
                this.NormalizedValue = this.DefaultValue / 1024.0 / 1024.0 / 1024.0 / 1024.0;
            }
            
            this.NormalizedValue = Math.Round(this.NormalizedValue, 2);
        }
    }
}

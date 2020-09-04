using System;
using System.Linq;
using System.IO.Ports;

namespace Iot.Device.HuskyLens
{
    /// <summary>
    /// Represent the base type for object detected by the Huskylens camera
    /// </summary>
    public abstract class HuskyObject
    {
        /// <summary>
        /// Learnt id
        /// </summary>
        /// <value>the id of the value learnt, 0 if unknown</value>
        public int Id { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The learnt id</param>
        protected HuskyObject(int id)
        {
            Id = id;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Exceptions
{
    class InvalidEntityException : Exception
    {
        /// <summary>
        /// Constructor used with a message.
        /// </summary>
        /// <param name="message">String message of exception.</param>
        public InvalidEntityException(string reason)
        : base("Registered a invalid entity. Reason: "+reason)
        {
        }
    }
}

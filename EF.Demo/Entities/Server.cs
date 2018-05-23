using System;
using System.Collections.Generic;
using System.Text;

namespace EF.Demo.Entities
{
    public class Server
    {
        #region Properties
        public string Id { get; set; }
        public string Name { get; set; }
        public string URI { get; set; }

        public virtual ICollection<Client> Clients { get; set; }
        #endregion

        public object[] GetKey() => new object[] { Id };
    }
}

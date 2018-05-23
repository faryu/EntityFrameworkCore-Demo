using System;
using System.Collections.Generic;
using System.Text;

namespace EF.Demo.Entities
{
    public class Client
    {
        #region Properties
        public int Id { get; set; }

        public Server Server { get; set; }
        public string Name { get; set; }
        #endregion

        public object[] GetKey() => new object[] { Id, Server.Id };
    }
}

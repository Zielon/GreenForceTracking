using System.Collections.Generic;
using Library.Frames.Client;

namespace Server.Mock
{
    public static class DataBaseMock
    {
        public static List<SystemUser> Users = new List<SystemUser>
        {
            new SystemUser { Login = "root", Password = "123" },
            new SystemUser { Login = "antek", Password = "aaa" },
            new SystemUser { Login = "korek", Password = "kkk" },
            new SystemUser { Login = "Wojtek", Password = "w" },
            new SystemUser { Login = "Filip", Password = "f" }
        };
    }
}

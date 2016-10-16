using Library.Frames.Client;
using System.Collections.Generic;

namespace Server.Mock
{
    public static class DataBaseMock
    {
        public static List<SystemUser> Users = new List<SystemUser>
        {
               new SystemUser {  Login = "root", Password = "123" },
               new SystemUser {  Login = "antek", Password = "aaa" },
               new SystemUser {  Login = "korek", Password = "kkk" }
        };
    }
}

using System;
using Pastebin;

namespace PathOfFilters
{
    public class Pastebin
    {
        private readonly global::Pastebin.Pastebin _pastebin;
        private User _user;
        public string Username { get; set; }
        public string Password { get; set; }
        public User PastebinUser {
            get { return _user ?? Login(); }
            set { value = _user; }
        }


        public Pastebin()
        {
            _pastebin = new global::Pastebin.Pastebin(@"b5ca25e4debd166a8ca1029de8aabffd");
        }

        internal User Login()
        {
            try
            {
                if (Username == null || Password == null) return null;
                _user = _pastebin.LogIn(Username, Password);
            }
            catch (PastebinException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            return _user ?? null;
        }
    }


}

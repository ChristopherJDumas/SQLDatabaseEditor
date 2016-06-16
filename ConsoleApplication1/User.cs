using System;
using System.Data.Linq.Mapping;

namespace DatabaseEditor
{
    [Table(Name = "Users")]
    public class User
    {

        private int _ID;
        [Column(IsPrimaryKey=true, Storage="_ID", IsDbGenerated=true)]
        public int ID
        {
            get
            {
                return this._ID;
            }
            set
            {
                this._ID = value;
            }
        }

        private string _FirstName;
        [Column(Storage="_FirstName")]
        public string FirstName
        {
            get
            {
                return this._FirstName.Trim();
            }
            set
            {
                this._FirstName = value;
            }
        }

        private string _LastName;
        [Column(Storage="_LastName")]
        public string LastName
        {
            get
            {
                return this._LastName.Trim();
            }
            set
            {
                this._LastName = value;
            }
        }

        private string _UserName;
        [Column(Storage = "_UserName")]
        public string UserName
        {
            get
            {
                return this._UserName.Trim();
            }
            set
            {
                this._UserName = value;
            }
        }

        private string _Email;
        [Column(Storage = "_Email")]
        public string Email
        {
            get
            {
                return this._Email.Trim();
            }
            set
            {
                this._Email = value;
            }
        }

        private DateTime _Created = DateTime.Now;
        [Column(Storage = "_Created")]
        public DateTime Created
        {
            get
            {
                return this._Created;
            }
            set
            {
                this._Created = value;
            }
        }
    }
}

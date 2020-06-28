using DataLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;

namespace DAL.Repository
{
	public class ContactRepositoryContrib : IContactRepository
	{
		private readonly IDbConnection _conn;

		public ContactRepositoryContrib(string connString)
		{
			_conn = new SqlConnection(connString);
		}

		public Contact Add(Contact contact)
		{
			contact.Id = (int) _conn.Insert(contact);
			return contact;
		}

		public Contact Find(int id) => _conn.Get<Contact>(id);

		public IEnumerable<Contact> GetAll() => _conn.GetAll<Contact>();

		public IEnumerable<Contact> GetAllWithChildren()
		{
			throw new NotImplementedException();
		}

		public Contact GetWithChildren(int id)
		{
			throw new NotImplementedException();
		}

		public void Remove(Contact contact) => _conn.Delete(contact);

		public Contact Save(Contact contact)
		{
			throw new NotImplementedException();
		}

		public void Update(Contact contact) => _conn.Update(contact);
	}
}

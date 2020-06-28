using DataLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Dapper;
using System.Linq;
using System.Transactions;

namespace DAL.Repository
{
	public class ContactRepositoryStoredPr : IContactRepository
	{
		private readonly IDbConnection _conn;

		public ContactRepositoryStoredPr(string connString)
		{
			_conn = new SqlConnection(connString);
		}

		public Contact Add(Contact contact) => Save(contact);

		public Contact Find(int id) =>
			_conn.Query<Contact>("GetContact", new { id }, commandType: CommandType.StoredProcedure).SingleOrDefault();

		public IEnumerable<Contact> GetAll() => GetAllWithChildren();

		public IEnumerable<Contact> GetAllWithChildren()
		{
			using(var scope = new TransactionScope())
			{
				var lookup = new Dictionary<int, Contact>();
				_conn.Query<Contact, Address, Contact>("GetContactsWithAdresses", (c, a) =>
				{
					Contact contact;
					if (!lookup.TryGetValue(c.Id, out contact))
						lookup.Add(c.Id, contact = c);
					if (contact.Addresses == null)
						contact.Addresses = new List<Address>();
					contact.Addresses.Add(a);
					return contact;
				},
				commandType: CommandType.StoredProcedure);
				scope.Complete();
				return lookup.Values;
			}
		}

		public Contact GetWithChildren(int id)
		{
			using (var results = _conn.QueryMultiple("GetContact", new { Id = id }, commandType: CommandType.StoredProcedure))
			{
				var contactEntity = results.Read<Contact>().SingleOrDefault();
				var addresses = results.Read<Address>();
				if (contactEntity != null && addresses.Any())
				{
					contactEntity.Addresses.AddRange(addresses);
				}
				return contactEntity;
			}
		}

		public void Remove(Contact contact) =>
			_conn.Execute("DeleteContact", new { contact.Id }, commandType: CommandType.StoredProcedure);
		
		public Contact Save(Contact contact)
		{
			using (var tran = new TransactionScope())
			{
				var parameters = new DynamicParameters();
				parameters.Add("@Id", contact.Id, DbType.Int32, ParameterDirection.InputOutput);
				parameters.Add("@FirstName", contact.FirstName);
				parameters.Add("@LastName", contact.LastName);
				parameters.Add("@Company", contact.Company);
				parameters.Add("@Title", contact.Title);
				parameters.Add("@Email", contact.Email);
				_conn.Execute("SaveContact", parameters, commandType: CommandType.StoredProcedure);
				
				contact.Id = parameters.Get<int>("@Id");

				foreach (var address in contact.Addresses.Where(a => !a.IsDeleted))
				{
					address.ContactId = contact.Id;
					var addressParams = new DynamicParameters(new
					{
						address.ContactId,
						address.AddressType,
						address.StreetAddress,
						address.City,
						address.StateId,
						address.PostalCode
					});
					addressParams.Add("@Id", address.Id, DbType.Int32, ParameterDirection.InputOutput);
					_conn.Execute("SaveAddress", addressParams, commandType: CommandType.StoredProcedure);
					address.Id = addressParams.Get<int>("@Id");
				}

				foreach (var address in contact.Addresses.Where(a => a.IsDeleted))
				{
					_conn.Execute("DeleteAddress", new { address.Id }, commandType: CommandType.StoredProcedure);
				}
				tran.Complete();
				return contact;
			}
		}

		public void Update(Contact contact)
		{
			throw new NotImplementedException();
		}
	}
}

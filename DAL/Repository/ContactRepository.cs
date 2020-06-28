using Dapper;
using DataLayer;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

namespace DAL.Repository
{
	public class ContactRepository : IContactRepository
	{
		private readonly IDbConnection _conn;

		public ContactRepository(string connString)
		{
			_conn = new SqlConnection(connString);
		}

		public Contact Find(int id) =>
			_conn.Query<Contact>("SELECT * FROM Contacts c WHERE c.Id = @Id", new { id }).SingleOrDefault();

		public IEnumerable<Contact> GetAll() => _conn.Query<Contact>("SELECT * FROM Contacts");

		public IEnumerable<Contact> GetAllWithChildren()
		{
			var lookup = new Dictionary<int, Contact>();
			_conn.Query<Contact, Address, Contact>(
				@"SELECT c.*, a.* " +
				"FROM Contacts c " +
				"INNER JOIN Addresses a ON c.Id = a.ContactId", (c, a) =>
				{
					Contact contact;
					if (!lookup.TryGetValue(c.Id, out contact))
						lookup.Add(c.Id, contact = c);

					if (contact.Addresses == null)
						contact.Addresses = new List<Address>();

					contact.Addresses.Add(a);
					return contact;
				});

			return lookup.Values;
		}

		public Contact GetWithChildren(int id)
		{
			var sql = "SELECT * FROM Contacts WHERE Id = @Id; " +
				"SELECT * FROM Addresses a WHERE a.ContactId = @Id";

			using (var results = _conn.QueryMultiple(sql, new { Id = id }))
			{
				var contact = results.Read<Contact>().SingleOrDefault();
				var addresses = results.Read<Address>();
				if (contact != null && addresses.Any())
				{
					contact.Addresses.AddRange(addresses);
				}
				return contact;
			}
		}

		public Contact Add(Contact contact)
		{
			var sta = "INSERT INTO Contacts (FirstName, LastName, Email, Company, Title)" +
				" VALUES(@FirstName, @LastName, @Email, @Company, @Title);" +
				" SELECT CAST(SCOPE_IDENTITY() as int)";
			contact.Id = _conn.Query<int>(sta, contact).Single();
			return contact;
		}

		public Contact Save(Contact contact)
		{
			using(var tran = new TransactionScope())
			{
				if (contact.IsNew)
					Add(contact);
				else
					Update(contact);

				foreach (var address in contact.Addresses.Where(a => !a.IsDeleted))
				{
					address.ContactId = contact.Id;
					if (address.IsNew)
						Add(address);
					else
						Update(address);
				}
				foreach (var address in contact.Addresses.Where(a => a.IsDeleted))
				{
					_conn.Execute("DELETE FROM Addresses WHERE Id = @Id", new { address.Id });
				}
				tran.Complete();
				return contact;
			}
		}

		public void Update(Contact contact)
		{
			var sql = "UPDATE Contacts " +
				"SET FirstName = @FirstName, " +
				"LastName = @LastName, " +
				"Email = @Email, " +
				"Company = @Company, " +
				"Title = @Title " +
				"WHERE Id = @Id";
			_conn.Execute(sql, contact);
		}

		public void Remove(Contact contact) => _conn.Execute("DELETE FROM Contacts WHERE Id = @Id", new { contact.Id });

		private void Add(Address address)
		{
			var sta = "INSERT INTO Addresses (ContactId, AddressType, City, StateId, PostalCode)" +
				" VALUES(@ContactId, @AddressType, @City, @StateId, @PostalCode);" +
				" SELECT CAST(SCOPE_IDENTITY() as int)";
			_conn.Query<int>(sta, address).Single();
		}

		private void Update(Address address)
		{
			var sql = "UPDATE Addresses " +
				"SET ContactId = @ContactId, " +
				"AddressType = @AddressType, " +
				"Streetaddress = @Streetaddress, " +
				"City = @City, " +
				"StateId = @StateId, " +
				"PostalCode = @PostalCode " +
				"WHERE Id = @Id";
			_conn.Execute(sql, address);
		}
	}
}

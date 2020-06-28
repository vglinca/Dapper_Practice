using Dapper;
using DataLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DAL.Repository
{
	public class ContactReposotoryEx : IContactRepositoryEx
	{
		private readonly IDbConnection _conn;

		public ContactReposotoryEx(string connString)
		{
			_conn = new SqlConnection(connString);
		}

		public IEnumerable<Contact> GetAllWithAddresses()
		{
			var sql = "SELECT c.*, a.* " +
				"FROM Contacts c " +
				"INNER JOIN Addresses a ON c.Id = a.ContactId";
			var lookup = new Dictionary<int, Contact>();
			var contacts = _conn.Query<Contact, Address, Contact>(sql, (c, a) =>
			{
				if(!lookup.TryGetValue(c.Id, out var curentContact))
				{
					curentContact = c;
					lookup.Add(curentContact.Id, curentContact);
				}
				curentContact.Addresses.Add(a);
				return curentContact;
			});
			return contacts;
		}

		public IEnumerable<Contact> GetListByIds(params int[] ids) =>
			_conn.Query<Contact>("SELECT * FROM Contacts WHERE Id IN @Ids", new { ids });

		public IEnumerable<dynamic> GetListByIdsDynamic(List<string> fileds, params int[] ids)
		{
			var filedsBuilder = new StringBuilder();
			for(int i = 0; i < fileds.Count; i++)
			{
				if(i != 0) 
				{
					filedsBuilder.Append(", ");
				}
				filedsBuilder.Append(fileds[i]);
			}
			return _conn.Query($"SELECT {filedsBuilder.ToString()} FROM Contacts WHERE Id IN @Ids", new { ids });
		}

		public int InsertContactsRange(List<Contact> contacts)
		{
			var sta = "INSERT INTO Contacts (FirstName, LastName, Email, Company, Title)" +
				" VALUES(@FirstName, @LastName, @Email, @Company, @Title);" +
				" SELECT CAST(SCOPE_IDENTITY() as int)";
			return _conn.Execute(sta, contacts);
		}
	}
}

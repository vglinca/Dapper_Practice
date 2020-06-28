using DAL.Repository;
using DataLayer;
using Microsoft.Extensions.Configuration;
using Runner.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Runner
{
	class Program
	{
		private static IConfigurationRoot config;

		static void Main(string[] args)
		{
			Initialize();

			//var id = InsertContact();
			//id.Output();

			//Console.WriteLine("-------------------------");

			//var contacts = GetContacts();
			//contacts.Output();

			//Console.WriteLine("-------------------------");

			//UpdateContact(2);
			//var contact = FindById(2);
			//contact.Output();

			//Console.WriteLine("-------------------------");

			//var lastContact = contacts.Last();
			//DeleteContact(lastContact.Id);

			//Console.WriteLine("-------------------------");

			//InsertContacts();

			var contacts = GetContacts();
			contacts.Output();

			var contactsDynamic = CreateRepositoryEx().GetListByIdsDynamic(new List<string>
			{
				nameof(Contact.Id), nameof(Contact.FirstName), nameof(Contact.Email)
			}, 2, 3, 4);
			contactsDynamic.Output();
		}

		static IEnumerable<Contact> GetContacts()
		{
			var repository = CreateRepository();
			return repository.GetAllWithChildren();
		}

		static Contact FindById(int id)
		{
			var contact = CreateRepository().GetWithChildren(id);
			return contact;
		}

		static int InsertContact()
		{
			var repository = CreateRepository();
			var contact = new Contact
			{
				FirstName = "Joe",
				LastName = "Blow",
				Email = "jow.blow@gmail.com",
				Company = "Microsoft",
				Title = "Developer"
			};

			var address = new Address
			{
				AddressType = "Home",
				StreetAddress = "123 Main Street",
				City = "Baltimore",
				StateId = 1,
				PostalCode = "22222"
			};
			contact.Addresses.Add(address);
			repository.Save(contact);

			return contact.Id;
		}

		static void InsertContacts()
		{
			var contacts = new List<Contact>
			{
				new Contact
				{
					FirstName = "Joe",
					LastName = "Blow",
					Email = "jow.blow@gmail.com",
					Company = "Microsoft",
					Title = "Developer"
				},
				new Contact
				{
					FirstName = "Rachel",
					LastName = "Andrews",
					Email = "rch.andr@gmail.com",
					Company = "PLuralsoght",
					Title = "Developer"
				}
			};
			var n = CreateRepositoryEx().InsertContactsRange(contacts);
		}

		static void UpdateContact(int id)
		{
			var repository = CreateRepository();
			var contact = repository.GetWithChildren(id);
			contact.FirstName = "Michael";
			if (contact.Addresses.Any())
			{
				contact.Addresses[0].StreetAddress = "699 Main Street";
			}
			contact.Addresses.Add(new Address
			{
				AddressType = "Home",
				StreetAddress = "124 Main Street",
				City = "Baltimore",
				StateId = 1,
				PostalCode = "123321"
			});

			repository.Save(contact);
		}

		static void DeleteContact(int id)
		{
			var repository = CreateRepository();
			var contactToDelete = repository.Find(id);
			repository.Remove(contactToDelete);
		}

		private static void Initialize()
		{
			config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.Build();
		}

		private static IContactRepository CreateRepository() => 
			new ContactRepositoryStoredPr(config.GetConnectionString("DefaultConnection"));

		private static IContactRepositoryEx CreateRepositoryEx() =>
			new ContactReposotoryEx(config.GetConnectionString("DefaultConnection"));
	}
}

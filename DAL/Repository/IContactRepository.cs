using DataLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
	public interface IContactRepository
	{
		Contact Find(int id);
		Contact GetWithChildren(int id);
		IEnumerable<Contact> GetAll();
		IEnumerable<Contact> GetAllWithChildren();
		Contact Add(Contact contact);
		Contact Save(Contact contact);
		void Update(Contact contact);
		void Remove(Contact contact);
	}
}

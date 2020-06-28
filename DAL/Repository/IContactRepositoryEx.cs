using DataLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repository
{
	public interface IContactRepositoryEx
	{
		IEnumerable<Contact> GetListByIds(params int[] ids);
		IEnumerable<Contact> GetAllWithAddresses();
;		IEnumerable<dynamic> GetListByIdsDynamic(List<string> fileds, params int[] ids);
		int InsertContactsRange(List<Contact> contacts);
	}
}

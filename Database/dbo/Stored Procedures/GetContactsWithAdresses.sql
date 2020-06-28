CREATE PROCEDURE [dbo].[GetContactsWithAdresses]
AS
BEGIN
	SELECT 
		c.Id, 
		c.FirstName, 
		c.LastName, 
		c.Email, 
		c.Company, 
		c.Title, 
		a.Id,
		a.ContactId,
		a.AddressType,
		a.StreetAddress,
		a.City,
		a.City,
		a.PostalCode
	FROM Contacts c
	INNER JOIN Addresses a ON c.Id = a.ContactId
END;

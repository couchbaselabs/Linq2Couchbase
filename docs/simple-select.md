
Controlling output with Select
==============================
The select clause produces the result of the query and allows you to shape the format of each element.

##Simple Select##

The simplest select is to return a document that maps directly to a POCO:

	var beers = from b in context.Query<Beer>()
				select b;

This will emit query that looks something like this:

	SELECT * FROM `beer-sample` WHERE type='beer';

Assuming the Beer class has a document filter; if not the predicate would be removed and the entire keystore will be returned. Any returning fields from the * will be mapped to the Properties of Beer unless a match cannot be made. If a match cannot be made, the value will be ignored.

##Selecting a subset of a document's fields##
If you wish to constrain the output, you specify which fields you want returned:

	var beers = from b in context.Query<Beer>()
		select new 
		{
			name = b.Name, 
			abv = b.Abv
		};

This will return only the Name and Abv fields for each document in the keystore. The result will be a projection of the original type - an anonymous type. Note that the execution of the query is deferred until the query is iterated over in foreach loop. This behavior is consistent and similar to the behavior of other LINQ providers.
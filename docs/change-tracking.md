Change Tracking (Experimental Developer Preview)
================================================
Change tracking allows for reading documents from Couchbase using LINQ queries, modifying the documents in memory, and then saving the documents back as a unit of work.  Only documents which are modified are saved.

Note that this feature is currently an experimental feature being offered as a developer preview.  The API is subject to change, and backwards compatibility following the semver versioning rules is not guaranteed.

##Preparing Your Document POCOs
Many change tracking systems for database access layers work based on comparing records.  As each record is read two copies are kept in memory, the original and the current record.  When it's time to save changes, the two records are compared to check for differences that need to be saved.

With Couchbase's document-based store, this can result in a very large memory footprint.  As each document can easily be several kilobytes, keeping two copies of each document in memory is doubling the required memory.  To address this, Linq2Couchbase doesn't use the two copy comparison approach.  Instead, only a single document is kept in memory, and it is flagged as dirty anytime a property is changed.

This support is provided using dynamic proxies.  As each document is read and new objects are created, instead of directly creating your POCO object a proxy of your POCO is created instead.  The properties are overridden dynamically so that changes to the property can trigger the dirty flag.  To support this feature, there are some requirements that must be met.

1. Document POCOs must not be sealed.  This allows the proxy to inherit from them.
2. Every property on the document must be declared virtual (Overridable in VB).  This allows each property to be overriden by the dynamic proxy.  Any property which is not virtual will not have change tracking applied to it.
3. For lists and arrays, the properties must be declared as the type IList<T> or ICollection<T>.  If they are not then that branch of the document tree will not have change tracking.
4. These rules must apply to the entire document tree, meaning any child objects of the main document object must also have virtual members, etc.

```
public class DocumentRoot
{
    public virtual string StringProperty { get; set; }

    public virtual SubDocument ObjectProperty { get; set; }

	public virtual IList<SubDocument> ListProperty { get; set; }
}

public class SubDocument
{
    public virtual int IntegerProperty { get; set; }
}
```

##Using Change Tracking
Using change tracking has these requirements.

1. Call BeginChangeTracking **before** you execute the query that reads the documents.
2. Make sure you are reading entire documents, without using a select projection.
3. Ensure that your document POCOs follow the rules in the previous section.
4. Call SubmitChanges to save any changes back to Couchbase
Any returns true if any of the documents in the list meet the criteria provided.  If no criteria is provided, then it will return true so long as there is at least one document in the list.
5. Alternatively, call EndChangeTracking to throw away your changes.  Note that your in-memory documents will still be modified, they are simply no longer queued to be saved.

```
using (var cluster = new Cluster()) {
	using (var bucket = cluster.OpenBucket("beer-sample")) {
		var context = new BucketContext(bucket);

		context.BeginChangeTracking();

		var result = context.Query<Beer>().First(p => p.Abv == 6);

		result.Abv = 6.5;

		context.SubmitChanges();
	}
}
```

**Note:** BeginChangeTracking is implemented using a counter, which is incremented each time it is called.  You must call SubmitChanges the same number of times as you call BeginChangeTracking before the changes are saved.

##Adding and Removing Documents
While change tracking is enabled, you may also add and remove documents from the bucket.  This is done using the Save and Remove methods.

When called outside of a change tracking context, these methods will act immediately against the bucket.  However, when called after BeginChangeTracking they will instead be queued and execute later when SubmitChanges is called.

```
using (var cluster = new Cluster()) {
	using (var bucket = cluster.OpenBucket("beer-sample")) {
		var context = new BucketContext(bucket);

		context.BeginChangeTracking();

		context.Save(new Beer()
		{
			Name = "My New Beer",
			BreweryId = "some_brewery",
			Abv = 6
		});

		context.SubmitChanges();
	}
}
```

```
using (var cluster = new Cluster()) {
	using (var bucket = cluster.OpenBucket("beer-sample")) {
		var context = new BucketContext(bucket);

		context.BeginChangeTracking();

		var result = context.Query<Beer>().First(p => p.Abv == 6);

		context.Remove(result);

		context.SubmitChanges();
	}
}
```

##Document IDs
Documents which are read from the database using a query are always saved to the database using the document ID that was read from the database during the query.  If you need to change the document ID of a document, you must manually remove the old document and add a new document.

For new documents being added via a call to Save, you must define the document ID.  This is done by applying the Key attribute to the property that represents the document ID.  This may be a read only or a read/write property.  Below is an example of one approach to building document IDs.

```
using System.ComponentModel.DataAnnotations;

namespace Documents
{
	[DocumentTypeFilter("shape")]
	public class Shape
	{
		[Key]
		public string Key {
			get { return Type + "-" + ShapeId; } 
		}

		public string Type {
			get { return "shape"; }
		}

		public virtual Guid ShapeId { get; set; }
		public virtual string Name { get; set; }
	}
}
```

##Using Custom Serializers
In addition to the requirements for custom serializers detailed in the [Custom JSON Serializers](custom-serializers.md) document, there is an extra requirement to support change tracking.

As objects are deserialized, they must be proxied.  Linq2Couchbase does this by injecting an [ICustomObjectCreator](https://github.com/couchbase/couchbase-net-client/blob/master/Src/Couchbase/Core/Serialization/ICustomObjectCreator.cs) into the deserialization process.  Therefore, the custom deserializer must support and use the ICustomObjectCreator that is provided.

The custom deserializer indicates support by returning true for CustomObjectCreator in the [SupportedDeserializationOptions](https://github.com/couchbase/couchbase-net-client/blob/master/Src/Couchbase/Core/Serialization/SupportedDeserializationOptions.cs) object.

When deserializing, the IExtendedTypeSerializer may receive an ICustomObjectCreator in the (DeserializationOptions)[https://github.com/couchbase/couchbase-net-client/blob/master/Src/Couchbase/Core/Serialization/DeserializationOptions.cs] object.  When it does, it should use this creator to create any types that return true for CanCreateObject.

Here is [an example of how this method was implemented for Newtonsoft's Json.Net](https://github.com/couchbase/couchbase-net-client/blob/03d7957226da6f7c3e05220a21e7ebeeb0519b93/Src/Couchbase/Core/Serialization/DefaultSerializer.cs#L216). 
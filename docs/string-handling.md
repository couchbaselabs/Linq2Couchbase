String Handling
================
Many .Net string operations are supported by N1QL, and can be used directly in your LINQ queries.

## Supported String Operations
- Length property
- Character index (i.e. str[2])
- ToUpper() and ToLower()
- String concatenation, using String.Concat(...) or the + operator
- Trim(), Trim(char[]), TrimStart(), and TrimEnd()
- Substring(startIndex) and Substring(startIndex, length)
- IndexOf(char) and IndexOf(string)
- Split() and Split(char)
- Replace(oldValue, newValue)

## String Comparisons
String equality comparisons can be performed using the == and != operators.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
						where beer.Name == "21A IPA"
						select beer;

			foreach (var doc in query) {
				// do work
				// query will return beers with the name 21A IPA
			}
		}
	}

For other string comparison types, you may use String.Compare.  The most readable method is to compare the two strings, then always compare the result to zero.

	using (var cluster = new Cluster()) {
		using (var bucket = cluster.OpenBucket("beer-sample")) {
			var context = new BucketContext(bucket);

			var query = from beer in context.Query<Beer>()
						where String.Compare(beer.Name, "21A IPA") > 0
						select beer;

			foreach (var doc in query) {
				// do work
				// query will return beers with a name greater than, but not equal to, 21A IPA
			}
		}
	}

If you use this format, then the comparison operator before the zero is always the same as the comparison you're using for the strings.

**Note:** Any call to String.Compare within a query must be combined with a comparison to either -1, 0, or 1.  Any other usage will throw an exception.
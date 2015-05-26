Linq2Couchbase
==================

A Language Integrated Query (LINQ) provider for querying Couchbase Server with N1QL using the Couchbase .NET SDK

List of supported and proposed N1QL features can be found [here](https://docs.google.com/document/d/1hPNZ-qTKpVzQsFwg_1uUueltzNL1wA75L5F-hYF92Cw/edit?usp=sharing). In some cases only partial support (e.g. generating the N1QL statement) is supported, the "context" may not know how to handle the output for example. 

To build the project, create a directory on your drive and clone both this project and couchbase-net-client in it. For example:

1. cd C:\
2. mkdir working
3. cd working
4. git clone git@github.com:couchbase/couchbase-net-client.git
5. git clone git@github.com:couchbaselabs/Linq2Couchbase.git
6. Your directory structure should look like this:
* C:\working
 * couchbase-net-client
 * Linq2Couchbase

Once that is done, the paths should sync-up and the Linq2Couchbase project should have the the couchbase-net-client\Couchbase project referenced.

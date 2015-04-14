# Dagent
Simple microORM for .NET. 

Works with SQL Server, MySQL, and SQLite.
## Simple Code
Data manipulation is possible with a simple code.
(Checkout the [Wiki](https://github.com/akawa777/Dagent/wiki) for more documentation.)
### Select
```cs
public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; }        
}

DagentDatabase db = new DagentDatabase("connectionStringName");
List<Customer> customers = db.Query<Customer>("select * from Customers").List();
```
### Insert, Update, Delete
```cs
Customer customer = new Customer { CustomerId = 1, Name = "Ziba-nyan" };

db.Command<Customer>("Customers", "CustomerId").Insert(customer);

customer.Name = "Buchi-nyan";
db.Command<Customer>("Customers", "CustomerId").Update(customer);

db.Command<Customer>("Customers", "CustomerId").Delete(customer);
```
## Performance
A key feature is performance. The following metrics show how long it takes to execute 500 SELECT statements against a DB and map the data returned to objects.

The performance tests are same test as the [Dapper](https://github.com/StackExchange/dapper-dot-net "Dapper").
![Alt Text](https://github.com/akawa777/Dagent/blob/master/resultOfPerformanceTest.png)
## Pure POCO
Dagent Keep the POCO. Your class does not need to inherit class or put attribute. And is not dependent on sql and table.
## License
The MIT License (MIT)

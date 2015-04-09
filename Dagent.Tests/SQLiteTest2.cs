using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.EntityClient;
using System.Linq.Expressions;
using Dagent.Models;

namespace Dagent.Tests2
{
public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; }
    public int CustomerCategoryId { get; set; }

    public CustomerCategory Category { get; set; } 
    public List<CustomerPurchase> Purchases { get; set; } 
}

public class CustomerCategory
{
    public int CustomerCategoryId { get; set; }
    public string CategoryName { get; set; }
}

public class CustomerPurchase
{
    public int CustomerId { get; set; }
    public int PurchaseNo { get; set; }
    public int ProductId { get; set; }
    public string PurchaseContent { get; set; }

    public Product Product { get; set; }
}

public class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }    
}   

    //[TestClass]
    public class SQLiteTest
    {
        //[TestMethod]
        public void GetStarted()
        {


DagentDatabase db = new DagentDatabase("connectionStringName");

List<Customer> customers = db.Query<Customer>("select * from Customers where CustomerId > @CustomerId", new { CustomerId = 1000 }).List();

Customer customer = new Customer { CustomerId = 1, Name = "Ziba-nyan" };

db.Command<Customer>("Customers", "CustomerId").Insert(customer);

customer.Name = "Buchi-nyan";
db.Command<Customer>("Customers", "CustomerId").Update(customer);

db.Command<Customer>("Customers", "CustomerId").Delete(customer);


        }

        public void QuerySingle()
        {


DagentDatabase db = new DagentDatabase("connectionStringName");

Customer customer = db.Query<Customer>("Customers", new { CustomerId = 1 }).Single();
customer = db.Query<Customer>("select * from Customers where CustomerId = @CustomerId", new { CustomerId = 1 }).Single();


        }

        public void QueryList()
        {


DagentDatabase db = new DagentDatabase("connectionStringName");
List<Customer> customers =
    db.Query<Customer>("Customers").List();

customers =
    db.Query<Customer>(
        "select * from Customers where CustomerId > @CustomerId",
        new { CustomerId = 10 })
    .List();


        }

        public void QueryPaging()
        {


DagentDatabase db = new DagentDatabase("connectionStringName");

int totalCount = 0;
List<Customer> customers =
    db.Query<Customer>(
        "select * from Customers where CustomerId > @CustomerId",
        new { CustomerId = 10 })
    .Page(10, 100, out totalCount);


        }

        public void Mapping()
        {
DagentDatabase db = new DagentDatabase("connectionStringName");
List<Customer> customers =
    db.Query<Customer>(
        "select * from Customers where CustomerId > @CustomerId",
        new { CustomerId = 10 })
    .Each((model, row) =>
    {
        model.CustomerId = row.Get<int>("CustomerId");
        model.Name = row.Get<string>("Name");
    })
    .List();
             
        }

        public void MappingOneToOne()
        {


DagentDatabase db = new DagentDatabase("connectionStringName");
List<Customer> customers =
    db.Query<Customer>(@"
        select 
            * 
        from 
            Customers Customer
        inner join
            CustomerCategories Category
        on
            Category.CustomerCategoryId = Customer.CustomerCategoryId
        where 
            Customer.CustomerId > @CustomerId",
        new { CustomerId = 10 })
    .Each((model, row) =>
    {
        CustomerCategory category = new CustomerCategory();
        category.CustomerCategoryId = row.Get<int>("CustomCaregoryId");
        category.CategoryName = row.Get<string>("CategoryName");

        model.Category = category;
    })
    .List();
customers =
    db.Query<Customer>(@"
        select 
            * 
        from 
            Customers Customer
        inner join
            CustomerCategories Category
        on
            Category.CustomerCategoryId = Customer.CustomerCategoryId
        where 
            Customer.CustomerId > @CustomerId",
        new { CustomerId = 10 })
    .Each((model, row) =>
    {
        row.Map(model, x => x.Category).Do();
    })
    .List();
    

        }

        public void MappingOneToMany()
        {


DagentDatabase db = new DagentDatabase("connectionStringName");

List<Customer> customers =
    db.Query<Customer>(@"
        select 
            * 
        from 
            Customers Customer
        left join
            CustomerPurchases Purchase
        on
            CustomerPurchase.CustomerId = Customer.CustomerId
        where 
            Customer.CustomerId > @CustomerId
        order by
            Customer.CustomerId,
            Purchase.PurchaseNo",
        new { CustomerId = 10 })
    .Unique("CustomerId")
    .Each((model, row) =>
    {
        if (model.Purchases == null)
        {
            model.Purchases = new List<CustomerPurchase>();
        }

        if (row["PurchaseNo"] != DBNull.Value)
        {
            CustomerPurchase purchase = new CustomerPurchase();
            purchase.CustomerId = row.Get<int>("CustomerId");
            purchase.PurchaseNo = row.Get<int>("PurchaseNo");
            purchase.PurchaseContent = row.Get<string>("PurchaseContent");

            model.Purchases.Add(purchase);
        }        
    })
    .List();

customers =
    db.Query<Customer>(@"
        select 
            * 
        from 
            Customers Customer
        left join
            CustomerPurchases Purchase
        on
            CustomerPurchase.CustomerId = Customer.CustomerId
        where 
            Customer.CustomerId > @CustomerId
        order by
            Customer.CustomerId,
            Purchase.PurchaseNo",
        new { CustomerId = 10 })
    .Unique("CustomerId")
    .Each((model, row) => 
    {
        row.Map(model, x => x.Purchases, "PurchaseNo").Do();
    })
    .List();


        }

        public void MappingNested()
        {


    DagentDatabase db = new DagentDatabase("connectionStringName");

List<Customer> customers =
    db.Query<Customer>(@"
        select 
            * 
        from 
            Customers Customer
        left join
            CustomerPurchases Purchase
        on
            CustomerPurchase.CustomerId = Customer.CustomerId
        inner join
            Products Product
        on
            Product.ProductId = Purcase.ProductId
        where 
            Customer.CustomerId > @CustomerId
        order by
            Customer.CustomerId,
            Purchase.PurchaseNo",
        new { CustomerId = 10 })
    .Unique("CustomerId")
    .Each((model, row) =>
    {
        if (model.Purchases == null)
        {
            model.Purchases = new List<CustomerPurchase>();
        }

        if (row["PurchaseNo"] != DBNull.Value)
        {
            CustomerPurchase purchase = new CustomerPurchase();
            purchase.CustomerId = row.Get<int>("CustomerId");
            purchase.PurchaseNo = row.Get<int>("PurchaseNo");
            purchase.PurchaseContent = row.Get<string>("PurchaseContent");

            Product product = new Product();
            product.ProductId = row.Get<int>("ProductId");
            product.ProductName = row.Get<string>("ProductName");

            purchase.Product = product;

            model.Purchases.Add(purchase);
        }
    })
    .List();

customers =
    db.Query<Customer>(@"
        select 
            * 
        from 
            Customers Customer
        left join
            CustomerPurchases Purchase
        on
            CustomerPurchase.CustomerId = Customer.CustomerId
        inner join
            Products Product
        on
            Product.ProductId = Purcase.ProductId
        where 
            Customer.CustomerId > @CustomerId
        order by
            Customer.CustomerId,
            Purchase.PurchaseNo",
        new { CustomerId = 10 })
    .Unique("CustomerId")
    .Each((model, row) =>
    {
        row.Map(model, x => x.Purchases, "PurchaseNo")
            .Unique("PurchaseNo")
            .Each(purchase => 
            {
                row.Map(purchase, x => x.Product).Do();
            }).Do();       
    })
    .List();


        }

        public void InsertUpdateCreate()
        {


DagentDatabase db = new DagentDatabase("connectionStringName");

Customer customer = new Customer { CustomerId = 1 };            

db.Command<Customer>("Customer", "CustomerId").Map((updateRow, model) =>
{
    updateRow["Name"] = "Ziba-nyan";
})
.Insert(customer);
        }

        public void ConnectionScope()
        {
DagentDatabase db = new DagentDatabase("connectionStringName");

using(IConnectionScope scope = db.ConnectionScope())
{
    List<Customer> customers = db.Query<Customer>("Customers").List();

    List<CustomerCategory> customerCategories = 
        db.Query<CustomerCategory>("CustomerCategory").List();
}


        }

        public void TransactionScope()
        {


DagentDatabase db = new DagentDatabase("connectionStringName");

using (ITransactionScope scope = db.TransactionScope())
{
    Customer customer = new Customer { CustomerId = 1, Name = "Ziba-nyan" };

    db.Command<Customer>("Customer", "CustomerId").Insert(customer);

    scope.Commit();

    customer.Name = "Buchi-nyan";

    db.Command<Customer>("Customer", "CustomerId").Update(customer);

    scope.Rollback();
}


        }

        public void TextBuilder()
        {


string selectSql = @"
    select 
        * 
    from 
        Customers Customer    
    where 
        {{condition}}";

TextBuilder textBuilder = new TextBuilder(
    selectSql,
    new { condition = "Customer.CustomerId > @CustomerId" }
);

string orderSql = @"
    order by
        {{sort}}";

textBuilder.Append(
    orderSql,
    new { sort = "Customer.Name" }
);

string sqlSelectOrder = textBuilder.Generate();

DagentDatabase db = new DagentDatabase("connectionStringName");

List<Customer> customers =
    db.Query<Customer>(sqlSelectOrder, new { CustomerId = 10 }).List();


        }

    }

    
    
}

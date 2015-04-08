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
    public CustomerCategory CustomerCategory { get; set; } 
    public List<CustomerPurchase> CustomerPurchases { get; set; } 
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
    public string PurchaseContent { get; set; }        
}    

    //[TestClass]
    public class SQLiteTest
    {
        //[TestMethod]
        public void GetStarted()
        {
            DagentDatabase db = new DagentDatabase("connectionStringName");

            List<Customer> customers = db.Query<Customer>("select * from Customers where CustomerId > @CustomerId", new { CustomerId = 1000 }).Fetch();

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
                db.Query<Customer>("Customers").Fetch();

            customers =
                db.Query<Customer>(
                    "select * from Customers where CustomerId > @CustomerId",
                    new { CustomerId = 10 })
                .Fetch();
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
    .ForEach((model, currentRow, state) =>
    {
        model.CustomerId = currentRow.Get<int>("CustomerId");
        model.Name = currentRow.Get<string>("Name");
    })
    .Fetch();
             
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
    .ForEach((model, currentRow, state) =>
    {
        currentRow.Map(model, x => x.CustomerCategory).Do();
    })
    .Fetch();
    
    customers =
    db.Query<Customer>(
    "select * from Customers where CustomerId > @CustomerId",
    new { CustomerId = 10 })
    .ForEach((model, currentRow, state) =>
    {
        CustomerCategory category = new CustomerCategory();
        category.CustomerCategoryId = currentRow.Get<int>("CustomCaregoryId");
        category.CategoryName = currentRow.Get<string>("CategoryName");

        model.CustomerCategory = category;
        
    })
    .Fetch();
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
            CustomerPurchases CustomerPurchase
        on
            CustomerPurchase.CustomerId = Customer.CustomerId
        where 
            Customer.CustomerId > @CustomerId",
        new { CustomerId = 10 })
    .Unique("CustomerId")
    .ForEach((model, currentRow, state) =>
    {
        if (model.CustomerPurchases == null)
        {
            model.CustomerPurchases = new List<CustomerPurchase>();
        }

        if (currentRow["PurchaseNo"] != DBNull.Value)
        {
            CustomerPurchase purchase = new CustomerPurchase();
            purchase.CustomerId = currentRow.Get<int>("CustomerId");
            purchase.PurchaseNo = currentRow.Get<int>("PurchaseNo");
            purchase.PurchaseContent = currentRow.Get<string>("PurchaseContent");

            model.CustomerPurchases.Add(purchase);
        }        
    })
    .Fetch();

customers =
    db.Query<Customer>(@"
        select 
            * 
        from 
            Customers Customer
        left join
            CustomerPurchases CustomerPurchase
        on
            CustomerPurchase.CustomerId = Customer.CustomerId
        where 
            Customer.CustomerId > @CustomerId",
        new { CustomerId = 10 })
    .Unique("CustomerId")
    .ForEach((model, currentRow, state) => 
    {
        currentRow.Map(model, x => x.CustomerPurchases).Do("PurchaseNo");
    })
    .Fetch();
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
    List<Customer> customers = db.Query<Customer>("Customers").Fetch();
    List<CustomerCategory> customerCategories = db.Query<CustomerCategory>("CustomerCategory").Fetch();
}
        }

        public void TransactionScope()
        {
DagentDatabase db = new DagentDatabase("connectionStringName");

using (ITransactionScope scope = db.TransactionScope())
{
    Customer customer = new Customer { CustomerId = 1 };

    db.Command<Customer>("Customer", "CustomerId").Map((updateRow, model) =>
    {
        updateRow["Name"] = "Ziba-nyan";
    })
    .Insert(customer);

    scope.Commit();

    db.Command<Customer>("Customer", "CustomerId").Map((updateRow, model) =>
    {
        updateRow["Name"] = "Buchi-nyan";
    })
    .Update(customer);

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
    left join
        CustomerPurchases CustomerPurchase
    on                    
        CustomerPurchase.CustomerId = Customer.CustomerId
    where 
        {{condition}} ";


TextBuilder textBuilder = new TextBuilder(
    selectSql,
    new { condition = "Customer.CustomerId > @CustomerId" }
);

string orderSql = @"
    order by
        {{sort}} ";

textBuilder.Append(
    orderSql,
    new { sort = "Customer.CustomerId, CustomerPurchase.PurchaseNo" }
);

string sqlSelectOrder = textBuilder.Generate();

DagentDatabase db = new DagentDatabase("connectionStringName");

db.Query<Customer>(sqlSelectOrder, new { CustomerId = 10 })
.Unique("CustomerId")
.ForEach((model, currentRow, state) =>
{
    currentRow.Map(model, x => x.CustomerPurchases).Do("PurchaseNo");
})
.Fetch();
        }

    }

    
    
}

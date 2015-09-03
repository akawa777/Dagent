using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.EntityClient;
using System.Linq.Expressions;
using Dagent.Models;
using Dapper;

namespace Dagent.Tests
{
    public class Customer
    {
        public int customerId { get; set; }        
        public string name { get; set; }
        public int businessId { get; set; }        

        public List<CustomerPurchase> CustomerPurchases { get; set; }
    }

    public class CustomerWithNullableId
    {
        public int? customerId { get; set; }
        public string name { get; set; }     
    }

    public class CustomerWithBusiness : Customer
    {        
        public Business Business { get; set; }
    }

    public class Business
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
    }

    public class CustomerPurchase
    {
        public int customerId { get; set; }
        public int no { get; set; }
        public string content { get; set; }
    }    

    [TestClass]
    public class SQLiteTest
    {
        [TestMethod]
        public void GetStarted()
        {            
            IDagentDatabase database = new DagentDatabase("SQLite");

            Customer customer = database.Query<Customer>("customers", new { customerId = 1 }).Single();

            Assert.AreEqual(1, customer.customerId);

            List<Customer> customers = database.Query<Customer>("customers").List();

            Assert.AreEqual(10000, customers.Count);

            customers = database.Query<Customer>("select * from customers where customerId > @customerId", new { customerId = 1000 }).List();

            Assert.AreEqual(9000, customers.Count);

            int maxId = database.Query("select max(customerId) from customers").Parameters(new { customerId = 1000 }).Scalar<int>();
            Assert.AreEqual(10000, maxId);

            int count = database.Query("select customerId from customers").Unique("customerId").Count();
            Assert.AreEqual(10000, count);

            customer = new Customer { customerId = maxId + 1, name = "getStarted" };

            int rtn = database.Command<Customer>("customers", "customerId").Insert(customer);

            Assert.AreEqual(1, rtn);

            rtn = database.Command<Customer>("customers", "customerId").Update(customer);

            Assert.AreEqual(1, rtn);            

            rtn = database.Command<Customer>("customers", "customerId").Delete(customer);

            Assert.AreEqual(1, rtn);

            count = 0;
            database.Query("customers").Each(row => count++).Execute();

            Assert.AreEqual(10000, count);
        }

        [TestMethod]
        public void ConnectionString()
        {
            IDagentDatabase database = new DagentDatabase(@"Data Source=..\..\pupsqlite_ver_1201000\dbs\dagentTest.db;Version=3;", "System.Data.SQLite");

            Assert.AreEqual("SQLiteConnection", database.Connection.GetType().Name);
        }


        [TestMethod]
        public void FetchEach()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            database.Query(@"
                    select 
                        *
                    from 
                        customers c 
                    inner join 
                        customerPurchases cp 
                    on 
                        c.customerId = cp.customerId                 
                    order by 
                        c.customerId, cp.no")
                .Each(row => { })
                .Execute();

            List<Customer> customers = database.Query<Customer>(@"
                        select 
                            *
                        from 
                            customers c 
                        inner join 
                            customerPurchases cp 
                        on 
                            c.customerId = cp.customerId                 
        	            order by 
                            c.customerId, cp.no")
                .Unique("customerId") 
                .Each((model, row) =>
                {
                    CustomerPurchase customerPurchaseModel = new CustomerPurchase
                    {
                        customerId = model.customerId,
                        no = row.Get<int>("no"),
                        content = row.Get<string>("content")
                    };

                    if (model.CustomerPurchases == null)
                    {
                        model.CustomerPurchases = new List<CustomerPurchase>();
                    }

                    model.CustomerPurchases.Add(customerPurchaseModel);
                })
                .List();

            ValidList(customers);
        }

        [TestMethod]
        public void DagentTest()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            var customers = database.Query<Customer>("select * from customers").List();

            using (var scope = database.ConnectionScope())
            {
                foreach (var customer in customers)
                {
                    customer.CustomerPurchases = database.Query<CustomerPurchase>("select * from customerPurchases where customerId = @customerId", new { customerId = customer.customerId }).List();
                }
            }

            ValidList(customers);
        }

        [TestMethod]
        public void DagentTestForIterator()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            var customers = database.Query<Customer>("select * from customers").Iterator();

            List<Customer> list = new List<Customer>();

            using (var scope = database.ConnectionScope())
            {
                foreach (var customer in customers)
                {
                    customer.CustomerPurchases = database.Query<CustomerPurchase>("select * from customerPurchases where customerId = @customerId", new { customerId = customer.customerId }).List();
                    list.Add(customer);
                }
            }

            ValidList(list);
        }

        [TestMethod]
        public void DapperTest()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            var customers = database.Connection.Query<Customer>("select * from customers", null);

            foreach (var customer in customers)
            {
                customer.CustomerPurchases = database.Connection.Query<CustomerPurchase>("select * from customerPurchases where customerId = @customerId", new { customerId = customer.customerId }).ToList();
            }

            ValidList(customers);
        }

        [TestMethod]
        public void NpocoTest()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");
            NPoco.IDatabase db = new NPoco.Database(database.Connection);

            //var customers = db.Query<Customer>("select * from customers");
            var customers = db.Fetch<Customer>("select * from customers");

            foreach (var customer in customers)
            {
                customer.CustomerPurchases = db.Fetch<CustomerPurchase>("select * from customerPurchases where customerId = @customerId", new { customerId = customer.customerId });
            }

            ValidList(customers);
        }

        [TestMethod]
        public void Fetch()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            database.Config.CommandTimeout = 60;

            var customers = database.Query<Customer>(@"
                select 
                    *
                from 
                    customers c                 
                inner join 
                    customerPurchases cp 
                on 
                    c.customerId = cp.customerId                 
	            order by 
                    c.customerId, cp.no")
                .Unique("customerId")
                .Each((model, row) => row.Map(model, x => x.CustomerPurchases).Do())                   
                .List();

            ValidList(customers);
        }

        [TestMethod]
        public void FetchForOneToOne()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");
            List<CustomerWithBusiness> customers = database.Query<CustomerWithBusiness>(@"
                select 
                    *
                from 
                    customers c 
                inner join
                    business b
                on
                    c.businessId = b.businessId
                inner join 
                    customerPurchases cp 
                on 
                    c.customerId = cp.customerId                 
	            order by 
                    c.customerId, cp.no")
                .Unique("customerId")
                .Each((model, row) =>
                {
                    row.Map(model, x => x.Business, "businessName").Each(x => 
                    {
                        x.BusinessId = row.Get<int>("businessId");
                        x.BusinessName = row.Get<string>("businessName");                            
                    }).Do();

                    row.Map(model, x => x.CustomerPurchases, "no").Do();
                })
                .List();

            ValidList(customers);
        }

        [TestMethod]
        public void FetchForOneToOneUsedIgnoreCase()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");
            List<CustomerWithBusiness> customers = database.Query<CustomerWithBusiness>(@"
                select 
                    *
                from 
                    customers c 
                inner join
                    business b
                on
                    c.businessId = b.businessId
                inner join 
                    customerPurchases cp 
                on 
                    c.customerId = cp.customerId                 
	            order by 
                    c.customerId, cp.no")
                .Unique("customerId")
                .Each((model, row) =>
                {
                    row.Map(model, x => x.Business, "businessName").IgnoreCase(true).Do();
                    row.Map(model, x => x.CustomerPurchases, "no").Do();
                })
                .List();

            ValidList(customers);
        }

        [TestMethod]
        public void FetchForOneToOneByTextBuilder()
        {
            TextBuilder textBuilder = new TextBuilder(@"
                select 
                    *
                from 
                    customers c");

            textBuilder.Append(@"
                inner join
                    {{join}}
                on
                    c.businessId = b.businessId", 
                new { join = "business b" });

            textBuilder.Append(@"
                inner join 
                    customerPurchases cp 
                on 
                    {{on}}
	            order by 
                    {{order}}",
                new { on = "c.customerId = cp.customerId", order = "c.customerId, cp.no" });

            string sql = textBuilder.Generate();

            IDagentDatabase database = new DagentDatabase("SQLite");
            List<CustomerWithBusiness> customers = database.Query<CustomerWithBusiness>(sql)
                .Unique("customerId")
                .Each((model, row) => {
                    row.Map(model, x => x.Business, "businessName").Do();
                    row.Map(model, x => x.CustomerPurchases, "no").Do();                  
                })
                .List();

            ValidList(customers);

            sql = textBuilder.Clear().Generate();

            Assert.AreEqual("", sql);
        }

        private void ValidList<T>(IEnumerable<T> customers) where T : Customer
        {
            Assert.AreEqual(10000, customers.Count());

            foreach (var customer in customers)
            {
                Assert.AreEqual(true, customer.customerId != 0);
                Assert.AreEqual(false, string.IsNullOrEmpty(customer.name));

                Assert.AreEqual(10, customer.CustomerPurchases.Count);

                foreach (var purchase in customer.CustomerPurchases)
                {
                    Assert.AreEqual(customer.customerId, purchase.customerId);
                    Assert.AreEqual(false, string.IsNullOrEmpty(purchase.content));
                }
            }
        }

        [TestMethod]
        public void Nullable()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            CustomerWithNullableId customer = database.Query<CustomerWithNullableId>("customers", new { customerId = 1 }).Single();

            Assert.AreEqual(new Nullable<int>(1), customer.customerId);
        }

        [TestMethod]
        public void FetchNotAutoMapping()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            List<CustomerWithBusiness> customers = database.Query<CustomerWithBusiness>(@"
                    select 
                        *
                    from 
                        customers c 
                    inner join
                        business b
                    on
                        c.businessId = b.businessId
                    inner join 
                        customerPurchases cp 
                    on 
                        c.customerId = cp.customerId                 
        	        order by 
                        c.customerId, cp.no")
                .AutoMapping(false)
                .Unique("customerId")
                .Each((model, row) => {                    
                    model.customerId = row.Get<int>("customerId");
                    model.name = row.Get<string>("name");

                    model.Business = new Business
                    {
                        BusinessId = row.Get<int>("businessId"),
                        BusinessName = row.Get<string>("businessName")
                    };

                    CustomerPurchase customerPurchaseModel = new CustomerPurchase
                    {
                        customerId = model.customerId,
                        no = row.Get<int>("no"),
                        content = row.Get<string>("content")
                    };

                    if (model.CustomerPurchases == null)
                    {
                        model.CustomerPurchases = new List<CustomerPurchase>();
                    }

                    model.CustomerPurchases.Add(customerPurchaseModel);
                })
                .List();

            ValidList(customers);
        }

        [TestMethod]
        public void FetchIgnoreCache()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            Expression<Func<CustomerPurchase, object>> ignoreProperty = x => x.content;

            List<Customer> customers = database.Query<Customer>(@"
                        select 
                            *
                        from 
                            customers c 
                        inner join 
                            customerPurchases cp 
                        on 
                            c.customerId = cp.customerId                 
        	            order by 
                            c.customerId, cp.no")
                .Unique("customerId")                
                .Ignore(x => x.name)
                .Each((model, row) => {
                    row.Map(model, x => x.CustomerPurchases, "no").Ignore(ignoreProperty).Do();
                })
                .List();

            foreach (var customer in customers)
            {
                Assert.AreEqual(null, customer.name);

                foreach (var customerPurchase in customer.CustomerPurchases)
                {
                    Assert.AreEqual(null, customerPurchase.content);
                }
            }
        }

        [TestMethod]
        public void FetchIgnore()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            List<Customer> customers = database.Query<Customer>(@"
                        select 
                            *
                        from 
                            customers c 
                        inner join 
                            customerPurchases cp 
                        on 
                            c.customerId = cp.customerId                 
        	            order by 
                            c.customerId, cp.no")
                .Unique("customerId")
                .Ignore(x => x.name)                
                .Each((model, row) => row.Map(model, x => x.CustomerPurchases, "no").Ignore(x => x.content).Do())
                .List();

            foreach (var customer in customers)
            {
                Assert.AreEqual(null, customer.name);

                foreach (var customerPurchase in customer.CustomerPurchases)
                {
                    Assert.AreEqual(null, customerPurchase.content);
                }
            }
        }

        [TestMethod]
        public void FetchParameterObject()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            List<Customer> customers = database.Query<Customer>(@"
                select 
                    *
                from 
                    customers c 
                inner join 
                    customerPurchases cp 
                on 
                    c.customerId = cp.customerId  
                where c.customerId between @fromId and @toId
	            order by 
                    c.customerId, cp.no", new { fromId = 1000, toId = 1999 })
                .Unique("customerId")
                .Each((model, row) => row.Map(model, x => x.CustomerPurchases, "no").Do())
                .List();

            ValidBetweenParameter(customers);            
        }

        public void ValidBetweenParameter(List<Customer> customers)
        {
            Assert.AreEqual(1000, customers.Count);
            Assert.AreEqual(1000, customers.First().customerId);
            Assert.AreEqual(1999, customers.Last().customerId);

            foreach (var customer in customers)
            {
                Assert.AreEqual(true, customer.customerId != 0);
                Assert.AreEqual(false, string.IsNullOrEmpty(customer.name));

                Assert.AreEqual(10, customer.CustomerPurchases.Count);

                foreach (var purchase in customer.CustomerPurchases)
                {
                    Assert.AreEqual(customer.customerId, purchase.customerId);
                    Assert.AreEqual(false, string.IsNullOrEmpty(purchase.content));
                }
            }
        }

        [TestMethod]
        public void FetchParameters()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            List<Customer> customers = database.Query<Customer>(@"
                select 
                    *
                from 
                    customers c 
                inner join 
                    customerPurchases cp 
                on 
                    c.customerId = cp.customerId  
                where c.customerId between @fromId and @toId
	            order by 
                    c.customerId, cp.no", new Parameter("fromId", 1000), new Parameter("toId", 1999))
                .Unique("customerId")
                .Each((model, row) => row.Map(model, x => x.CustomerPurchases, "no").Do())
                .List();

            ValidBetweenParameter(customers);             
        }

        [TestMethod]
        public void FetchPrefixColumName()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            List<Customer> customers = database.Query<Customer>(@"
                select 
                    c.*,
                    cp.customerId as cp_customerId,
                    cp.no as cp_no,
                    cp.content as cp_content
                from 
                    customers c 
                inner join 
                    customerPurchases cp 
                on 
                    c.customerId = cp.customerId                 
	            order by 
                    c.customerId, cp.no")
                .Unique("customerId")
                .Each((model, row) => {
                    row.Map(model, x => x.CustomerPurchases, "cp_customerId").Prefix("cp_").Do();
                })
                .List();

            ValidList(customers);
        }

        [TestMethod]
        public void Single()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");            

            Customer customer = database.Query<Customer>(@"
                select 
                    *
                from 
                    customers c 
                inner join 
                    customerPurchases cp 
                on 
                    c.customerId = cp.customerId  
                where c.customerId between @fromId and @toId
	            order by 
                    c.customerId, cp.no", new Parameter("fromId", 1000), new Parameter("toId", 1999))
                .Unique("customerId")
                .Each((model, row) => row.Map(model, x => x.CustomerPurchases, "no").Do())
                .Single();

            Assert.AreEqual(true, customer.customerId == 1000);
            Assert.AreEqual(false, string.IsNullOrEmpty(customer.name));

            foreach (var purchase in customer.CustomerPurchases)
            {
                Assert.AreEqual(customer.customerId, purchase.customerId);
                Assert.AreEqual(false, string.IsNullOrEmpty(purchase.content));
            }
        }

        [TestMethod]
        public void Page()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            int count = 0;

            List<Customer> customers = database.Query<Customer>(@"
                select 
                    c.*,
                    cp.customerId as cp_customerId,
                    cp.no as cp_no,
                    cp.content as cp_content
                from 
                    customers c 
                inner join 
                    customerPurchases cp 
                on 
                    c.customerId = cp.customerId                 
	            order by 
                    c.customerId, cp.no")
                .Unique("customerId")
                .Each((model, row) => row.Map(model, x => x.CustomerPurchases, "cp_customerId").Prefix("cp_").Do())
                .Page(100, 10, out count).ToList();

            Assert.AreEqual(10, customers.Count);
            Assert.AreEqual(10000, count);
            Assert.AreEqual(1001, customers.First().customerId);
            Assert.AreEqual(1010, customers.Last().customerId);

            foreach (var customer in customers)
            {
                foreach (var purchase in customer.CustomerPurchases)
                {
                    Assert.AreEqual(customer.customerId, purchase.customerId);
                    Assert.AreEqual(false, string.IsNullOrEmpty(purchase.content));
                }
            }
        }

        [TestMethod]
        public void InsertManyData()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");            

            using (ITransactionScope scope = database.TransactionScope())
            {
                database.ExequteNonQuery("delete from customers", null);
                database.ExequteNonQuery("delete from customerPurchases", null);
                database.ExequteNonQuery("delete from business", null);

                int customerNo = 10000;
                int customerPurchasesNo = 10;
                int businessNo = 10;

                //var customerCommand = database.Command<Customer>("customers", "customerId");
                //var customerPurchaseCommand = database.Command<CustomerPurchase>("customerPurchases", "customerId", "no");
                //var businessCommand = database.Command<Business>("business", "businessId");                

                int businessId = 1;
                for (int i = 1; i <= customerNo; i++)
                {                    
                    Customer customer = new Customer
                    {
                        customerId = i,
                        name = "name_" + i.ToString(),
                        businessId = businessId
                    };
                    database.Command<Customer>("customers", "customerId").Insert(customer);                    

                    if (businessId == 10) businessId = 0;
                    businessId++;


                    for (int j = 1; j <= customerPurchasesNo; j++)
                    {
                        CustomerPurchase customerPurchase = new CustomerPurchase
                        {
                            customerId = i,
                            no = j,
                            content = "content_" + j.ToString()
                        };
                        database.Command<CustomerPurchase>("customerPurchases", "customerId", "no").Insert(customerPurchase);
                    }
                }

                for (int i = 1; i <= businessNo; i++)
                {
                    Business business = new Business
                    {
                        BusinessId = i,
                        BusinessName = "business_" + i.ToString()

                    };
                    database.Command<Business>("business", "businessId")   
                        .Ignore(x => x.BusinessId, x => x.BusinessName)
                        .Map((row, model) => 
                        {
                            row["businessId"] = model.BusinessId;
                            row["businessName"] = model.BusinessName;
                        })
                        .Insert(business);
                }

                scope.Commit();
            }

            FetchForOneToOne();
        }

        [TestMethod]
        public void Fill()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            DataTable dt = new DataTable();
            database.Fill(dt, @"
                select 
                    *
                from 
                    customers c 
                inner join 
                    customerPurchases cp 
                on 
                    c.customerId = cp.customerId                 
	            order by 
                    c.customerId, cp.no");

            List<Customer> customers = new List<Customer>();
            int id = 0;
            foreach (DataRow row in dt.Rows)
            {
                int currentId = int.Parse(row["customerId"].ToString());

                if (currentId != id)
                {
                    customers.Add(new Customer { customerId = currentId, name = row["name"].ToString() });
                }

                if (customers.Last().CustomerPurchases == null)
                {
                    customers.Last().CustomerPurchases = new List<CustomerPurchase>();
                }

                customers.Last().CustomerPurchases.Add(new CustomerPurchase
                {
                    customerId = currentId,
                    no = Convert.ToInt16(row["no"]),
                    content = Convert.ToString(row["content"])                  
                });

                id = currentId;
            }

            ValidList(customers);
        }

        [TestMethod]
        public void Update()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            string sql = @"
                select 
                    *
                from 
                    customers c 
                inner join 
                    customerPurchases cp 
                on 
                    c.customerId = cp.customerId                 
	            order by 
                    c.customerId, cp.no";

            DataTable dt = new DataTable();

            database.Fill(dt, sql);
            database.Update(dt, sql);

            Fill();
        }

        [TestMethod]
        public void ExecuteReader()
        {
            IDagentDatabase database = new DagentDatabase("SQLite");

            var command = database.Connection.CreateCommand();
            command.CommandText = @"
                select 
                    *
                from 
                    customers c 
                inner join 
                    customerPurchases cp 
                on 
                    c.customerId = cp.customerId                 
	            order by 
                    c.customerId, cp.no";


            command.Connection.Open();
            var reader = command.ExecuteReader();

            List<Customer> customers = new List<Customer>();
            int id = 0;
            while (reader.Read())
            {
                int currentId = int.Parse(reader["customerId"].ToString());
                if (currentId != id)
                {
                    customers.Add(new Customer { customerId = currentId, name = reader["name"].ToString() });
                }

                if (customers.Last().CustomerPurchases == null)
                {
                    customers.Last().CustomerPurchases = new List<CustomerPurchase>();
                }

                customers.Last().CustomerPurchases.Add(new CustomerPurchase
                {
                    customerId = currentId,
                    no = Convert.ToInt16(reader["no"]),
                    content = Convert.ToString(reader["content"])                    
                });

                id = currentId;
            }

            command.Connection.Close();

            ValidList(customers);
        }

        [TestMethod]
        public void InsertOrUpdateOrDelete()
        {
            Customer customer = new Customer();
            customer.customerId = 999999;
            customer.name = "name_" + customer.customerId.ToString();

            IDagentDatabase database = new DagentDatabase("SQLite");

            ICommand<Customer> command = database.Command<Customer>("customers", "customerId");
            int ret = command.Insert(customer);

            Assert.AreEqual(1, ret);

            IQuery<Customer> query = database.Query<Customer>("customers", new { customerId = customer.customerId });

            Customer registerdCustomer = query.Single();

            Assert.AreEqual(customer.customerId, registerdCustomer.customerId);
            Assert.AreEqual(customer.name, registerdCustomer.name);

            customer.name = "update_" + customer.name;

            ret = command.Update(customer);

            Assert.AreEqual(1, ret);

            registerdCustomer = query.Single();

            Assert.AreEqual(customer.customerId, registerdCustomer.customerId);
            Assert.AreEqual(customer.name, registerdCustomer.name);

            ret = command.AutoMapping(false).Map((row, model) => 
            {
                row["customerId"] = model.customerId;
                row["name"] = model.name;
                row["businessId"] = model.businessId;
            })
            .Update(customer);

            Assert.AreEqual(1, ret);

            registerdCustomer = query.Single();

            Assert.AreEqual(customer.customerId, registerdCustomer.customerId);
            Assert.AreEqual(customer.name, registerdCustomer.name);

            ret = command.Delete(customer);

            Assert.AreEqual(1, ret);

            registerdCustomer = query.Single();
            
            Assert.AreEqual(null, registerdCustomer);            
        }        

        //[TestMethod]
        public void RefreshTestData()
        {
            DagentDatabase database = new DagentDatabase("SQLite");

            using (ITransactionScope scope = database.TransactionScope())
            {
                database.ExequteNonQuery("delete from customers", null);
                database.ExequteNonQuery("delete from customerPurchases", null);
                database.ExequteNonQuery("delete from business", null);

                int customerNo = 10000;
                int customerPurchasesNo = 10;
                int businessNo = 10;

                int businessId = 1;
                for (int i = 1; i <= customerNo; i++)
                {
                    database.ExequteNonQuery(string.Format("insert into customers values ({0}, '{1}', '{2}')", i.ToString(), "name_" + i.ToString(), businessId));

                    if (businessId == 10) businessId = 0;
                    businessId++;


                    for (int j = 1; j <= customerPurchasesNo; j++)
                    {
                        database.ExequteNonQuery(string.Format("insert into customerPurchases values ({0}, {1}, '{2}')", i.ToString(), j.ToString(), "content_" + j.ToString()));
                    }
                }

                for (int i = 1; i <= businessNo; i++)
                {
                    database.ExequteNonQuery(string.Format("insert into business values ({0}, '{1}')", i.ToString(), "business_" + i.ToString()));
                }

                scope.Commit();
            }
        }

        private bool refreshTestData = false;
        
        [TestInitialize]
        public void Initialize()
        {
            if (!refreshTestData) return;

            RefreshTestData();
        }

        [TestCleanup]
        public void Cleanup()
        {
            //if (!refreshTestData) return;

            //DagentDatabase database = new DagentDatabase("SQLite");

            //using (ITransactionScope scope = database.TransactionScope())
            //{
            //    database.ExequteNonQuery("delete from customers", null);
            //    database.ExequteNonQuery("delete from customerPurchases", null);

            //    scope.Commit();
            //}
        }

        [TestMethod]
        public void TransactionTest()
        {
            string sql = @"
                delete from customers where customerId = -1;
            ";

            DagentDatabase database = new DagentDatabase("SQLite");

            using (var scope = database.TransactionScope())
            {         
                int rtn1 = database.ExequteNonQuery(sql);

                Assert.AreEqual(0, rtn1);

                scope.Commit();
            }

            int rtn2 = database.ExequteNonQuery(sql);

            Assert.AreEqual(0, rtn2);
        }
    }
}

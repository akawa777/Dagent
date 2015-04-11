using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.EntityClient;
using System.Linq.Expressions;
using Dagent.Models;

namespace Dagent.Tests3
{
    public class Entity1
    {
        public int Entity1Id { get; set; }

        public int? SubEntity1Id { get; set; }
        public SubEntity1 SubEntity1 { get; set; }

        public List<Entity2> Entity2s { get; set; }
    }

    public class Entity2
    {
        public int Entity1Id { get; set; }
        public int Entity2Id { get; set; }

        public int? SubEntity2Id { get; set; }
        public SubEntity2 SubEntity2 { get; set; }

        public List<Entity3> Entity3s { get; set; }
    }

    public class Entity3
    {
        public int Entity1Id { get; set; }
        public int Entity2Id { get; set; }
        public int Entity3Id { get; set; }

        public int? SubEntity3Id { get; set; }
        public SubEntity3 SubEntity3 { get; set; }
    }

    public class SubEntity1
    {
        public int SubEntity1Id { get; set; }
    }

    public class SubEntity2
    {
        public int SubEntity2Id { get; set; }
    }

    public class SubEntity3
    {
        public int SubEntity3Id { get; set; }
    }

    [TestClass]
    public class SQLiteTest
    {
        //[TestInitialize]
        public void Init()
        {
            DagentDatabase db = new DagentDatabase("SQLite");

            using (var scope = db.TransactionScope())
            {
                db.ExequteNonQuery("delete from Entity1");
                db.ExequteNonQuery("delete from Entity2");
                db.ExequteNonQuery("delete from Entity3");
                db.ExequteNonQuery("delete from SubEntity1");
                db.ExequteNonQuery("delete from SubEntity2");
                db.ExequteNonQuery("delete from SubEntity3");

                int iMax = 100;
                int iiMax = 100;
                int iiiMax = 100;

                for (int i = 1; i <= iMax; i++)
                {
                    Entity1 e1 = new Entity1();
                    e1.Entity1Id = i;

                    if (i % 2 == 0) e1.SubEntity1Id = null;
                    else e1.SubEntity1Id = i;

                    if (i % 3 == 0)
                    {
                        for (int ii = 1; ii <= iiMax; ii++)
                        {
                            Entity2 e2 = new Entity2();
                            e2.Entity1Id = i;
                            e2.Entity2Id = ii;

                            if (ii % 2 == 0) e2.SubEntity2Id = null;
                            else e2.SubEntity2Id = ii;

                            if (ii % 3 == 0)
                            {
                                for (int iii = 1; iii <= iiiMax; iii++)
                                {
                                    Entity3 e3 = new Entity3();
                                    e3.Entity1Id = i;
                                    e3.Entity2Id = ii;
                                    e3.Entity3Id = iii;

                                    if (iii % 2 == 0) e3.SubEntity3Id = null;
                                    else e3.SubEntity3Id = iii;

                                    db.Command<Entity3>("Entity3", "Entity1Id", "Entity2Id", "Entity3Id").Insert(e3);
                                }
                            }                    

                            db.Command<Entity2>("Entity2", "Entity1Id", "Entity2Id").Insert(e2);
                        }
                    }

                    SubEntity1 se1 = new SubEntity1 { SubEntity1Id = i };
                    SubEntity2 se2 = new SubEntity2 { SubEntity2Id = i };
                    SubEntity3 se3 = new SubEntity3 { SubEntity3Id = i };

                    db.Command<Entity1>("Entity1", "Entity1Id").Insert(e1);
                    db.Command<SubEntity1>("SubEntity1", "SubEntity1Id").Insert(se1);
                    db.Command<SubEntity2>("SubEntity2", "SubEntity2Id").Insert(se2);
                    db.Command<SubEntity3>("SubEntity3", "SubEntity3Id").Insert(se3);
                }

                scope.Commit();
            }
        }

        [TestMethod]
        public void NestedTest()
        {
            DagentDatabase db = new DagentDatabase("SQLite");

            List<Entity1> entities = db.Query<Entity1>(@"
                select
                    *
                from
                    Entity1
                left join
                    SubEntity1
                on
                    SubEntity1.SubEntity1Id = Entity1.SubEntity1Id
                left join
                    Entity2
                on
                    Entity1.Entity1Id = Entity2.Entity1Id
                left join
                    SubEntity2
                on
                    Entity2.SubEntity2Id = SubEntity2.SubEntity2Id
                left join
                    Entity3
                on
                    Entity2.Entity1Id = Entity3.Entity1Id and
                    Entity2.Entity2Id = Entity3.Entity2Id 
                left join
                    SubEntity3
                on
                    Entity3.SubEntity3Id = SubEntity3.SubEntity3Id ")
                .Unique("Entity1Id")
                .Each((e1, row) =>
                {
                    row.Map(e1, x => x.SubEntity1, "SubEntity1Id").Do();

                    row.Map(e1, x => x.Entity2s, "Entity2Id")
                        .Unique("Entity1Id", "Entity2Id")
                        .Each(e2 =>
                        {
                            row.Map(e2, x => x.SubEntity2, "SubEntity2Id").Do();

                            row.Map(e2, x => x.Entity3s, "Entity3Id")
                                .Unique("Entity1Id", "Entity2Id", "Entity3Id")
                                .Each(e3 => 
                                {
                                    row.Map(e3, x => x.SubEntity3, "SubEntity3Id").Do();
                                })
                                .Do();
                        })
                        .Do();
                }).List();

            var list = entities.ToList<Entity1>();

            Assert.AreEqual(100, list.Count);
        }
    }
}

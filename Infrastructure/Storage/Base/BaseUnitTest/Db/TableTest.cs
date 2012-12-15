using System;
using System.Data;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using InMemoryDb.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;

namespace BaseUnitTest.Db
{
    public class SpecificDao
    {
        public DataTable GetLocations()
        {
            var aa = new DataTable();
            return null;
        }
    }

    public class ApiCls
    {
        public string GetLocations(string filter)
        {
            var dao = new SpecificDao();
            using (var dt = dao.GetLocations())
            {
                return @"aa";
            }
        }
    }

    [TestClass]
    public class TableTest
    {
        private readonly List<DataTable> _listLocSchemas;

        static DataTable datatable = LocationTables.GetLocationEx();
        public TableTest()
        {
            _listLocSchemas = LocationTables.GetLocSchemas();
        }


        [TestMethod]
        [Isolated]
        public void TestGetLocations()
        {
            var ins = new ApiCls();

            {
                var fake = Isolate.Fake.Instance<SpecificDao>();
                //Isolate.WhenCalled(() => fake.ApplyFilter("")).WillReturn("");
                Isolate.WhenCalled(() => fake.GetLocations()).WillReturn(datatable);

                var csv = ins.GetLocations("");
                Assert.AreEqual(@"aa", csv);
            }
        }
    }
}

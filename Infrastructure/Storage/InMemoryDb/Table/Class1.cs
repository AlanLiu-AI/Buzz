using System;
using System.Collections.Generic;
using System.Data;

namespace InMemoryDb.Table
{
    public class LocationTables
    {
        public static List<DataTable> GetLocSchemas()
        {
            var ret = new List<DataTable> { 
                GetLocationSchema(),
                GetLocationTypeSchema(),
                GetLocationExtensionSchema()
            };
            ret.AddRange(GetLocationTypeExSchemas());
            return ret;
        }

        public static DataTable GetLocationTypeSchema()
        {
            var ret = new DataTable("LocationType");
            AddLocationTypeColumns(ret, false);
            return ret;
        }

        private static void AddLocationTypeColumns(DataTable ret, bool duplicate)
        {
            if (!duplicate) ret.Columns.Add("LocationTypeID", typeof(Int64));
            ret.Columns.Add("LocationTypeName", typeof(string));
            ret.Columns.Add("AttributeTableName", typeof(string));
        }

        public static DataTable GetLocationType()
        {
            var ret = GetLocationTypeSchema();
            ret.Rows.Add(1, "Hydrology Station", "Location_Hydrology");
            ret.Rows.Add(2, "Meteorology Station", "Location_Meteorology");
            ret.Rows.Add(3, "Water Quality Site", "Location_Quality");
            return ret;
        }

        public static DataTable GetLocationSchema()
        {
            var ret = new DataTable("Location");
            ret.Columns.Add("LocationID", typeof(Int64));
            ret.Columns.Add("LocationFolderID", typeof(Int64));
            ret.Columns.Add("LastModified", typeof(DateTime));
            ret.Columns.Add("LocationName", typeof(String));
            ret.Columns.Add("Description", typeof(String));
            ret.Columns.Add("Identifier", typeof(String));
            ret.Columns.Add("LocationTypeID", typeof(Int64));
            ret.Columns.Add("Latitude", typeof(double));
            ret.Columns.Add("Longitude", typeof(double));
            ret.Columns.Add("Elevation", typeof(double));
            ret.Columns.Add("ElevationUnits", typeof(String));
            ret.Columns.Add("UTCOffset", typeof(double));
            ret.Columns.Add("TimeZone", typeof(String));
            ret.Columns.Add("DefaultRoleID", typeof(Int64));
            ret.Columns.Add("AQUserData_", typeof(String));
            return ret;
        }

        public static DataTable GetLocation()
        {
            var ret = GetLocationSchema();
            var t = DateTime.Parse("2010-01-01");
            long dataId = 155;  //start from 1000 to 2000
            long folderId = 100; //between 100~1000
            for (var i = 0; i < 10; i++) //for type1
            {
                ret.Rows.Add(dataId++, folderId, t, "LocationName" + dataId, "Description1" + dataId,
                             "Identifier" + dataId, 1, 42.9975, -108.37472, 4269, "ft", 0, "GMT", null, null);
            }

            folderId++;
            for (var i = 0; i < 10; i++) //for type 2
            {
                ret.Rows.Add(dataId++, folderId++, t, "LocationName" + dataId, "Description1" + dataId,
                             "Identifier" + dataId, 2, 42.9975, -108.37472, 4269, "ft", 0, "GMT", null, null);
            }

            folderId++;
            for (var i = 0; i < 10; i++) //for type 3
            {
                ret.Rows.Add(dataId++, folderId++, t, "LocationName" + dataId, "Description1" + dataId,
                             "Identifier" + dataId, 2, 42.9975, -108.37472, 4269, "ft", 0, "GMT", null, null);
            }
            return ret;
        }

        public static DataTable GetLocationExtensionSchema()
        {
            var ret = new DataTable("Location_Extension");
            AddLocationExtensionColumns(ret, false);
            return ret;
        }

        private static void AddLocationExtensionColumns(DataTable ret, bool duplicate)
        {
            if (!duplicate) ret.Columns.Add("LocationID", typeof(Int64));
            ret.Columns.Add("SiteType_", typeof(string));
            ret.Columns.Add("RecordType_", typeof(string));
        }

        public static DataTable GetLocationExtension()
        {
            var ret = GetLocationTypeSchema();
            using (var location = GetLocation())
            {
                foreach (DataRow row in location.Rows)
                {
                    var locId = Convert.ToInt64(row[0]);
                    ret.Rows.Add(locId, "SiteType_" + locId, "RecordType_" + locId);
                }
            }
            return ret;
        }

        public static DataTable[] GetLocationTypeExSchemas()
        {
            var ret = new List<DataTable> { 
                new DataTable("Location_Hydrology"),
                new DataTable("Location_Meteorology"),
                new DataTable("Location_Quality"),
            };
            ret[0].Columns.Add("LocationID", typeof(Int64));
            ret[0].Columns.Add("Hydrology", typeof(String));
            ret[1].Columns.Add("LocationID", typeof(Int64));
            ret[1].Columns.Add("Meteorology", typeof(String));
            ret[2].Columns.Add("LocationID", typeof(Int64));
            ret[2].Columns.Add("Quality", typeof(String));
            return ret.ToArray();
        }

        public static DataTable[] GetLocationTypeEx()
        {
            var ret = GetLocationTypeExSchemas();
            var map = new Dictionary<long, long>();
            using (var locTypes = GetLocationType())
            {
                foreach (DataRow locType in locTypes.Rows)
                {
                    var locTypeId = Convert.ToInt64(locType[0]);
                    var locTypeTbl = (string)locType[2];
                    for (var pos = 0; pos < ret.Length; pos++)
                    {
                        if (string.Compare(ret[pos].TableName, locTypeTbl, StringComparison.Ordinal) == 0)
                            map.Add(locTypeId, pos);
                    }
                }
            }
            using (var location = GetLocation())
            {
                foreach (DataRow row in location.Rows)
                {
                    var locId = Convert.ToInt64(row[0]);
                    var locTypeId = Convert.ToInt64(row[6]);
                    var pos = map[locTypeId];
                    ret[pos].Rows.Add(locId, ret[pos].Columns[1].ColumnName + locId);
                }
            }
            return ret;
        }

        public static DataTable GetLocationEx()
        {
            var ret = new DataTable("LocationEx");
            //location
            ret.Columns.Add("LocationID", typeof(Int64));
            ret.Columns.Add("LocationFolderID", typeof(Int64));
            ret.Columns.Add("LastModified", typeof(DateTime));
            ret.Columns.Add("LocationName", typeof(String));
            ret.Columns.Add("Description", typeof(String));
            ret.Columns.Add("Identifier", typeof(String));
            ret.Columns.Add("LocationTypeID", typeof(Int64));
            ret.Columns.Add("Latitude", typeof(double));
            ret.Columns.Add("Longitude", typeof(double));
            ret.Columns.Add("Elevation", typeof(double));
            ret.Columns.Add("ElevationUnits", typeof(String));
            ret.Columns.Add("UTCOffset", typeof(double));
            ret.Columns.Add("TimeZone", typeof(String));
            ret.Columns.Add("DefaultRoleID", typeof(Int64));
            ret.Columns.Add("AQUserData_", typeof(String));
            //LocationType
            AddLocationTypeColumns(ret, true);
            //Location_Extension
            AddLocationExtensionColumns(ret, true);
            //location_ex_tables
            ret.Columns.Add("Hydrology", typeof(String));
            ret.Columns.Add("Meteorology", typeof(String));
            ret.Columns.Add("Quality", typeof(String));

            var t = DateTime.Parse("2010-01-01");
            long dataId = 155;  //start from 1000 to 2000
            long folderId = 100; //between 100~1000
            for (var i = 0; i < 10; i++) //for type1
            {
                var locId = dataId++;
                ret.Rows.Add(
                    //for Location
                    locId, folderId, t, "LocationName" + locId, "Description" + locId, "Identifier" + locId, 1, 42.9975, -108.37472, 4269, "ft", 0, "GMT", null, null,
                    //for LocationType
                    /*locTypeId,*/ "Hydrology Station", "Location_Hydrology",
                    //for Location_Extension
                    /*locId,*/ "SiteType_" + locId, "RecordType_" + locId,
                    //for Location_Hydrology
                    /*locId,*/ "Hydrology" + locId,
                    //for Location_Meteorology
                    /*null,*/ null,
                    //for Location_Quality
                    /*null,*/ null
                    );
            }
            folderId++;
            for (var i = 0; i < 10; i++) //for type 2
            {
                var locId = dataId++;
                ret.Rows.Add(
                    //for Location
                    locId, folderId, t, "LocationName" + locId, "Description1" + locId, "Identifier" + dataId, 2, 42.9975, -108.37472, 4269, "ft", 0, "GMT", null, null,
                    //for LocationType
                    /*locTypeId,*/ "Meteorology Station", "Location_Meteorology",
                    //for Location_Extension
                    /*locId,*/ "SiteType_" + locId, "RecordType_" + locId,
                    //for Location_Hydrology
                    /*null,*/ null,
                    //for Location_Meteorology
                    /*locId,*/ "Meteorology" + locId,
                    //for Location_Quality
                    /*null,*/ null
                    );
            }

            folderId++;
            for (var i = 0; i < 10; i++) //for type 3
            {
                var locId = dataId++;
                ret.Rows.Add(
                    //for Location
                    locId, folderId, t, "LocationName" + locId, "Description1" + locId, "Identifier" + dataId, 2, 42.9975, -108.37472, 4269, "ft", 0, "GMT", null, null,
                    //for LocationType
                    /*locTypeId,*/ "Water Quality Site", "Location_Quality",
                    //for Location_Extension
                    /*locId,*/ "SiteType_" + locId, "RecordType_" + locId,
                    //for Location_Hydrology
                    /*null,*/ null,
                    //for Location_Meteorology
                    /*null,*/ null,
                    //for Location_Quality
                    /*locId,*/ "Quality" + locId
                    );
            }

            return ret;
        }
    }
}

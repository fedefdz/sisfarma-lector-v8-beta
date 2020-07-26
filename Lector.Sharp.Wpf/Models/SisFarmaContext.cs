﻿using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;

namespace Lector.Sharp.Wpf.Models
{
    public partial class SisFarmaEntities : DbContext
    {
        public SisFarmaEntities(string server, string catalog) 
            : base(BuildConnectionString(server, catalog))
        {            
        }

        private static string BuildConnectionString(string dataSource, string database)
        {
            // Construcción de connectionString
            var connectionString = $"server={dataSource};user id=fisiotes_admin;password=77338081;persistsecurityinfo=True;database={database}; Allow Zero Datetime=True; Convert Zero Datetime=True";
            var metadata = "res://*/Models.SisFarmaModel.csdl|res://*/Models.SisFarmaModel.ssdl|res://*/Models.SisFarmaModel.msl";
            var provider = "MySql.Data.MySqlClient";
            // Build the connection string from the provided datasource and database
            //String connString = @"data source=" + DataSource + ";initial catalog=" +
            //Database + ";integrated security=True;MultipleActiveResultSets=True;App=EntityFramework;";

            // Build the MetaData... feel free to copy/paste it from the connection string in the config file.
            EntityConnectionStringBuilder esb = new EntityConnectionStringBuilder();
            esb.Metadata = metadata;
            esb.Provider = provider;
            esb.ProviderConnectionString = connectionString;

            // Generate the full string and return it
            var str = esb.ToString();
            return str;
        }
    }
    
}

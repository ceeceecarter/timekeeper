using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Infrastructure
{
    public class AutomapperConfiguration
    {
        public static void Configure()
        {
            var types = Assembly.GetExecutingAssembly().GetExportedTypes();
            LoadCustomMappings(types);
            LoadStandardMappings(types);
        }
        
        private static void LoadCustomMappings(IEnumerable<Type> types)
        {

        }

        private static void LoadStandardMappings(IEnumerable<Type> types)
        {
            var maps = (from t in types
                        from i in t.GetInterfaces()
                        where i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IMapFrom<>) &&
                            !t.IsAbstract &&
                            !t.IsInterface
                        select new
                        {
                            Source = i.GetGenericArguments()[0],
                            Destination = t
                        }).ToArray();
            
            foreach(var map in maps)
            {
                //Mapped from Source to Destination
                AutoMapper.Mapper.CreateMap(map.Source, map.Destination);

                //Mapped from Destination to Source
                AutoMapper.Mapper.CreateMap(map.Destination, map.Source);
            }

        }
    }
}

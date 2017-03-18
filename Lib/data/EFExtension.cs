using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Lib.extension;

namespace Lib.data
{
    public static class EFExtension
    {
        /// <summary>
        /// 注册fluent mapping
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ass"></param>
        public static void RegisterTableFluentMapping(this DbModelBuilder builder, params Assembly[] ass)
        {
            foreach (var a in ass)
            {
                var tps = a.GetTypes().Where(x =>
                x.IsClass
                && !x.IsAbstract
                && x.BaseType != null
                && x.BaseType.IsGenericType
                && x.BaseType.GetGenericTypeDefinition() == typeof(EFMappingBase<>));
                foreach (var t in tps)
                {
                    dynamic configurationInstance = Activator.CreateInstance(t);
                    //mapping
                    builder.Configurations.Add(configurationInstance);
                }
            }
        }

        /// <summary>
        /// 注册attribute mapping
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ass"></param>
        public static void RegisterTableAttributeMapping(this DbModelBuilder builder, params Assembly[] ass)
        {
            foreach (var a in ass)
            {
                var tps = a.GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsAssignableTo_<IDBTable>());
                foreach (var t in tps)
                {
                    builder.RegisterEntityType(t);
                }
            }
        }

    }
}

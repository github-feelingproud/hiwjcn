using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.data.ef
{
    /// <summary>
    /// fluent map base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EFMappingBase<T> : IEntityTypeConfiguration<T> where T : class, IDBTable
    {
        public abstract void Configure(EntityTypeBuilder<T> builder);
    }
}

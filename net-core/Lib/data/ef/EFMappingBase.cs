using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

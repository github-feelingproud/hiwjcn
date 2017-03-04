using Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Core.Model.EFMapping
{
    public interface IMappingBase { }

    public class MappingBase<T> : EntityTypeConfiguration<T>, IMappingBase where T : BaseEntity
    {
        //
    }
}

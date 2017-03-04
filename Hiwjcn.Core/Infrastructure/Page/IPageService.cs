using Lib.core;
using Lib.helper;
using Lib.infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebLogic.Model.Page;

namespace Hiwjcn.Core.Infrastructure.Page
{
    public interface IPageService : IServiceBase<SectionModel>
    {
        SectionModel GetSection(string name, string section_type = null);

        List<SectionModel> GetSections(string[] names = null, string section_type = null,
            string rel_group = null, int maxcount = 100);

        string AddSection(SectionModel model);

        string UpdateSection(SectionModel updatemodel);

        string DeleteSections(params string[] names);

        PagerData<SectionModel> GetSectionList(
            string q = null, string sectionType = null,
            int page = 1, int pagesize = 10);
    }
}

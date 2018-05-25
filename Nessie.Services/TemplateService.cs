using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nessie.Services
{
    public class TemplateService
    {
        public IList<FileLocation> GetApplicableTemplates(IList<FileLocation> allTemplates, FileLocation file)
        {
            // only markdown files have templates applied to them
            if(file.Extension != ".md")
            {
                return new List<FileLocation>();
            }

            var templatesWithCategory = (
                from template in allTemplates
                select new
                {
                    template,
                    category = template.FileNameWithoutExtension.Replace("_template_", "")
                }
            ).ToArray();

            // if this file has a category, but there's no associated template, don't return any templates.
            // this prevents a item/_item_foo.md being rendered with a top-level template in a parent directory.
            if(!templatesWithCategory.Any() || (file.Category != string.Empty && templatesWithCategory.Last().category != file.Category))
            {
                return new List<FileLocation>();
            }

            var directories = file.Directory
                .Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(new List<string> { "" },
                           (list, dir) =>
                           {
                               list.Add(Path.Combine(list.Last(), dir));
                               return list;
                           })
                .ToArray();

            var applicableTemplates = (from template in templatesWithCategory
                                       where directories.Contains(template.template.Directory)
                                         && (template.category == "" || file.Category == template.category)
                                       orderby template.template.FullyQualifiedName.Length descending
                                       select template.template).ToList();

            return applicableTemplates.ToList();
        }
    }
}

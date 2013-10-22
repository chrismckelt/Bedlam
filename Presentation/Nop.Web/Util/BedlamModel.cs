using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Util
{
    public class BedlamModel
    {
        public BedlamModel()
        {
            this.IsActive = true;
        }

        public string Name { get; set; }
        public string SeName { get; set; }
        public int ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
        public int Id { get; set; }

    }
}
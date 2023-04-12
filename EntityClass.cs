using System;
using System.Collections.Generic;
using System.Text;

namespace AnalysisClass
{
    internal class FatherClass : Perple
    {
        //public string ddd { get; set; }
        public string Fathername { get; set; }
        public ChildrenClass Children { get; set; }
        public List<ChildrenClass> childrens { get; set; }

    }

    internal class ChildrenClass : Perple
    {
        public string Childrenname { get; set; }
    }
    internal class Perple
    {
        public int age { get; set; }
    }
}

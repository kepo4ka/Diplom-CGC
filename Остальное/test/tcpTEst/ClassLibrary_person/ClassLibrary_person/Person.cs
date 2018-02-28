using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary_person
{
    [Serializable]
    public class Person
    {
        private string fn;
        private string ln;
        private int age;

        public string FirstName
        {
            get
            {
                return fn;
            }
            set
            {
                fn = value;
            }
        }

        public Person()
        {
            fn = "fn";
            ln = "ln";
            age = 3;

        }
    }
}

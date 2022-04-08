using System;
using System.Collections.Generic;
using System.Reflection;

namespace AnalysisClass
{
    internal class Program
    {
        static void Main(string[] args)
        {
            EntityClass.GetClassProperinfo(typeof(FatherClass));
            foreach (var item in EntityClass.propertyInfos)
            {
                Console.WriteLine(item.Name);
            }
            var fatherclass = new FatherClass();
            
        }

      
    }
    internal static class EntityClass
    {
        public static List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
        public static void GetClassProperinfo(Type type)
        {
            
            //Console.WriteLine(type.FullName);
            var properlist = type.GetProperties();
            foreach (var item in properlist)
            {
               
                var itemty = item.PropertyType;
                var ss = Type.GetTypeCode(itemty);
                if (!itemty.Namespace.Contains("System"))
                {
                    GetClassProperinfo(itemty);
                }

                //判断是否为泛型
                if (itemty.IsGenericType)
                {

                    //获取泛型的T
                    var dd = itemty.GenericTypeArguments;
                    foreach (var item1 in dd)
                    {
                        Console.WriteLine(item1.BaseType);
                        //判断是否为用户自定义类型
                        if (item1.IsTypeDefinition)
                        {
                           
                            GetClassProperinfo(item1);
                        }
                    }

                }

            }
            propertyInfos.AddRange(properlist);
        }
    }

}

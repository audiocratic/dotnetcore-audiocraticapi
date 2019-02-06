using System;
using System.Dynamic;
using System.Collections;
using System.Collections.Generic;

namespace CustomExtensions
{
    public static class ExpandoObjectExtensions
    {
        public static bool HasProperty(this ExpandoObject obj, string propertyAddress)
        {
            bool hasProperty = true;
            
            string[] props = propertyAddress.Split(".");

            dynamic thisObject = obj;

            for(int i = 0; i < props.Length; i++)
            {
                if(!((IDictionary<string, object>)thisObject).ContainsKey(props[i]))
                {
                    hasProperty = false;
                }

                if(props.Length - i > 1)
                {
                    if(hasProperty) thisObject = ((IDictionary<string, object>)thisObject)[props[i]];
                }
            }

            return hasProperty;
        }
    }  
}

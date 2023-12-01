using System;
using System.Runtime.CompilerServices;

namespace HiddenGems.Runtime
{
    //Taken from https://stackoverflow.com/questions/9062235/get-properties-in-order-of-declaration-using-reflection
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class OrderAttribute : Attribute
    {
        private readonly int order_;
        public OrderAttribute([CallerLineNumber] int order = 0)
        {
            order_ = order;
        }

        public int Order { get { return order_; } }
    }
}

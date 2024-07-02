using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PowerShellTools.Test.Common
{
    [TestClass]
    public class VariableTest
    {
        [TestMethod]
        public void ShouldReturnPSVariable()
        {
            var variable = new Variable(new PSVariable("value", "string"));

            Assert.AreEqual("value", variable.VarName);
            Assert.AreEqual("string", variable.VarValue);
            Assert.AreEqual("System.String", variable.Type);
            Assert.IsFalse(variable.HasChildren);
        }

        [TestMethod]
        public void ShouldReturnStringVariable()
        {
            var psobject = new PSObject(new { Name = "value", Value = "test" });
            var variable = new Variable(psobject);

            Assert.AreEqual("value", variable.VarName);
            Assert.AreEqual("test", variable.VarValue);
            Assert.AreEqual("System.String", variable.Type);
            Assert.IsFalse(variable.HasChildren);
        }

        [TestMethod]
        public void ShouldReturnHashtable()
        {
            var psobject = new PSObject(new
                {
                    Name = "value",
                    Value = new PSObject(new Hashtable {
                        { "Name", "Value1" },
                        { "Name2", "Value2" },
                        { "Name3", 1234 }
                })
            });
            var variable = new Variable(psobject);

            Assert.AreEqual("value", variable.VarName);
            Assert.AreEqual("System.Collections.Hashtable", variable.VarValue);
            Assert.AreEqual("System.Collections.Hashtable", variable.Type);
            Assert.IsTrue(variable.HasChildren);

            var children = variable.GetChildren();

            Assert.AreEqual("Name", children.ElementAt(0).VarName);
            Assert.AreEqual("System.String", children.ElementAt(0).Type);
            Assert.AreEqual("value", children.ElementAt(0).VarValue);
            Assert.AreEqual("$value.Name", children.ElementAt(0).Path);
            Assert.IsFalse(children.ElementAt(0).HasChildren);

            var hashtableChildren = children.ElementAt(1);

            Assert.AreEqual("Value", hashtableChildren.VarName);
            Assert.AreEqual("System.Collections.Hashtable", hashtableChildren.VarValue);
            Assert.AreEqual("System.Collections.Hashtable", hashtableChildren.Type);
            Assert.AreEqual("$value.Value", hashtableChildren.Path);
            Assert.IsTrue(children.ElementAt(1).HasChildren);

            children = hashtableChildren.GetChildren();

            Assert.AreEqual("Name", children.ElementAt(5).VarName);
            Assert.AreEqual("System.String", children.ElementAt(5).Type);
            Assert.AreEqual("Value1", children.ElementAt(5).VarValue);
            Assert.AreEqual("$value.Value.Name", children.ElementAt(5).Path);
            Assert.IsFalse(children.ElementAt(5).HasChildren);

            Assert.AreEqual("Name2", children.ElementAt(4).VarName);
            Assert.AreEqual("System.String", children.ElementAt(4).Type);
            Assert.AreEqual("Value2", children.ElementAt(4).VarValue);
            Assert.AreEqual("$value.Value.Name2", children.ElementAt(4).Path);
            Assert.IsFalse(children.ElementAt(4).HasChildren);

            Assert.AreEqual("Name3", children.ElementAt(3).VarName);
            Assert.AreEqual("System.Int32", children.ElementAt(3).Type);
            Assert.AreEqual("1234", children.ElementAt(3).VarValue);
            Assert.AreEqual("$value.Value.Name3", children.ElementAt(3).Path);
            Assert.IsFalse(children.ElementAt(3).HasChildren);

            var child = variable.FindChild("$value.Name");
            Assert.AreEqual("value", child.VarValue);
        }
    }
}

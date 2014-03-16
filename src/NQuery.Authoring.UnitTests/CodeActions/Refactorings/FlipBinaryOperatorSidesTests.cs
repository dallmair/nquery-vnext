﻿using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.UnitTests.CodeActions.Refactorings
{
    [TestClass]
    public class FlipBinaryOperatorSidesTests : RefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new FlipBinaryOperatorSidesCodeRefactoringProvider();
        }

        [TestMethod]
        public void FlipBinaryOperatorSides_SwapsOperators()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   /* prefix */ e.FirstName /* before */ =| /* after */ 'Andrew' /* suffix */
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                WHERE   /* prefix */ 'Andrew' /* before */ = /* after */ e.FirstName /* suffix */
            ";

            var actions = GetActions(query);
            Assert.AreEqual(1, actions.Length);

            var action = actions.Single();
            Assert.AreEqual("Flip arguments of operator '='", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }
    }
}